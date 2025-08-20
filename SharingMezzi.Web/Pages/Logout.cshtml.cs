using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using Microsoft.AspNetCore.Http;

namespace SharingMezzi.Web.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Logout dal servizio di autenticazione
                _authService.LogoutAsync();
                
                // Pulisci la sessione
                HttpContext.Session.Clear();
                
                // Rimuovi tutti i cookie di autenticazione
                Response.Cookies.Delete("PersistentToken");
                Response.Cookies.Delete("PersistentUser");
                
                // Aggiungi header per prevenire il caching
                Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Append("Pragma", "no-cache");
                Response.Headers.Append("Expires", "0");
                
                // Reindirizza alla home page
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                // In caso di errore, reindirizza comunque
                Console.WriteLine($"Errore durante il logout: {ex.Message}");
                return RedirectToPage("/Index");
            }
        }
    }
}
