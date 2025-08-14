using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SharingMezzi.Web.Pages
{
    public class SimpleLoginModel : PageModel
    {
        public void OnGet()
        {
            // Pagina di login semplificata per test
        }

        public IActionResult OnPost(string email, string password)
        {
            Console.WriteLine($"=== SIMPLE LOGIN TEST ===");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Password: {password}");
            
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                Console.WriteLine("✅ Credenziali ricevute, reindirizzo alla Dashboard");
                return RedirectToPage("/Dashboard");
            }
            
            Console.WriteLine("❌ Credenziali mancanti");
            return Page();
        }
    }
}
