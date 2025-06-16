using Microsoft.AspNetCore.Mvc;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MersinArabaKiralama.Controllers
{
    [Authorize(Roles = "Admin")] // Controller seviyesinde yetkilendirme
    public class CustomerController(
        ICustomerService customerService,
        ILogger<CustomerController> logger) : Controller
    {
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewData["CurrentFilter"] = searchTerm;
            var customers = await customerService.GetAllAsync();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                customers = customers.Where(c => 
                                               c.Email!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return View(customers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await customerService.GetByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists for another customer
                var existingCustomerByEmail = await customerService.GetByEmailAsync(customer.Email!);
                if (existingCustomerByEmail != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                    return View(customer);
                }

                await customerService.AddAsync(customer);
                TempData["SuccessMessage"] = "Müşteri başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await customerService.GetByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Address")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists for another customer
                    var existingCustomerByEmail = await customerService.GetByEmailAsync(customer.Email!);
                    if (existingCustomerByEmail != null && existingCustomerByEmail.Id != customer.Id)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi başka bir müşteriye ait.");
                        return View(customer);
                    }

                    await customerService.UpdateAsync(customer);
                    TempData["SuccessMessage"] = "Müşteri başarıyla güncellendi.";
                }
                catch (Exception ex) // DbUpdateConcurrencyException olabilir
                {
                    logger.LogError(ex, "Müşteri güncellenirken bir hata oluştu: {CustomerId}", customer.Id);
                    if (!await CustomerExists(customer.Id))
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
            return View(customer);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await customerService.GetByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await customerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Müşteri başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await customerService.GetByIdAsync(id) != null;
        }
    }
}