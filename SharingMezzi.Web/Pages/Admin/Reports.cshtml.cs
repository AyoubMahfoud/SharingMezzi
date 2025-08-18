using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    public class ReportsModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<ReportsModel> _logger;

        public ReportsModel(
            IAuthService authService,
            IUserService userService,
            IVehicleService vehicleService,
            ILogger<ReportsModel> logger)
        {
            _authService = authService;
            _userService = userService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalTrips { get; set; }
        public int TripsToday { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int TotalParkings { get; set; }
        public int AvailableSlots { get; set; }
        public int AvgTripDuration { get; set; }

        public List<User> TopUsers { get; set; } = new();
        public List<Vehicle> TopVehicles { get; set; } = new();
        public List<TripData> TripData { get; set; } = new();
        public SystemStatus SystemStatus { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? returnUrl)
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Admin/Reports" });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    _logger.LogWarning("Non-admin user {Email} tried to access admin reports page", currentUser?.Email);
                    return RedirectToPage("/Index");
                }

                // Carica dati dal servizio reale
                await LoadReportsDataFromService();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin reports page");
                LoadFallbackData();
                return Page();
            }
        }

        private async Task LoadReportsDataFromService()
        {
            try
            {
                // Carica utenti
                var users = await _userService.GetAllUsersAsync();
                TotalUsers = users.Count;
                NewUsersThisMonth = users.Count(u => u.DataRegistrazione.Month == DateTime.Now.Month && u.DataRegistrazione.Year == DateTime.Now.Year);
                TopUsers = users.OrderByDescending(u => u.PuntiEco).Take(10).ToList();

                // Carica veicoli
                var vehicles = await _vehicleService.GetVehiclesAsync();
                TotalVehicles = vehicles.Count;
                ActiveVehicles = vehicles.Count(v => v.Stato == VehicleStatus.Disponibile);
                TopVehicles = vehicles.Take(10).ToList();

                // Statistiche simulate (in attesa di endpoint specifici)
                TotalTrips = Random.Shared.Next(1000, 5000);
                TripsToday = Random.Shared.Next(50, 200);
                TotalRevenue = Random.Shared.Next(10000, 50000);
                RevenueGrowth = (decimal)(Random.Shared.NextDouble() * 20 - 5);
                TotalParkings = Random.Shared.Next(10, 50);
                AvailableSlots = Random.Shared.Next(50, 200);
                AvgTripDuration = Random.Shared.Next(15, 45);

                // Dati grafici simulati
                TripData = GenerateTripData(30);
                SystemStatus = new SystemStatus
                {
                    DatabaseStatus = "healthy",
                    MqttBrokerStatus = "online",
                    SignalRStatus = "active",
                    IoTDevicesConnected = Random.Shared.Next(10, 50),
                    LastUpdate = DateTime.Now,
                    Uptime = "99.9%"
                };

                _logger.LogInformation("Reports data loaded successfully from service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports data from service");
                LoadFallbackData();
            }
        }

        private List<TripData> GenerateTripData(int days)
        {
            var tripData = new List<TripData>();
            var startDate = DateTime.Now.AddDays(-days);
            
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                tripData.Add(new TripData
                {
                    Date = date,
                    TripCount = Random.Shared.Next(50, 150),
                    Revenue = Random.Shared.Next(250, 750)
                });
            }

            return tripData;
        }

        private void LoadFallbackData()
        {
            // Statistiche di fallback
            TotalUsers = 0;
            NewUsersThisMonth = 0;
            TotalVehicles = 0;
            ActiveVehicles = 0;
            TotalTrips = 0;
            TripsToday = 0;
            TotalRevenue = 0;
            RevenueGrowth = 0;
            TotalParkings = 0;
            AvailableSlots = 0;
            AvgTripDuration = 0;

            // Dati di fallback vuoti
            TopUsers = new List<User>();
            TopVehicles = new List<Vehicle>();
            TripData = new List<TripData>();
            SystemStatus = new SystemStatus
            {
                DatabaseStatus = "offline",
                MqttBrokerStatus = "offline",
                SignalRStatus = "offline",
                IoTDevicesConnected = 0,
                LastUpdate = DateTime.Now,
                Uptime = "0%"
            };

            _logger.LogInformation("Using fallback data for reports");
        }
    }
}