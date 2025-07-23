using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

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
            // Check if user is already logged in
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    return RedirectToPage("/Index");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _authService.LoginAsync(LoginRequest);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("User {Email} logged in successfully", LoginRequest.Email);
                    return RedirectToPage("/Index");
                }
                
                ErrorMessage = response?.Message ?? "Credenziali non valide";
                ModelState.AddModelError("", ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", LoginRequest.Email);
                ErrorMessage = "Si è verificato un errore durante il login. Riprova più tardi.";
                ModelState.AddModelError("", ErrorMessage);
            }

            return Page();
        }
    }
}
