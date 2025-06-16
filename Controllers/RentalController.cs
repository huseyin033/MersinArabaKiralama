using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MersinAracKiralama.Data;
using MersinAracKiralama.Models;
using MersinAracKiralama.Services;namespace MersinAracKiralama.Controllers
{
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;
        private readonly ICustomerService _customerService;
        public RentalController(IRentalService rentalService, ICarService carService, ICustomerService customerService)
        {
            _rentalService = rentalService;
            _carService = carService;
            _customerService = customerService;
        }        public async Task<IActionResult> Index()
        {
            var rentals = await _rentalService.GetAllAsync();
            return View(rentals);
        }        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var rental = await _rentalService.GetByIdAsync(id.Value);
            if (rental == null) return NotFound();
            return View(rental);
        }        public async Task<IActionResult> Create()
        {
            ViewBag.Cars = await _carService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View();
        }        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental)
        {
            if (ModelState.IsValid)
            {
                await _rentalService.AddAsync(rental);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Cars = await _carService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View(rental);
        }        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var rental = await _rentalService.GetByIdAsync(id.Value);
            if (rental == null) return NotFound();
            ViewBag.Cars = await _carService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View(rental);
        }        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rental rental)
        {
            if (id != rental.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _rentalService.UpdateAsync(rental);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Cars = await _carService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View(rental);
        }        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var rental = await _rentalService.GetByIdAsync(id.Value);
            if (rental == null) return NotFound();
            return View(rental);
        }        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _rentalService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
} 
