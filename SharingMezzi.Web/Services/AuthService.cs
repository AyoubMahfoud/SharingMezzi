using SharingMezzi.Web.Models;

namespace SharingMezzi.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TokenKey = "AuthToken";
        private const string UserKey = "CurrentUser";

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
                    SetToken(response.Token);
                    if (response.User != null)
                    {
                        SetCurrentUser(response.User);
                        Console.WriteLine($"Utente loggato con successo: {response.User.Email}, Ruolo: {response.User.Ruolo}");
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
        Console.WriteLine($"Errore nel recupero del token: {ex.Message}");
        return null;
    }
}

public User? GetCurrentUser()
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
        Console.WriteLine($"Errore nel recupero dell'utente: {ex.Message}");
        return null;
    }
}
        // ===== METODO ASYNC AGGIUNTO =====
        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                // Prima prova dalla sessione
                var userFromSession = GetCurrentUser();
                if (userFromSession != null)
                {
                    return userFromSession;
                }

                // Se non c'è nella sessione, prova dall'API
                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Tentativo di recupero utente dall'API...");
                    var userFromApi = await _apiService.GetAsync<User>("/api/user/profile", token);
                    
                    if (userFromApi != null)
                    {
                        // Salva nella sessione per le prossime volte
                        SetCurrentUser(userFromApi);
                        Console.WriteLine($"Utente recuperato dall'API e salvato in sessione: {userFromApi.Email}");
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

        public bool IsAuthenticated()
        {
            var token = GetToken();
            var isAuth = !string.IsNullOrEmpty(token);
            Console.WriteLine($"Stato autenticazione: {isAuth}");
            return isAuth;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella pulizia della sessione: {ex.Message}");
            }
        }
    }
}