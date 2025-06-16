using MersinArabaKiralama.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MersinArabaKiralama.Data
{
    // Uygulamanın ana veritabanı context'i. Identity ve kendi modellerini içerir.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Cars = Set<Car>();
            Customers = Set<Customer>();
            Rentals = Set<Rental>();
            // Initialize Favorites DbSet in constructor
            this.Favorites = Set<Favorite>();
        }

        // Araç, müşteri ve kiralama tabloları
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fiyat alanları için hassasiyet ayarı
            modelBuilder.Entity<Car>()
                .Property(c => c.DailyPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Rental>()
                .Property(r => r.TotalPrice)
                .HasColumnType("decimal(18,2)");
        }
    }
}