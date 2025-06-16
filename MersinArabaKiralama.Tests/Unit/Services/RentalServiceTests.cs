using Moq;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MersinArabaKiralama.Tests.Unit.Services
{
    public class RentalServiceTests
    {
        private readonly Mock<ILogger<RentalService>> _mockLogger;
        private readonly Mock<ICarService> _mockCarService;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public RentalServiceTests()
        {
            _mockLogger = new Mock<ILogger<RentalService>>();
            _mockCarService = new Mock<ICarService>();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Rental")
                .Options;
        }

        [Fact]
        public async Task GetRentalByIdAsync_ShouldReturnRental_WhenRentalExists()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                var car = new Car { Id = 1, Brand = "Test Brand", Model = "Test Model", DailyPrice = 100, IsAvailable = true };
                var customer = new Customer { Id = 1, FirstName = "Test", LastName = "User", Email = "test@example.com" };
                
                context.Cars.Add(car);
                context.Customers.Add(customer);
                
                context.Rentals.Add(new Rental
                {
                    Id = 1,
                    CarId = 1,
                    CustomerId = 1,
                    StartDate = DateTime.Now.Date,
                    EndDate = DateTime.Now.Date.AddDays(3),
                    TotalPrice = 300,
                    Status = RentalStatus.Pending
                });
                
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var service = new RentalService(context, _mockLogger.Object, _mockCarService.Object);

                // Act
                var result = await service.GetRentalByIdAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.Id);
                Assert.Equal(1, result.CarId);
                Assert.Equal(1, result.CustomerId);
                Assert.Equal(RentalStatus.Pending, result.Status);
            }
        }

        [Fact]
        public async Task AddRentalAsync_ShouldAddRental_WhenCarIsAvailable()
        {
            // Arrange
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(3);
            
            using (var context = new ApplicationDbContext(_options))
            {
                var car = new Car { Id = 2, Brand = "Test Brand", Model = "Test Model", DailyPrice = 100, IsAvailable = true };
                var customer = new Customer { Id = 2, FirstName = "Test", LastName = "User", Email = "test2@example.com" };
                
                context.Cars.Add(car);
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                _mockCarService.Setup(x => x.IsCarAvailableAsync(2, startDate, endDate, null))
                    .ReturnsAsync(true);
                    
                _mockCarService.Setup(x => x.UpdateCarStatusAsync(2, false))
                    .ReturnsAsync(true);

                var service = new RentalService(context, _mockLogger.Object, _mockCarService.Object);
                
                var rental = new Rental
                {
                    CarId = 2,
                    CustomerId = 2,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = RentalStatus.Pending
                };

                // Act
                var result = await service.AddRentalAsync(rental);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Id > 0);
                Assert.Equal(2, result.CarId);
                Assert.Equal(2, result.CustomerId);
                Assert.Equal(300, result.TotalPrice); // 3 gün * 100 TL
                Assert.Equal(RentalStatus.Pending, result.Status);
                
                // Veritabanında kaydedildiğini kontrol et
                var savedRental = await context.Rentals.FirstOrDefaultAsync(r => r.Id == result.Id);
                Assert.NotNull(savedRental);
            }
        }

        [Fact]
        public async Task CancelRentalAsync_ShouldCancelRental_WhenRentalIsPending()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                var rental = new Rental
                {
                    Id = 3,
                    CarId = 3,
                    CustomerId = 3,
                    StartDate = DateTime.Now.Date.AddDays(1),
                    EndDate = DateTime.Now.Date.AddDays(4),
                    TotalPrice = 300,
                    Status = RentalStatus.Pending
                };
                
                context.Rentals.Add(rental);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                _mockCarService.Setup(x => x.UpdateCarStatusAsync(3, true))
                    .ReturnsAsync(true);

                var service = new RentalService(context, _mockLogger.Object, _mockCarService.Object);

                // Act
                var result = await service.CancelRentalAsync(3, "Test cancellation");


                // Assert
                Assert.True(result);
                
                // Durumun güncellendiğini kontrol et
                var updatedRental = await context.Rentals.FindAsync(3);
                Assert.Equal(RentalStatus.Cancelled, updatedRental.Status);
                Assert.Equal("Test cancellation", updatedRental.CancellationReason);
            }
        }
    }
}
