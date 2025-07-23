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
            var token = _authService.GetToken();
            var vehicles = await _apiService.GetAsync<List<Vehicle>>("/api/mezzi/disponibili", token);
            return vehicles ?? new List<Vehicle>();
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
    }

    public class ParkingService : IParkingService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public ParkingService(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public async Task<List<Parking>> GetParkingsAsync()
        {
            var token = _authService.GetToken();
            var parkings = await _apiService.GetAsync<List<Parking>>("/api/parcheggi", token);
            return parkings ?? new List<Parking>();
        }

        public async Task<Parking?> GetParkingAsync(int id)
        {
            var token = _authService.GetToken();
            return await _apiService.GetAsync<Parking>($"/api/parcheggi/{id}", token);
        }

        public async Task<List<Slot>> GetParkingSlotsAsync(int parkingId)
        {
            var token = _authService.GetToken();
            var slots = await _apiService.GetAsync<List<Slot>>($"/api/parcheggi/{parkingId}/slots", token);
            return slots ?? new List<Slot>();
        }

        public async Task<bool> ReserveParkingSlotAsync(int slotId)
        {
            var token = _authService.GetToken();
            var response = await _apiService.PostAsync<object>($"/api/slots/{slotId}/prenota", new { }, token);
            return response != null;
        }
    }

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
            var recharges = await _apiService.GetAsync<List<Recharge>>($"/api/utenti/{userId}/ricariche", token);
            return recharges ?? new List<Recharge>();
        }

        public async Task<bool> CreateRechargeAsync(RechargeRequest request)
        {
            var token = _authService.GetToken();
            var response = await _apiService.PostAsync<object>("/api/ricariche", request, token);
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
            var response = await _apiService.GetAsync<decimal>($"/api/utenti/{userId}/saldo", token);
            return response;
        }
    }
}
