using Microsoft.AspNetCore.Mvc;
using SharingMezzi.Web.Services;
using Microsoft.Extensions.Logging;

namespace SharingMezzi.Web.Controllers
{
    [ApiController]
    // Expose routes under /api/user so client code that calls /api/user/profile and
    // /api/user/ricarica-credito is handled by this proxy.
    [Route("api/user")]
    public class UserProxyController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<UserProxyController> _logger;

        public UserProxyController(IApiService apiService, IAuthService authService, ILogger<UserProxyController> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var token = _authService.GetToken();
                _logger.LogDebug("UserProxy: proxying profile request; token present: {hasToken}, tokenLength: {len}", !string.IsNullOrEmpty(token), token?.Length ?? 0);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("UserProxy: missing token for profile request");
                    return Unauthorized(new { message = "Token mancante" });
                }

                var profile = await _apiService.GetAsync<object>("/api/user/profile", token);
                if (profile == null)
                {
                    return NotFound(new { message = "Profilo non trovato" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying profile request");
                return StatusCode(500, new { message = "Errore interno proxy" });
            }
        }

        [HttpPost("ricarica-credito")]
        public async Task<IActionResult> RicaricaCredito([FromBody] object ricaricaDto)
        {
            try
            {
                var token = _authService.GetToken();
                _logger.LogDebug("UserProxy: proxying recharge request; token present: {hasToken}, tokenLength: {len}", !string.IsNullOrEmpty(token), token?.Length ?? 0);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("UserProxy: missing token for recharge request");
                    return Unauthorized(new { message = "Token mancante" });
                }

                var result = await _apiService.PostAsync<object>("/api/user/ricarica-credito", ricaricaDto, token);
                if (result == null)
                {
                    return StatusCode(500, new { message = "Errore durante la ricarica" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying recharge request");
                return StatusCode(500, new { message = "Errore interno proxy" });
            }
        }

        [HttpGet("{userId}/ricariche")]
        public async Task<IActionResult> GetRicariche(int userId)
        {
            try
            {
                var token = _authService.GetToken();
                _logger.LogDebug("UserProxy: proxying GetRicariche for user {UserId}; token present: {hasToken}", userId, !string.IsNullOrEmpty(token));
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("UserProxy: missing token for GetRicariche request");
                    return Unauthorized(new { message = "Token mancante" });
                }

                var result = await _apiService.GetAsync<List<object>>($"/api/user/{userId}/ricariche", token);
                return Ok(result ?? new List<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying GetRicariche for user {UserId}", userId);
                return StatusCode(500, new { message = "Errore interno proxy" });
            }
        }

        [HttpGet("{userId}/pagamenti")]
        public async Task<IActionResult> GetPagamenti(int userId)
        {
            try
            {
                var token = _authService.GetToken();
                _logger.LogDebug("UserProxy: proxying GetPagamenti for user {UserId}; token present: {hasToken}", userId, !string.IsNullOrEmpty(token));
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("UserProxy: missing token for GetPagamenti request");
                    return Unauthorized(new { message = "Token mancante" });
                }

                var result = await _apiService.GetAsync<List<object>>($"/api/user/{userId}/pagamenti", token);
                return Ok(result ?? new List<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying GetPagamenti for user {UserId}", userId);
                return StatusCode(500, new { message = "Errore interno proxy" });
            }
        }
    }
}
