using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MersinAracKiralama.Data;
using MersinAracKiralama.Models;namespace MersinAracKiralama.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CustomersApiController(ApplicationDbContext context)
        {
            _context = context;
        }        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return customer;
        }        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.Id) return BadRequest();
            _context.Entry(customer).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Customers.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 
