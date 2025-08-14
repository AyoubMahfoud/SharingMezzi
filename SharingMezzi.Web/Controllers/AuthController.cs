using Microsoft.AspNetCore.Mvc;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("SetSession")]
        public async Task<IActionResult> SetSession([FromBody] SetSessionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new { success = false, message = "Token is required" });
                }

                // Store token in session
                _authService.SetToken(request.Token);
                
                _logger.LogInformation("Token stored in session successfully");
                
                return Ok(new { success = true, message = "Session set successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting session");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    public class SetSessionRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
