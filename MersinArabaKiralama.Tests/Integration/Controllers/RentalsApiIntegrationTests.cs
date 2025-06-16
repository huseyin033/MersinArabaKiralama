using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using Moq;
using Xunit;

namespace MersinArabaKiralama.Tests.Integration.Controllers
{
    public class RentalsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ICarService> _mockCarService;
        private readonly string _testUserId = "test-user-id";

        public RentalsApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _mockCarService = new Mock<ICarService>();
            
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Gerçek veritabanı bağlantısını kaldır
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Bellek içi veritabanı kullan
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("IntegrationTestDb_Rentals");
                    });
                    
                    // Mock servisleri ekle
                    services.AddScoped<ICarService>(_ => _mockCarService.Object);
                });
            });

            _client = _factory.CreateClient();
            _dbContext = _factory.Services.GetRequiredService<ApplicationDbContext>();
            _dbContext.Database.EnsureCreated();
            
            // Test kullanıcısı için kimlik doğrulama başlığı ekle
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("TestAuth", _testUserId);
        }

        [Fact]
        public async Task GetRental_ReturnsRental_WhenRentalExists()
        {
            // Arrange
            var car = new Car { Id = 1, Brand = "Test Brand", Model = "Test Model", DailyPrice = 100, IsAvailable = true };
            var customer = new Customer { Id = 1, FirstName = "Test", LastName = "User", Email = "test@example.com", UserId = _testUserId };
            var rental = new Rental
            {
                Id = 1,
                CarId = 1,
                CustomerId = 1,
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date.AddDays(3),
                TotalPrice = 300,
                Status = RentalStatus.Confirmed
            };
            
            _dbContext.Cars.Add(car);
            _dbContext.Customers.Add(customer);
            _dbContext.Rentals.Add(rental);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"api/v1/rentals/{rental.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Rental>>();
            
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, result.Data.CarId);
            Assert.Equal(1, result.Data.CustomerId);
            Assert.Equal(RentalStatus.Confirmed, result.Data.Status);
        }

        [Fact]
        public async Task CreateRental_ReturnsCreatedResponse_WhenCarIsAvailable()
        {
            // Arrange
            var car = new Car { Id = 2, Brand = "Available Car", Model = "Model X", DailyPrice = 150, IsAvailable = true };
            var customer = new Customer { Id = 2, FirstName = "Test", LastName = "Customer", Email = "customer@example.com", UserId = _testUserId };
            
            _dbContext.Cars.Add(car);
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            var startDate = DateTime.Now.Date.AddDays(1);
            var endDate = startDate.AddDays(3);
            
            _mockCarService.Setup(x => x.IsCarAvailableAsync(2, startDate, endDate, null))
                .ReturnsAsync(true);
                
            _mockCarService.Setup(x => x.UpdateCarStatusAsync(2, false))
                .ReturnsAsync(true);

            var newRental = new 
            {
                CarId = 2,
                StartDate = startDate,
                EndDate = endDate,
                CustomerId = 2
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/v1/rentals", newRental);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Rental>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.CarId);
            Assert.Equal(2, result.Data.CustomerId);
            Assert.Equal(450, result.Data.TotalPrice); // 3 gün * 150 TL
            Assert.Equal(RentalStatus.Confirmed, result.Data.Status);
            
            // Veritabanında oluşturulduğunu kontrol et
            var createdRental = await _dbContext.Rentals
                .FirstOrDefaultAsync(r => r.CarId == 2 && r.CustomerId == 2);
                
            Assert.NotNull(createdRental);
            Assert.Equal(RentalStatus.Confirmed, createdRental.Status);
        }

        [Fact]
        public async Task CancelRental_ReturnsNoContent_WhenRentalIsCancellable()
        {
            // Arrange
            var car = new Car { Id = 3, Brand = "Test Car", Model = "Model Y", DailyPrice = 200, IsAvailable = false };
            var customer = new Customer { Id = 3, FirstName = "Test", LastName = "User", Email = "user@example.com", UserId = _testUserId };
            var rental = new Rental
            {
                Id = 3,
                CarId = 3,
                CustomerId = 3,
                StartDate = DateTime.Now.Date.AddDays(1),
                EndDate = DateTime.Now.Date.AddDays(4),
                TotalPrice = 600,
                Status = RentalStatus.Confirmed
            };
            
            _dbContext.Cars.Add(car);
            _dbContext.Customers.Add(customer);
            _dbContext.Rentals.Add(rental);
            await _dbContext.SaveChangesAsync();

            _mockCarService.Setup(x => x.UpdateCarStatusAsync(3, true))
                .ReturnsAsync(true);

            // Act
            var response = await _client.PutAsJsonAsync($"api/v1/rentals/{rental.Id}/cancel", new { Reason = "Test cancellation" });

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Durumun güncellendiğini kontrol et
            var updatedRental = await _dbContext.Rentals.FindAsync(rental.Id);
            Assert.Equal(RentalStatus.Cancelled, updatedRental.Status);
            Assert.Equal("Test cancellation", updatedRental.CancellationReason);
            
            // Araç durumunun güncellendiğini kontrol et
            _mockCarService.Verify(x => x.UpdateCarStatusAsync(3, true), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUserRentals_ReturnsRentals_WhenUserHasRentals()
        {
            // Arrange
            var car1 = new Car { Id = 4, Brand = "Car 1", Model = "Model 1", DailyPrice = 100, IsAvailable = false };
            var car2 = new Car { Id = 5, Brand = "Car 2", Model = "Model 2", DailyPrice = 150, IsAvailable = false };
            var customer = new Customer { Id = 4, FirstName = "Current", LastName = "User", Email = "current@example.com", UserId = _testUserId };
            
            var rentals = new List<Rental>
            {
                new Rental { Id = 4, CarId = 4, CustomerId = 4, StartDate = DateTime.Now.Date.AddDays(-5), 
                    EndDate = DateTime.Now.Date.AddDays(-2), TotalPrice = 300, Status = RentalStatus.Completed },
                new Rental { Id = 5, CarId = 5, CustomerId = 4, StartDate = DateTime.Now.Date.AddDays(1), 
                    EndDate = DateTime.Now.Date.AddDays(4), TotalPrice = 450, Status = RentalStatus.Confirmed }
            };
            
            _dbContext.Cars.AddRange(car1, car2);
            _dbContext.Customers.Add(customer);
            _dbContext.Rentals.AddRange(rentals);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("api/v1/rentals/me");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Rental>>>();
            
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, r => r.Id == 4 && r.Status == RentalStatus.Completed);
            Assert.Contains(result.Data, r => r.Id == 5 && r.Status == RentalStatus.Confirmed);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _client.Dispose();
        }
    }

    // Test için gerekli yardımcı sınıf
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
