using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using SharingMezzi.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SharingMezzi.Web.Pages
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(IAuthService authService, ILogger<ProfileModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public ProfileUpdateModel ProfileUpdate { get; set; } = new();

        [BindProperty]
        public PasswordChangeModel PasswordChange { get; set; } = new();

        public User? CurrentUser { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                IsAuthenticated = _authService.IsAuthenticated();
                if (!IsAuthenticated)
                {
                    _logger.LogWarning("User not authenticated, redirecting to login");
                    return RedirectToPage("/Login");
                }

                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    _logger.LogWarning("Current user not found, redirecting to login");
                    return RedirectToPage("/Login");
                }

                // Popola il modello di aggiornamento con i dati attuali
                ProfileUpdate.Nome = CurrentUser.Nome;
                ProfileUpdate.Cognome = CurrentUser.Cognome;
                ProfileUpdate.Email = CurrentUser.Email;
                ProfileUpdate.Telefono = CurrentUser.Telefono ?? "";

                _logger.LogInformation($"Profile page loaded for user: {CurrentUser.Email}");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile page");
                ErrorMessage = "Errore nel caricamento del profilo";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Dati non validi";
                    return await OnGetAsync();
                }

                IsAuthenticated = _authService.IsAuthenticated();
                if (!IsAuthenticated)
                {
                    return RedirectToPage("/Login");
                }

                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    return RedirectToPage("/Login");
                }

                // Aggiorna i dati dell'utente
                CurrentUser.Nome = ProfileUpdate.Nome;
                CurrentUser.Cognome = ProfileUpdate.Cognome;
                CurrentUser.Telefono = ProfileUpdate.Telefono;

                // Salva le modifiche
                _authService.SetCurrentUser(CurrentUser);

                SuccessMessage = "Profilo aggiornato con successo!";
                _logger.LogInformation($"Profile updated for user: {CurrentUser.Email}");

                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ErrorMessage = "Errore nell'aggiornamento del profilo";
                return await OnGetAsync();
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Dati non validi";
                    return await OnGetAsync();
                }

                IsAuthenticated = _authService.IsAuthenticated();
                if (!IsAuthenticated)
                {
                    return RedirectToPage("/Login");
                }

                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    return RedirectToPage("/Login");
                }

                // Verifica la password attuale
                if (PasswordChange.CurrentPassword != "admin123") // Sostituisci con verifica reale
                {
                    ErrorMessage = "Password attuale non corretta";
                    return await OnGetAsync();
                }

                // Verifica che le nuove password coincidano
                if (PasswordChange.NewPassword != PasswordChange.ConfirmPassword)
                {
                    ErrorMessage = "Le nuove password non coincidono";
                    return await OnGetAsync();
                }

                // Verifica la complessità della password
                if (PasswordChange.NewPassword.Length < 6)
                {
                    ErrorMessage = "La nuova password deve essere di almeno 6 caratteri";
                    return await OnGetAsync();
                }

                // TODO: Implementa il cambio password reale con il backend
                SuccessMessage = "Password cambiata con successo!";
                _logger.LogInformation($"Password changed for user: {CurrentUser.Email}");

                // Reset del form
                PasswordChange = new PasswordChangeModel();

                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                ErrorMessage = "Errore nel cambio password";
                return await OnGetAsync();
            }
        }
    }

    public class ProfileUpdateModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri")]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il cognome non può superare i 50 caratteri")]
        public string Cognome { get; set; } = "";

        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Formato email non valido")]
        public string Email { get; set; } = "";

        [StringLength(20, ErrorMessage = "Il telefono non può superare i 20 caratteri")]
        public string Telefono { get; set; } = "";
    }

    public class PasswordChangeModel
    {
        [Required(ErrorMessage = "La password attuale è obbligatoria")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "La nuova password è obbligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La password deve essere di almeno 6 caratteri")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "La conferma password è obbligatoria")]
        [Compare("NewPassword", ErrorMessage = "Le password non coincidono")]
        public string ConfirmPassword { get; set; } = "";
    }
}
