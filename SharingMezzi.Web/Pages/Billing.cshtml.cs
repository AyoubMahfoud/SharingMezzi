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
        try
        {
            // Get current user from session
            CurrentUser = await _authService.GetCurrentUserAsync();
            
            // Set default values - the actual data will be loaded via JavaScript
            CurrentBalance = 0;
            MinimumCredit = 5;
            EcoPoints = 0;
            Recharges = new List<Recharge>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Billing page");
            ErrorMessage = "Errore nella pagina di fatturazione.";
        }

        return Page();
    }
    }
}
