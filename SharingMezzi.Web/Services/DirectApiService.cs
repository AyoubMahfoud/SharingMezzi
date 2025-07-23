using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace SharingMezzi.Web.Services
{
    public class DirectApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public DirectApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            
            // Configure HttpClient to handle redirects appropriately
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            Console.WriteLine($"========= DIRECT API SERVICE =========");
            Console.WriteLine($"BaseUrl configurato: {_baseUrl}");
            Console.WriteLine($"HttpClient creato: {_httpClient.GetHashCode()}");
            Console.WriteLine($"====================================");
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            var fullUrl = $"{_baseUrl}{endpoint}";
            Console.WriteLine($"DirectApiService GET: {fullUrl}");
            
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync(fullUrl);
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"GET Response: {response.StatusCode} - {content}");
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectApiService GET Error: {ex.Message}");
                return default(T);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
        {
            var fullUrl = $"{_baseUrl}{endpoint}";
            
            Console.WriteLine($"========= DIRECT API POST =========");
            Console.WriteLine($"URL completo: {fullUrl}");
            Console.WriteLine($"Data: {JsonConvert.SerializeObject(data)}");
            
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Forza una nuova richiesta HTTP pulita
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await client.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"POST Response Status: {response.StatusCode}");
                Console.WriteLine($"POST Response Content: {responseContent}");
                Console.WriteLine($"==================================");
                
                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        return default(T);
                    }
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                
                // Gestione errori
                dynamic errorResponse = new System.Dynamic.ExpandoObject();
                errorResponse.Success = false;
                errorResponse.Message = $"Errore API: {response.StatusCode} - {responseContent}";
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(errorResponse));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectApiService POST Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                dynamic errorResponse = new System.Dynamic.ExpandoObject();
                errorResponse.Success = false;
                errorResponse.Message = $"Errore di connessione: {ex.Message}";
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(errorResponse));
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, string? token = null)
        {
            var fullUrl = $"{_baseUrl}{endpoint}";
            
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectApiService PUT Error: {ex.Message}");
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, string? token = null)
        {
            var fullUrl = $"{_baseUrl}{endpoint}";
            
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.DeleteAsync(fullUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectApiService DELETE Error: {ex.Message}");
                return false;
            }
        }
    }
}
