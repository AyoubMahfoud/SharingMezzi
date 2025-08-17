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
            _logger.LogInformation("🚀 DEBUG: Register page GET request");
            
            // Check if user is already logged in
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    _logger.LogInformation("👤 DEBUG: User already logged in, redirecting to dashboard");
                    return RedirectToPage("/Dashboard");
                }
            }

            _logger.LogInformation("📄 DEBUG: Showing register page");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("📤 DEBUG: Register POST request received");
            _logger.LogInformation($"🔍 DEBUG: Request Method: {Request.Method}");
            _logger.LogInformation($"🔍 DEBUG: Content Type: {Request.ContentType}");
            _logger.LogInformation($"🔍 DEBUG: Form Keys: {string.Join(", ", Request.Form.Keys)}");
            
            // Log form data (without passwords)
            _logger.LogInformation($"📋 DEBUG: RegisterRequest state:");
            _logger.LogInformation($"   Nome: '{RegisterRequest?.Nome ?? "NULL"}'");
            _logger.LogInformation($"   Cognome: '{RegisterRequest?.Cognome ?? "NULL"}'");
            _logger.LogInformation($"   Email: '{RegisterRequest?.Email ?? "NULL"}'");
            _logger.LogInformation($"   Telefono: '{RegisterRequest?.Telefono ?? "NULL"}'");
            _logger.LogInformation($"   Password Length: {RegisterRequest?.Password?.Length ?? 0}");
            _logger.LogInformation($"   ConfirmPassword Length: {RegisterRequest?.ConfirmPassword?.Length ?? 0}");
            _logger.LogInformation($"   AcceptTerms: {RegisterRequest?.AcceptTerms ?? false}");
            
            // Check ModelState
            _logger.LogInformation($"✅ DEBUG: ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ DEBUG: ModelState validation errors:");
                foreach (var error in ModelState)
                {
                    foreach (var errorMsg in error.Value.Errors)
                    {
                        _logger.LogWarning($"   {error.Key}: {errorMsg.ErrorMessage}");
                    }
                }
                return Page();
            }

            // Validate required fields manually
            if (string.IsNullOrWhiteSpace(RegisterRequest?.Nome))
            {
                _logger.LogWarning("❌ DEBUG: Nome is required but empty");
                ErrorMessage = "Il nome è obbligatorio";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(RegisterRequest?.Cognome))
            {
                _logger.LogWarning("❌ DEBUG: Cognome is required but empty");
                ErrorMessage = "Il cognome è obbligatorio";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(RegisterRequest?.Email))
            {
                _logger.LogWarning("❌ DEBUG: Email is required but empty");
                ErrorMessage = "L'email è obbligatoria";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(RegisterRequest?.Password))
            {
                _logger.LogWarning("❌ DEBUG: Password is required but empty");
                ErrorMessage = "La password è obbligatoria";
                return Page();
            }

            if (RegisterRequest.Password != RegisterRequest?.ConfirmPassword)
            {
                _logger.LogWarning("❌ DEBUG: Passwords do not match");
                ErrorMessage = "Le password non corrispondono";
                return Page();
            }

            if (!RegisterRequest.AcceptTerms)
            {
                _logger.LogWarning("❌ DEBUG: Terms not accepted");
                ErrorMessage = "Devi accettare i termini di servizio";
                return Page();
            }

            try
            {
                _logger.LogInformation("🔄 DEBUG: Calling AuthService.RegisterAsync...");
                var success = await _authService.RegisterAsync(RegisterRequest);
                _logger.LogInformation($"📊 DEBUG: Registration result: {success}");
                
                if (success)
                {
                    _logger.LogInformation($"✅ DEBUG: User {RegisterRequest.Email} registered successfully");
                    SuccessMessage = "Registrazione completata con successo! Ora puoi effettuare il login.";
                    
                    // Clear form
                    RegisterRequest = new();
                    
                    // Could redirect to login page after a delay
                    // return RedirectToPage("/Login");
                }
                else
                {
                    _logger.LogWarning($"❌ DEBUG: Registration failed for {RegisterRequest.Email}");
                    ErrorMessage = "Errore durante la registrazione. L'email potrebbe essere già in uso.";
                    ModelState.AddModelError("", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 DEBUG: Exception during registration for user {RegisterRequest?.Email}");
                ErrorMessage = "Si è verificato un errore durante la registrazione. Riprova più tardi.";
                ModelState.AddModelError("", ErrorMessage);
            }

            _logger.LogInformation("📄 DEBUG: Returning to register page");
            return Page();
        }
    }
}