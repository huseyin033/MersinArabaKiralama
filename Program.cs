using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MersinAracKiralama.Data;
using MersinAracKiralama.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext ve Identity ayarları
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Gerekli servisler
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Razor Pages servisi EKLENDİ
builder.Services.AddControllers(); // API controller'lar için

// Swagger (geliştirme ortamında)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MersinArabaKiralama API", Version = "v1" });
});

// Uygulama servisleri (Dependency Injection)
builder.Services.AddScoped<MersinAracKiralama.Services.ICarService, MersinAracKiralama.Services.CarService>();
builder.Services.AddScoped<MersinAracKiralama.Services.ICustomerService, MersinAracKiralama.Services.CustomerService>();
builder.Services.AddScoped<MersinAracKiralama.Services.IRentalService, MersinAracKiralama.Services.RentalService>();

var app = builder.Build();

// Ortam kontrolü (Swagger yalnızca geliştirmede gösterilir)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MersinArabaKiralama API v1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route ayarları
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages(); // Razor Pages route'u EKLENDİ

// Seed veriler
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedCarsAsync(context);
    // Gerekirse kullanıcı ve rol seed işlemleri de buraya eklenebilir.
}

app.Run();
