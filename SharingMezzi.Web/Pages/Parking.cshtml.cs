using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class ParkingModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IParkingService _parkingService;
        private readonly ILogger<ParkingModel> _logger;

        public ParkingModel(
            IAuthService authService,
            IParkingService parkingService,
            ILogger<ParkingModel> logger)
        {
            _authService = authService;
            _parkingService = parkingService;
            _logger = logger;
        }

        public List<Parking> Parkings { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Get current user to ensure they're authenticated
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToPage("/Login");
                }

                // Load parkings
                Parkings = await _parkingService.GetParkingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parkings");
                ErrorMessage = "Errore nel caricamento dei parcheggi. Riprova più tardi.";
                Parkings = new List<Parking>();
            }

            return Page();
        }
    }
}
