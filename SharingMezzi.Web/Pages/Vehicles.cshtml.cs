using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using SharingMezzi.Web.Models;
using System.Net.Http;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace SharingMezzi.Web.Pages
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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

                // 🔧 SOLUZIONE TEMPORANEA: Usa direttamente l'API pubblica che funziona
                _logger.LogInformation("🔧 DEBUG: Tentativo caricamento diretto da API pubblica");
                try
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync("http://localhost:5000/api/public/mezzi/disponibili");
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
                    {
                        _logger.LogInformation($"🌐 DEBUG: API pubblica response: {response.StatusCode}");

                        try
                        {
                            var jArray = JArray.Parse(content);
                            if (jArray != null && jArray.Count > 0)
                            {
                                Vehicles = jArray.Select(j => new Vehicle
                                {
                                    Id = (int?)(j["id"]?.Value<int?>()) ?? 0,
                                    Modello = (string?)(j["modello"]?.Value<string>()) ?? "",
                                    Tipo = ParseVehicleTypeFromJson((int?)(j["tipo"]?.Value<int?>()) ?? 0),
                                    IsElettrico = (bool?)(j["isElettrico"]?.Value<bool?>()) ?? false,
                                    Stato = ParseVehicleStatusFromJson((string?)(j["stato"]?.Value<string>())),
                                    LivelloBatteria = (int?)(j["livelloBatteria"]?.Value<int?>()),
                                    TariffaPerMinuto = j["tariffaPerMinuto"] != null ? Convert.ToDecimal(j["tariffaPerMinuto"].Value<double>()) : 0m,
                                    TariffaFissa = j["tariffaFissa"] != null ? Convert.ToDecimal(j["tariffaFissa"].Value<double>()) : 0m,
                                    ParcheggioId = (int?)(j["parcheggioAttualeId"]?.Value<int?>())
                                }).ToList();

                                _logger.LogInformation($"✅ DEBUG: Parsati {Vehicles.Count} mezzi dall'API pubblica (Newtonsoft)");
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ DEBUG: Nessun dato valido dall'API pubblica (Newtonsoft)");
                            }
                        }
                        catch (Exception parseEx)
                        {
                            _logger.LogError(parseEx, "❌ DEBUG: Errore parsing JSON API pubblica");
                            _logger.LogInformation($"🌐 DEBUG: content: {content}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ DEBUG: API pubblica fallita: {response.StatusCode}");
                    }
                }
                catch (Exception apiEx)
                {
                    _logger.LogError(apiEx, "❌ DEBUG: Errore chiamata diretta API pubblica");
                }

                // Se ancora non ci sono mezzi, prova i servizi tradizionali
                if (Vehicles.Count == 0)
                {
                    _logger.LogInformation("📡 DEBUG: Tentativo endpoint GetAvailableVehiclesAsync()");
                    Vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                    _logger.LogInformation($"📊 DEBUG: GetAvailableVehiclesAsync restituito {Vehicles.Count} mezzi");
                }

                if (Vehicles.Count == 0)
                {
                    _logger.LogInformation("📡 DEBUG: Tentativo endpoint GetVehiclesAsync()");
                    Vehicles = await _vehicleService.GetVehiclesAsync();
                    _logger.LogInformation($"📊 DEBUG: GetVehiclesAsync restituito {Vehicles.Count} mezzi");
                }

                if (Vehicles.Count == 0 && user?.Ruolo == UserRole.Admin)
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

        // 🔧 METODI DI PARSING PER JSON
        private static VehicleType ParseVehicleTypeFromJson(int tipo)
        {
            return tipo switch
            {
                0 => VehicleType.Bicicletta,
                1 => VehicleType.Scooter,
                2 => VehicleType.Auto,
                3 => VehicleType.Monopattino,
                4 => VehicleType.EBike,
                _ => VehicleType.Bicicletta
            };
        }

        private static VehicleStatus ParseVehicleStatusFromJson(string? stato)
        {
            if (string.IsNullOrEmpty(stato)) return VehicleStatus.Disponibile;
            
            return stato.ToLower() switch
            {
                "disponibile" => VehicleStatus.Disponibile,
                "inuso" or "occupato" => VehicleStatus.InUso,
                "manutenzione" => VehicleStatus.Manutenzione,
                "fuori_servizio" or "fuoriservizio" => VehicleStatus.Fuori_Servizio,
                _ => VehicleStatus.Disponibile
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("🚀 DEBUG: Vehicles page loading started");

            // Check authentication with detailed logging
            var token = _authService.GetToken();
            _logger.LogInformation($"🔑 DEBUG: Token presente: {!string.IsNullOrEmpty(token)}");
            
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogInformation($"🔑 DEBUG: Token length: {token.Length}");
                _logger.LogInformation($"🔑 DEBUG: Token inizio: {token.Substring(0, Math.Min(20, token.Length))}...");
            }

            // Verifica anche l'utente corrente
            var currentUser = await _authService.GetCurrentUserAsync();
            _logger.LogInformation($"👤 DEBUG: Current user: {currentUser != null}");
            if (currentUser != null)
            {
                _logger.LogInformation($"👤 DEBUG: User ID: {currentUser.Id}, Nome: {currentUser.Nome}, Ruolo: {currentUser.Ruolo}");
            }

            // Se non c'è token ma c'è utente, prova a recuperare il token
            if (string.IsNullOrEmpty(token) && currentUser != null)
            {
                _logger.LogWarning("⚠️ DEBUG: Token mancante ma utente presente, tentativo recupero...");
                
                // Prova a forzare il refresh dell'autenticazione
                try
                {
                    // Forza il refresh della sessione
                    _authService.SetToken(_authService.GetToken() ?? "");
                    token = _authService.GetToken();
                    _logger.LogInformation($"🔄 DEBUG: Token dopo refresh: {!string.IsNullOrEmpty(token)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ DEBUG: Errore durante refresh token");
                }
            }

            // Se ancora non c'è token, mostra errore invece di reindirizzare
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("🔒 DEBUG: User not authenticated, showing error instead of redirecting");
                ErrorMessage = "Errore di autenticazione. Il tuo token di accesso è scaduto o non è valido. Effettua nuovamente il login.";
                Vehicles = new List<Vehicle>();
                IsDataLoaded = false;
                return Page(); // Non reindirizza, mostra la pagina con errore
            }

            try
            {
                _logger.LogInformation("🔍 DEBUG: Tentativo caricamento mezzi con autenticazione");
                
                // 🔧 USA IL METODO DEBUG TEMPORANEAMENTE 🔧
                await LoadVehiclesWithDebugAsync();

                _logger.LogInformation($"✅ DEBUG: Vehicles page loaded successfully. Total vehicles: {Vehicles.Count}");
                
                // Log dettagliato dei mezzi caricati
                if (Vehicles.Any())
                {
                    _logger.LogInformation("🚲 DEBUG: Mezzi caricati con successo:");
                    foreach (var vehicle in Vehicles.Take(3))
                    {
                        _logger.LogInformation($"   - ID: {vehicle.Id}, Modello: {vehicle.Modello}, Stato: {vehicle.Stato}, Tipo: {vehicle.Tipo}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ DEBUG: Nessun mezzo caricato dalla pagina");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DEBUG: Error loading vehicles page");
                ErrorMessage = $"Errore nel caricamento dei mezzi: {ex.Message}";
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

        // API endpoint for refreshing authentication
        public async Task<IActionResult> OnGetRefreshAuthAsync()
        {
            try
            {
                _logger.LogInformation("🔄 DEBUG: Tentativo refresh autenticazione");
                
                // Prova a recuperare l'utente corrente
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    _logger.LogInformation($"👤 DEBUG: Utente trovato: {currentUser.Nome}");
                    
                    // Prova a forzare il refresh del token
                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        _logger.LogInformation("✅ DEBUG: Token presente, autenticazione valida");
                        return new JsonResult(new { success = true, message = "Autenticazione valida", user = currentUser.Nome });
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ DEBUG: Token mancante nonostante utente presente");
                        return new JsonResult(new { success = false, message = "Token mancante, richiesto nuovo login" });
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ DEBUG: Nessun utente trovato");
                    return new JsonResult(new { success = false, message = "Nessun utente autenticato" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DEBUG: Errore durante refresh autenticazione");
                return new JsonResult(new { success = false, message = $"Errore: {ex.Message}" });
            }
        }
    }
}