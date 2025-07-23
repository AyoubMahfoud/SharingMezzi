using Microsoft.AspNetCore.Mvc;

namespace SharingMezzi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint di test per verificare la connettività API
    /// </summary>
    [HttpGet]
    public ActionResult<object> GetTest()
    {
        _logger.LogInformation("Test endpoint chiamato con successo");
        
        return Ok(new 
        {
            message = "API funzionante!",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            server = "SharingMezzi.Api"
        });
    }

    /// <summary>
    /// Test di connettività con informazioni più dettagliate
    /// </summary>
    [HttpGet("connectivity")]
    public ActionResult<object> GetConnectivityTest()
    {
        _logger.LogInformation("Test di connettività chiamato");
        
        return Ok(new 
        {
            status = "success",
            message = "Backend raggiungibile correttamente",
            timestamp = DateTime.UtcNow,
            server_info = new 
            {
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                machine_name = Environment.MachineName,
                os_version = Environment.OSVersion.ToString(),
                framework = Environment.Version.ToString()
            },
            api_info = new 
            {
                name = "SharingMezzi API",
                version = "1.0.0",
                endpoints_available = new[] 
                {
                    "/api/auth/login",
                    "/api/auth/register", 
                    "/api/mezzi",
                    "/api/parcheggi",
                    "/api/corse",
                    "/api/test"
                }
            }
        });
    }
}
