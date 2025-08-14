using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly IBillingService _billingService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IAuthService authService,
            IVehicleService vehicleService,
            IBillingService billingService,
            ILogger<DashboardModel> logger)
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
            Console.WriteLine("=== DASHBOARD PAGE LOADED ===");
            Console.WriteLine($"Request URL: {Request.Path}");
            Console.WriteLine($"Request Method: {Request.Method}");
            
            try
            {
                // Get current user from auth service
                CurrentUser = await _authService.GetCurrentUserAsync();
                
                if (CurrentUser == null)
                {
                    Console.WriteLine("❌ No authenticated user found, redirecting to login");
                    return RedirectToPage("/Login");
                }
                
                Console.WriteLine($"✅ Authenticated user: {CurrentUser.Nome} {CurrentUser.Cognome} ({CurrentUser.Email})");
                
                // Load dashboard data from API services
                await LoadDashboardData();
                
                Console.WriteLine("✅ Dashboard loaded successfully");
                Console.WriteLine($"- Available Vehicles: {AvailableVehicles}");
                Console.WriteLine($"- Total Trips: {TotalTrips}");
                Console.WriteLine($"- Current Balance: €{CurrentBalance}");
                Console.WriteLine($"- Eco Points: {EcoPoints}");
                Console.WriteLine("=== DASHBOARD READY ===");
                
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading Dashboard: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error loading dashboard data");
                
                // Set fallback data in case of error
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
            if (CurrentUser == null) 
            {
                Console.WriteLine("❌ CurrentUser is null, cannot load dashboard data");
                return;
            }

            Console.WriteLine($"🔄 Loading dashboard data for user {CurrentUser.Id}...");

            try
            {
                // Load available vehicles
                Console.WriteLine("📋 Loading available vehicles...");
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                AvailableVehicles = vehicles?.Count ?? 0;
                Console.WriteLine($"✅ Found {AvailableVehicles} available vehicles");

                // Load user trips
                Console.WriteLine("🚗 Loading user trips...");
                var userTrips = await _billingService.GetUserTripsAsync(CurrentUser.Id);
                TotalTrips = userTrips?.Count ?? 0;
                RecentTrips = userTrips?.OrderByDescending(t => t.Inizio).Take(5).ToList() ?? new List<Trip>();
                Console.WriteLine($"✅ Found {TotalTrips} total trips, {RecentTrips.Count} recent");

                // Load user balance
                Console.WriteLine("💰 Loading user balance...");
                CurrentBalance = await _billingService.GetUserBalanceAsync(CurrentUser.Id);
                Console.WriteLine($"✅ Current balance: €{CurrentBalance}");

                // Set eco points from user
                EcoPoints = CurrentUser.PuntiEco;
                Console.WriteLine($"🌱 Eco points: {EcoPoints}");

                Console.WriteLine("✅ Dashboard data loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading dashboard data: {ex.Message}");
                _logger.LogError(ex, "Error loading dashboard data for user {UserId}", CurrentUser.Id);
                
                // Set fallback values on error
                AvailableVehicles = 0;
                TotalTrips = 0;
                CurrentBalance = 0.00m;
                EcoPoints = CurrentUser?.PuntiEco ?? 0;
                RecentTrips = new List<Trip>();
                
                Console.WriteLine("⚠️ Using fallback values due to error");
            }
        }
    }
}
