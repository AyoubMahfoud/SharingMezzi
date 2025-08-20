using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;

namespace SharingMezzi.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private readonly ILogger<BillingController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BillingController(ILogger<BillingController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                // Ottieni il token dalla sessione
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    // Per debug, restituisci un profilo di test
                    _logger.LogWarning("Token non trovato nella sessione, restituisco profilo di test");
                    return Ok(new
                    {
                        id = 1,
                        email = "admin@sharingmezzi.it",
                        nome = "Admin",
                        cognome = "System",
                        credito = 100.00m,
                        puntiEco = 50
                    });
                }

                // Chiama il backend API
                var backendUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5000";
                var request = new HttpRequestMessage(HttpMethod.Get, $"{backendUrl}/api/user/profile");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }

                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero del profilo utente");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("recharges")]
        public async Task<IActionResult> GetUserRecharges()
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    // Per debug, restituisci ricariche di test
                    _logger.LogWarning("Token non trovato nella sessione, restituisco ricariche di test");
                    return Ok(new[]
                    {
                        new
                        {
                            id = 1,
                            dataRicarica = DateTime.Now.AddDays(-7),
                            importo = 50.00m,
                            metodoPagamento = "CartaCredito",
                            stato = "Completato",
                            saldoFinale = 150.00m
                        },
                        new
                        {
                            id = 2,
                            dataRicarica = DateTime.Now.AddDays(-14),
                            importo = 25.00m,
                            metodoPagamento = "PayPal",
                            stato = "Completato",
                            saldoFinale = 100.00m
                        }
                    });
                }

                // Prima ottieni il profilo per avere l'ID utente
                var profileUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5000";
                var profileRequest = new HttpRequestMessage(HttpMethod.Get, $"{profileUrl}/api/user/profile");
                profileRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var profileResponse = await _httpClient.SendAsync(profileRequest);
                if (!profileResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)profileResponse.StatusCode, "Impossibile recuperare il profilo utente");
                }

                var profileContent = await profileResponse.Content.ReadAsStringAsync();
                var profile = JsonSerializer.Deserialize<JsonElement>(profileContent);
                
                if (!profile.TryGetProperty("id", out var idElement) && !profile.TryGetProperty("Id", out idElement))
                {
                    return BadRequest(new { message = "ID utente non trovato nel profilo" });
                }

                var userId = idElement.GetInt32();

                // Ora ottieni le ricariche
                var rechargesUrl = $"{profileUrl}/api/user/{userId}/ricariche";
                var rechargesRequest = new HttpRequestMessage(HttpMethod.Get, rechargesUrl);
                rechargesRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var rechargesResponse = await _httpClient.SendAsync(rechargesRequest);
                var rechargesContent = await rechargesResponse.Content.ReadAsStringAsync();

                if (rechargesResponse.IsSuccessStatusCode)
                {
                    return Ok(JsonSerializer.Deserialize<object>(rechargesContent));
                }

                return StatusCode((int)rechargesResponse.StatusCode, rechargesContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero delle ricariche");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("recharge")]
        public async Task<IActionResult> ProcessRecharge([FromBody] RechargeRequest request)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Token non trovato nella sessione" });
                }

                // Prima ottieni il profilo per avere l'ID utente
                var profileUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5000";
                var profileRequest = new HttpRequestMessage(HttpMethod.Get, $"{profileUrl}/api/user/profile");
                profileRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var profileResponse = await _httpClient.SendAsync(profileRequest);
                if (!profileResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)profileResponse.StatusCode, "Impossibile recuperare il profilo utente");
                }

                var profileContent = await profileResponse.Content.ReadAsStringAsync();
                var profile = JsonSerializer.Deserialize<JsonElement>(profileContent);
                
                if (!profile.TryGetProperty("id", out var idElement) && !profile.TryGetProperty("Id", out idElement))
                {
                    return BadRequest(new { message = "ID utente non trovato nel profilo" });
                }

                var userId = idElement.GetInt32();

                // Prepara il payload per la ricarica
                var rechargePayload = new
                {
                    UtenteId = userId,
                    Importo = request.Importo,
                    MetodoPagamento = request.MetodoPagamento,
                    Note = request.Note
                };

                var rechargeUrl = $"{profileUrl}/api/user/ricarica-credito";
                var rechargeRequest = new HttpRequestMessage(HttpMethod.Post, rechargeUrl);
                rechargeRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                rechargeRequest.Content = new StringContent(
                    JsonSerializer.Serialize(rechargePayload),
                    Encoding.UTF8,
                    "application/json"
                );

                var rechargeResponse = await _httpClient.SendAsync(rechargeRequest);
                var rechargeContent = await rechargeResponse.Content.ReadAsStringAsync();

                if (rechargeResponse.IsSuccessStatusCode)
                {
                    return Ok(JsonSerializer.Deserialize<object>(rechargeContent));
                }

                return StatusCode((int)rechargeResponse.StatusCode, rechargeContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricarica");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }
    }

    public class RechargeRequest
    {
        public decimal Importo { get; set; }
        public string MetodoPagamento { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
