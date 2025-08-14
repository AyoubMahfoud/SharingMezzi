using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace SharingMezzi.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            var baseUrl = _httpClient.BaseAddress?.ToString() ?? _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            _logger.LogInformation("ApiService configurato con BaseUrl: {BaseUrl}", baseUrl);
            
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SharingMezzi.Web/1.0");
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                
                _logger.LogDebug("GET Request: {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("GET Success: {StatusCode}", response.StatusCode);
                    
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        return default(T);
                    }
                    
                    return JsonConvert.DeserializeObject<T>(content);
                }
                
                _logger.LogWarning("GET Failed: {StatusCode} - {Endpoint}", response.StatusCode, endpoint);
                return default(T);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Errore di connessione HTTP per GET {Endpoint}", endpoint);
                return default(T);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout per GET {Endpoint}", endpoint);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generico per GET {Endpoint}", endpoint);
                return default(T);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                
                var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings 
                { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                });
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("=== POST REQUEST DEBUG ===");
                _logger.LogInformation("Endpoint: {Endpoint}", endpoint);
                _logger.LogInformation("BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
                _logger.LogInformation("Full URL: {FullUrl}", new Uri(_httpClient.BaseAddress, endpoint));
                _logger.LogInformation("JSON Data: {Data}", json);
                _logger.LogInformation("Content-Type: {ContentType}", content.Headers.ContentType);
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Response Content: {Content}", responseContent);
                _logger.LogInformation("=== END POST REQUEST DEBUG ===");
                
                _logger.LogDebug("POST Response: {StatusCode} - Content: {Content}", 
                    response.StatusCode, responseContent);
                
                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        // Crea una risposta di successo vuota
                        dynamic emptyResponse = new System.Dynamic.ExpandoObject();
                        emptyResponse.Success = true;
                        emptyResponse.Message = "Operazione completata con successo";
                        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(emptyResponse));
                    }
                    
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(responseContent);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Errore deserializzazione JSON: {Content}", responseContent);
                        
                        dynamic errorResponse = new System.Dynamic.ExpandoObject();
                        errorResponse.Success = false;
                        errorResponse.Message = "Errore nel formato della risposta del server";
                        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(errorResponse));
                    }
                }
                
                // Gestione errori HTTP
                _logger.LogWarning("POST Failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                
                return HandleErrorResponse<T>(response, responseContent, endpoint);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Errore di connessione HTTP per POST {Endpoint}", endpoint);
                return CreateErrorResponse<T>("Errore di connessione. Verifica che l'API sia in esecuzione.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout per POST {Endpoint}", endpoint);
                return CreateErrorResponse<T>("La richiesta ha impiegato troppo tempo. Riprova.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generico per POST {Endpoint}", endpoint);
                return CreateErrorResponse<T>($"Errore imprevisto: {ex.Message}");
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                
                var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings 
                { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogDebug("PUT Request: {Endpoint}", endpoint);
                var response = await _httpClient.PutAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        return default(T);
                    }
                    
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                
                _logger.LogWarning("PUT Failed: {StatusCode}", response.StatusCode);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore PUT {Endpoint}", endpoint);
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                
                _logger.LogDebug("DELETE Request: {Endpoint}", endpoint);
                var response = await _httpClient.DeleteAsync(endpoint);
                
                var success = response.IsSuccessStatusCode;
                _logger.LogDebug("DELETE Response: {StatusCode}", response.StatusCode);
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore DELETE {Endpoint}", endpoint);
                return false;
            }
        }

        private void SetAuthorizationHeader(string? token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private T HandleErrorResponse<T>(HttpResponseMessage response, string content, string endpoint)
        {
            var statusCode = (int)response.StatusCode;
            
            // Gestione errori specifici
            string errorMessage = statusCode switch
            {
                400 => "Richiesta non valida. Controlla i dati inseriti.",
                401 => "Non autorizzato. Effettua nuovamente il login.",
                403 => "Accesso negato. Non hai i permessi necessari.",
                404 => $"Risorsa non trovata: {endpoint}",
                409 => "Conflitto. La risorsa potrebbe essere già in uso.",
                429 => "Troppe richieste. Riprova tra qualche minuto.",
                500 => "Errore interno del server. Contatta l'amministratore.",
                502 => "Server non disponibile. Riprova più tardi.",
                503 => "Servizio temporaneamente non disponibile.",
                _ => $"Errore del server: {statusCode}"
            };

            // Prova a estrarre il messaggio di errore dalla risposta
            try
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    if (errorResponse?.message != null)
                    {
                        errorMessage = errorResponse.message.ToString();
                    }
                    else if (errorResponse?.Message != null)
                    {
                        errorMessage = errorResponse.Message.ToString();
                    }
                }
            }
            catch
            {
                // Usa il messaggio di errore di default se non riusciamo a deserializzare
            }

            return CreateErrorResponse<T>(errorMessage);
        }

        private T CreateErrorResponse<T>(string message)
        {
            dynamic errorResponse = new System.Dynamic.ExpandoObject();
            errorResponse.Success = false;
            errorResponse.Message = message;
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(errorResponse));
        }

        public string GetBaseUrl()
        {
            return _httpClient.BaseAddress?.ToString() ?? _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }
    }
}