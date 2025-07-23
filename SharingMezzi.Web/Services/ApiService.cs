using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace SharingMezzi.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
            
            Console.WriteLine($"=== DEBUG API SERVICE ===");
            Console.WriteLine($"Configurazione ApiSettings:BaseUrl = {_configuration["ApiSettings:BaseUrl"]}");
            Console.WriteLine($"URL base finale = {_baseUrl}");
            Console.WriteLine($"HttpClient BaseAddress sarà impostato a: {_baseUrl}");
            
            // IMPORTANTE: Non impostare BaseAddress se stiamo usando URL completi
            // _httpClient.BaseAddress = new Uri(_baseUrl);
            
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Aggiungiamo Origin per aiutare con possibili problemi CORS
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://localhost:7050");
            
            // Timeout esteso
            _httpClient.Timeout = TimeSpan.FromSeconds(Convert.ToInt32(_configuration["ApiSettings:Timeout"] ?? "60"));
            
            Console.WriteLine($"HttpClient configurato completamente");
            Console.WriteLine($"=========================");
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            try
            {
                var fullUrl = $"{_baseUrl}{endpoint}";
                Console.WriteLine($"GET request to: {fullUrl}");
                
                SetAuthorizationHeader(token);
                var response = await _httpClient.GetAsync(fullUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }
                
                Console.WriteLine($"GET failed: {response.StatusCode}");
                return default(T);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"API GET Error: {ex.Message}");
                return default(T);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                // Costruisci l'URL completo
                var fullUrl = $"{_baseUrl}{endpoint}";
                Console.WriteLine($"=== DEBUG POST REQUEST ===");
                Console.WriteLine($"URL completo: {fullUrl}");
                Console.WriteLine($"Endpoint: {endpoint}");
                Console.WriteLine($"BaseUrl: {_baseUrl}");
                Console.WriteLine($"Dati inviati: {JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
                
                SetAuthorizationHeader(token);
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Log dei headers per debug
                Console.WriteLine("Headers della richiesta:");
                foreach (var header in _httpClient.DefaultRequestHeaders)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                
                // Usa l'URL completo invece del relativo
                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Risposta ricevuta - Stato: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"Contenuto della risposta: {responseContent}");
                Console.WriteLine($"========================");
                
                if (response.IsSuccessStatusCode)
                {
                    try 
                    {
                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            Console.WriteLine("ATTENZIONE: Risposta vuota dal server");
                            dynamic emptyResponse = new System.Dynamic.ExpandoObject();
                            emptyResponse.Success = false;
                            emptyResponse.Message = "Il server ha restituito una risposta vuota";
                            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(emptyResponse));
                        }
                        
                        return JsonConvert.DeserializeObject<T>(responseContent);
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"Errore di deserializzazione JSON: {jsonEx.Message}");
                        Console.WriteLine($"Contenuto problematico: {responseContent}");
                        
                        // Se non riusciamo a deserializzare, creiamo una risposta dinamica
                        dynamic errorResponse = new System.Dynamic.ExpandoObject();
                        errorResponse.Success = false;
                        errorResponse.Message = $"Errore nel formato della risposta: {jsonEx.Message}";
                        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(errorResponse));
                    }
                }
                
                // Gestire errori HTTP
                Console.WriteLine($"Errore API: Stato {(int)response.StatusCode}, Contenuto: {responseContent}");
                
                if ((int)response.StatusCode == 0 || (int)response.StatusCode >= 500)
                {
                    dynamic serverErrorResponse = new System.Dynamic.ExpandoObject();
                    serverErrorResponse.Success = false;
                    serverErrorResponse.Message = $"Il server API non è disponibile o ha restituito un errore ({(int)response.StatusCode}). Assicurati che il backend sia in esecuzione.";
                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(serverErrorResponse));
                }
                
                if ((int)response.StatusCode == 404)
                {
                    dynamic notFoundResponse = new System.Dynamic.ExpandoObject();
                    notFoundResponse.Success = false;
                    notFoundResponse.Message = $"L'endpoint API richiesto ({endpoint}) non esiste. Controlla l'URL o contatta l'amministratore.";
                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(notFoundResponse));
                }
                
                try
                {
                    // Prova a deserializzare la risposta di errore
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        dynamic typedErrorResponse = new System.Dynamic.ExpandoObject();
                        typedErrorResponse.Success = false;
                        typedErrorResponse.Message = errorResponse?.message ?? errorResponse?.Message ?? $"Errore del server: {(int)response.StatusCode}";
                        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(typedErrorResponse));
                    }
                }
                catch
                {
                    // Se la deserializzazione fallisce, continua con l'errore generico
                }
                
                // Se non siamo riusciti a estrarre un messaggio dalla risposta
                dynamic genericErrorResponse = new System.Dynamic.ExpandoObject();
                genericErrorResponse.Success = false;
                genericErrorResponse.Message = $"Errore del server: {(int)response.StatusCode} {response.StatusCode}";
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(genericErrorResponse));
            }
            catch (HttpRequestException reqEx)
            {
                Console.WriteLine($"Errore di connessione HTTP: {reqEx.Message}");
                if (reqEx.InnerException != null)
                {
                    Console.WriteLine($"Causa: {reqEx.InnerException.Message}");
                }
                
                dynamic connectionErrorResponse = new System.Dynamic.ExpandoObject();
                connectionErrorResponse.Success = false;
                connectionErrorResponse.Message = $"Impossibile connettersi all'API. Assicurati che il backend sia in esecuzione: {reqEx.Message}";
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(connectionErrorResponse));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore generico API POST: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                // Crea una risposta di errore
                dynamic errorResponse = new System.Dynamic.ExpandoObject();
                errorResponse.Success = false;
                errorResponse.Message = $"Errore nella comunicazione con il server: {ex.Message}";
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(errorResponse));
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API PUT Error: {ex.Message}");
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API DELETE Error: {ex.Message}");
                return false;
            }
        }

        private void SetAuthorizationHeader(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }
    }
}
