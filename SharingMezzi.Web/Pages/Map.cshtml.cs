using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SharingMezzi.Web.Pages
{
    public class MapModel : PageModel
    {
        private readonly ILogger<MapModel>? _logger;

        public MapModel(ILogger<MapModel>? logger = null)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger?.LogInformation("Map page loaded");
        }
    }
}
