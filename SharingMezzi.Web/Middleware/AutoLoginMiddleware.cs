using Microsoft.AspNetCore.Http;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.Middleware
{
    /// <summary>
    /// Middleware per il ripristino automatico dell'autenticazione dai cookie persistenti
    /// </summary>
    public class AutoLoginMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AutoLoginMiddleware> _logger;

        public AutoLoginMiddleware(RequestDelegate next, ILogger<AutoLoginMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            try
            {
                // Controlla se l'utente è già autenticato in sessione
                var sessionToken = context.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(sessionToken))
                {
                    // Prova a ripristinare l'autenticazione dai cookie persistenti
                    var persistentToken = context.Request.Cookies["PersistentToken"];
                    var persistentUserJson = context.Request.Cookies["PersistentUser"];
                    
                    if (!string.IsNullOrEmpty(persistentToken) && !string.IsNullOrEmpty(persistentUserJson))
                    {
                        try
                        {
                            // Deserializza l'utente dal cookie
                            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.User>(persistentUserJson);
                            if (user != null)
                            {
                                // Ripristina la sessione direttamente
                                context.Session.SetString("AuthToken", persistentToken);
                                var userJson = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                                context.Session.SetString("CurrentUser", userJson);
                                
                                _logger.LogInformation($"Auto-login riuscito per l'utente: {user.Email}");
                                Console.WriteLine($"🔄 Auto-login riuscito per l'utente: {user.Email}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Errore durante l'auto-login: {ex.Message}");
                            Console.WriteLine($"⚠️ Errore durante l'auto-login: {ex.Message}");
                            
                            // Rimuovi i cookie corrotti
                            context.Response.Cookies.Delete("PersistentToken");
                            context.Response.Cookies.Delete("PersistentUser");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errore nel middleware AutoLogin: {ex.Message}");
                Console.WriteLine($"❌ Errore nel middleware AutoLogin: {ex.Message}");
            }

            // Continua con la pipeline
            await _next(context);
        }
    }

    /// <summary>
    /// Estensioni per registrare il middleware
    /// </summary>
    public static class AutoLoginMiddlewareExtensions
    {
        public static IApplicationBuilder UseAutoLogin(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AutoLoginMiddleware>();
        }
    }
}
