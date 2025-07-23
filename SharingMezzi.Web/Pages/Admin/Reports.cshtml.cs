using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReportsModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly IBillingService _billingService;
        private readonly IParkingService _parkingService;
        private readonly ILogger<ReportsModel> _logger;

        public ReportsModel(
            IUserService userService,
            IVehicleService vehicleService,
            IBillingService billingService,
            IParkingService parkingService,
            ILogger<ReportsModel> logger)
        {
            _userService = userService;
            _vehicleService = vehicleService;
            _billingService = billingService;
            _parkingService = parkingService;
            _logger = logger;
        }

        // Statistics Properties
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

        // Data Collections
        public List<User> TopUsers { get; set; } = new();
        public List<Vehicle> TopVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading admin reports page");

                // Load basic data
                await LoadStatistics();
                await LoadTopUsers();
                await LoadTopVehicles();

                _logger.LogInformation("Admin reports page loaded successfully");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin reports");
                TempData["Error"] = "Errore nel caricamento dei report. Riprova più tardi.";
                return Page();
            }
        }

        private async Task LoadStatistics()
        {
            try
            {
                // Load users
                var users = await _userService.GetAllUsersAsync();
                TotalUsers = users.Count;
                NewUsersThisMonth = users.Count(u => u.CreatedAt.Month == DateTime.Now.Month);

                // Load vehicles
                var vehicles = await _vehicleService.GetVehiclesAsync();
                TotalVehicles = vehicles.Count;
                ActiveVehicles = vehicles.Count(v => v.Stato == VehicleStatus.Disponibile);

                // Load trips
                var trips = await _billingService.GetTripsAsync();
                TotalTrips = trips.Count;
                TripsToday = trips.Count(t => t.DataInizio.Date == DateTime.Today);

                // Load recharges for revenue calculation
                var recharges = await _billingService.GetRechargesAsync();
                TotalRevenue = recharges.Sum(r => r.Importo);
                RevenueGrowth = CalculateRevenueGrowth(recharges);

                // Load parkings
                var parkings = await _parkingService.GetParkingsAsync();
                TotalParkings = parkings.Count;
                AvailableSlots = parkings.Sum(p => p.PostiLiberi);

                // Calculate average trip duration
                var completedTrips = trips.Where(t => t.Stato == TripStatus.Completata && t.DataFine.HasValue);
                if (completedTrips.Any())
                {
                    var avgDuration = completedTrips.Average(t => (t.DataFine!.Value - t.DataInizio).TotalMinutes);
                    AvgTripDuration = (int)Math.Round(avgDuration);
                }
                else
                {
                    AvgTripDuration = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics");
                // Set default values
                TotalUsers = 0;
                TotalVehicles = 0;
                TotalTrips = 0;
                TotalRevenue = 0;
                RevenueGrowth = 0;
            }
        }

        private decimal CalculateRevenueGrowth(List<Recharge> recharges)
        {
            try
            {
                var thisMonth = recharges.Where(r => r.DataRicarica.Month == DateTime.Now.Month).Sum(r => r.Importo);
                var lastMonth = recharges.Where(r => r.DataRicarica.Month == DateTime.Now.AddMonths(-1).Month).Sum(r => r.Importo);
                
                if (lastMonth == 0) return 0;
                
                return Math.Round(((thisMonth - lastMonth) / lastMonth) * 100, 1);
            }
            catch
            {
                return 0;
            }
        }

        private async Task LoadTopUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                TopUsers = users.OrderByDescending(u => u.TotalTrips).Take(10).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading top users");
                TopUsers = new List<User>();
            }
        }

        private async Task LoadTopVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetVehiclesAsync();
                TopVehicles = vehicles.OrderByDescending(v => new Random().Next(1, 100)).Take(10).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading top vehicles");
                TopVehicles = new List<Vehicle>();
            }
        }
    }
}
