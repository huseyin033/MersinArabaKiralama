using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Middlewares;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Serilog'u ekle
    builder.Host.UseSerilog();

    // DbContext servisini ekle
builder.Services.AddDbContext<MersinArabaKiralama.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Add CORS policy for React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
    });
});
// Uygulama servisleri (Dependency Injection)
builder.Services.AddScoped<MersinArabaKiralama.Services.ICarService, MersinArabaKiralama.Services.CarService>();
builder.Services.AddScoped<MersinArabaKiralama.Services.ICustomerService, MersinArabaKiralama.Services.CustomerService>();
builder.Services.AddScoped<MersinArabaKiralama.Services.IRentalService, MersinArabaKiralama.Services.RentalService>();
builder.Services.AddTransient<ErrorHandlerMiddleware>();

// API Versiyonlama
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddScoped<MersinArabaKiralama.Services.IFavoriteService, MersinArabaKiralama.Services.FavoriteService>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, MersinArabaKiralama.Services.NullEmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    // Geliştirme ortamında HTTPS yönlendirmesini kullanma
}
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Hata yönetimini middleware olarak ekleyin
app.UseMiddleware<ErrorHandlerMiddleware>();

// API attribute-routed controllers
app.MapControllers();

// MVC default route for Razor pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages (e.g., /Identity/Account/Login)
app.MapRazorPages();

// ✅ Hata loglama middleware'i EN SONDA değil, UseRouting'tan sonra ama UseEndpoints'tan önce olmalı
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n";
        await File.AppendAllTextAsync(logPath, logEntry);

        if (ex is NullReferenceException || ex is InvalidOperationException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"KRİTİK HATA: {ex.Message}");
            Console.ResetColor();
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var errorResponse = JsonSerializer.Serialize(new { error = "Bir hata oluştu.", detail = ex.Message });
        await context.Response.WriteAsync(errorResponse);
    }
});

// ✅ Seed işlemi sona alınabilir, loglamadan sonra da olabilir
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    await MersinArabaKiralama.Data.DbInitializer.SeedCarsAsync(context);
    await MersinArabaKiralama.Data.DbInitializer.SeedRolesAndAdminAsync(services);
}

// SPA fallback: Bilinmeyen yolları index.html'e yönlendir
app.Use(async (context, next) =>
{
    await next();
    var path = context.Request.Path.Value;
    if (context.Response.StatusCode == 404 &&
        path != null &&
        path.StartsWith("/app") &&
        !Path.HasExtension(path))
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "app", "index.html"));
    }
});

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
