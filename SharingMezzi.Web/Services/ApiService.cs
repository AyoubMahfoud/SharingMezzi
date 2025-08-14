using System.Text;
using System.Text.Json;

namespace SharingMezzi.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/api/";
            
            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }

        /// <summary>
        /// GET request - Compatibile con interfaccia esistente
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            try
            {
                _logger.LogDebug($"GET request to: {endpoint}");
                
                // Set token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, GetJsonOptions());
                }
                else
                {
                    _logger.LogWarning($"GET request failed: {response.StatusCode} - {endpoint}");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GET request to {endpoint}");
                return default(T);
            }
        }

        /// <summary>
        /// POST request - Compatibile con interfaccia esistente
        /// </summary>
        public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                _logger.LogDebug($"POST request to: {endpoint}");

                // Set token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(data, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseJson, GetJsonOptions());
                }
                else
                {
                    _logger.LogWarning($"POST request failed: {response.StatusCode} - {endpoint}");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in POST request to {endpoint}");
                return default(T);
            }
        }

        /// <summary>
        /// PUT request - Compatibile con interfaccia esistente
        /// </summary>
        public async Task<T?> PutAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                _logger.LogDebug($"PUT request to: {endpoint}");

                // Set token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(data, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseJson, GetJsonOptions());
                }
                else
                {
                    _logger.LogWarning($"PUT request failed: {response.StatusCode} - {endpoint}");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PUT request to {endpoint}");
                return default(T);
            }
        }

        /// <summary>
        /// DELETE request - Compatibile con interfaccia esistente
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint, string? token = null)
        {
            try
            {
                _logger.LogDebug($"DELETE request to: {endpoint}");

                // Set token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DELETE request to {endpoint}");
                return false;
            }
        }

        /// <summary>
        /// Set authorization token (metodi di compatibilità)
        /// </summary>
        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Clear authorization token (metodi di compatibilità)
        /// </summary>
        public void ClearAuthorizationToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// JSON serialization options
        /// </summary>
        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }
    }
}