using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Services;
using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IVehicleService _vehicleService;
        private readonly IParkingService _parkingService;
        private readonly IBillingService _billingService;
        private readonly IUserService _userService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IAuthService authService,
            IVehicleService vehicleService,
            IParkingService parkingService,
            IBillingService billingService,
            IUserService userService,
            ILogger<DashboardModel> logger)
        {
            _authService = authService;
            _vehicleService = vehicleService;
            _parkingService = parkingService;
            _billingService = billingService;
            _userService = userService;
            _logger = logger;
        }

        // User Info
        public User? CurrentUser { get; set; }
        
        // Stats
        public int AvailableVehicles { get; set; } = 0;
        public int TotalTrips { get; set; } = 0;
        public decimal? CurrentCredit { get; set; } = 0;
        public decimal Co2Saved { get; set; } = 0;
        public int AvailableParkings { get; set; } = 0;
        
        // Growth percentages
        public decimal VehicleGrowth { get; set; } = 0;
        public decimal TripGrowth { get; set; } = 0;
        public decimal RevenueGrowth { get; set; } = 0;
        
        // Recent activity
        public List<TripSummary> RecentTrips { get; set; } = new();
        public DateTime? LastChargeDate { get; set; }
        
        // Admin Properties (solo per admin)
        public int TotalUsers { get; set; }
        public int NewUsersThisWeek { get; set; } = 0; // Non abbiamo ancora un endpoint per nuovi utenti settimanali
        public decimal TotalRevenue { get; set; }
        public int MaintenanceVehicles { get; set; }
        public int UrgentMaintenance { get; set; }
        public int OpenReports { get; set; }
        public int PendingReports { get; set; }
        public List<SystemActivity> SystemActivities { get; set; } = new();
    public bool IsAdmin => CurrentUser?.Ruolo == UserRole.Admin;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("=== LOADING DASHBOARD DATA ===");
                
                // Get current user
                CurrentUser = await _authService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    _logger.LogWarning("User not found, redirecting to login");
                    return RedirectToPage("/Login");
                }

                _logger.LogInformation($"Loading dashboard for user: {CurrentUser.Email} (Role: {CurrentUser.Ruolo})");

                // Load data in parallel for better performance
                _logger.LogInformation("Starting parallel data loading...");
                var tasks = new List<Task>
                {
                    LoadVehicleStats(),
                    LoadParkingStats(),
                    LoadTripStats(),
                    LoadUserStats()
                };
                
                // Se è admin, carica anche le statistiche admin
                if (IsAdmin)
                {
                    _logger.LogInformation("User is admin, loading admin statistics...");
                    tasks.Add(LoadAdminStats());
                }

                _logger.LogInformation($"Executing {tasks.Count} parallel tasks...");
                await Task.WhenAll(tasks);
                _logger.LogInformation("All parallel tasks completed successfully");

                _logger.LogInformation("=== DASHBOARD DATA LOADED ===");
                _logger.LogInformation($"Final values - AvailableVehicles: {AvailableVehicles}, TotalTrips: {TotalTrips}, CurrentCredit: {CurrentCredit}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }

            return Page();
        }

        private async Task LoadVehicleStats()
        {
            try
            {
                _logger.LogInformation("Loading vehicle statistics...");
                
                // Prova prima con le statistiche pubbliche
                try
                {
                    var publicStats = await LoadPublicStats();
                    if (publicStats != null)
                    {
                        AvailableVehicles = publicStats.MezziDisponibili;
                        _logger.LogInformation($"Found {AvailableVehicles} available vehicles from public stats");
                        
                        // Carica anche le statistiche di crescita
                        var growthStats = await LoadPublicGrowthStats();
                        if (growthStats != null)
                        {
                            VehicleGrowth = GetDynamicValue(growthStats, "VehicleGrowth");
                            _logger.LogInformation($"Vehicle growth: {VehicleGrowth}%");
                        }
                        return; // IMPORTANTE: Esce qui se i dati sono stati caricati correttamente
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Public stats failed, trying vehicle service");
                }
                
                // Fallback al servizio veicoli SOLO se public stats non ha funzionato
                if (AvailableVehicles == 0) // Solo se non abbiamo già caricato i dati
                {
                    var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
                    AvailableVehicles = vehicles?.Count ?? 0;
                    _logger.LogInformation($"Found {AvailableVehicles} available vehicles from vehicle service (fallback)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle stats");
                // Non sovrascrivere se abbiamo già dati validi
                if (AvailableVehicles == 0)
                {
                    AvailableVehicles = 0;
                }
            }
        }

        private async Task<PublicStats?> LoadPublicStats()
        {
            try
            {
                var response = await new HttpClient().GetFromJsonAsync<PublicStats>("http://localhost:5000/api/public/stats");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public stats");
                return null;
            }
        }

        private async Task<dynamic?> LoadPublicGrowthStats()
        {
            try
            {
                var response = await new HttpClient().GetFromJsonAsync<dynamic>("http://localhost:5000/api/public/growth-stats");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public growth stats");
                return null;
            }
        }

        private decimal GetDynamicValue(dynamic? obj, string propertyName, decimal defaultValue = 0)
        {
            try
            {
                if (obj == null) return defaultValue;
                
                var property = obj.GetType().GetProperty(propertyName);
                if (property == null) return defaultValue;
                
                var value = property.GetValue(obj);
                if (value == null) return defaultValue;
                
                if (decimal.TryParse(value.ToString(), out decimal result))
                {
                    return result;
                }
                
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public class PublicStats
        {
            public int MezziDisponibili { get; set; }
            public int TotaleParcheggi { get; set; }
            public int CorseAttive { get; set; }
            public int TotaleUtenti { get; set; }
            public decimal UserGrowth { get; set; }
        }

        private async Task LoadParkingStats()
        {
            try
            {
                _logger.LogInformation("Loading parking statistics...");
                var parkings = await _parkingService.GetParkingsAsync();
                AvailableParkings = parkings?.Count ?? 0;
                _logger.LogInformation($"Found {AvailableParkings} parking stations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parking stats");
                AvailableParkings = 0; // Rimuovo il valore hardcoded
            }
        }

        private async Task LoadTripStats()
        {
            try
            {
                _logger.LogInformation("Loading trip statistics...");
                var trips = await _billingService.GetTripsAsync();
                
                if (trips != null && trips.Any())
                {
                    TotalTrips = trips.Count;
                    
                    // Calculate CO2 saved
                    var totalMinutes = trips.Where(t => t.DurataMinuti > 0)
                                           .Sum(t => t.DurataMinuti);
                    var estimatedKm = (totalMinutes / 60.0m) * 18;
                    Co2Saved = Math.Round(estimatedKm * 0.12m, 1);
                    
                    // Get recent trips for activity feed
                    RecentTrips = trips.Where(t => t.Fine.HasValue)
                                      .OrderByDescending(t => t.Fine)
                                      .Take(5)
                                      .Select(t => new TripSummary
                                      {
                                          VehicleModel = t.Mezzo?.Modello ?? "Mezzo",
                                          Duration = t.DurataMinuti,
                                          Cost = t.CostoTotale,
                                          EndTime = t.Fine
                                      })
                                      .ToList();

                    _logger.LogInformation($"Loaded {TotalTrips} trips, {Co2Saved}kg CO2 saved");
                }
                else
                {
                    TotalTrips = 0;
                    Co2Saved = 0;
                    _logger.LogInformation("No trips found for user");
                }
                
                // Carica statistiche di crescita delle corse
                try
                {
                    var growthStats = await LoadPublicGrowthStats();
                    if (growthStats != null)
                    {
                        TripGrowth = GetDynamicValue(growthStats, "TripGrowth");
                        _logger.LogInformation($"Trip growth: {TripGrowth}%");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load trip growth stats");
                    TripGrowth = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trip stats");
                TotalTrips = 0;
                Co2Saved = 0;
            }
        }

        private async Task LoadUserStats()
        {
            try
            {
                _logger.LogInformation("Loading user statistics...");
                
                if (CurrentUser != null)
                {
                    // GetUserCreditAsync returns decimal (non-nullable), assign directly.
                    CurrentCredit = await _billingService.GetUserCreditAsync();
                    
                    try
                    {
                        var transactions = await _billingService.GetTransactionsAsync();
                        if (transactions?.Any() == true)
                        {
                            LastChargeDate = DateTime.Now.AddDays(-7);
                        }
                        else
                        {
                            LastChargeDate = null; // Non abbiamo transazioni
                        }
                    }
                    catch
                    {
                        LastChargeDate = null; // Errore nel caricamento
                    }

                    _logger.LogInformation($"User credit: €{CurrentCredit}");
                }
                
                // Carica statistiche di crescita degli utenti
                try
                {
                    var growthStats = await LoadPublicGrowthStats();
                    if (growthStats != null)
                    {
                        // UserGrowth è già definito nel modello ma non viene usato nella vista
                        // Potremmo aggiungerlo in futuro per mostrare la crescita utenti
                        _logger.LogInformation($"User growth: {GetDynamicValue(growthStats, "UserGrowth")}%");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load user growth stats");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user stats");
                CurrentCredit = 0; // Rimuovo il valore hardcoded
                LastChargeDate = null;
            }
        }

        private async Task LoadAdminStats()
        {
            try
            {
                _logger.LogInformation("Loading admin statistics...");

                // Carica statistiche da endpoint pubblico
                try
                {
                    var publicStats = await LoadPublicStats();
                    if (publicStats != null)
                    {
                        TotalUsers = publicStats.TotaleUtenti;
                        AvailableVehicles = publicStats.MezziDisponibili;
                        TotalTrips = publicStats.CorseAttive;
                        _logger.LogInformation("Loaded stats from public API");
                    }
                    else
                    {
                        // Fallback: prova con i servizi individuali SOLO se non abbiamo già dati
                        if (TotalUsers == 0)
                        {
                            var users = await _userService.GetAllUsersAsync();
                            TotalUsers = users?.Count ?? 0;
                        }
                    }
                    NewUsersThisWeek = 0; // Non abbiamo ancora un endpoint per nuovi utenti settimanali
                }
                catch
                {
                    // Non sovrascrivere se abbiamo già dati validi
                    if (TotalUsers == 0)
                    {
                        TotalUsers = 0;
                    }
                    if (NewUsersThisWeek == 0)
                    {
                        NewUsersThisWeek = 0;
                    }
                }

                // Carica statistiche di crescita
                try
                {
                    var growthStats = await LoadPublicGrowthStats();
                    if (growthStats != null)
                    {
                        VehicleGrowth = GetDynamicValue(growthStats, "VehicleGrowth");
                        TripGrowth = GetDynamicValue(growthStats, "TripGrowth");
                        RevenueGrowth = GetDynamicValue(growthStats, "RevenueGrowth");
                        _logger.LogInformation("Loaded growth stats from public API");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load growth stats, using defaults");
                    VehicleGrowth = 0;
                    TripGrowth = 0;
                    RevenueGrowth = 0;
                }

                // Carica ricavi totali
                try
                {
                    var transactions = await _billingService.GetTransactionsAsync();
                    TotalRevenue = transactions?.Sum(t => t.Amount) ?? 0;
                    _logger.LogInformation($"Total revenue: {TotalRevenue}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load revenue data");
                    TotalRevenue = 0;
                }

                // Carica mezzi in manutenzione
                try
                {
                    var vehicles = await _vehicleService.GetVehiclesAsync();
                    MaintenanceVehicles = vehicles?.Count(v => v.Stato == VehicleStatus.Manutenzione) ?? 0;
                    UrgentMaintenance = vehicles?.Count(v => v.Stato == VehicleStatus.Manutenzione) ?? 0; // Per ora tutti i mezzi in manutenzione sono considerati urgenti
                    _logger.LogInformation($"Maintenance vehicles: {MaintenanceVehicles}, Urgent: {UrgentMaintenance}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load maintenance data");
                    MaintenanceVehicles = 0;
                    UrgentMaintenance = 0;
                }

                // Placeholder per segnalazioni (da implementare quando avremo l'endpoint)
                OpenReports = 0;
                PendingReports = 0;

                // Carica attività di sistema
                await GenerateRealSystemActivities();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin stats, using fallback");
                LoadFallbackAdminStats();
            }
        }

        private void LoadFallbackAdminStats()
        {
            TotalUsers = 0;
            NewUsersThisWeek = 0;
            TotalRevenue = 0;
            MaintenanceVehicles = 0;
            UrgentMaintenance = 0;
            OpenReports = 0;
            PendingReports = 0;
            VehicleGrowth = 0;
            TripGrowth = 0;
            RevenueGrowth = 0;
            GenerateSystemActivities();
        }

        private async Task GenerateRealSystemActivities()
        {
            var activities = new List<SystemActivity>();

            try
            {
                // Carica attività reali da utenti
                var users = await _userService.GetAllUsersAsync();
                if (users?.Any() == true)
                {
                    var recentUsers = users.Where(u => u.DataRegistrazione >= DateTime.Now.AddHours(-24))
                                          .OrderByDescending(u => u.DataRegistrazione)
                                          .Take(2);
                    
                    foreach (var user in recentUsers)
                    {
                        activities.Add(new SystemActivity
                        {
                            Timestamp = user.DataRegistrazione,
                            Type = "Registrazione",
                            TypeColor = "primary",
                            UserName = "Admin",
                            Description = $"Nuovo utente: {user.Nome} {user.Cognome}",
                            Status = "Completato",
                            StatusColor = "success"
                        });
                    }
                }

                // Carica attività da veicoli in manutenzione
                var vehicles = await _vehicleService.GetVehiclesAsync();
                if (vehicles?.Any() == true)
                {
                    var maintenanceVehicles = vehicles.Where(v => v.Stato == VehicleStatus.Manutenzione).Take(2);
                    
                    foreach (var vehicle in maintenanceVehicles)
                    {
                        activities.Add(new SystemActivity
                        {
                            Timestamp = vehicle.UltimaManutenzione ?? DateTime.Now.AddHours(-2),
                            Type = "Manutenzione",
                            TypeColor = "warning",
                            UserName = "Sistema",
                            Description = $"Manutenzione - {vehicle.Modello} #{vehicle.Id}",
                            Status = "In corso",
                            StatusColor = "warning"
                        });
                    }
                }

                // Carica attività da transazioni recenti
                var transactions = await _billingService.GetTransactionsAsync();
                if (transactions?.Any() == true)
                {
                    var recentTransactions = transactions.Take(2);
                    
                    foreach (var transaction in recentTransactions)
                    {
                        var importo = transaction.GetType().GetProperty("Importo")?.GetValue(transaction)?.ToString() ?? "0";
                        var userName = transaction.GetType().GetProperty("UserName")?.GetValue(transaction)?.ToString() ?? "Utente";
                        
                        activities.Add(new SystemActivity
                        {
                            Timestamp = DateTime.Now.AddMinutes(-5),
                            Type = "Pagamento",
                            TypeColor = "success",
                            UserName = userName,
                            Description = $"Ricarica credito €{importo}",
                            Status = "Completato",
                            StatusColor = "success"
                        });
                    }
                }

                // Se non ci sono abbastanza attività reali, aggiungi alcune simulate
                if (activities.Count < 3)
                {
                    activities.Add(new SystemActivity
                    {
                        Timestamp = DateTime.Now.AddMinutes(-5),
                        Type = "Sistema",
                        TypeColor = "info",
                        UserName = "Sistema",
                        Description = "Sincronizzazione dati completata",
                        Status = "Completato",
                        StatusColor = "success"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating real system activities, using fallback");
                GenerateSystemActivities();
                return;
            }

            SystemActivities = activities.OrderByDescending(a => a.Timestamp).Take(5).ToList();
        }

        private void GenerateSystemActivities()
        {
            // Genera attività di sistema realistiche invece di hardcoded
            SystemActivities = new List<SystemActivity>
            {
                new SystemActivity
                {
                    Timestamp = DateTime.Now.AddMinutes(-5),
                    Type = "Sistema",
                    TypeColor = "info",
                    UserName = "Sistema",
                    Description = "Dashboard aggiornata",
                    Status = "Completato",
                    StatusColor = "success"
                }
            };
        }

        public async Task<IActionResult> OnGetRefreshStatsAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing dashboard stats via AJAX...");
                
                await Task.WhenAll(
                    LoadVehicleStats(),
                    LoadParkingStats(),
                    LoadTripStats(),
                    LoadUserStats()
                );

                if (IsAdmin)
                {
                    await LoadAdminStats();
                }

                var stats = new
                {
                    availableVehicles = AvailableVehicles,
                    totalTrips = TotalTrips,
                    currentCredit = CurrentCredit?.ToString("F2"),
                    co2Saved = Co2Saved,
                    availableParkings = AvailableParkings,
                    lastUpdated = DateTime.Now.ToString("HH:mm:ss"),
                    // Admin stats
                    totalUsers = IsAdmin ? TotalUsers : 0,
                    totalRevenue = IsAdmin ? TotalRevenue : 0,
                    maintenanceVehicles = IsAdmin ? MaintenanceVehicles : 0,
                    openReports = IsAdmin ? OpenReports : 0
                };

                return new JsonResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard stats");
                return new JsonResult(new { error = "Errore nel refresh" });
            }
        }
    }

    // Helper classes
    public class TripSummary
    {
        public string VehicleModel { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal Cost { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class SystemActivity
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeColor { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
    }
}