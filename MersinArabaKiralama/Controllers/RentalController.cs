using Microsoft.AspNetCore.Mvc;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using Microsoft.AspNetCore.Identity;

namespace MersinArabaKiralama.Controllers
{
    public class RentalController(
        IRentalService rentalService,
        ICarService carService,
        ICustomerService customerService,
        UserManager<ApplicationUser> userManager) : Controller
    {

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var rentals = await rentalService.GetAllAsync();
            return View(rentals);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var rental = await rentalService.GetByIdAsync(id.Value);
            if (rental == null) return NotFound();
            return View(rental);
        }

        // Bu eski Create metodu, direkt kiralama oluşturuyordu, şimdi ödeme adımını ekliyoruz.
        // public async Task<IActionResult> Create()
        // {
        //     ViewBag.Cars = await _carService.GetAllAsync();
        //     ViewBag.Customers = await _customerService.GetAllAsync();
        //     return View();
        // }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Create(int carId, int days = 1) // Kiralama isteği için carId ve gün sayısı alıyoruz
        {
            var car = await carService.GetByIdAsync(carId);
            if (car == null || !car.IsAvailable)
            {
                TempData["ErrorMessage"] = "Araç bulunamadı veya şu anda müsait değil.";
                return RedirectToAction("Index", "Car");
            }

            // Gün sayısı geçerli değilse 1 yap
            if (days < 1) days = 1;

            // Kullanıcı bilgilerini al
            var user = await userManager.GetUserAsync(User);
            string campaignInfo = "";
            decimal totalPrice = car.DailyPrice * days;

            if (user != null)
            {
                // Kampanyaları uygula
                totalPrice = ApplyCampaigns(days, car.DailyPrice, user.Id, out campaignInfo);
            }

            // Ödeme view modelini hazırla
            var paymentViewModel = new MersinArabaKiralama.ViewModels.PaymentViewModel
            {
                Car = car,
                RentalDays = days,
                TotalPrice = totalPrice
            };

            ViewBag.RentalDays = days;
            ViewBag.TotalPrice = paymentViewModel.TotalPrice;
            ViewBag.CarId = carId;
            ViewBag.CampaignInfo = campaignInfo;

            return View("Payment", paymentViewModel); // Payment.cshtml view'ine yönlendir
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Rental rental) // Bu metot admin panelinden direkt kiralama için kalabilir veya kaldırılabilir.
        {
            if (ModelState.IsValid)
            {
                var car = await carService.GetByIdAsync(rental.CarId);
                if (car != null && car.IsAvailable)
                {
                    await rentalService.AddAsync(rental);
                    // Araç müsaitlik durumunu güncelle
                    car.IsAvailable = false;
                    await carService.UpdateAsync(car);
                    TempData["SuccessMessage"] = "Kiralama başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index)); // Admin kiralama listesine yönlendir
                }
                else
                {
                    ModelState.AddModelError("", "Seçilen araç bulunamadı veya müsait değil.");
                }
            }
            ViewBag.Cars = await carService.GetAllAsync();
            ViewBag.Customers = await customerService.GetAllAsync();
            return View(rental);
        }

        private decimal ApplyCampaigns(int rentalDays, decimal dailyPrice, string userId, out string campaignInfo)
        {
            decimal totalPrice = dailyPrice * rentalDays;
            var campaignInfos = new List<string>();
            var userRentals = rentalService.GetUserRentals(userId).Result.Count();

            // %10 İndirimli İlk Kiralama
            if (userRentals == 0)
            {
                decimal discount = totalPrice * 0.1m;
                totalPrice -= discount;
                campaignInfos.Add($"%10 İndirimli İlk Kiralama Kampanyası uygulandı. İndirim: {discount:C}");
            }

            // 3 Gün Kirala 1 Gün Bedava (Yaz Kampanyası - Haziran-Temmuz-Ağustos)
            if ((DateTime.Now.Month >= 6 && DateTime.Now.Month <= 8) && rentalDays >= 3)
            {
                int freeDays = rentalDays / 3;
                decimal discount = dailyPrice * freeDays;
                totalPrice -= discount;
                campaignInfos.Add($"Yaz Kampanyası: 3 Gün Kirala 1 Gün Bedava! {freeDays} gün ücretsiz. İndirim: {discount:C}");
            }

            campaignInfo = string.Join(" ", campaignInfos);
            return totalPrice;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ProcessPayment(MersinArabaKiralama.ViewModels.PaymentViewModel model, int carId, int rentalDays)
        {
            var user = await userManager.GetUserAsync(User);
            var car = await carService.GetByIdAsync(carId);

            if (user == null || car == null || !car.IsAvailable)
            {
                TempData["ErrorMessage"] = "Bir hata oluştu. Araç bulunamadı, kullanıcı girişi yapılmamış veya araç müsait değil.";
                return RedirectToAction("Index", "Car");
            }

            var customer = await customerService.GetByEmailAsync(user.Email!);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Müşteri kaydınız bulunamadı.";
                return RedirectToAction("Index", "Car"); // Veya müşteri oluşturma/profil sayfasına
            }

            if (ModelState.IsValid)
            {
                // Kampanyaları uygula
                string campaignInfo;
                decimal totalPrice = ApplyCampaigns(rentalDays, car.DailyPrice, user.Id, out campaignInfo);

                var rental = new Rental
                {
                    CarId = car.Id,
                    CustomerId = customer.Id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(rentalDays),
                    TotalPrice = totalPrice,
                    CampaignInfo = campaignInfo
                };

                await rentalService.AddAsync(rental);

                car.IsAvailable = false;
                await carService.UpdateAsync(car);

                string successMessage;
                if (!string.IsNullOrEmpty(campaignInfo))
                {
                    successMessage = $"Ödemeniz başarılı!! {campaignInfo} Keyifli sürüşler dileriz";
                }
                else
                {
                    successMessage = "Ödemeniz başarılı!! Keyifli sürüşler dileriz";
                }
                
                ViewBag.PaymentSuccess = true;
                ViewBag.SuccessMessage = successMessage;
                
                // Aynı view'i döndür
                model.Car = car;
                ViewBag.RentalDays = rentalDays;
                ViewBag.TotalPrice = totalPrice;
                return View("Payment", model);
            }

            model.Car = car;
            ViewBag.RentalDays = rentalDays;
            ViewBag.TotalPrice = car.DailyPrice * rentalDays;
            ViewBag.CarId = carId;
            return View("Payment", model);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var rental = await rentalService.GetByIdAsync(id.Value);
            if (rental == null) return NotFound();
            ViewBag.Cars = await carService.GetAllAsync();
            ViewBag.Customers = await customerService.GetAllAsync();
            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Rental rental)
        {
            if (ModelState.IsValid)
            {
                await rentalService.UpdateAsync(rental);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Cars = await carService.GetAllAsync();
            ViewBag.Customers = await customerService.GetAllAsync();
            return View(rental);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var rental = await rentalService.GetByIdAsync(id.Value);
            if (rental == null) return NotFound();
            return View(rental);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await rentalService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}