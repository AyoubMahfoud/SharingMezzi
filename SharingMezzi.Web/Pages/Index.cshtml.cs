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
            Console.WriteLine("=== INDEX PAGE LOADED ===");
            
            try
            {
                // Get current user
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    CurrentUser = currentUser;
                    Console.WriteLine($"Utente corrente: {currentUser.Email}");
                }
                else
                {
                    Console.WriteLine("Nessun utente corrente trovato");
                }

                // Set default values
                AvailableVehicles = 25;
                TotalTrips = 42;
                CurrentBalance = 15.50m;
                EcoPoints = 120;
                RecentTrips = new List<Trip>();

                Console.WriteLine("Index page caricata con successo");
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore caricamento Index: {ex.Message}");
                _logger.LogError(ex, "Error loading dashboard data");
                
                // Set default values anche in caso di errore
                AvailableVehicles = 0;
                TotalTrips = 0;
                CurrentBalance = 0;
                EcoPoints = 0;
                RecentTrips = new List<Trip>();
                
                return Page();
            }
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
