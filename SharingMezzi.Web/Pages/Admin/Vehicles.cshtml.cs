using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SharingMezzi.Web.Pages.Admin
{
    [Authorize(Roles = "Admin,Amministratore")]
    public class VehiclesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
