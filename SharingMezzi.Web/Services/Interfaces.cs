using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? token = null);
        Task<T?> PostAsync<T>(string endpoint, object data, string? token = null);
        Task<T?> PutAsync<T>(string endpoint, object data, string? token = null);
        Task<bool> DeleteAsync(string endpoint, string? token = null);
        string GetBaseUrl();
    }

    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<bool> LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        string? GetToken();
        void SetToken(string token);
        void ClearToken();
    }

    public interface IVehicleService
    {
        Task<List<Vehicle>> GetVehiclesAsync();
        Task<Vehicle?> GetVehicleAsync(int id);
        Task<List<Vehicle>> GetAvailableVehiclesAsync();
        Task<bool> UnlockVehicleAsync(int vehicleId);
        Task<bool> ReportMaintenanceAsync(int vehicleId, string description);
    }

    public interface IParkingService
    {
        Task<List<Parking>> GetParkingsAsync();
        Task<Parking?> GetParkingAsync(int id);
        Task<List<Slot>> GetParkingSlotsAsync(int parkingId);
        Task<bool> ReserveParkingSlotAsync(int slotId);
    }

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

    public interface IBillingService
    {
        Task<List<Recharge>> GetRechargesAsync();
        Task<List<Recharge>> GetUserRechargesAsync(int userId);
        Task<bool> CreateRechargeAsync(RechargeRequest request);
        Task<List<Trip>> GetTripsAsync();
        Task<List<Trip>> GetUserTripsAsync(int userId);
        Task<decimal> GetUserBalanceAsync(int userId);
    }
}
