using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using Xunit;

namespace MersinArabaKiralama.Tests.Integration.Controllers
{
    public class CustomersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _dbContext;

        public CustomersApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
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
                        options.UseInMemoryDatabase("IntegrationTestDb_Customers");
                    });
                });
            });

            _client = _factory.CreateClient();
            _dbContext = _factory.Services.GetRequiredService<ApplicationDbContext>();
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetCustomer_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = new Customer 
            { 
                FirstName = "Test", 
                LastName = "User", 
                Email = "test@example.com",
                PhoneNumber = "5551234567"
            };
            
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"api/v1/customers/{customer.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Customer>>();
            
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Test", result.Data.FirstName);
            Assert.Equal("User", result.Data.LastName);
        }

        [Fact]
        public async Task CreateCustomer_ReturnsCreatedResponse_WhenModelIsValid()
        {
            // Arrange
            var newCustomer = new 
            {
                FirstName = "New",
                LastName = "Customer",
                Email = "new.customer@example.com",
                PhoneNumber = "5559876543"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/v1/customers", newCustomer);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Customer>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("New", result.Data.FirstName);
            Assert.Equal("new.customer@example.com", result.Data.Email);
            
            // Veritabanında oluşturulduğunu kontrol et
            var createdCustomer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Email == "new.customer@example.com");
                
            Assert.NotNull(createdCustomer);
            Assert.Equal("New", createdCustomer.FirstName);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNoContent_WhenCustomerExists()
        {
            // Arrange
            var customer = new Customer 
            { 
                FirstName = "Old", 
                LastName = "Name", 
                Email = "old@example.com",
                PhoneNumber = "5551112233"
            };
            
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            var updatedCustomer = new 
            {
                Id = customer.Id,
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@example.com",
                PhoneNumber = "5554445566"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"api/v1/customers/{customer.Id}", updatedCustomer);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Veritabanında güncellendiğini kontrol et
            var dbCustomer = await _dbContext.Customers.FindAsync(customer.Id);
            Assert.NotNull(dbCustomer);
            Assert.Equal("Updated", dbCustomer.FirstName);
            Assert.Equal("updated@example.com", dbCustomer.Email);
            Assert.Equal("5554445566", dbCustomer.PhoneNumber);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNoContent_WhenCustomerExists()
        {
            // Arrange
            var customer = new Customer 
            { 
                FirstName = "ToDelete", 
                LastName = "User", 
                Email = "delete@example.com",
                PhoneNumber = "5559998877"
            };
            
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"api/v1/customers/{customer.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Veritabanından silindiğini kontrol et
            var deletedCustomer = await _dbContext.Customers.FindAsync(customer.Id);
            Assert.Null(deletedCustomer);
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
