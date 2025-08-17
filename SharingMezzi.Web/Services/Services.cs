using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public VehicleService(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public async Task<List<Vehicle>> GetVehiclesAsync()
        {
            var token = _authService.GetToken();
            var vehicles = await _apiService.GetAsync<List<Vehicle>>("/api/mezzi", token);
            return vehicles ?? new List<Vehicle>();
        }

        public async Task<Vehicle?> GetVehicleAsync(int id)
        {
            var token = _authService.GetToken();
            return await _apiService.GetAsync<Vehicle>($"/api/mezzi/{id}", token);
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            Console.WriteLine("🔍 VehicleService.GetAvailableVehiclesAsync() chiamato");
            var token = _authService.GetToken();
            Console.WriteLine($"🔑 Token presente: {!string.IsNullOrEmpty(token)}");
            
            try
            {
                // Prima prova con endpoint pubblico
                var vehicles = await _apiService.GetAsync<List<Vehicle>>("/api/public/mezzi/disponibili");
                Console.WriteLine($"📊 API response ricevuta - mezzi disponibili: {vehicles?.Count ?? 0}");
                
                if (vehicles != null && vehicles.Count > 0)
                {
                    Console.WriteLine("🚲 Mezzi trovati:");
                    foreach (var vehicle in vehicles)
                    {
                        Console.WriteLine($"   - ID: {vehicle.Id}, Modello: {vehicle.Modello}, Stato: {vehicle.Stato}");
                    }
                    return vehicles;
                }
                
                // Se l'endpoint pubblico non funziona, prova con quello autenticato
                Console.WriteLine("🔄 Tentativo con endpoint autenticato...");
                vehicles = await _apiService.GetAsync<List<Vehicle>>("/api/mezzi/disponibili", token);
                
                if (vehicles != null && vehicles.Count > 0)
                {
                    Console.WriteLine($"✅ Mezzi caricati da endpoint autenticato: {vehicles.Count}");
                    return vehicles;
                }
                
                Console.WriteLine("❌ Nessun mezzo disponibile restituito dall'API");
                return new List<Vehicle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore in GetAvailableVehiclesAsync: {ex.Message}");
                return new List<Vehicle>();
            }
        }

        public async Task<bool> UnlockVehicleAsync(int vehicleId)
        {
            var token = _authService.GetToken();
            var response = await _apiService.PostAsync<object>($"/api/mezzi/{vehicleId}/sblocca", new { }, token);
            return response != null;
        }

        public async Task<bool> ReportMaintenanceAsync(int vehicleId, string description)
        {
            var token = _authService.GetToken();
            var request = new { MezzoId = vehicleId, Descrizione = description };
            var response = await _apiService.PostAsync<object>("/api/segnalazioni", request, token);
            return response != null;
        }

        // ===== METODI ALIAS PER COMPATIBILITÀ =====
        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await GetVehiclesAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await GetVehicleAsync(id);
        }
    }

    // ParkingService implementation intentionally kept in ParkingService.cs
    // Duplicate trimmed to avoid type/interface redefinition conflicts.

    public class UserService : IUserService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public UserService(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var token = _authService.GetToken();
            var users = await _apiService.GetAsync<List<User>>("/api/utenti", token);
            return users ?? new List<User>();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await GetUsersAsync();
        }

        public async Task<User?> GetUserAsync(int id)
        {
            var token = _authService.GetToken();
            return await _apiService.GetAsync<User>($"/api/utenti/{id}", token);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await GetUserAsync(id);
        }

        public async Task<User?> CreateUserAsync(User user, string password)
        {
            var token = _authService.GetToken();
            var request = new
            {
                Nome = user.Nome,
                Cognome = user.Cognome,
                Email = user.Email,
                Password = password,
                Ruolo = user.Ruolo,
                Credito = user.Credito
            };
            return await _apiService.PostAsync<User>("/api/utenti", request, token);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var token = _authService.GetToken();
            var response = await _apiService.PutAsync<User>($"/api/utenti/{user.Id}", user, token);
            return response != null;
        }

        public async Task<bool> UpdateUserAsync(int id, User user)
        {
            var token = _authService.GetToken();
            var response = await _apiService.PutAsync<User>($"/api/utenti/{id}", user, token);
            return response != null;
        }

        public async Task<bool> SuspendUserAsync(int id, string reason)
        {
            var token = _authService.GetToken();
            var request = new { Motivo = reason };
            var response = await _apiService.PostAsync<object>($"/api/utenti/{id}/sospendi", request, token);
            return response != null;
        }

        public async Task<bool> ReactivateUserAsync(int id)
        {
            var token = _authService.GetToken();
            var response = await _apiService.PostAsync<object>($"/api/utenti/{id}/attiva", new { }, token);
            return response != null;
        }

        public async Task<UserStatistics?> GetUserStatisticsAsync(int userId)
        {
            var token = _authService.GetToken();
            return await _apiService.GetAsync<UserStatistics>($"/api/utenti/{userId}/statistiche", token);
        }
    }

    public class BillingService : IBillingService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public BillingService(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public async Task<List<Recharge>> GetRechargesAsync()
        {
            var token = _authService.GetToken();
            var recharges = await _apiService.GetAsync<List<Recharge>>("/api/ricariche", token);
            return recharges ?? new List<Recharge>();
        }

        public async Task<List<Recharge>> GetUserRechargesAsync(int userId)
        {
            var token = _authService.GetToken();
            // API exposes user recharges under /api/user/{userId}/ricariche
            var recharges = await _apiService.GetAsync<List<Recharge>>($"/api/user/{userId}/ricariche", token);
            return recharges ?? new List<Recharge>();
        }

        public async Task<bool> CreateRechargeAsync(RechargeRequest request)
        {
            var token = _authService.GetToken();
            // Backend expects ricarica credito on UserController: POST /api/user/ricarica-credito
            // Obtain current user id from profile endpoint (the API requires UtenteId in the payload)
            int userId = 0;
            try
            {
                var profile = await _apiService.GetAsync<User>("/api/user/profile", token);
                userId = profile?.Id ?? 0;
            }
            catch
            {
                // ignore - userId will remain 0 and API will return NotFound
            }

            var payload = new
            {
                UtenteId = userId,
                Importo = request.Importo,
                MetodoPagamento = request.MetodoPagamento.ToString()
            };

            var response = await _apiService.PostAsync<object>("/api/user/ricarica-credito", payload, token);
            return response != null;
        }

        public async Task<List<Trip>> GetTripsAsync()
        {
            var token = _authService.GetToken();
            var trips = await _apiService.GetAsync<List<Trip>>("/api/corse", token);
            return trips ?? new List<Trip>();
        }

        public async Task<List<Trip>> GetUserTripsAsync(int userId)
        {
            var token = _authService.GetToken();
            var trips = await _apiService.GetAsync<List<Trip>>($"/api/utenti/{userId}/corse", token);
            return trips ?? new List<Trip>();
        }

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            var token = _authService.GetToken();
            // There is no dedicated /saldo endpoint in the API; use profile or statistics to get current credit
            try
            {
                var profile = await _apiService.GetAsync<User>("/api/user/profile", token);
                return profile?.Credito ?? 0m;
            }
            catch
            {
                return 0m;
            }
        }

        // ===== METODI AGGIUNTIVI PER COMPATIBILITÀ =====
        public async Task<decimal> GetUserCreditAsync()
        {
            try
            {
                var token = _authService.GetToken();
                var profile = await _apiService.GetAsync<User>("/api/user/profile", token);
                return profile?.Credito ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore nel recupero credito: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> RechargeAsync(decimal amount, string paymentMethod)
        {
            try
            {
                // CORRETTO: Converti string in PaymentMethod enum
                PaymentMethod paymentMethodEnum;
                if (!Enum.TryParse<PaymentMethod>(paymentMethod, out paymentMethodEnum))
                {
                    // Default fallback
                    paymentMethodEnum = PaymentMethod.CartaCredito;
                }

                var request = new RechargeRequest 
                { 
                    Importo = amount, 
                    MetodoPagamento = paymentMethodEnum  // Ora è del tipo corretto
                };
                return await CreateRechargeAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore nella ricarica: {ex.Message}");
                return false;
            }
        }

        public async Task<List<dynamic>?> GetTransactionsAsync()
        {
            try
            {
                var token = _authService.GetToken();
                return await _apiService.GetAsync<List<dynamic>>("/api/user/transazioni", token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore nel recupero transazioni: {ex.Message}");
                return null;
            }
        }
    }

    public class TripService : ITripService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public TripService(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public async Task<List<Trip>> GetTripsAsync()
        {
            var token = _authService.GetToken();
            var trips = await _apiService.GetAsync<List<Trip>>("/api/corse/storico", token);
            return trips ?? new List<Trip>();
        }

        public async Task<List<Trip>> GetUserTripsAsync()
        {
            return await GetTripsAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
            var token = _authService.GetToken();
            return await _apiService.GetAsync<Trip>($"/api/corse/{id}", token);
        }

        public async Task<bool> EndTripAsync(int tripId, int destinationParkingId)
        {
            try
            {
                var token = _authService.GetToken();
                var result = await _apiService.PutAsync<dynamic>($"/api/corse/{tripId}/termina", 
                    new { ParcheggioDestinazioneId = destinationParkingId }, token);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore nella terminazione corsa: {ex.Message}");
                return false;
            }
        }
    }
}