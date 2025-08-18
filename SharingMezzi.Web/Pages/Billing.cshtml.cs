using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    public class BillingModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IBillingService _billingService;
        private readonly ILogger<BillingModel> _logger;

        public BillingModel(
            IAuthService authService,
            IBillingService billingService,
            ILogger<BillingModel> logger)
        {
            _authService = authService;
            _billingService = billingService;
            _logger = logger;
        }

        public User? CurrentUser { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal MinimumCredit { get; set; }
        public int EcoPoints { get; set; }
        public List<Recharge> Recharges { get; set; } = new();
        public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl)
        {
            // Check if user is authenticated
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
            {
        return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Billing" });
            }

            try
            {
                // Get current user
                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Billing" });
                }

                // Load billing data
                await LoadBillingData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading billing data for user {UserId}", CurrentUser?.Id);
                ErrorMessage = "Errore nel caricamento dei dati di fatturazione. Riprova più tardi.";
            }

            return Page();
        }

        private async Task LoadBillingData()
        {
            if (CurrentUser == null) return;

            try
            {
                // Load user balance
                CurrentBalance = await _billingService.GetUserBalanceAsync(CurrentUser.Id);
                
                // Set user data
                MinimumCredit = CurrentUser.CreditoMinimo;
                EcoPoints = CurrentUser.PuntiEco;

                // Load recharges
                Recharges = await _billingService.GetUserRechargesAsync(CurrentUser.Id);
                Recharges = Recharges.OrderByDescending(r => r.DataRicarica).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading billing data for user {UserId}", CurrentUser.Id);
                
                // Set default values
                CurrentBalance = 0;
                MinimumCredit = 5;
                EcoPoints = 0;
                Recharges = new List<Recharge>();
            }
        }
    }
}
