using Microsoft.AspNetCore.Mvc;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; // ILogger için eklendi
using Microsoft.EntityFrameworkCore; // Gerekirse ApplicationDbContext için
using MersinArabaKiralama.Data; // Gerekirse ApplicationDbContext için
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MersinArabaKiralama.Controllers
{
    public class CarController(
        ICarService carService,
        IFavoriteService favoriteService,
        UserManager<ApplicationUser> userManager,
        ILogger<CarController> logger, // ILogger eklendi
        ICustomerService customerService // ICustomerService eklendi
        // ApplicationDbContext context // Eğer doğrudan context'e ihtiyaç varsa eklenebilir, ancak genellikle servisler üzerinden erişim tercih edilir.
        ) : Controller
    {
        // Index metodu ve diğer metotlar, birincil oluşturucudan gelen servisleri kullanacak şekilde güncellenmeli.
        // Örneğin: _carService yerine carService kullanılacak.

        // Araç listesi ve filtreleme
        [Authorize]
        public async Task<IActionResult> Index(string? brand, string? model, bool? isAvailable, decimal? minPrice, decimal? maxPrice)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return Challenge();
            }

            var customer = await customerService.GetByEmailAsync(user.Email);
            if (customer == null)
            {
                ViewBag.CustomerError = "Sistemde müşteri kaydınız bulunamadı. Lütfen müşteri kaydınızı tamamlayınız.";
                return View("CustomerNotFound");
            }

            try
            {
                var cars = (await carService.GetAllAsync()).AsQueryable();

                if (!string.IsNullOrEmpty(brand))
                    cars = cars.Where(c => c.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(model))
                    cars = cars.Where(c => c.Model.Contains(model, StringComparison.OrdinalIgnoreCase));

                if (isAvailable.HasValue)
                    cars = cars.Where(c => c.IsAvailable == isAvailable.Value);

                if (minPrice.HasValue)
                    cars = cars.Where(c => c.DailyPrice >= minPrice.Value);

                if (maxPrice.HasValue)
                    cars = cars.Where(c => c.DailyPrice <= maxPrice.Value);

                logger.LogInformation("Araç listesi görüntülendi. Filtre: Marka={Brand}, Model={Model}, Müsaitlik={IsAvailable}, MinFiyat={MinPrice}, MaxFiyat={MaxPrice}",
                                        brand, model, isAvailable, minPrice, maxPrice);

                // Favori durumlarını da view'e taşıyalım
                var favoriteCarIds = (await favoriteService.GetUserFavoritesAsync(user.Id)).Select(f => f.CarId).ToList();
                ViewBag.FavoriteCarIds = favoriteCarIds;

                return View(cars.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Araç listesi görüntülenirken hata oluştu.");
                return View("Error");
            }
        }

        // GET: Car/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Car/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Brand,Model,Year,DailyPrice,IsAvailable,ImageUrl")] Car car, IFormFile? imageFile) // ImageUrl ve IFormFile eklendi
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    // Güvenlik için dosya adını ve uzantısını kontrol edin
                    var extension = Path.GetExtension(fileName).ToLowerInvariant();
                    if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
                    {
                        ModelState.AddModelError("imageFile", "Sadece .jpg, .png, .jpeg uzantılı dosyalar yüklenebilir.");
                        return View(car);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    car.ImageUrl = "/images/cars/" + uniqueFileName;
                }
                else if (string.IsNullOrEmpty(car.ImageUrl)) // Yeni resim yüklenmediyse ve mevcut resim yoksa
                {
                    car.ImageUrl = "/images/cars/default-car.png"; // Varsayılan resim
                }

                await carService.AddAsync(car);
                TempData["SuccessMessage"] = "Araç başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Car/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await carService.GetByIdAsync(id.Value);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Car/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Year,DailyPrice,IsAvailable,ImageUrl")] Car car, IFormFile? imageFile) // ImageUrl ve IFormFile eklendi
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Eski resmi sil (opsiyonel ama önerilir)
                        if (!string.IsNullOrEmpty(car.ImageUrl) && car.ImageUrl != "/images/cars/default-car.png")
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", car.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var fileName = Path.GetFileName(imageFile.FileName);
                        var extension = Path.GetExtension(fileName).ToLowerInvariant();
                        if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
                        {
                            ModelState.AddModelError("imageFile", "Sadece .jpg, .png, .jpeg uzantılı dosyalar yüklenebilir.");
                            return View(car);
                        }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars", uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        car.ImageUrl = "/images/cars/" + uniqueFileName;
                    }
                    // Yeni resim yüklenmediyse ve mevcut ImageUrl boşsa veya varsayılan ise, varsayılanı ata
                    else if (string.IsNullOrEmpty(car.ImageUrl))
                    {
                        car.ImageUrl = "/images/cars/default-car.png";
                    }

                    await carService.UpdateAsync(car);
                    TempData["SuccessMessage"] = "Araç başarıyla güncellendi.";
                }
                catch (Exception ex) // DbUpdateConcurrencyException olabilir
                {
                    logger.LogError(ex, "Araç güncellenirken bir hata oluştu: {CarId}", car.Id);
                    if (!await CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Car/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await carService.GetByIdAsync(id.Value);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await carService.GetByIdAsync(id);
            if (car != null && !string.IsNullOrEmpty(car.ImageUrl) && car.ImageUrl != "/images/cars/default-car.png")
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", car.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            await carService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Araç başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CarExists(int id)
        {
            return await carService.GetByIdAsync(id) != null;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var car = await carService.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.IsFavorite = await favoriteService.FindAsync(id, user.Id) != null;
            }
            else
            {
                ViewBag.IsFavorite = false; // Kullanıcı giriş yapmamışsa favori olamaz
            }
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Authorize eklendi, sadece giriş yapanlar favoriye ekleyebilir
        public async Task<IActionResult> ToggleFavorite(int carId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingFavorite = await favoriteService.FindAsync(carId, user.Id);
            if (existingFavorite == null)
            {
                await favoriteService.AddAsync(new Favorite { CarId = carId, UserId = user.Id });
                TempData["SuccessMessage"] = "Araç favorilerinize eklendi.";
            }
            else
            {
                await favoriteService.RemoveAsync(existingFavorite);
                TempData["SuccessMessage"] = "Araç favorilerinizden çıkarıldı.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToFavorites(int carId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingFavorite = await favoriteService.FindAsync(carId, user.Id);
            if (existingFavorite == null)
            {
                await favoriteService.AddAsync(new Favorite { CarId = carId, UserId = user.Id });
                TempData["SuccessMessage"] = "Araç favorilerinize eklendi.";
            }
            else
            {
                TempData["InfoMessage"] = "Bu araç zaten favorilerinizde.";
            }
            // Kullanıcıyı geldiği sayfaya veya araç detaylarına yönlendir
            // string? referer = Request.Headers["Referer"].ToString(); // Eski hali
            string? referer = Request.Headers.Referer.ToString(); // Yeni hali
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction(nameof(Details), new { id = carId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RemoveFromFavorites(int carId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var favorite = await favoriteService.FindAsync(carId, user.Id);
            if (favorite != null)
            {
                await favoriteService.RemoveAsync(favorite);
                TempData["SuccessMessage"] = "Araç favorilerden çıkarıldı.";
                return RedirectToAction("Index"); // Anasayfaya yönlendir
            }
            
            TempData["ErrorMessage"] = "Bu araç favorilerinizde bulunmuyor.";
            return RedirectToAction("Index"); // Hata durumunda da anasayfaya yönlendir
        }

        [HttpGet]
        [Authorize] // Authorize eklendi, sadece giriş yapanlar favorilerini görebilir
        public async Task<IActionResult> Favorites()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Giriş yapmamışsa Challenge gönder

            var favorites = await favoriteService.GetUserFavoritesAsync(user.Id);
            // Car nesnelerini de yükleyerek gönderelim (eager loading veya explicit loading ile)
            var favoriteCars = favorites.Select(f => f.Car).ToList();
            return View(favoriteCars);
        }

        // Ödeme Sayfası (Bu kısım RentalController'a taşınabilir veya burada kalabilir)
        [Authorize]
        public async Task<IActionResult> Payment(int carId)
        {
            var car = await carService.GetByIdAsync(carId);
            if (car == null || !car.IsAvailable)
            {
                TempData["ErrorMessage"] = "Araç bulunamadı veya şu anda müsait değil.";
                return RedirectToAction(nameof(Index));
            }

            var paymentViewModel = new MersinArabaKiralama.ViewModels.PaymentViewModel
            {
                Car = car
                // Gerekirse diğer ViewModel özelliklerini burada ayarlayın
            };

            // Kiralama süresi ve toplam fiyatı burada belirleyebilir veya kullanıcıdan alabiliriz.
            // Şimdilik varsayılan bir kiralama süresi (örn: 1 gün) ve fiyatı hesaplayalım.
            ViewBag.RentalDays = 1; // Örnek olarak 1 gün
            ViewBag.TotalPrice = car.DailyPrice * ViewBag.RentalDays;
            ViewBag.CarId = carId;

            return View("~/Views/Rental/Payment.cshtml", paymentViewModel); // Payment view'ının yolunu belirtin
        }
    }
}