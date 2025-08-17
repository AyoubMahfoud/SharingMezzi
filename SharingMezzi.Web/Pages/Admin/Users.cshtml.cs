using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Pages.Admin
{
    [Authorize(Roles = "Admin,Amministratore")]
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(IUserService userService, ILogger<UsersModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public List<User> Users { get; set; } = new();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int NewUsersToday { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading admin users page");

                // Load all users
                Users = await _userService.GetAllUsersAsync();

                // Calculate statistics
                TotalUsers = Users.Count;
                ActiveUsers = Users.Count(u => u.IsActive);
                AdminUsers = Users.Count(u => u.Role == "Admin");
                NewUsersToday = Users.Count(u => u.CreatedAt.Date == DateTime.Today);

                _logger.LogInformation("Loaded {Count} users for admin panel", Users.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for admin panel");
                TempData["Error"] = "Errore nel caricamento degli utenti. Riprova più tardi.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateUserAsync([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Role = request.Role,
                    IsActive = true,
                    Balance = request.Balance,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUserAsync(user, request.Password);
                
                _logger.LogInformation("Admin created new user: {Email}", request.Email);
                return new JsonResult(new { success = true, user = createdUser });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from admin panel");
                return BadRequest(new { message = "Errore durante la creazione dell'utente" });
            }
        }

        public async Task<IActionResult> OnPutToggleUserAsync(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsActive = !user.IsActive;
                await _userService.UpdateUserAsync(user);

                _logger.LogInformation("Admin toggled user {UserId} status to {Status}", userId, user.IsActive);
                return new JsonResult(new { success = true, isActive = user.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                return BadRequest(new { message = "Errore durante l'operazione" });
            }
        }

        public async Task<IActionResult> OnGetUserDetailsAsync(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                // Get additional user statistics
                var userStats = await _userService.GetUserStatisticsAsync(userId);
                
                var userDetails = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Role,
                    user.IsActive,
                    user.Balance,
                    user.CreatedAt,
                    user.TotalTrips,
                    LastLogin = userStats?.LastLogin,
                    TotalSpent = userStats?.TotalSpent ?? 0
                };

                return new JsonResult(userDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details");
                return BadRequest(new { message = "Errore nel caricamento dei dettagli utente" });
            }
        }
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public decimal Balance { get; set; } = 0;
    }
}
