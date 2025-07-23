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
                Console.WriteLine($"Tentativo di login per l'utente: {request.Email}");
                
                // Aggiungiamo un log della richiesta per debug
                Console.WriteLine($"Inviando richiesta a: {_apiService.GetBaseUrl()}/api/auth/login");
                
                var response = await _apiService.PostAsync<AuthResponse>("/api/auth/login", request);
                
                // Log della risposta
                Console.WriteLine($"Risposta login: Success={response?.Success}, Message={response?.Message}, HasToken={!string.IsNullOrEmpty(response?.Token)}");
                
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

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                Console.WriteLine($"Tentativo di registrazione per l'utente: {request.Email}");
                
                // Rimuoviamo la password dalla richiesta per il logging
                var logRequest = new { 
                    request.Nome, 
                    request.Cognome, 
                    request.Email, 
                    request.Telefono, 
                    PasswordLength = request.Password?.Length ?? 0 
                };
                Console.WriteLine($"Dati di registrazione: {Newtonsoft.Json.JsonConvert.SerializeObject(logRequest)}");
                
                // Rimuovi la conferma password prima di inviare al server (non necessaria per l'API)
                var apiRequest = new RegisterRequest
                {
                    Nome = request.Nome,
                    Cognome = request.Cognome,
                    Email = request.Email,
                    Password = request.Password,
                    Telefono = request.Telefono
                };
                
                var response = await _apiService.PostAsync<AuthResponse>("/api/auth/register", apiRequest);
                
                if (response?.Success == true)
                {
                    Console.WriteLine($"Registrazione avvenuta con successo per: {request.Email}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Registrazione fallita: {response?.Message ?? "Risposta nulla"}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione durante la registrazione: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    await _apiService.PostAsync<object>("/api/auth/logout", new { }, token);
                }
                
                ClearToken();
                ClearCurrentUser();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout Error: {ex.Message}");
                ClearToken();
                ClearCurrentUser();
                return false;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var user = GetCurrentUserFromSession();
                if (user != null)
                    return user;

                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                    return null;

                var response = await _apiService.GetAsync<User>("/api/auth/profile", token);
                if (response != null)
                {
                    SetCurrentUser(response);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Current User Error: {ex.Message}");
                return null;
            }
        }

        public string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(TokenKey);
        }

        public void SetToken(string token)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(TokenKey, token);
        }

        public void ClearToken()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(TokenKey);
        }

        private User? GetCurrentUserFromSession()
        {
            var userJson = _httpContextAccessor.HttpContext?.Session.GetString(UserKey);
            if (string.IsNullOrEmpty(userJson))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
            }
            catch
            {
                return null;
            }
        }

        private void SetCurrentUser(User user)
        {
            var userJson = System.Text.Json.JsonSerializer.Serialize(user);
            _httpContextAccessor.HttpContext?.Session.SetString(UserKey, userJson);
        }

        private void ClearCurrentUser()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(UserKey);
        }
    }
}
