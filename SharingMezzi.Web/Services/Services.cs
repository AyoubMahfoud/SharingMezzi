using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Services
{
    // ===== VEHICLE SERVICE =====
    public class VehicleService : IVehicleService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(IApiService apiService, IAuthService authService, ILogger<VehicleService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<Vehicle>> GetVehiclesAsync()
        {
            try
        {
            var token = _authService.GetToken();
                var vehicles = await _apiService.GetAsync<List<dynamic>>("/api/mezzi", token);
                
                if (vehicles == null || !vehicles.Any())
                {
                    return new List<Vehicle>();
                }

                return vehicles.Select(v => new Vehicle
                {
                    Id = (int)v.GetType().GetProperty("Id")?.GetValue(v),
                    Modello = v.GetType().GetProperty("Modello")?.GetValue(v)?.ToString() ?? "",
                    Tipo = ParseVehicleType(v.GetType().GetProperty("Tipo")?.GetValue(v)),
                    IsElettrico = (bool)(v.GetType().GetProperty("IsElettrico")?.GetValue(v) ?? false),
                    Stato = ParseVehicleStatus(v.GetType().GetProperty("Stato")?.GetValue(v)),
                    LivelloBatteria = (int?)(v.GetType().GetProperty("LivelloBatteria")?.GetValue(v)),
                    TariffaPerMinuto = (decimal)(v.GetType().GetProperty("TariffaPerMinuto")?.GetValue(v) ?? 0),
                    TariffaFissa = (decimal)(v.GetType().GetProperty("TariffaFissa")?.GetValue(v) ?? 0),
                    ParcheggioId = (int?)(v.GetType().GetProperty("ParcheggioAttualeId")?.GetValue(v))
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles");
                return new List<Vehicle>();
            }
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await GetVehiclesAsync();
        }

        public async Task<Vehicle?> GetVehicleAsync(int id)
        {
            return await GetVehicleByIdAsync(id);
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            try
        {
            var token = _authService.GetToken();
                var vehicleDto = await _apiService.GetAsync<dynamic>($"/api/mezzi/{id}", token);
                
                if (vehicleDto == null) return null;

                return new Vehicle
                {
                    Id = (int)vehicleDto.GetType().GetProperty("Id")?.GetValue(vehicleDto),
                    Modello = vehicleDto.GetType().GetProperty("Modello")?.GetValue(vehicleDto)?.ToString() ?? "",
                    Tipo = ParseVehicleType(vehicleDto.GetType().GetProperty("Tipo")?.GetValue(vehicleDto)),
                    IsElettrico = (bool)(vehicleDto.GetType().GetProperty("IsElettrico")?.GetValue(vehicleDto) ?? false),
                    Stato = ParseVehicleStatus(vehicleDto.GetType().GetProperty("Stato")?.GetValue(vehicleDto)),
                    LivelloBatteria = (int?)(vehicleDto.GetType().GetProperty("LivelloBatteria")?.GetValue(vehicleDto)),
                    TariffaPerMinuto = (decimal)(vehicleDto.GetType().GetProperty("TariffaPerMinuto")?.GetValue(vehicleDto) ?? 0),
                    TariffaFissa = (decimal)(vehicleDto.GetType().GetProperty("TariffaFissa")?.GetValue(vehicleDto) ?? 0),
                    ParcheggioId = (int?)(vehicleDto.GetType().GetProperty("ParcheggioAttualeId")?.GetValue(vehicleDto))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle {VehicleId}", id);
                return null;
            }
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            try
            {
                _logger.LogInformation("🔍 DEBUG: Chiamata API per mezzi disponibili");
                
                // Ottieni il token di autenticazione
                var token = _authService.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ DEBUG: Token mancante, impossibile recuperare mezzi disponibili");
                    return new List<Vehicle>();
                }
                
                // Usa l'endpoint autenticato per i mezzi disponibili
                var vehicles = await _apiService.GetAsync<List<dynamic>>("/api/mezzi/disponibili", token);
                
                _logger.LogInformation($"📡 DEBUG: API response per mezzi disponibili: {vehicles?.Count ?? 0} mezzi");
                
                if (vehicles == null || !vehicles.Any())
                {
                    _logger.LogWarning("⚠️ DEBUG: Nessun mezzo disponibile dall'API");
                    return new List<Vehicle>();
                }

                var result = vehicles.Select(v => new Vehicle
                {
                    Id = (int)v.GetType().GetProperty("Id")?.GetValue(v),
                    Modello = v.GetType().GetProperty("Modello")?.GetValue(v)?.ToString() ?? "",
                    Tipo = ParseVehicleType(v.GetType().GetProperty("Tipo")?.GetValue(v)),
                    IsElettrico = (bool)(v.GetType().GetProperty("IsElettrico")?.GetValue(v) ?? false),
                    Stato = ParseVehicleStatus(v.GetType().GetProperty("Stato")?.GetValue(v)),
                    LivelloBatteria = (int?)(v.GetType().GetProperty("LivelloBatteria")?.GetValue(v)),
                    TariffaPerMinuto = (decimal)(v.GetType().GetProperty("TariffaPerMinuto")?.GetValue(v) ?? 0),
                    TariffaFissa = (decimal)(v.GetType().GetProperty("TariffaFissa")?.GetValue(v) ?? 0),
                    ParcheggioId = (int?)(v.GetType().GetProperty("ParcheggioAttualeId")?.GetValue(v))
                }).ToList();
                
                _logger.LogInformation($"✅ DEBUG: Convertiti {result.Count} mezzi dal formato API");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DEBUG: Errore nel recupero mezzi disponibili: {Error}", ex.Message);
                return new List<Vehicle>();
            }
        }

        public async Task<bool> UnlockVehicleAsync(int vehicleId)
        {
            try
        {
            var token = _authService.GetToken();
                var response = await _apiService.PostAsync<object>($"/api/mezzi/{vehicleId}/unlock", new { }, token);
            return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        public async Task<bool> ReportMaintenanceAsync(int vehicleId, string description)
        {
            try
            {
                var token = _authService.GetToken();
                var response = await _apiService.PostAsync<object>($"/api/admin/vehicles/{vehicleId}/repair", new { }, token);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting maintenance for vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        public async Task<bool> SetMaintenanceAsync(int vehicleId)
        {
            try
        {
            var token = _authService.GetToken();
                var response = await _apiService.PostAsync<object>($"/admin/vehicles/{vehicleId}/maintenance", new { }, token);
            return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting vehicle {VehicleId} to maintenance", vehicleId);
                return false;
            }
        }

        public async Task<bool> SetAvailableAsync(int vehicleId)
        {
            try
            {
                var token = _authService.GetToken();
                var response = await _apiService.PostAsync<object>($"/admin/vehicles/{vehicleId}/repair", new { }, token);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting vehicle {VehicleId} to available", vehicleId);
                return false;
            }
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            try
            {
                var token = _authService.GetToken();
                var response = await _apiService.DeleteAsync($"/mezzi/{vehicleId}", token);
                return true; // Se non ci sono eccezioni, consideriamo l'operazione riuscita
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        private static VehicleType ParseVehicleType(object typeValue)
        {
            if (typeValue == null) return VehicleType.Bicicletta;
            
            // Se è già un numero, convertilo direttamente
            if (int.TryParse(typeValue.ToString(), out int numericType))
            {
                return (VehicleType)numericType;
            }
            
            // Se è una stringa, parsala
            var typeString = typeValue.ToString()?.ToLower();
            return typeString switch
            {
                "bicicletta" => VehicleType.Bicicletta,
                "scooter" => VehicleType.Scooter,
                "auto" => VehicleType.Auto,
                "monopattino" => VehicleType.Monopattino,
                "ebike" => VehicleType.EBike,
                _ => VehicleType.Bicicletta
            };
        }

        private static VehicleStatus ParseVehicleStatus(object statusValue)
        {
            if (statusValue == null) return VehicleStatus.Disponibile;
            
            // Se è già un numero, convertilo direttamente
            if (int.TryParse(statusValue.ToString(), out int numericStatus))
            {
                return (VehicleStatus)numericStatus;
            }
            
            // Se è una stringa, parsala
            var statusString = statusValue.ToString()?.ToLower();
            return statusString switch
            {
                "disponibile" => VehicleStatus.Disponibile,
                "inuso" or "occupato" => VehicleStatus.InUso,
                "manutenzione" => VehicleStatus.Manutenzione,
                "fuori_servizio" or "fuoriservizio" => VehicleStatus.Fuori_Servizio,
                _ => VehicleStatus.Disponibile
            };
        }
    }

    // ===== USER SERVICE =====
    public class UserService : IUserService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<UserService> _logger;

        public UserService(IApiService apiService, IAuthService authService, ILogger<UserService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await GetAllUsersAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var token = _authService.GetToken();
                var users = await _apiService.GetAsync<List<UserDto>>("/admin/users", token);
                
                if (users == null || !users.Any())
                {
                    return new List<User>();
                }

                return users.Select(dto => new User
                {
                    Id = dto.Id,
                    Nome = dto.Nome,
                    Cognome = dto.Cognome,
                    Email = dto.Email,
                    Telefono = dto.Telefono,
                    Ruolo = ParseUserRole(dto.Ruolo),
                    Credito = dto.Credito,
                    PuntiEco = dto.PuntiEco,
                    Stato = ParseUserStatus(dto.Stato),
                    DataRegistrazione = dto.DataRegistrazione,
                    DataSospensione = dto.DataSospensione,
                    MotivoSospensione = dto.MotivoSospensione
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<User>();
            }
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                var token = _authService.GetToken();
                var userDto = await _apiService.GetAsync<UserDto>("/user/profile", token);
                
                if (userDto == null) return null;

                return new User
                {
                    Id = userDto.Id,
                    Nome = userDto.Nome,
                    Cognome = userDto.Cognome,
                    Email = userDto.Email,
                    Telefono = userDto.Telefono,
                    Ruolo = ParseUserRole(userDto.Ruolo),
                    Credito = userDto.Credito,
                    PuntiEco = userDto.PuntiEco,
                    Stato = ParseUserStatus(userDto.Stato),
                    DataRegistrazione = userDto.DataRegistrazione,
                    DataSospensione = userDto.DataSospensione,
                    MotivoSospensione = userDto.MotivoSospensione
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return null;
            }
        }

        public async Task<User?> CreateUserAsync(User user, string password)
        {
            try
            {
                // Implementa creazione utente se necessario
                _logger.LogInformation("Create user functionality not implemented yet");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await UpdateUserAsync(user.Id, user);
        }

        public async Task<bool> UpdateUserAsync(int id, User user)
        {
            try
        {
            var token = _authService.GetToken();
                var updateRequest = new
                {
                    user.Nome,
                    user.Cognome,
                    user.Email,
                    user.Telefono,
                    user.Ruolo
                };

                var response = await _apiService.PutAsync<object>($"/api/admin/users/{id}", updateRequest, token);
            return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return false;
            }
        }

        public async Task<bool> SuspendUserAsync(int id, string reason)
        {
            try
        {
            var token = _authService.GetToken();
                var suspendRequest = new { Motivo = reason };
                var response = await _apiService.PostAsync<object>($"/api/admin/users/{id}/suspend", suspendRequest, token);
            return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending user {UserId}", id);
                return false;
            }
        }

        public async Task<bool> UnblockUserAsync(int id)
        {
            try
        {
            var token = _authService.GetToken();
                var unblockRequest = new { Note = "Riattivato dall'amministratore" };
                var response = await _apiService.PostAsync<object>($"/api/admin/users/{id}/unblock", unblockRequest, token);
            return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking user {UserId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                // Per ora simuliamo la cancellazione sospendendo l'utente
                return await SuspendUserAsync(id, "Utente cancellato dall'amministratore");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return false;
            }
        }

        public async Task<UserStatistics?> GetUserStatisticsAsync(int userId)
        {
            try
            {
                var token = _authService.GetToken();
                var stats = await _apiService.GetAsync<UserStatisticsDto>($"/api/user/{userId}/statistiche", token);
                
                if (stats == null) return null;

                return new UserStatistics
                {
                    TotalTrips = stats.TotaleCorse,
                    CompletedTrips = stats.CorseCompletate,
                    TotalSpent = stats.SpesaTotale,
                    CurrentCredit = stats.CreditoAttuale,
                    EcoPoints = stats.PuntiEcoTotali,
                    TotalMinutes = stats.MinutiTotali,
                    FavoriteVehicle = stats.MezzoPreferito,
                    LastTrip = stats.UltimaCorsa
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ReactivateUserAsync(int id)
        {
            try
        {
            var token = _authService.GetToken();
                var unblockRequest = new { Note = "Riattivato dall'amministratore" };
                var response = await _apiService.PostAsync<object>($"/api/admin/users/{id}/unblock", unblockRequest, token);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", id);
                return false;
            }
        }

        private static UserRole ParseUserRole(string roleString)
        {
            return roleString?.ToLower() switch
            {
                "admin" or "amministratore" => UserRole.Admin,
                _ => UserRole.Utente
            };
        }

        private static UserStatus ParseUserStatus(string statusString)
        {
            return statusString?.ToLower() switch
            {
                "attivo" => UserStatus.Attivo,
                "sospeso" => UserStatus.Sospeso,
                "cancellato" => UserStatus.Cancellato,
                _ => UserStatus.Attivo
            };
        }
    }

    // ===== BILLING SERVICE =====
    public class BillingService : IBillingService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<BillingService> _logger;

        public BillingService(IApiService apiService, IAuthService authService, ILogger<BillingService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<Recharge>> GetRechargesAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null) return new List<Recharge>();

                return await GetUserRechargesAsync(currentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recharges");
                return new List<Recharge>();
            }
        }

        public async Task<List<Recharge>> GetUserRechargesAsync(int userId)
        {
            try
        {
            var token = _authService.GetToken();
                var recharges = await _apiService.GetAsync<List<RechargeDto>>($"/api/user/{userId}/ricariche", token);
                
                if (recharges == null || !recharges.Any())
                {
                    return new List<Recharge>();
                }

                return recharges.Select(dto => new Recharge
                {
                    Id = dto.Id,
                    UtenteId = dto.UtenteId,
                    Importo = dto.Importo,
                    MetodoPagamento = ParsePaymentMethod(dto.MetodoPagamento),
                    StatoPagamento = ParsePaymentStatus(dto.Stato),
                    DataRicarica = dto.DataRicarica,
                    TransactionId = dto.TransactionId,
                    SaldoFinale = dto.SaldoFinale
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user recharges {UserId}", userId);
                return new List<Recharge>();
            }
        }

        public async Task<bool> CreateRechargeAsync(RechargeRequest request)
        {
            try
            {
                var token = _authService.GetToken();
                var rechargeRequest = new
                {
                    UtenteId = request.UserId,
                    Importo = request.Amount,
                    MetodoPagamento = request.PaymentMethodString
                };

                var response = await _apiService.PostAsync<object>("/user/ricarica-credito", rechargeRequest, token);
            return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recharge");
                return false;
            }
        }

        public async Task<List<Trip>> GetTripsAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null) return new List<Trip>();

                return await GetUserTripsAsync(currentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trips");
                return new List<Trip>();
            }
        }

        public async Task<List<Trip>> GetUserTripsAsync(int userId)
        {
            try
        {
            var token = _authService.GetToken();
                var trips = await _apiService.GetAsync<List<TripDto>>($"/api/corse/utente/{userId}", token);
                
                if (trips == null || !trips.Any())
                {
                    return new List<Trip>();
                }

                return trips.Select(dto => new Trip
                {
                    Id = dto.Id,
                    UserId = dto.UtenteId,
                    VehicleId = dto.MezzoId,
                    StartParkingId = dto.ParcheggioPartenzaId,
                    EndParkingId = dto.ParcheggioDestinazioneId,
                    Inizio = dto.Inizio,
                    Fine = dto.Fine,
                    DurataMinuti = dto.DurataMinuti,
                    CostoTotale = dto.CostoTotale,
                    Stato = ParseTripStatus(dto.Stato)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user trips {UserId}", userId);
                return new List<Trip>();
            }
        }

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                return user?.Credito ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user balance {UserId}", userId);
                return 0;
            }
        }

        public async Task<decimal> GetUserCreditAsync()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                return user?.Credito ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user credit");
                return 0;
            }
        }

        public async Task<bool> RechargeAsync(decimal amount, string paymentMethod)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null) return false;

                var request = new RechargeRequest 
                { 
                    UserId = currentUser.Id,
                    Amount = amount,
                    PaymentMethodString = paymentMethod
                };

                return await CreateRechargeAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recharging account");
                return false;
            }
        }

        public async Task<List<dynamic>?> GetTransactionsAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null) return null;

                var token = _authService.GetToken();
                return await _apiService.GetAsync<List<dynamic>>($"/user/{currentUser.Id}/pagamenti", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions");
                return null;
            }
        }

        private static PaymentMethod ParsePaymentMethod(string methodString)
        {
            return methodString?.ToLower() switch
            {
                "cartacredito" or "carta_credito" => PaymentMethod.CartaCredito,
                "paypal" => PaymentMethod.PayPal,
                "bonifico" => PaymentMethod.Bonifico,
                "creditowallet" or "credito_wallet" => PaymentMethod.CreditoWallet,
                _ => PaymentMethod.CartaCredito
            };
        }

        private static PaymentStatus ParsePaymentStatus(string statusString)
        {
            return statusString?.ToLower() switch
            {
                "completato" => PaymentStatus.Completato,
                "inattesa" or "in_attesa" => PaymentStatus.InAttesa,
                "fallito" => PaymentStatus.Fallito,
                "annullato" => PaymentStatus.Annullato,
                _ => PaymentStatus.InAttesa
            };
        }

        private static TripStatus ParseTripStatus(string statusString)
        {
            return statusString?.ToLower() switch
            {
                "incorso" or "in_corso" => TripStatus.InCorso,
                "completata" => TripStatus.Completata,
                "annullata" => TripStatus.Annullata,
                _ => TripStatus.InCorso
            };
        }
    }

    // ===== DTO CLASSES =====
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Modello { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool IsElettrico { get; set; }
        public string Stato { get; set; } = string.Empty;
        public int? LivelloBatteria { get; set; }
        public decimal TariffaPerMinuto { get; set; }
        public decimal TariffaFissa { get; set; }
        public DateTime? UltimaManutenzione { get; set; }
        public int? ParcheggioId { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Ruolo { get; set; } = string.Empty;
        public DateTime DataRegistrazione { get; set; }
        public decimal Credito { get; set; }
        public int PuntiEco { get; set; }
        public string Stato { get; set; } = "Attivo";
        public DateTime? DataSospensione { get; set; }
        public string? MotivoSospensione { get; set; }
    }

    public class RechargeDto
    {
        public int Id { get; set; }
        public int UtenteId { get; set; }
        public decimal Importo { get; set; }
        public string MetodoPagamento { get; set; } = string.Empty;
        public DateTime DataRicarica { get; set; }
        public string? TransactionId { get; set; }
        public string Stato { get; set; } = string.Empty;
        public decimal SaldoFinale { get; set; }
    }

    public class TripDto
    {
        public int Id { get; set; }
        public int UtenteId { get; set; }
        public int MezzoId { get; set; }
        public int ParcheggioPartenzaId { get; set; }
        public int? ParcheggioDestinazioneId { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime? Fine { get; set; }
        public int DurataMinuti { get; set; }
        public decimal CostoTotale { get; set; }
        public string Stato { get; set; } = string.Empty;
    }

    public class UserStatisticsDto
    {
        public int TotaleCorse { get; set; }
        public int CorseCompletate { get; set; }
        public decimal SpesaTotale { get; set; }
        public decimal CreditoAttuale { get; set; }
        public int PuntiEcoTotali { get; set; }
        public int MinutiTotali { get; set; }
        public string MezzoPreferito { get; set; } = string.Empty;
        public DateTime? UltimaCorsa { get; set; }
    }
}