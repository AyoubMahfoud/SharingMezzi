using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;
using System.Linq;

namespace SharingMezzi.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Skip auth check for testing
            Console.WriteLine("Login page loaded");
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                ViewData["ReturnUrl"] = ReturnUrl;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"=== LOGIN PAGE POST ===");
            Console.WriteLine($"Request Method: {Request.Method}");
            Console.WriteLine($"Content Type: {Request.ContentType}");
            Console.WriteLine($"Form Keys: {string.Join(", ", Request.Form.Keys)}");
            Console.WriteLine($"Email: {LoginRequest?.Email}");
            Console.WriteLine($"Password Length: {LoginRequest?.Password?.Length ?? 0}");
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"   {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            // Validation
            if (LoginRequest?.Email == null || LoginRequest?.Password == null)
            {
                Console.WriteLine("❌ Missing credentials");
                ErrorMessage = "Email e password sono richiesti";
                return Page();
            }
            
            try
            {
                Console.WriteLine("🔄 Attempting login with AuthService...");
                
                // Use the real AuthService
                var authResponse = await _authService.LoginAsync(LoginRequest);
                
                if (authResponse?.Success == true)
                {
                    Console.WriteLine("✅ Login successful");
                    var returnUrl = ReturnUrl;
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        Console.WriteLine($"Redirecting to ReturnUrl: {returnUrl}");
                        return LocalRedirect(returnUrl);
                    }
                    Console.WriteLine("Redirecting to Dashboard");
                    return RedirectToPage("/Dashboard");
                }
                else
                {
                    Console.WriteLine($"❌ Login failed: {authResponse?.Message}");
                    ErrorMessage = authResponse?.Message ?? "Login fallito. Verifica le credenziali.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during login: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error during login");
                ErrorMessage = "Errore durante il login. Riprova più tardi.";
                return Page();
            }
        }
    }
}
