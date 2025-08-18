using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SharingMezzi.Web.Services;

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
                // Logout usando il nostro servizio di autenticazione personalizzato
                _authService.LogoutAsync();
                
                // Rimuovi tutti i dati di sessione
                HttpContext.Session.Clear();
                
                // Reindirizza alla homepage
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                // In caso di errore, reindirizza comunque alla homepage
                return RedirectToPage("/Index");
            }
        }
    }
}
