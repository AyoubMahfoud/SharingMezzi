using SharingMezzi.Web.Models;
using Microsoft.AspNetCore.Http;

namespace SharingMezzi.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TokenKey = "AuthToken";
        private const string UserKey = "CurrentUser";
        private const string RefreshTokenKey = "RefreshToken";

        public AuthService(IApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                Console.WriteLine($"=== AUTH SERVICE LOGIN START ===");
                Console.WriteLine($"Tentativo di login per l'utente: {request.Email}");
                Console.WriteLine($"Password length: {request.Password?.Length ?? 0}");
                Console.WriteLine($"ApiService BaseUrl: {_apiService.GetBaseUrl()}");
                
                // Test the request object
                var testJson = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                Console.WriteLine($"LoginRequest JSON: {testJson}");
                
                var response = await _apiService.PostAsync<AuthResponse>("/api/auth/login", request);
                
                Console.WriteLine($"=== AUTH SERVICE RESPONSE ===");
                Console.WriteLine($"Response is null: {response == null}");
                Console.WriteLine($"Response Success: {response?.Success}");
                Console.WriteLine($"Response Message: {response?.Message}");
                Console.WriteLine($"Response Token: {(!string.IsNullOrEmpty(response?.Token) ? "Present" : "Missing")}");
                Console.WriteLine($"Response User: {(response?.User != null ? response.User.Email : "Missing")}");
                Console.WriteLine($"=== AUTH SERVICE END ===");
                
                if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
                {
                    if (response.User != null)
                    {
                        // Usa il nuovo sistema di persistenza
                        SetPersistentToken(response.Token, response.User);
                        
                        // Salva anche il refresh token se presente
                        if (!string.IsNullOrEmpty(response.RefreshToken))
                        {
                            SetRefreshToken(response.RefreshToken);
                            Console.WriteLine($"✅ Refresh token salvato per: {response.User.Email}");
                        }
                        
                        Console.WriteLine($"Utente loggato con successo e token persistente salvato: {response.User.Email}, Ruolo: {response.User.Ruolo}");
                    }
                }
                else
                {
                    Console.WriteLine($"Login fallito: {response?.Message ?? "Risposta nulla"}");
                }
                
                return response ?? new AuthResponse { Success = false, Message = "Risposta non valida dal server" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione durante il login: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new AuthResponse { Success = false, Message = $"Errore durante il login: {ex.Message}" };
            }
        }

        // SOSTITUISCI IL METODO RegisterAsync nel tuo AuthService.cs con questo:

// SOSTITUISCI SOLO IL METODO RegisterAsync nel tuo AuthService.cs esistente con questo:

// SOSTITUISCI il metodo RegisterAsync nel tuo AuthService.cs del frontend con questo:

public async Task<bool> RegisterAsync(RegisterRequest request)
{
    try
    {
        Console.WriteLine($"=== REGISTRATION DEBUG START ===");
        Console.WriteLine($"📧 Email: {request.Email}");
        Console.WriteLine($"👤 Nome: {request.Nome}");
        Console.WriteLine($"👤 Cognome: {request.Cognome}");
        Console.WriteLine($"📱 Telefono: {request.Telefono ?? "NULL"}");
        Console.WriteLine($"🔑 Password Length: {request.Password?.Length ?? 0}");
        Console.WriteLine($"✅ Accept Terms: {request.AcceptTerms}");
        Console.WriteLine($"🔌 API Base URL: {_apiService.GetBaseUrl()}");
        
        // Prepara richiesta API (rimuovi ConfirmPassword)
        var apiRequest = new 
        {
            Nome = request.Nome?.Trim(),
            Cognome = request.Cognome?.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Password = request.Password,
            Telefono = request.Telefono?.Trim()
        };
        
        Console.WriteLine($"📤 API Request: {Newtonsoft.Json.JsonConvert.SerializeObject(apiRequest)}");
        
        // USA SOLO l'endpoint corretto che funziona
        Console.WriteLine($"🔄 Calling registration endpoint: /api/auth/register");
        var response = await _apiService.PostAsync<AuthResponse>("/api/auth/register", apiRequest);
        
        Console.WriteLine($"📊 Registration Response:");
        Console.WriteLine($"   Success: {response?.Success}");
        Console.WriteLine($"   Message: {response?.Message}");
        Console.WriteLine($"   Token: {(!string.IsNullOrEmpty(response?.Token) ? "Present" : "Missing")}");
        
        if (response?.Success == true)
        {
            Console.WriteLine($"✅ Registration successful!");
            
            // Se la registrazione include automaticamente il login (con token), salva i dati
            if (!string.IsNullOrEmpty(response.Token) && response.User != null)
            {
                SetToken(response.Token);
                SetCurrentUser(response.User);
                Console.WriteLine($"🔑 User automatically logged in after registration");
            }
            
            return true;
        }
        else
        {
            Console.WriteLine($"❌ Registration failed: {response?.Message ?? "Unknown error"}");
            return false;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"💥 REGISTRATION EXCEPTION: {ex.Message}");
        Console.WriteLine($"💥 Stack Trace: {ex.StackTrace}");
        return false;
    }
    finally
    {
        Console.WriteLine($"=== REGISTRATION DEBUG END ===");
    }
}

