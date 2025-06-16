using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MersinAracKiralama.Data;
using MersinAracKiralama.Models;
using MersinAracKiralama.Services;
using Microsoft.AspNetCore.Authorization;namespace MersinAracKiralama.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarService _carService;
        private readonly IFavoriteService _favoriteService;
        private readonly IRentalService _rentalService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;
        public CarController(ICarService carService, IFavoriteService favoriteService, IRentalService rentalService, ICustomerService customerService, UserManager<ApplicationUser> userManager)
        {
            _carService = carService;
            _favoriteService = favoriteService;
            _rentalService = rentalService;
            _customerService = customerService;
            _userManager = userManager;
        }        [Authorize]
        public async Task<IActionResult> Index(string? brand, string? model, bool? isAvailable, decimal? minPrice, decimal? maxPrice)
        {
            var cars = await _carService.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(brand))
                cars = cars.Where(c => c.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(model))
                cars = cars.Where(c => c.Model.Contains(model, StringComparison.OrdinalIgnoreCase)).ToList();
            if (isAvailable.HasValue)
                cars = cars.Where(c => c.IsAvailable == isAvailable.Value).ToList();
            if (minPrice.HasValue)
                cars = cars.Where(c => c.DailyPrice >= minPrice.Value).ToList();
            if (maxPrice.HasValue)
                cars = cars.Where(c => c.DailyPrice <= maxPrice.Value).ToList();
            // Favori bilgisi
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var favs = await _favoriteService.GetUserFavoritesAsync(userId);
                ViewBag.FavIds = favs.Select(f => f.CarId).ToList();
            }
            return View(cars);
        }        [Authorize]
        [Authorize]
        public async Task<IActionResult> Favorite(int id)
        {
            var userId = _userManager.GetUserId(User);
            var exists = await _favoriteService.FindAsync(id, userId);
            if (exists == null)
            {
                await _favoriteService.AddAsync(new Favorite { CarId = id, UserId = userId });
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Rent(int id)
        {
            var car = await _carService.GetByIdAsync(id);
            if (car == null) return NotFound();
            var vm = new PaymentViewModel { Car = car };
            return View("Payment", vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(int id, PaymentViewModel vm)
        {
            var car = await _carService.GetByIdAsync(id);
            if (car == null) return NotFound();
            if (!ModelState.IsValid)
            {
                vm.Car = car;
                return View("Payment", vm);
            }
            // Basit ödeme doğrulaması tamam
            var userId = _userManager.GetUserId(User);
            var customer = (await _customerService.GetAllAsync()).FirstOrDefault(c => c.Email == User.Identity!.Name);
            if (customer == null)
            {
                customer = await _customerService.AddAsync(new Customer { Email = User.Identity!.Name!, Name = User.Identity!.Name!.Split('@')[0] });
            }
            var rental = new Rental
            {
                CarId = car.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                TotalPrice = car.DailyPrice
            };
            await _rentalService.AddAsync(rental);
            TempData["Success"] = "Ödemeniz başarıyla alınmıştır. İyi yolculuklar dileriz.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var car = await _carService.GetByIdAsync(id.Value);
            if (car == null) return NotFound();
            return View(car);
        }        public IActionResult Create()
        {
            return View();
        }        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            if (ModelState.IsValid)
            {
                await _carService.AddAsync(car);
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var car = await _carService.GetByIdAsync(id.Value);
            if (car == null) return NotFound();
            return View(car);
        }        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car)
        {
            if (id != car.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _carService.UpdateAsync(car);
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var car = await _carService.GetByIdAsync(id.Value);
            if (car == null) return NotFound();
            return View(car);
        }        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _carService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
} 
