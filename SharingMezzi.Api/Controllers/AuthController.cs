using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SharingMezzi.Infrastructure.Database;

namespace SharingMezzi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SharingMezziContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            SharingMezziContext context, 
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("=== LOGIN REQUEST RICEVUTA ===");
                _logger.LogInformation("Request is null: {IsNull}", request == null);
                
                if (request == null)
                {
                    _logger.LogError("LoginRequest è null - problema di deserializzazione");
                    return BadRequest(new { Success = false, Message = "Dati di login non validi" });
                }
                
                _logger.LogInformation("Email ricevuta: '{Email}', Password length: {PasswordLength}", 
                    request.Email ?? "NULL", request.Password?.Length ?? 0);
                
                // Verifica ModelState prima di validare i campi
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState non valido:");
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning("  {Key}: {Errors}", error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                    return BadRequest(new { Success = false, Message = "Dati di login non validi", Errors = ModelState });
                }

                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Email o password mancanti - Email: '{Email}', Password empty: {PasswordEmpty}", 
                        request.Email ?? "NULL", string.IsNullOrEmpty(request.Password));
                    return BadRequest(new { Success = false, Message = "Email e password sono obbligatorie" });
                }

                var utente = await _context.Utenti
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (utente == null)
                {
                    _logger.LogWarning("Utente non trovato: {Email}", request.Email);
                    return Unauthorized(new { Success = false, Message = "Email o password non corretti" });
                }

                if (!VerifyPassword(request.Password, utente.Password))
                {
                    _logger.LogWarning("Password errata per: {Email}", request.Email);
                    return Unauthorized(new { Success = false, Message = "Email o password non corretti" });
                }

                var token = GenerateJwtToken(utente);

                _logger.LogInformation("Login riuscito per: {Email}", request.Email);

                var response = new
                {
                    Success = true, // Maiuscolo per compatibilità frontend
                    Message = "Login effettuato con successo",
                    Token = token,
                    User = new
                    {
                        Id = utente.Id,
                        Nome = utente.Nome,
                        Cognome = utente.Cognome,
                        Email = utente.Email,
                        Ruolo = GetUserRole(utente),
                        Credito = utente.Credito,
                        PuntiEco = GetUserPoints(utente)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il login per: {Email}", request.Email);
                return StatusCode(500, new { Success = false, Message = "Errore interno del server" });
            }
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin()
        {
            try
            {
                var existingAdmin = await _context.Utenti
                    .FirstOrDefaultAsync(u => u.Email == "admin@sharingmezzi.it");

                if (existingAdmin != null)
                {
                    return Ok(new { Message = "Admin già esistente", Email = existingAdmin.Email });
                }

                var adminUser = new SharingMezzi.Core.Entities.Utente();
                
                adminUser.Email = "admin@sharingmezzi.it";
                adminUser.Nome = "Admin";
                adminUser.Cognome = "System";
                adminUser.Password = HashPassword("admin123");
                adminUser.DataRegistrazione = DateTime.UtcNow;
                adminUser.Credito = 100.00m;

                // Imposta proprietà dinamicamente
                var userType = adminUser.GetType();
                
                var ruoloProperty = userType.GetProperty("Ruolo");
                if (ruoloProperty != null && ruoloProperty.PropertyType.IsEnum)
                {
                    try
                    {
                        var adminRole = Enum.Parse(ruoloProperty.PropertyType, "Amministratore");
                        ruoloProperty.SetValue(adminUser, adminRole);
                    }
                    catch { }
                }
                
                var statoProperty = userType.GetProperty("Stato");
                if (statoProperty != null && statoProperty.PropertyType.IsEnum)
                {
                    try
                    {
                        var attivoValue = Enum.Parse(statoProperty.PropertyType, "Attivo");
                        statoProperty.SetValue(adminUser, attivoValue);
                    }
                    catch { }
                }
                
                var puntiEcoProperty = userType.GetProperty("PuntiEco");
                if (puntiEcoProperty != null)
                {
                    try
                    {
                        puntiEcoProperty.SetValue(adminUser, 50);
                    }
                    catch { }
                }

                _context.Utenti.Add(adminUser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Utente admin creato con successo",
                    Email = "admin@sharingmezzi.it",
                    Password = "admin123"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione dell'admin");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private string GenerateJwtToken(dynamic utente)
        {
            var secretKey = _configuration["Jwt:SecretKey"] ?? "SharingMezzi-SecretKey-2024-VeryLongAndSecureKey123456789";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utente.Id.ToString()),
                new Claim(ClaimTypes.Name, utente.Nome ?? ""),
                new Claim(ClaimTypes.Email, utente.Email ?? ""),
                new Claim(ClaimTypes.Role, GetUserRole(utente))
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "SharingMezzi.Api",
                audience: _configuration["Jwt:Audience"] ?? "SharingMezzi.Web",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetUserRole(dynamic utente)
        {
            try
            {
                var ruoloProperty = utente.GetType().GetProperty("Ruolo");
                if (ruoloProperty != null)
                {
                    var ruolo = ruoloProperty.GetValue(utente);
                    return ruolo?.ToString() ?? "Utente";
                }
                return "Utente";
            }
            catch
            {
                return "Utente";
            }
        }

        private int GetUserPoints(dynamic utente)
        {
            try
            {
                var puntiProperty = utente.GetType().GetProperty("PuntiEco");
                if (puntiProperty != null)
                {
                    var punti = puntiProperty.GetValue(utente);
                    return punti is int intValue ? intValue : 0;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}