using Microsoft.AspNetCore.Identity;
using MersinArabaKiralama.Models;

namespace MersinArabaKiralama.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            string adminEmail = "admin@mersinarac.com";
            string adminPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task SeedCarsAsync(ApplicationDbContext context)
        {
            if (!context.Cars.Any())
            {
                var cars = new Car[]
                {
                    new Car { Brand = "Toyota", Model = "Corolla", Year = 2019, DailyPrice = 550, IsAvailable = true },
                    new Car { Brand = "Honda", Model = "Civic", Year = 2018, DailyPrice = 520, IsAvailable = true },
                    new Car { Brand = "Ford", Model = "Focus", Year = 2017, DailyPrice = 480, IsAvailable = true },
                    new Car { Brand = "Renault", Model = "Clio", Year = 2020, DailyPrice = 500, IsAvailable = false },
                    new Car { Brand = "Volkswagen", Model = "Golf", Year = 2016, DailyPrice = 530, IsAvailable = true },
                    new Car { Brand = "Hyundai", Model = "i20", Year = 2019, DailyPrice = 510, IsAvailable = true },
                    new Car { Brand = "Peugeot", Model = "308", Year = 2018, DailyPrice = 490, IsAvailable = false },
                    new Car { Brand = "Skoda", Model = "Octavia", Year = 2017, DailyPrice = 500, IsAvailable = true },
                    new Car { Brand = "Opel", Model = "Astra", Year = 2019, DailyPrice = 505, IsAvailable = true },
                    new Car { Brand = "Fiat", Model = "Egea", Year = 2020, DailyPrice = 470, IsAvailable = true },
                    new Car { Brand = "Kia", Model = "Ceed", Year = 2018, DailyPrice = 515, IsAvailable = true },
                    new Car { Brand = "Nissan", Model = "Qashqai", Year = 2017, DailyPrice = 550, IsAvailable = false },
                    new Car { Brand = "Dacia", Model = "Duster", Year = 2019, DailyPrice = 460, IsAvailable = true },
                    new Car { Brand = "Mazda", Model = "3", Year = 2018, DailyPrice = 520, IsAvailable = true },
                    new Car { Brand = "Seat", Model = "Leon", Year = 2017, DailyPrice = 510, IsAvailable = false },
                    new Car { Brand = "Citroen", Model = "C4", Year = 2019, DailyPrice = 490, IsAvailable = true },
                    new Car { Brand = "Suzuki", Model = "Vitara", Year = 2020, DailyPrice = 540, IsAvailable = true },
                    new Car { Brand = "Mercedes-Benz", Model = "A-Class", Year = 2018, DailyPrice = 720, IsAvailable = true },
                    new Car { Brand = "BMW", Model = "3 Series", Year = 2017, DailyPrice = 750, IsAvailable = false },
                    new Car { Brand = "Audi", Model = "A3", Year = 2019, DailyPrice = 730, IsAvailable = true },
                    new Car { Brand = "Renault", Model = "Megane", Year = 2018, DailyPrice = 500, IsAvailable = true },
                    new Car { Brand = "Toyota", Model = "Yaris", Year = 2020, DailyPrice = 470, IsAvailable = true },
                    new Car { Brand = "Hyundai", Model = "Elantra", Year = 2019, DailyPrice = 520, IsAvailable = false },
                    new Car { Brand = "Ford", Model = "Fiesta", Year = 2018, DailyPrice = 460, IsAvailable = true },
                    new Car { Brand = "Volkswagen", Model = "Passat", Year = 2017, DailyPrice = 700, IsAvailable = true }
                };
                context.Cars.AddRange(cars);
                await context.SaveChangesAsync();
            }
        }
    }
}