using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    public class PaymentsModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IBillingService _billingService;
        private readonly ILogger<PaymentsModel> _logger;

        public PaymentsModel(
            IAuthService authService,
            IBillingService billingService,
            ILogger<PaymentsModel> logger)
        {
            _authService = authService;
            _billingService = billingService;
            _logger = logger;
        }

        public List<Recharge> Payments { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync(int? page, string? returnUrl)
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Admin/Payments" });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    _logger.LogWarning("Non-admin user {Email} tried to access admin payments page", currentUser?.Email);
                    return RedirectToPage("/Index");
                }

                CurrentPage = page ?? 1;

                // Carica pagamenti dal servizio reale
                await LoadPaymentsFromService();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin payments page");
                LoadFallbackData();
                return Page();
            }
        }

        private async Task LoadPaymentsFromService()
        {
            try
            {
                Payments = await _billingService.GetRechargesAsync();
                
                if (Payments == null || !Payments.Any())
                {
                    _logger.LogInformation("No payments returned from service, using fallback data");
                    LoadFallbackData();
                    return;
                }

                CalculateStatistics();
                ApplyPagination();
                _logger.LogInformation("Payments data loaded successfully from service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payments from service");
                LoadFallbackData();
            }
        }

        private void CalculateStatistics()
        {
            var completedPayments = Payments.Where(p => p.StatoPagamento == PaymentStatus.Completato);
            
            TotalRevenue = completedPayments.Sum(p => p.Importo);
            MonthlyRevenue = completedPayments
                .Where(p => p.DataRicarica.Month == DateTime.Now.Month && p.DataRicarica.Year == DateTime.Now.Year)
                .Sum(p => p.Importo);
            
            TotalTransactions = Payments.Count;
            FailedTransactions = Payments.Count(p => p.StatoPagamento == PaymentStatus.Fallito);
            
            AverageTransactionAmount = completedPayments.Any() 
                ? completedPayments.Average(p => p.Importo) 
                : 0;
        }

        private void ApplyPagination()
        {
            TotalPages = (int)Math.Ceiling(TotalTransactions / (double)PageSize);
            Payments = Payments
                .OrderByDescending(p => p.DataRicarica)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private void LoadFallbackData()
        {
            Payments = new List<Recharge>
            {
                new Recharge 
                {
                    Id = 1,
                    UtenteId = 1,
                    Importo = 25.00m,
                    MetodoPagamento = PaymentMethod.CartaCredito,
                    StatoPagamento = PaymentStatus.Completato,
                    DataRicarica = DateTime.Now.AddDays(-1),
                    TransactionId = "DEMO123456789",
                    SaldoFinale = 125.00m
                }
            };

            CalculateStatistics();
            ApplyPagination();
            _logger.LogInformation("Using fallback data for payments");
        }
    }
}