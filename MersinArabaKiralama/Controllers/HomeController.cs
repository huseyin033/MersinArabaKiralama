using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;

namespace MersinArabaKiralama.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICarService _carService;

    public HomeController(ILogger<HomeController> logger, ICarService carService)
    {
        _logger = logger;
        _carService = carService;
    }

    public async Task<IActionResult> Index()
    {
        var cars = (await _carService.GetAllAsync()).Take(4).ToList(); // Son 4 araç
        var campaigns = new List<string> { "%10 İndirimli İlk Kiralama!", "Yaz Kampanyası: 3 Gün Kirala 1 Gün Bedava!" };
        ViewBag.Cars = cars;
        ViewBag.Campaigns = campaigns;
        return View();
    }

    public IActionResult Privacy()
    {
        var randomTexts = new List<string>
        {
            "Gizlilik politikamız, kişisel verilerinizin korunması ve işlenmesi hakkında detaylı bilgi sunar. Verileriniz, yasal düzenlemelere uygun olarak ve yalnızca belirtilen amaçlar doğrultusunda kullanılır.",
            "Şirketimiz, müşteri gizliliğine büyük önem vermektedir. Topladığımız tüm bilgiler, hizmet kalitemizi artırmak ve size daha iyi bir deneyim sunmak amacıyla kullanılır.",
            "Kişisel bilgilerinizin güvenliği bizim için önceliktir. Verileriniz, yetkisiz erişim, değişiklik veya ifşaya karşı korunmak için çeşitli güvenlik önlemleriyle saklanmaktadır.",
            "Web sitemizi kullanarak, gizlilik politikamızda belirtilen şartları kabul etmiş olursunuz. Politikamızdaki güncellemeler için lütfen düzenli olarak bu sayfayı ziyaret edin.",
            "Gizlilikle ilgili herhangi bir sorunuz veya endişeniz varsa, lütfen bizimle iletişime geçmekten çekinmeyin. Ekibimiz size yardımcı olmaktan memnuniyet duyacaktır."
        };
        var random = new Random();
        var randomIndex = random.Next(randomTexts.Count);
        ViewBag.PrivacyText = randomTexts[randomIndex];
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
