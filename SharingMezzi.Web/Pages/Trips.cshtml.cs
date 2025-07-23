using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class TripsModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IBillingService _billingService;
        private readonly ILogger<TripsModel> _logger;

        public TripsModel(
            IAuthService authService,
            IBillingService billingService,
            ILogger<TripsModel> logger)
        {
            _authService = authService;
            _billingService = billingService;
            _logger = logger;
        }

        public User? CurrentUser { get; set; }
        public List<Trip> Trips { get; set; } = new();
        public int TotalMinutes { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalEcoPoints { get; set; }
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
                // Get current user
                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    return RedirectToPage("/Login");
                }

                // Load trips data
                await LoadTripsData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trips data for user {UserId}", CurrentUser?.Id);
                ErrorMessage = "Errore nel caricamento delle corse. Riprova più tardi.";
            }

            return Page();
        }

        private async Task LoadTripsData()
        {
            if (CurrentUser == null) return;

            try
            {
                // Load user trips
                Trips = await _billingService.GetUserTripsAsync(CurrentUser.Id);
                Trips = Trips.OrderByDescending(t => t.Inizio).ToList();

                // Calculate totals
                TotalMinutes = Trips.Sum(t => t.DurataMinuti);
                TotalCost = Trips.Sum(t => t.CostoTotale);
                TotalEcoPoints = Trips.Where(t => t.PuntiEcoAssegnati.HasValue).Sum(t => t.PuntiEcoAssegnati!.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trips data for user {UserId}", CurrentUser.Id);
                
                // Set default values
                Trips = new List<Trip>();
                TotalMinutes = 0;
                TotalCost = 0;
                TotalEcoPoints = 0;
            }
        }
    }
}
