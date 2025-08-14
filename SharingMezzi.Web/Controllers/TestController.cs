using Microsoft.AspNetCore.Mvc;
using SharingMezzi.Web.Services;
using SharingMezzi.Web.Models;

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
                Console.WriteLine($"=== FRONTEND TEST START ===");
                Console.WriteLine($"Testing API connection to backend...");
                Console.WriteLine($"Email: {request.Email}");
                Console.WriteLine($"API Base URL: {_configuration["ApiSettings:BaseUrl"]}");
                
                var result = await _apiService.PostAsync<AuthResponse>("/api/auth/login", new { 
                    Email = request.Email, 
                    Password = request.Password 
                });
                
                Console.WriteLine($"=== FRONTEND TEST RESULT ===");
                Console.WriteLine($"Result Success: {result?.Success}");
                Console.WriteLine($"Result Message: {result?.Message}");
                Console.WriteLine($"Result Token: {(!string.IsNullOrEmpty(result?.Token) ? "Present" : "Missing")}");
                Console.WriteLine($"Result User: {(result?.User != null ? result.User.Email : "Missing")}");
                Console.WriteLine($"=== FRONTEND TEST END ===");
                
                return Ok(new {
                    success = true,
                    message = "Test API connection completed",
                    result = result,
                    apiBaseUrl = _configuration["ApiSettings:BaseUrl"],
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== FRONTEND TEST ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.WriteLine($"=== FRONTEND TEST ERROR END ===");
                
                return Ok(new {
                    success = false,
                    message = ex.Message,
                    error = ex.ToString(),
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
