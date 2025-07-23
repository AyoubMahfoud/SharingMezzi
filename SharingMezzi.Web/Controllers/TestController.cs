using Microsoft.AspNetCore.Mvc;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public TestController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                message = "Il frontend funziona correttamente",
                timestamp = DateTime.Now,
                apiBaseUrl = _configuration["ApiSettings:BaseUrl"]
            });
        }

        [HttpPost("echo")]
        public IActionResult Echo([FromBody] object data)
        {
            return Ok(new {
                message = "Richiesta ricevuta correttamente",
                data,
                timestamp = DateTime.Now
            });
        }

        [HttpPost("test-api-connection")]
        public async Task<IActionResult> TestApiConnection([FromBody] TestLoginRequest request)
        {
            try 
            {
                Console.WriteLine($"Testing API connection to backend...");
                var result = await _apiService.PostAsync<object>("/api/auth/login", new { 
                    Email = request.Email, 
                    Password = request.Password 
                });
                
                return Ok(new {
                    success = true,
                    message = "Test API connection completed",
                    result = result,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Ok(new {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }
    }

    public class TestLoginRequest 
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
