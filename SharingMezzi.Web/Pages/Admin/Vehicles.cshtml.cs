using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    public class VehiclesModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesModel> _logger;

        public VehiclesModel(
            IAuthService authService,
            IVehicleService vehicleService,
            ILogger<VehiclesModel> logger)
        {
            _authService = authService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public List<Vehicle> Vehicles { get; set; } = new();
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int InUseVehicles { get; set; }
        public int MaintenanceVehicles { get; set; }

        public async Task<IActionResult> OnGetAsync(string? returnUrl)
        {
            try
            {
                // Verifica che l'utente sia admin
                if (!_authService.IsAuthenticated())
                {
                    return RedirectToPage("/Login", new { ReturnUrl = returnUrl ?? "/Admin/Vehicles" });
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser?.Ruolo != UserRole.Admin)
                {
                    _logger.LogWarning("Non-admin user {Email} tried to access admin vehicles page", currentUser?.Email);
                    return RedirectToPage("/Index");
                }

                // Carica veicoli dal servizio reale
                await LoadVehiclesFromService();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin vehicles page");
                LoadFallbackData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSetMaintenanceAsync(int id)
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

                var success = await _vehicleService.SetMaintenanceAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Mezzo messo in manutenzione con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nell'impostazione della manutenzione";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting maintenance for vehicle {VehicleId}", id);
                TempData["ErrorMessage"] = "Errore nell'impostazione della manutenzione";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostSetAvailableAsync(int id)
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

                var success = await _vehicleService.SetAvailableAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Mezzo reso disponibile con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nel rendere disponibile il mezzo";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting vehicle available {VehicleId}", id);
                TempData["ErrorMessage"] = "Errore nel rendere disponibile il mezzo";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteVehicleAsync(int id)
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

                var success = await _vehicleService.DeleteVehicleAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Mezzo eliminato con successo";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore nell'eliminazione del mezzo";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
                TempData["ErrorMessage"] = "Errore nell'eliminazione del mezzo";
                return RedirectToPage();
            }
        }

        private async Task LoadVehiclesFromService()
        {
            try
            {
                Vehicles = await _vehicleService.GetVehiclesAsync();
                
                if (Vehicles == null || !Vehicles.Any())
                {
                    _logger.LogInformation("No vehicles returned from service, using fallback data");
                    LoadFallbackData();
                    return;
                }

                CalculateStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicles from service");
                LoadFallbackData();
            }
        }

        private void CalculateStatistics()
        {
            TotalVehicles = Vehicles.Count;
            AvailableVehicles = Vehicles.Count(v => v.Stato == VehicleStatus.Disponibile);
            InUseVehicles = Vehicles.Count(v => v.Stato == VehicleStatus.InUso);
            MaintenanceVehicles = Vehicles.Count(v => v.Stato == VehicleStatus.Manutenzione);
        }

        private void LoadFallbackData()
        {
            Vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = 1,
                    Modello = "Demo Bike",
                    Tipo = VehicleType.Bicicletta,
                    IsElettrico = false,
                    Stato = VehicleStatus.Disponibile,
                    TariffaPerMinuto = 0.20m,
                    TariffaFissa = 1.00m,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now
                }
            };

            CalculateStatistics();
        }
    }
}