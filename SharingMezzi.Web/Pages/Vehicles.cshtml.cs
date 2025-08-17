using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
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

        // Properties for the page
        public List<Vehicle> Vehicles { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsDataLoaded { get; set; } = false;
        
        // Statistics for hero section
        public int TotalVehicles => Vehicles.Count;
        public int AvailableVehicles => Vehicles.Count(v => v.Stato == VehicleStatus.Disponibile);
        public int InUseVehicles => Vehicles.Count(v => v.Stato == VehicleStatus.InUso);
        public int MaintenanceVehicles => Vehicles.Count(v => v.Stato == VehicleStatus.Manutenzione);

        // 🔧 METODO DEBUG TEMPORANEO 🔧
        private async Task LoadVehiclesWithDebugAsync()
        {
            try
            {
                _logger.LogInformation("🔍 DEBUG: Inizio caricamento vehicles");
                
                // Verifica autenticazione
                var token = _authService.GetToken();
                _logger.LogInformation($"🔑 DEBUG: Token presente: {!string.IsNullOrEmpty(token)}");
                
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation($"🔑 DEBUG: Token length: {token.Length}");
                    _logger.LogInformation($"🔑 DEBUG: Token inizio: {token.Substring(0, Math.Min(20, token.Length))}...");
                }

                // Verifica utente corrente
                var user = await _authService.GetCurrentUserAsync();
                _logger.LogInformation($"👤 DEBUG: User caricato: {user != null}");
                if (user != null)
                {
                    _logger.LogInformation($"👤 DEBUG: User ID: {user.Id}, Nome: {user.Nome}, Ruolo: {user.Ruolo}");
                }

                // Test diversi endpoint
                _logger.LogInformation("📡 DEBUG: Tentativo endpoint GetAvailableVehiclesAsync()");
                Vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                _logger.LogInformation($"📊 DEBUG: GetAvailableVehiclesAsync restituito {Vehicles.Count} mezzi");

                if (Vehicles.Count == 0)
                {
                    _logger.LogInformation("📡 DEBUG: Tentativo endpoint GetVehiclesAsync()");
                    Vehicles = await _vehicleService.GetVehiclesAsync();
                    _logger.LogInformation($"📊 DEBUG: GetVehiclesAsync restituito {Vehicles.Count} mezzi");
                }

                if (Vehicles.Count == 0 && user?.Ruolo == UserRole.Amministratore)
                {
                    _logger.LogInformation("📡 DEBUG: Tentativo endpoint GetAllVehiclesAsync() per admin");
                    Vehicles = await _vehicleService.GetAllVehiclesAsync();
                    _logger.LogInformation($"📊 DEBUG: GetAllVehiclesAsync restituito {Vehicles.Count} mezzi");
                }

                // Log dettagli mezzi
                if (Vehicles.Any())
                {
                    _logger.LogInformation("🚲 DEBUG: Mezzi caricati:");
                    foreach (var vehicle in Vehicles.Take(3))
                    {
                        _logger.LogInformation($"   - ID: {vehicle.Id}, Modello: {vehicle.Modello}, Stato: {vehicle.Stato}, Tipo: {vehicle.Tipo}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ DEBUG: Nessun mezzo restituito da tutti gli endpoint!");
                    ErrorMessage = "Nessun mezzo trovato nel database. Verifica che ci siano mezzi inseriti.";
                }

                IsDataLoaded = Vehicles.Count > 0;
                
                _logger.LogInformation($"✅ DEBUG: Caricamento completato. Totale mezzi: {Vehicles.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DEBUG: Errore durante caricamento vehicles");
                ErrorMessage = $"Errore caricamento: {ex.Message}";
                
                // Carica dati mock come fallback
                _logger.LogInformation("🔄 DEBUG: Caricamento dati mock come fallback");
                await LoadMockDataAsync();
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("🚀 DEBUG: Vehicles page loading started");

            // Check authentication
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("🔒 DEBUG: User not authenticated, redirecting to login");
                return RedirectToPage("/Login");
            }

            try
            {
                // 🔧 USA IL METODO DEBUG TEMPORANEAMENTE 🔧
                await LoadVehiclesWithDebugAsync();

                _logger.LogInformation($"✅ DEBUG: Vehicles page loaded successfully. Total vehicles: {Vehicles.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DEBUG: Error loading vehicles page");
                ErrorMessage = "Errore nel caricamento dei mezzi. Riprova più tardi.";
                Vehicles = new List<Vehicle>();
                IsDataLoaded = false;
            }

            return Page();
        }

        private async Task LoadMockDataAsync()
        {
            _logger.LogInformation("Loading mock vehicle data as fallback");
            
            // Provide mock data if API is not available
            Vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = 1,
                    Modello = "CityBike Pro",
                    Tipo = VehicleType.Bicicletta,
                    IsElettrico = true,
                    Stato = VehicleStatus.Disponibile,
                    LivelloBatteria = 85,
                    TariffaPerMinuto = 0.15m,
                    TariffaFissa = 1.00m,
                    ParcheggioId = 1,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now
                },
                new Vehicle
                {
                    Id = 2,
                    Modello = "EcoScooter X1",
                    Tipo = VehicleType.Scooter,
                    IsElettrico = true,
                    Stato = VehicleStatus.Disponibile,
                    LivelloBatteria = 92,
                    TariffaPerMinuto = 0.25m,
                    TariffaFissa = 1.50m,
                    ParcheggioId = 2,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    UpdatedAt = DateTime.Now
                },
                new Vehicle
                {
                    Id = 3,
                    Modello = "Tesla Model 3",
                    Tipo = VehicleType.Auto,
                    IsElettrico = true,
                    Stato = VehicleStatus.Disponibile,
                    LivelloBatteria = 78,
                    TariffaPerMinuto = 0.45m,
                    TariffaFissa = 2.00m,
                    ParcheggioId = 3,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    UpdatedAt = DateTime.Now
                },
                new Vehicle
                {
                    Id = 4,
                    Modello = "CityBike Lite",
                    Tipo = VehicleType.Bicicletta,
                    IsElettrico = false,
                    Stato = VehicleStatus.InUso,
                    LivelloBatteria = null,
                    TariffaPerMinuto = 0.12m,
                    TariffaFissa = 0.50m,
                    ParcheggioId = 1,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    UpdatedAt = DateTime.Now
                },
                new Vehicle
                {
                    Id = 5,
                    Modello = "EcoScooter Pro",
                    Tipo = VehicleType.Scooter,
                    IsElettrico = true,
                    Stato = VehicleStatus.Manutenzione,
                    LivelloBatteria = 15,
                    TariffaPerMinuto = 0.28m,
                    TariffaFissa = 1.50m,
                    ParcheggioId = 4,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    UpdatedAt = DateTime.Now
                }
            };

            IsDataLoaded = false; // Mark as mock data
            ErrorMessage += " Visualizzando dati di esempio.";
            
            await Task.CompletedTask; // Make it async
        }

        // API endpoint for AJAX calls
        public async Task<IActionResult> OnPostUnlockVehicleAsync(int vehicleId)
        {
            try
            {
                _logger.LogInformation("User attempting to unlock vehicle {VehicleId}", vehicleId);

                var success = await _vehicleService.UnlockVehicleAsync(vehicleId);
                
                if (success)
                {
                    _logger.LogInformation("Vehicle {VehicleId} unlocked successfully", vehicleId);
                    return new JsonResult(new { success = true, message = "Mezzo sbloccato con successo!" });
                }
                else
                {
                    _logger.LogWarning("Failed to unlock vehicle {VehicleId}", vehicleId);
                    return new JsonResult(new { success = false, message = "Errore nello sblocco del mezzo." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking vehicle {VehicleId}", vehicleId);
                return new JsonResult(new { success = false, message = "Errore interno del server." });
            }
        }

        // API endpoint for reporting maintenance
        public async Task<IActionResult> OnPostReportMaintenanceAsync(int vehicleId, string description)
        {
            try
            {
                _logger.LogInformation("User reporting maintenance for vehicle {VehicleId}", vehicleId);

                var success = await _vehicleService.ReportMaintenanceAsync(vehicleId, description);
                
                if (success)
                {
                    _logger.LogInformation("Maintenance reported for vehicle {VehicleId}", vehicleId);
                    return new JsonResult(new { success = true, message = "Segnalazione inviata con successo!" });
                }
                else
                {
                    _logger.LogWarning("Failed to report maintenance for vehicle {VehicleId}", vehicleId);
                    return new JsonResult(new { success = false, message = "Errore nell'invio della segnalazione." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting maintenance for vehicle {VehicleId}", vehicleId);
                return new JsonResult(new { success = false, message = "Errore interno del server." });
            }
        }

        // API endpoint for refreshing vehicle data
        public async Task<IActionResult> OnGetRefreshVehiclesAsync()
        {
            try
            {
                await LoadVehiclesWithDebugAsync();
                
                return new JsonResult(new 
                { 
                    success = true, 
                    total = TotalVehicles,
                    available = AvailableVehicles,
                    inUse = InUseVehicles,
                    maintenance = MaintenanceVehicles,
                    isRealData = IsDataLoaded
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing vehicles");
                return new JsonResult(new { success = false, message = "Errore nel refresh dei dati." });
            }
        }
    }
}