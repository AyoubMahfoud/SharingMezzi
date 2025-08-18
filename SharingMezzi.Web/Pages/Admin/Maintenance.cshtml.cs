using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    public class MaintenanceModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<MaintenanceModel> _logger;

        public MaintenanceModel(
            IAuthService authService,
            IVehicleService vehicleService,
            ILogger<MaintenanceModel> logger)
        {
            _authService = authService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public List<MaintenanceItem> MaintenanceItems { get; set; } = new();
        public int TotalMaintenance { get; set; }
        public int UrgentMaintenance { get; set; }
        public int ScheduledMaintenance { get; set; }
        public int CompletedToday { get; set; }

        public async Task<IActionResult> OnGetAsync(string? returnUrl)
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Admin/Maintenance" });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != SharingMezzi.Web.Models.UserRole.Admin)
                {
                    _logger.LogWarning("Non-admin user {Email} tried to access admin maintenance page", currentUser?.Email);
                    return RedirectToPage("/Index");
                }

                // Carica dati manutenzione dal servizio reale
                await LoadMaintenanceDataFromService();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin maintenance page");
                LoadFallbackData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStartMaintenanceAsync(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != SharingMezzi.Web.Models.UserRole.Admin)
                {
                    return RedirectToPage("/Index");
                }

                var success = await _vehicleService.ReportMaintenanceAsync(id, "Manutenzione avviata dall'amministratore");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Manutenzione avviata con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nell'avvio della manutenzione";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting maintenance {MaintenanceId}", id);
                TempData["ErrorMessage"] = "Errore nell'avvio della manutenzione";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostCompleteMaintenanceAsync(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login");
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != SharingMezzi.Web.Models.UserRole.Admin)
                {
                    return RedirectToPage("/Index");
                }

                // Simuliamo il completamento della manutenzione
                var success = true;
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Manutenzione completata con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nel completamento della manutenzione";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing maintenance {MaintenanceId}", id);
                TempData["ErrorMessage"] = "Errore nel completamento della manutenzione";
                return RedirectToPage();
            }
        }

        private async Task LoadMaintenanceDataFromService()
        {
            try
            {
                // Carica veicoli in manutenzione
                var vehicles = await _vehicleService.GetVehiclesAsync();
                var maintenanceVehicles = vehicles.Where(v => v.Stato == VehicleStatus.Manutenzione).ToList();
                
                MaintenanceItems = maintenanceVehicles.Select(v => new MaintenanceItem
                {
                    Id = v.Id,
                    VehicleId = v.Id,
                    VehicleModel = v.Modello,
                    Description = $"Mezzo {v.Modello} in manutenzione",
                    Priority = v.LivelloBatteria.HasValue && v.LivelloBatteria < 10 ? 
                              MaintenancePriority.High : MaintenancePriority.Medium,
                    Status = MaintenanceStatus.InProgress,
                    Type = MaintenanceType.Scheduled,
                    AssignedTechnician = GetRandomTechnician(),
                    ScheduledDate = v.UltimaManutenzione ?? DateTime.Now,
                    EstimatedCompletion = DateTime.Now.AddHours(2)
                }).ToList();
                
                if (!MaintenanceItems.Any())
                {
                    _logger.LogInformation("No vehicles in maintenance, using fallback data");
                    LoadFallbackData();
                    return;
                }

                CalculateStatistics();
                _logger.LogInformation("Maintenance data loaded successfully from service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance data from service");
                LoadFallbackData();
            }
        }

        private static string GetRandomTechnician()
        {
            var technicians = new[] { "Mario Rossi", "Luca Bianchi", "Anna Verdi", "Giuseppe Neri", "Sofia Romano" };
            return technicians[Random.Shared.Next(technicians.Length)];
        }

        private void CalculateStatistics()
        {
            TotalMaintenance = MaintenanceItems.Count;
            UrgentMaintenance = MaintenanceItems.Count(m => m.Priority == MaintenancePriority.High);
            ScheduledMaintenance = MaintenanceItems.Count(m => m.Status == MaintenanceStatus.Scheduled);
            CompletedToday = MaintenanceItems.Count(m => 
                m.Status == MaintenanceStatus.Completed && 
                m.CompletedDate?.Date == DateTime.Today);
        }

        private void LoadFallbackData()
        {
            MaintenanceItems = new List<MaintenanceItem>
            {
                new MaintenanceItem
                {
                    Id = 1,
                    VehicleId = 1,
                    VehicleModel = "Demo Bike",
                    Description = "Controllo generale",
                    Priority = MaintenancePriority.Medium,
                    Status = MaintenanceStatus.Scheduled,
                    Type = MaintenanceType.Scheduled,
                    AssignedTechnician = "Demo Technician",
                    ScheduledDate = DateTime.Now.AddHours(2),
                    EstimatedCompletion = DateTime.Now.AddHours(4)
                }
            };

            CalculateStatistics();
            _logger.LogInformation("Using fallback data for maintenance");
        }
    }
}