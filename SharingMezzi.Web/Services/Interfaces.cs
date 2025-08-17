using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Services
{
    // ===== AUTH SERVICE INTERFACE =====
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
        void LogoutAsync();
        string? GetToken();
        User? GetCurrentUser();
        Task<User?> GetCurrentUserAsync(); // METODO AGGIUNTO
        bool IsAuthenticated();
        void SetToken(string token);
        void SetCurrentUser(User user);
        void ClearSession();
    }

    // ===== API SERVICE INTERFACE =====
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? token = null);
        Task<T?> PostAsync<T>(string endpoint, object data, string? token = null);
        Task<T?> PutAsync<T>(string endpoint, object data, string? token = null);
        Task<bool> DeleteAsync(string endpoint, string? token = null);
        string GetBaseUrl();
    }

    // ===== VEHICLE SERVICE INTERFACE =====
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetVehiclesAsync();
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleAsync(int id);
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task<List<Vehicle>> GetAvailableVehiclesAsync();
        Task<bool> UnlockVehicleAsync(int vehicleId);
        Task<bool> ReportMaintenanceAsync(int vehicleId, string description);
    }

    // ===== PARKING SERVICE INTERFACE =====
    public interface IParkingService
    {
        Task<List<Parking>> GetParkingsAsync();
        Task<List<Parking>> GetAllParkingsAsync();
        Task<Parking?> GetParkingAsync(int id);
        Task<Parking?> GetParkingByIdAsync(int id);
        Task<List<Slot>> GetParkingSlotsAsync(int parkingId);
        Task<bool> ReserveParkingSlotAsync(int slotId);
    }

    // ===== USER SERVICE INTERFACE =====
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserAsync(int id);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateUserAsync(int id, User user);
        Task<bool> SuspendUserAsync(int id, string reason);
        Task<bool> ReactivateUserAsync(int id);
        Task<UserStatistics?> GetUserStatisticsAsync(int userId);
    }

    // ===== TRIP SERVICE INTERFACE =====
    public interface ITripService
    {
        Task<List<Trip>> GetTripsAsync();
        Task<List<Trip>> GetUserTripsAsync();
        Task<Trip?> GetTripByIdAsync(int id);
        Task<bool> EndTripAsync(int tripId, int destinationParkingId);
    }

    // ===== BILLING SERVICE INTERFACE =====
    public interface IBillingService
    {
        Task<List<Recharge>> GetRechargesAsync();
        Task<List<Recharge>> GetUserRechargesAsync(int userId);
        Task<bool> CreateRechargeAsync(RechargeRequest request);
        Task<List<Trip>> GetTripsAsync();
        Task<List<Trip>> GetUserTripsAsync(int userId);
        Task<decimal> GetUserBalanceAsync(int userId);
        Task<decimal> GetUserCreditAsync();
        Task<bool> RechargeAsync(decimal amount, string paymentMethod);
        Task<List<dynamic>?> GetTransactionsAsync();
    }
}