using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IBillingService _billingService;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            IUserService userService,
            IAuthService authService,
            IBillingService billingService,
            ILogger<ProfileModel> logger)
        {
            _userService = userService;
            _authService = authService;
            _billingService = billingService;
            _logger = logger;
        }

        public new User User { get; set; } = new();
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int EcoPoints { get; set; }
        public int TotalTrips { get; set; }
        public int TotalDistance { get; set; }
        public int TotalTime { get; set; }
        public decimal TotalSpent { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading user profile page");

                // Get current user
                User = await _authService.GetCurrentUserAsync() ?? new User();
                
                if (User.Id == 0)
                {
                    return RedirectToPage("/Login");
                }

                // Load additional profile data
                await LoadProfileData();

                _logger.LogInformation("User profile loaded for user {UserId}", User.Id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["Error"] = "Errore nel caricamento del profilo. Riprova più tardi.";
                return Page();
            }
        }

        private async Task LoadProfileData()
        {
            try
            {
                // Load user statistics
                var userStats = await _userService.GetUserStatisticsAsync(User.Id);
                if (userStats != null)
                {
                    EcoPoints = userStats.EcoPoints;
                    TotalTrips = userStats.TotalTrips;
                    TotalSpent = userStats.TotalSpent;
                }

                // Load user trips for additional statistics
                var trips = await _billingService.GetUserTripsAsync(User.Id);
                TotalTrips = trips.Count;
                
                // Calculate total distance and time from trips
                var completedTrips = trips.Where(t => t.Stato == TripStatus.Completata && t.DataFine.HasValue);
                TotalDistance = (int)completedTrips.Sum(t => t.DistanzaPercorsa ?? 0);
                TotalTime = (int)completedTrips.Sum(t => t.DataFine.HasValue ? (t.DataFine.Value - t.DataInizio).TotalMinutes : 0);

                // Mock data for phone and address (these would come from a more complete user profile)
                Phone = User.Telefono ?? "+39 123 456 7890";
                Address = "Via Roma, 123 - Milano";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile data for user {UserId}", User.Id);
                // Set default values
                EcoPoints = 0;
                TotalTrips = 0;
                TotalDistance = 0;
                TotalTime = 0;
                TotalSpent = 0;
                Phone = string.Empty;
                Address = string.Empty;
            }
        }

        public async Task<IActionResult> OnPutUpdateProfileAsync([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Update user information
                currentUser.Nome = request.FirstName;
                currentUser.Cognome = request.LastName;
                currentUser.Telefono = request.Phone;

                var updated = await _userService.UpdateUserAsync(currentUser);
                
                if (updated)
                {
                    _logger.LogInformation("User profile updated for user {UserId}", currentUser.Id);
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return BadRequest(new { message = "Errore durante l'aggiornamento del profilo" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return BadRequest(new { message = "Errore durante l'aggiornamento del profilo" });
            }
        }

        public async Task<IActionResult> OnPutChangePasswordAsync([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Here you would typically verify the current password and update to the new one
                // For this example, we'll just return success
                // In a real implementation, you would call an API endpoint to change the password

                _logger.LogInformation("Password change requested for user {UserId}", currentUser.Id);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return BadRequest(new { message = "Errore durante il cambio password" });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
