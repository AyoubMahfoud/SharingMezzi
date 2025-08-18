using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(
            IAuthService authService,
            IUserService userService,
            ILogger<UsersModel> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        public List<User> Users { get; set; } = new();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int AdminCount { get; set; }
        
        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync(int? page, string? returnUrl)
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Admin/Users" });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    _logger.LogWarning("Non-admin user {Email} tried to access admin page", currentUser?.Email);
                    return RedirectToPage("/Index");
                }

                CurrentPage = page ?? 1;

                // Carica utenti dal servizio reale
                await LoadUsersFromService();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin users page");
                LoadFallbackData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSuspendUserAsync(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    return RedirectToPage("/Index");
                }

                var success = await _userService.SuspendUserAsync(id, "Sospeso dall'amministratore");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Utente sospeso con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nella sospensione dell'utente";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending user {UserId}", id);
                TempData["ErrorMessage"] = "Errore nella sospensione dell'utente";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostActivateUserAsync(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    return RedirectToPage("/Index");
                }

                var success = await _userService.ReactivateUserAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Utente riattivato con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nella riattivazione dell'utente";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", id);
                TempData["ErrorMessage"] = "Errore nella riattivazione dell'utente";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    return RedirectToPage("/Index");
                }

                var success = await _userService.DeleteUserAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Utente eliminato con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nell'eliminazione dell'utente";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "Errore nell'eliminazione dell'utente";
                return RedirectToPage();
            }
        }

        private async Task LoadUsersFromService()
        {
            try
            {
                Users = await _userService.GetAllUsersAsync();
                
                if (Users == null || !Users.Any())
                {
                    _logger.LogInformation("No users returned from service, using fallback data");
                    LoadFallbackData();
                    return;
                }

                CalculateStatistics();
                ApplyPagination();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users from service");
                LoadFallbackData();
            }
        }

        private void CalculateStatistics()
        {
            TotalUsers = Users.Count;
            ActiveUsers = Users.Count(u => u.Stato == UserStatus.Attivo);
            NewUsersToday = Users.Count(u => u.DataRegistrazione.Date == DateTime.Today);
            AdminCount = Users.Count(u => u.Ruolo == UserRole.Admin);
        }

        private void ApplyPagination()
        {
            TotalPages = (int)Math.Ceiling(TotalUsers / (double)PageSize);
            Users = Users.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        }

        private void LoadFallbackData()
        {
            Users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Nome = "Demo",
                    Cognome = "User",
                    Email = "demo@sharingmezzi.com",
                    Telefono = "123456789",
                    Ruolo = UserRole.Utente,
                    Credito = 25.50m,
                    PuntiEco = 150,
                    Stato = UserStatus.Attivo,
                    DataRegistrazione = DateTime.Now.AddDays(-30),
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now
                }
            };

            CalculateStatistics();
            ApplyPagination();
        }
    }
}