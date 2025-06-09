using DemoExternalSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoExternalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private const string API_KEY = "tCPAGETDKzjLAIX1hUzF5v2YJuUkrj8f";
        private const string EXTERNAL_SYSTEM_ID = "0ff65d6d-1603-4137-bfd2-94316792ff2a";
        private const string DPMS_BASE_URL = "https://localhost:7226";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        public AccountController(ILogger<AccountController> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            _logger = logger;
        }

        // Danh sách user cứng: key = username, value = password
        private static readonly IDictionary<string, string> _users =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "admin@gmail.com", "1672003Dts@" },
            { "hung03022003@gmail.com",  "1672003Dts@" }
            // ... thêm user tuỳ ý
        };

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra in-memory
            if (_users.TryGetValue(model.Username, out var pwd) && pwd == model.Password)
            {
                // Call API get UString 
                var linkUrl = $"{DPMS_BASE_URL}/api-consent/get-ustring/{model.Username}";
                // Add API key
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", API_KEY);
                // Get Response
                var linkResponse = await _httpClient.GetAsync(linkUrl);
                // To uString
                var uString = await linkResponse.Content.ReadAsStringAsync();
                _logger.LogInformation(uString);

                // Tạo identity và principal
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim("uString",           uString),
                    new Claim("systemId",          EXTERNAL_SYSTEM_ID ?? "")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Sign in
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    });

               



                // Điều hướng sau login
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Sai username hoặc password");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
