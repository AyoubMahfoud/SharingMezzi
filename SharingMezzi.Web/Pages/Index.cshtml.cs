using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly IBillingService _billingService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IAuthService authService,
            IVehicleService vehicleService,
            IBillingService billingService,
            ILogger<IndexModel> logger)
        {
            _authService = authService;
            _vehicleService = vehicleService;
            _billingService = billingService;
            _logger = logger;
        }

        public User? CurrentUser { get; set; }
        public int AvailableVehicles { get; set; }
        public int TotalTrips { get; set; }
        public decimal CurrentBalance { get; set; }
        public int EcoPoints { get; set; }
        public List<Trip> RecentTrips { get; set; } = new();

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
                // Get current user
                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    return RedirectToPage("/Login");
                }

                // Load dashboard data
                await LoadDashboardData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data for user");
                // Show error message but don't redirect
            }

            return Page();
        }

        private async Task LoadDashboardData()
        {
            if (CurrentUser == null) return;

            try
            {
                // Load available vehicles
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                AvailableVehicles = vehicles.Count;

                // Load user trips
                var userTrips = await _billingService.GetUserTripsAsync(CurrentUser.Id);
                TotalTrips = userTrips.Count;
                RecentTrips = userTrips.OrderByDescending(t => t.Inizio).Take(5).ToList();

                // Load user balance
                CurrentBalance = await _billingService.GetUserBalanceAsync(CurrentUser.Id);

                // Set eco points from user
                EcoPoints = CurrentUser.PuntiEco;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data for user {UserId}", CurrentUser.Id);
                
                // Set default values
                AvailableVehicles = 0;
                TotalTrips = 0;
                CurrentBalance = 0;
                EcoPoints = 0;
            }
        }
    }
}