public void LogoutAsync()
{
    try
    {
        Console.WriteLine("=== LOGOUT START ===");
        ClearSession();
        Console.WriteLine("Logout completato con successo");
        Console.WriteLine("=== LOGOUT END ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore durante il logout: {ex.Message}");
    }
}

        public string? GetToken()
        {
            // Recupera il token solo dalla sessione (metodo interno)
            return GetTokenFromSession();
        }


        // ===== METODO ASYNC AGGIUNTO =====
        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                // Usa il nuovo sistema che prova prima dalla sessione, poi dai cookie persistenti
                var user = GetCurrentUser();
                if (user != null)
                {
                    return user;
                }

                // Se non c'è da nessuna parte, prova dall'API
                var token = GetPersistentToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Tentativo di recupero utente dall'API...");
                    var userFromApi = await _apiService.GetAsync<User>("/api/user/profile", token);
                    
                    if (userFromApi != null)
                    {
                        // Salva sia in sessione che nei cookie persistenti
                        SetPersistentToken(token, userFromApi);
                        Console.WriteLine($"Utente recuperato dall'API e salvato persistentemente: {userFromApi.Email}");
                        return userFromApi;
                    }
                }

                Console.WriteLine("Impossibile recuperare l'utente corrente");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel recupero async dell'utente: {ex.Message}");
                return null;
            }
        }



        public void SetToken(string token)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    session.SetString(TokenKey, token);
                    Console.WriteLine("Token salvato nella sessione");
                }
                else
                {
                    Console.WriteLine("Impossibile salvare il token: sessione non disponibile");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvataggio del token: {ex.Message}");
            }
        }

        public void SetCurrentUser(User user)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    var userJson = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                    session.SetString(UserKey, userJson);
                    Console.WriteLine($"Utente salvato nella sessione: {user.Email}");
                }
                else
                {
                    Console.WriteLine("Impossibile salvare l'utente: sessione non disponibile");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvataggio dell'utente: {ex.Message}");
            }
        }

        public void SetRefreshToken(string refreshToken)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    session.SetString(RefreshTokenKey, refreshToken);
                    Console.WriteLine("Refresh token salvato nella sessione");
                }
                else
                {
                    Console.WriteLine("Impossibile salvare il refresh token: sessione non disponibile");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvataggio del refresh token: {ex.Message}");
            }
        }

        public string? GetRefreshToken()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Session.GetString(RefreshTokenKey);
            Console.WriteLine($"GetRefreshToken: Refresh token dalla sessione: {(string.IsNullOrEmpty(refreshToken) ? "Missing" : "Present")}");
            return refreshToken;
        }

        // ===== NUOVI METODI PER PERSISTENZA =====
        
        /// <summary>
        /// Salva il token sia in sessione che in un cookie persistente
        /// </summary>
        public void SetPersistentToken(string token, User user)
        {
            try
                {
                // Salva in sessione (per la sessione corrente)
                SetToken(token);
                SetCurrentUser(user);
                
                // Salva in cookie persistente (per mantenere l'accesso dopo riavvio server)
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // false per HTTP locale
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.Now.AddDays(30), // Cookie valido per 30 giorni
                        IsEssential = true
                    };
                    
                    // Salva token e user in cookie separati
                    httpContext.Response.Cookies.Append("PersistentToken", token, cookieOptions);
                    var userJson = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                    httpContext.Response.Cookies.Append("PersistentUser", userJson, cookieOptions);
                    
                    Console.WriteLine($"Token e utente salvati in cookie persistenti per: {user.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvataggio persistente: {ex.Message}");
            }
        }

        /// <summary>
        /// Recupera il token dal cookie persistente se la sessione è vuota
        /// </summary>
        public string? GetPersistentToken()
        {
            try
            {
                // Prima prova dalla sessione
                var sessionToken = GetTokenFromSession();
                if (!string.IsNullOrEmpty(sessionToken))
                {
                    return sessionToken;
                }
                
                // Se non c'è in sessione, prova dal cookie persistente
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var persistentToken = httpContext.Request.Cookies["PersistentToken"];
                    if (!string.IsNullOrEmpty(persistentToken))
                    {
                        Console.WriteLine("Token recuperato dal cookie persistente");
                        // Ripristina anche in sessione
                        SetToken(persistentToken);
                        return persistentToken;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel recupero del token persistente: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Recupera l'utente dal cookie persistente se la sessione è vuota
        /// </summary>
        public User? GetPersistentUser()
        {
            try
            {
                // Prima prova dalla sessione
                var sessionUser = GetCurrentUserFromSession();
                if (sessionUser != null)
                {
                    return sessionUser;
                }
                
                // Se non c'è in sessione, prova dal cookie persistente
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var persistentUserJson = httpContext.Request.Cookies["PersistentUser"];
                    if (!string.IsNullOrEmpty(persistentUserJson))
                    {
                        var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(persistentUserJson);
                        if (user != null)
                        {
                            Console.WriteLine($"Utente recuperato dal cookie persistente: {user.Email}");
                            // Ripristina anche in sessione
                            SetCurrentUser(user);
                            return user;
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel recupero dell'utente persistente: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Verifica se l'utente è autenticato usando sia sessione che cookie persistenti
        /// </summary>
        public bool IsAuthenticated()
        {
            // Prova prima dalla sessione
            var sessionToken = GetTokenFromSession();
            if (!string.IsNullOrEmpty(sessionToken))
            {
                Console.WriteLine("Utente autenticato dalla sessione");
                return true;
            }
            
            // Se non c'è in sessione, prova dal cookie persistente
            var persistentToken = GetPersistentToken();
            if (!string.IsNullOrEmpty(persistentToken))
            {
                Console.WriteLine("Utente autenticato dal cookie persistente");
                return true;
            }
            
            Console.WriteLine("Utente non autenticato");
            return false;
        }

        /// <summary>
        /// Recupera l'utente corrente usando sia sessione che cookie persistenti
        /// </summary>
        public User? GetCurrentUser()
        {
            // Prima prova dalla sessione
            var sessionUser = GetCurrentUserFromSession();
            if (sessionUser != null)
            {
                return sessionUser;
            }
            
            // Se non c'è in sessione, prova dal cookie persistente
            var persistentUser = GetPersistentUser();
            if (persistentUser != null)
            {
                return persistentUser;
            }
            
            return null;
        }

        /// <summary>
        /// Recupera l'utente solo dalla sessione (metodo interno)
        /// </summary>
        private User? GetCurrentUserFromSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    var userJson = session.GetString(UserKey);
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(userJson);
                        Console.WriteLine($"Utente recuperato dalla sessione: {user?.Email ?? "Unknown"}");
                        return user;
                    }
                }
                
                Console.WriteLine("Nessun utente trovato nella sessione");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel recupero dell'utente dalla sessione: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Recupera il token solo dalla sessione (metodo interno)
        /// </summary>
        private string? GetTokenFromSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    var token = session.GetString(TokenKey);
                    Console.WriteLine($"Token recuperato dalla sessione: {(!string.IsNullOrEmpty(token) ? "Present" : "Missing")}");
                    return token;
                }
                
                Console.WriteLine("Sessione non disponibile per recuperare il token");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel recupero del token dalla sessione: {ex.Message}");
                return null;
            }
        }

        public void ClearSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    session.Remove(TokenKey);
                    session.Remove(UserKey);
                    Console.WriteLine("Sessione pulita con successo");
                }
                else
                {
                    Console.WriteLine("Sessione non disponibile per la pulizia");
                }
                
                // Rimuovi anche i cookie persistenti
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Response.Cookies.Delete("PersistentToken");
                    httpContext.Response.Cookies.Delete("PersistentUser");
                    Console.WriteLine("Cookie persistenti rimossi");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella pulizia della sessione: {ex.Message}");
            }
        }
    }
}