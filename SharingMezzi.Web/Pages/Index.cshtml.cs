using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SharingMezzi.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IApiService _apiService;

        public IndexModel(ILogger<IndexModel> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        [Display(Name = "Mezzi Disponibili")]
        public int AvailableVehicles { get; set; } = 0;

        [Display(Name = "Utenti Attivi")]
        public int ActiveUsers { get; set; } = 0;

        [Display(Name = "Stazioni Attive")]
        public int ActiveStations { get; set; } = 0;

        [Display(Name = "Km Percorsi Oggi")]
        public decimal KilometersToday { get; set; } = 0;

        [Display(Name = "CO₂ Risparmiata (kg)")]
        public decimal Co2Saved { get; set; } = 0;

        [Display(Name = "Viaggi Completati")]
        public int CompletedTrips { get; set; } = 0;

        [Display(Name = "Crescita Mezzi")]
        public decimal VehicleGrowth { get; set; } = 12.5m;

        [Display(Name = "Crescita Utenti")]
        public decimal UserGrowth { get; set; } = 8.3m;

        [Display(Name = "Crescita Stazioni")]
        public decimal StationGrowth { get; set; } = 5.2m;

        [Display(Name = "Crescita CO₂")]
        public decimal Co2Growth { get; set; } = 25.7m;

        public bool IsDataLoaded { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> DebugMessages { get; set; } = new List<string>();

        /// <summary>
        /// Carica i dati reali dalle API corrette
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("=== CARICAMENTO DATI DA API REALI ===");
                DebugMessages.Add($"Inizio: {DateTime.Now:HH:mm:ss}");

                // Carica dati in parallelo dalle API corrette
                var tasks = new List<Task>
                {
                    LoadVehicleStatistics(),
                    LoadParkingStatistics(),
                    LoadTripStatistics(),
                    LoadSystemStatus()
                };

                await Task.WhenAll(tasks);

                // Calcola statistiche derivate
                CalculateDerivedStats();

                // Verifica se i dati sono stati caricati
                if (AvailableVehicles > 0 || ActiveStations > 0 || CompletedTrips > 0)
                {
                    IsDataLoaded = true;
                    DebugMessages.Add("✅ Dati reali caricati con successo");
                }
                else
                {
                    DebugMessages.Add("⚠️ Nessun dato reale - usando fallback");
                    LoadFallbackData();
                }

                _logger.LogInformation("=== FINE CARICAMENTO DATI ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento dati homepage");
                ErrorMessage = $"Errore: {ex.Message}";
                DebugMessages.Add($"❌ Errore globale: {ex.Message}");
                LoadFallbackData();
            }

            return Page();
        }

        /// <summary>
        /// Carica statistiche mezzi dalle API corrette
        /// </summary>
        private async Task LoadVehicleStatistics()
        {
            try
            {
                _logger.LogInformation("🚲 Caricamento mezzi da /api/mezzi/disponibili...");
                DebugMessages.Add("Caricando mezzi disponibili...");

                // Usa l'endpoint corretto: /api/mezzi/disponibili
                var availableVehicles = await _apiService.GetAsync<List<dynamic>>("/api/mezzi/disponibili");
                
                if (availableVehicles != null)
                {
                    AvailableVehicles = availableVehicles.Count;
                    _logger.LogInformation($"✅ Trovati {AvailableVehicles} mezzi disponibili");
                    DebugMessages.Add($"✅ Mezzi disponibili: {AvailableVehicles}");
                    return;
                }

                // Fallback: prova tutti i mezzi e conta quelli disponibili
                var allVehicles = await _apiService.GetAsync<List<dynamic>>("/api/mezzi");
                
                if (allVehicles != null)
                {
                    AvailableVehicles = allVehicles.Count(v => 
                        v.stato?.ToString() == "Disponibile");
                    
                    _logger.LogInformation($"✅ Mezzi disponibili (da tutti): {AvailableVehicles}/{allVehicles.Count}");
                    DebugMessages.Add($"✅ Mezzi disponibili: {AvailableVehicles}/{allVehicles.Count} totali");
                    return;
                }

                DebugMessages.Add("❌ Nessun endpoint mezzi funziona");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel caricamento statistiche mezzi");
                DebugMessages.Add($"❌ Errore mezzi: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica statistiche parcheggi
        /// </summary>
        private async Task LoadParkingStatistics()
        {
            try
            {
                _logger.LogInformation("🅿️ Caricamento parcheggi da /api/parcheggi...");
                DebugMessages.Add("Caricando parcheggi...");

                // Usa l'endpoint corretto: /api/parcheggi
                var parkings = await _apiService.GetAsync<List<dynamic>>("/api/parcheggi");
                
                if (parkings != null)
                {
                    ActiveStations = parkings.Count;
                    _logger.LogInformation($"✅ Trovati {ActiveStations} parcheggi");
                    DebugMessages.Add($"✅ Parcheggi: {ActiveStations}");
                    return;
                }

                DebugMessages.Add("❌ Endpoint parcheggi non funziona");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel caricamento statistiche parcheggi");
                DebugMessages.Add($"❌ Errore parcheggi: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica statistiche viaggi/corse
        /// </summary>
        private async Task LoadTripStatistics()
        {
            try
            {
                _logger.LogInformation("🛣️ Caricamento corse da /api/corse/storico...");
                DebugMessages.Add("Caricando storico corse...");

                // Usa l'endpoint corretto: /api/corse/storico
                // Nota: questo endpoint richiede autenticazione, quindi potrebbe fallire
                var trips = await _apiService.GetAsync<List<dynamic>>("/api/corse/storico");
                
                if (trips != null)
                {
                    CompletedTrips = trips.Count;
                    
                    // Calcola km totali se disponibili
                    KilometersToday = trips
                        .Where(t => t.durataMinuti != null && t.durataMinuti > 0)
                        .Sum(t => {
                            // Stima: 1 minuto di corsa = ~0.3 km (velocità media 18 km/h)
                            var minutes = (decimal)(t.durataMinuti ?? 0);
                            return Math.Round(minutes * 0.3m, 2);
                        });
                    
                    _logger.LogInformation($"✅ Trovati {CompletedTrips} viaggi, stimati {KilometersToday}km");
                    DebugMessages.Add($"✅ Viaggi: {CompletedTrips}, Km stimati: {KilometersToday}");
                    return;
                }

                DebugMessages.Add("❌ Endpoint corse/storico non accessibile (richiede auth)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel caricamento statistiche corse");
                DebugMessages.Add($"❌ Errore corse: {ex.Message}");
                
                // Se fallisce per mancanza di auth, è normale
                if (ex.Message.Contains("401") || ex.Message.Contains("Non autorizzato"))
                {
                    DebugMessages.Add("ℹ️ Endpoint corse richiede autenticazione - normale per homepage pubblica");
                }
            }
        }

        /// <summary>
        /// Carica stato sistema (per admin)
        /// </summary>
        private async Task LoadSystemStatus()
        {
            try
            {
                _logger.LogInformation("📊 Tentativo caricamento system status...");
                DebugMessages.Add("Tentando system status...");

                // Questo endpoint richiede auth admin, quindi probabilmente fallirà
                var systemStatus = await _apiService.GetAsync<dynamic>("/api/admin/system-status");
                
                if (systemStatus != null)
                {
                    // Estrai dati se disponibili
                    AvailableVehicles = systemStatus.mezziDisponibili ?? AvailableVehicles;
                    ActiveStations = systemStatus.totaleParcheggi ?? ActiveStations;
                    ActiveUsers = systemStatus.corsaAttive ?? 0; // Corse attive come proxy utenti attivi
                    
                    DebugMessages.Add($"✅ System status: V:{AvailableVehicles}, S:{ActiveStations}, U:{ActiveUsers}");
                    return;
                }

                DebugMessages.Add("❌ System status non accessibile");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "System status non disponibile (normale senza auth admin)");
                DebugMessages.Add($"ℹ️ System status richiede auth admin - {ex.Message}");
            }
        }

        /// <summary>
        /// Calcola statistiche derivate
        /// </summary>
        private void CalculateDerivedStats()
        {
            // CO2 risparmiata: 1km in bici vs auto = ~0.12kg CO2 risparmiata
            Co2Saved = Math.Round(KilometersToday * 0.12m, 2);
            
            // Se non abbiamo utenti attivi, stimiamo dal numero di viaggi
            if (ActiveUsers == 0 && CompletedTrips > 0)
            {
                // Stima: ~20% degli utenti totali sono attivi
                ActiveUsers = Math.Max(1, CompletedTrips / 5);
            }
            
            DebugMessages.Add($"✅ Statistiche derivate: CO2={Co2Saved}kg, Utenti stimati={ActiveUsers}");
        }

        /// <summary>
        /// Dati di fallback se le API non rispondono
        /// </summary>
        private void LoadFallbackData()
        {
            _logger.LogWarning("🔄 Caricamento dati di fallback");
            
            AvailableVehicles = 125;
            ActiveUsers = 15000;
            ActiveStations = 50;
            Co2Saved = 2500;
            KilometersToday = 8750;
            CompletedTrips = 45000;
            
            IsDataLoaded = false; // Indica che sono dati demo
            DebugMessages.Add("🔄 Dati di fallback caricati");
        }

        /// <summary>
        /// API endpoint per refresh via AJAX
        /// </summary>
        public async Task<IActionResult> OnGetRefreshStatsAsync()
        {
            try
            {
                await OnGetAsync();

                var stats = new
                {
                    availableVehicles = AvailableVehicles,
                    activeUsers = ActiveUsers,
                    activeStations = ActiveStations,
                    kilometersToday = KilometersToday,
                    co2Saved = Co2Saved,
                    completedTrips = CompletedTrips,
                    vehicleGrowth = VehicleGrowth,
                    userGrowth = UserGrowth,
                    stationGrowth = StationGrowth,
                    co2Growth = Co2Growth,
                    isDataLoaded = IsDataLoaded,
                    lastUpdated = DateTime.Now.ToString("HH:mm:ss"),
                    errorMessage = ErrorMessage,
                    debugMessages = DebugMessages
                };

                return new JsonResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel refresh delle statistiche");
                return new JsonResult(new { 
                    error = "Errore nel refresh",
                    details = ex.Message,
                    lastUpdated = DateTime.Now.ToString("HH:mm:ss")
                })
                {
                    StatusCode = 500
                };
            }
        }

        public bool IsUserAuthenticated() => User?.Identity?.IsAuthenticated ?? false;
        public string GetCurrentUserName() => IsUserAuthenticated() ? (User?.Identity?.Name ?? "Utente") : "Ospite";
        public bool IsUserAdmin() => User?.IsInRole("Admin") ?? false;
    }
}