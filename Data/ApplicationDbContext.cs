using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MersinAracKiralama.Models;namespace MersinAracKiralama.Data
{    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }        public DbSet<Car> Cars { get; set; }        public DbSet<Reservation> Reservations { get; set; }        public DbSet<Customer> Customers { get; set; }        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Favorite> Favorites { get; set; }        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);        }
    }
} 
