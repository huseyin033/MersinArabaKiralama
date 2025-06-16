#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MersinArabaKiralama.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Models.ApplicationUser> _signInManager;
        private readonly UserManager<Models.ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<Models.ApplicationUser> signInManager, UserManager<Models.ApplicationUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Beni Hatırla")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            ReturnUrl = returnUrl ?? Url.Content("~/");
            await Task.CompletedTask;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            _logger.LogInformation($"OnPostAsync çağrıldı. Input null mu: {Input == null}");
            if (Input != null)
            {
                _logger.LogInformation($"Input.Email: {Input.Email ?? "null"}");
                _logger.LogInformation($"Input.Password: {(string.IsNullOrEmpty(Input.Password) ? "null veya boş" : "dolu")}");
                _logger.LogInformation($"Input.RememberMe: {Input.RememberMe}");
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState geçerli. Giriş denemesi yapılıyor.");
                var result = await _signInManager.PasswordSignInAsync(Input?.Email ?? string.Empty, Input?.Password ?? string.Empty, Input?.RememberMe ?? false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı giriş yaptı.");
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kullanıcı hesabı kilitlendi.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Şifrenizi yanlış girdiniz. Lütfen tekrar deneyiniz.");
                    _logger.LogWarning("Giriş başarısız: Şifre yanlış.");
                    return Page();
                }
            }
            else
            {
                _logger.LogWarning("ModelState geçerli değil. Hatalar:");
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        _logger.LogError($"ModelState Hatası: {error.ErrorMessage}");
                    }
                }
            }
            return Page();
        }
    }
}