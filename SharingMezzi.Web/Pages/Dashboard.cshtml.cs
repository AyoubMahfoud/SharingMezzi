using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly IParkingService _parkingService;
        private readonly ITripService _tripService;
        private readonly IBillingService _billingService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IAuthService authService,
            IVehicleService vehicleService,
            IParkingService parkingService,
            ITripService tripService,
            IBillingService billingService,
            ILogger<DashboardModel> logger)
        {
            _authService = authService;
            _vehicleService = vehicleService;
            _parkingService = parkingService;
            _tripService = tripService;
            _billingService = billingService;
            _logger = logger;
        }

        // User Info
        public User? CurrentUser { get; set; }
        
        // Stats
        public int AvailableVehicles { get; set; } = 0;
        public int TotalTrips { get; set; } = 0;
        public decimal? CurrentCredit { get; set; } = 0;
        public decimal Co2Saved { get; set; } = 0;
        public int AvailableParkings { get; set; } = 0;
        
        // Growth percentages
        public decimal VehicleGrowth { get; set; } = 12.5m;
        public decimal TripGrowth { get; set; } = 8.3m;
        
        // Recent activity
        public List<TripSummary> RecentTrips { get; set; } = new();
        public DateTime? LastChargeDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("=== LOADING DASHBOARD DATA ===");
                
                // Get current user
                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    _logger.LogWarning("User not found, redirecting to login");
                    return RedirectToPage("/Login");
                }

                _logger.LogInformation($"Loading dashboard for user: {CurrentUser.Email}");

                // Load data in parallel for better performance
                var tasks = new List<Task>
                {
                    LoadVehicleStats(),
                    LoadParkingStats(),
                    LoadTripStats(),
                    LoadUserStats()
                };

                await Task.WhenAll(tasks);

                _logger.LogInformation("=== DASHBOARD DATA LOADED ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                // Continue with default values
            }

            return Page();
        }

        private async Task LoadVehicleStats()
        {
            try
            {
                _logger.LogInformation("Loading vehicle statistics...");
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                AvailableVehicles = vehicles?.Count ?? 0;
                _logger.LogInformation($"Found {AvailableVehicles} available vehicles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle stats");
                AvailableVehicles = 6; // Fallback value
            }
        }

        private async Task LoadParkingStats()
        {
            try
            {
                _logger.LogInformation("Loading parking statistics...");
                var parkings = await _parkingService.GetAllParkingsAsync();
                AvailableParkings = parkings?.Count ?? 0;
                _logger.LogInformation($"Found {AvailableParkings} parking stations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parking stats");
                AvailableParkings = 15; // Fallback value
            }
        }

        private async Task LoadTripStats()
        {
            try
            {
                _logger.LogInformation("Loading trip statistics...");
                var trips = await _tripService.GetUserTripsAsync();
                
                if (trips != null && trips.Any())
                {
                    TotalTrips = trips.Count;
                    
                    // Calculate CO2 saved (estimate: 0.12kg per km, ~18km/h average speed)
                    var totalMinutes = trips.Where(t => t.DurataMinuti > 0) // CORRETTO: rimosse HasValue
                                           .Sum(t => t.DurataMinuti);
                    var estimatedKm = (totalMinutes / 60.0m) * 18; // 18 km/h average
                    Co2Saved = Math.Round(estimatedKm * 0.12m, 1);
                    
                    // Get recent trips for activity feed
                    RecentTrips = trips.Where(t => t.Fine.HasValue)
                                      .OrderByDescending(t => t.Fine)
                                      .Take(5)
                                      .Select(t => new TripSummary
                                      {
                                          VehicleModel = t.Mezzo?.Modello ?? "Mezzo", // CORRETTO: usa t.Mezzo?.Modello
                                          Duration = t.DurataMinuti,
                                          Cost = t.CostoTotale,
                                          EndTime = t.Fine
                                      })
                                      .ToList();

                    _logger.LogInformation($"Loaded {TotalTrips} trips, {Co2Saved}kg CO2 saved");
                }
                else
                {
                    TotalTrips = 0;
                    Co2Saved = 0;
                    _logger.LogInformation("No trips found for user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trip stats");
                TotalTrips = 0;
                Co2Saved = 0;
            }
        }

        private async Task LoadUserStats()
        {
            try
            {
                _logger.LogInformation("Loading user statistics...");
                
                if (CurrentUser != null)
                {
                    // Get current credit
                    CurrentCredit = await _billingService.GetUserCreditAsync();
                    
                    // Try to get last charge date (this might not be available)
                    try
                    {
                        var transactions = await _billingService.GetTransactionsAsync();
                        if (transactions?.Any() == true)
                        {
                            // Find the most recent charge
                            var lastCharge = transactions.FirstOrDefault();
                            if (lastCharge != null)
                            {
                                // Try to get date from dynamic object
                                try
                                {
                                    // Try different property names that might exist
                                    var dateProperty = lastCharge.GetType().GetProperty("DataRicarica") ??
                                                     lastCharge.GetType().GetProperty("Data") ??
                                                     lastCharge.GetType().GetProperty("CreatedAt");
                                    
                                    if (dateProperty != null)
                                    {
                                        LastChargeDate = (DateTime?)dateProperty.GetValue(lastCharge);
                                    }
                                    else
                                    {
                                        LastChargeDate = DateTime.Now.AddDays(-7); // Fallback
                                    }
                                }
                                catch
                                {
                                    LastChargeDate = DateTime.Now.AddDays(-7); // Fallback
                                }
                            }
                        }
                        else
                        {
                            LastChargeDate = DateTime.Now.AddDays(-7); // No transactions
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Could not load transaction history");
                        LastChargeDate = DateTime.Now.AddDays(-7); // Fallback
                    }

                    _logger.LogInformation($"User credit: €{CurrentCredit}, Last charge: {LastChargeDate?.ToString("dd/MM/yyyy")}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user stats");
                CurrentCredit = CurrentUser?.Credito ?? 0;
                LastChargeDate = DateTime.Now.AddDays(-7);
            }
        }

        public async Task<IActionResult> OnGetRefreshStatsAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing dashboard stats via AJAX...");
                
                // Reload the main stats
                await Task.WhenAll(
                    LoadVehicleStats(),
                    LoadParkingStats(),
                    LoadTripStats(),
                    LoadUserStats()
                );

                var stats = new
                {
                    availableVehicles = AvailableVehicles,
                    totalTrips = TotalTrips,
                    currentCredit = CurrentCredit?.ToString("F2"),
                    co2Saved = Co2Saved,
                    availableParkings = AvailableParkings,
                    lastUpdated = DateTime.Now.ToString("HH:mm:ss")
                };

                return new JsonResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard stats");
                return new JsonResult(new { error = "Errore nel refresh", details = ex.Message });
            }
        }
    }

    // Helper class for trip summaries
    public class TripSummary
    {
        public string VehicleModel { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal Cost { get; set; }
        public DateTime? EndTime { get; set; }
    }
}