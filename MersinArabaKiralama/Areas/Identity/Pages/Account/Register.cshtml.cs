// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using MersinArabaKiralama.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Services;

namespace MersinArabaKiralama.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ICustomerService _customerService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ICustomerService customerService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _customerService = customerService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? string.Empty;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (!Input.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Input.Email", "Sadece gmail adresi ile kayıt olabilirsiniz.");
                    return Page();
                }
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email ?? string.Empty, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email ?? string.Empty, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password ?? string.Empty);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    // Customer tablosuna otomatik kayıt ekle
                    var name = Input.Email.Split('@')[0];
                    try
                    {
                        var customer = await _customerService.AddAsync(new Customer { Name = name, Email = Input.Email });
                        if (customer == null)
                        {
                            ModelState.AddModelError(string.Empty, "Müşteri kaydı oluşturulamadı. Lütfen tekrar deneyin.");
                            // Kullanıcıyı sil
                            await _userManager.DeleteAsync(user);
                            return Page();
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "Müşteri kaydı oluşturulurken bir hata oluştu: " + ex.Message);
                        // Kullanıcıyı sil
                        await _userManager.DeleteAsync(user);
                        return Page();
                    }
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Car");
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordTooShort")
                        ModelState.AddModelError("Input.Password", "Şifreniz en az 6 karakter olmalıdır.");
                    else if (error.Code == "PasswordRequiresNonAlphanumeric")
                        ModelState.AddModelError("Input.Password", "Şifreniz en az bir özel karakter içermelidir.");
                    else if (error.Code == "PasswordRequiresDigit")
                        ModelState.AddModelError("Input.Password", "Şifreniz en az bir rakam içermelidir.");
                    else if (error.Code == "PasswordRequiresLower")
                        ModelState.AddModelError("Input.Password", "Şifreniz en az bir küçük harf içermelidir.");
                    else if (error.Code == "PasswordRequiresUpper")
                        ModelState.AddModelError("Input.Password", "Şifreniz en az bir büyük harf içermelidir.");
                    else
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
