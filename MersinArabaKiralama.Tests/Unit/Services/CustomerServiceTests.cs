using Moq;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MersinArabaKiralama.Tests.Unit.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ILogger<CustomerService>> _mockLogger;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public CustomerServiceTests()
        {
            _mockLogger = new Mock<ILogger<CustomerService>>();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                context.Customers.Add(new Customer { 
                    Id = 1, 
                    FirstName = "Test", 
                    LastName = "User", 
                    Email = "test@example.com",
                    PhoneNumber = "5551234567"
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CustomerService(context, _mockLogger.Object);

                // Act
                var result = await service.GetCustomerByIdAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Test", result.FirstName);
                Assert.Equal("User", result.LastName);
                Assert.Equal("test@example.com", result.Email);
            }
        }

        [Fact]
        public async Task AddCustomerAsync_ShouldAddCustomer_WhenEmailIsUnique()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CustomerService(context, _mockLogger.Object);
                var customer = new Customer { 
                    FirstName = "New", 
                    LastName = "User", 
                    Email = "new@example.com",
                    PhoneNumber = "5557654321"
                };

                // Act
                var result = await service.AddCustomerAsync(customer);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Id > 0);
                Assert.Equal("new@example.com", result.Email);
                
                // Veritabanında kaydedildiğini kontrol et
                var savedCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "new@example.com");
                Assert.NotNull(savedCustomer);
                Assert.Equal("New", savedCustomer.FirstName);
            }
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldUpdateCustomer_WhenCustomerExists()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                context.Customers.Add(new Customer { 
                    Id = 3, 
                    FirstName = "Old", 
                    LastName = "Name", 
                    Email = "old@example.com",
                    PhoneNumber = "5551112233"
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CustomerService(context, _mockLogger.Object);
                var updatedCustomer = new Customer { 
                    Id = 3, 
                    FirstName = "Updated", 
                    LastName = "Name", 
                    Email = "updated@example.com",
                    PhoneNumber = "5554445566"
                };

                // Act
                var result = await service.UpdateCustomerAsync(3, updatedCustomer);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Updated", result.FirstName);
                Assert.Equal("updated@example.com", result.Email);
                
                // Veritabanında güncellendiğini kontrol et
                var dbCustomer = await context.Customers.FindAsync(3);
                Assert.Equal("Updated", dbCustomer.FirstName);
                Assert.Equal("5554445566", dbCustomer.PhoneNumber);
            }
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldReturnTrue_WhenCustomerExists()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                context.Customers.Add(new Customer { 
                    Id = 4, 
                    FirstName = "ToDelete", 
                    LastName = "User", 
                    Email = "delete@example.com",
                    PhoneNumber = "5559998877"
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var service = new CustomerService(context, _mockLogger.Object);

                // Act
                var result = await service.DeleteCustomerAsync(4);

                // Assert
                Assert.True(result);
                
                // Veritabanından silindiğini kontrol et
                var deletedCustomer = await context.Customers.FindAsync(4);
                Assert.Null(deletedCustomer);
            }
        }
    }
}
