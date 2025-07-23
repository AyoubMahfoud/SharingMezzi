using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterRequest RegisterRequest { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

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
                var success = await _authService.RegisterAsync(RegisterRequest);
                
                if (success)
                {
                    _logger.LogInformation("User {Email} registered successfully", RegisterRequest.Email);
                    SuccessMessage = "Registrazione completata con successo! Ora puoi effettuare il login.";
                    
                    // Clear form
                    RegisterRequest = new();
                    
                    // Could redirect to login page after a delay
                    // return RedirectToPage("/Login");
                }
                else
                {
                    ErrorMessage = "Errore durante la registrazione. L'email potrebbe essere già in uso.";
                    ModelState.AddModelError("", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", RegisterRequest.Email);
                ErrorMessage = "Si è verificato un errore durante la registrazione. Riprova più tardi.";
                ModelState.AddModelError("", ErrorMessage);
            }

            return Page();
        }
    }
}
