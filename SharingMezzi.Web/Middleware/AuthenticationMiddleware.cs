using Microsoft.AspNetCore.Http;
using SharingMezzi.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SharingMezzi.Web.Middleware
{
    /// <summary>
    /// Middleware per proteggere le pagine autenticate e gestire l'accesso dopo il logout
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            try
            {
                var path = context.Request.Path.Value?.ToLower();
                
                // Lista delle pagine che richiedono autenticazione
                var protectedPaths = new[]
                {
                    "/dashboard",
                    "/vehicles", 
                    "/trips",
                    "/billing",
                    "/profile",
                    "/admin"
                };

                // Controlla se la pagina richiede autenticazione
                var requiresAuth = protectedPaths.Any(p => path?.StartsWith(p) == true);
                
                if (requiresAuth)
                {
                    // Verifica se l'utente è autenticato
                    if (!authService.IsAuthenticated())
                    {
                        _logger.LogWarning($"Accesso non autorizzato a {path}");
                        
                        // Reindirizza al login
                        context.Response.Redirect("/Login");
                        return;
                    }
                    
                    // Verifica se il token è ancora valido
                    var token = authService.GetToken();
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning($"Token mancante per {path}");
                        
                        // Pulisci la sessione e reindirizza
                        authService.ClearSession();
                        context.Response.Redirect("/Login");
                        return;
                    }
                }

                // Continua con la pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errore nel middleware di autenticazione: {ex.Message}");
                
                // In caso di errore, reindirizza al login
                context.Response.Redirect("/Login");
                return;
            }
        }
    }

    /// <summary>
    /// Estensioni per registrare il middleware
    /// </summary>
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
