using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class VehiclesModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesModel> _logger;

        public VehiclesModel(
            IAuthService authService,
            IVehicleService vehicleService,
            ILogger<VehiclesModel> logger)
        {
            _authService = authService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public List<Vehicle> Vehicles { get; set; } = new();
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

                // Load available vehicles
                Vehicles = await _vehicleService.GetAvailableVehiclesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicles");
                ErrorMessage = "Errore nel caricamento dei mezzi. Riprova più tardi.";
                Vehicles = new List<Vehicle>();
            }

            return Page();
        }
    }
}
