using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharingMezzi.Web.Models;
using System.Net.Http;
using System.Text;

namespace SharingMezzi.Web.Pages
{
    public class TestApiModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TestApiModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public bool ApiStatus { get; set; }
        public string ApiStatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string TestResult { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            await CheckApiStatus();
        }

        public async Task OnPostAsync()
        {
            await CheckApiStatus();

            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"];
                try
                {
                    var loginRequest = new { Email, Password };
                    var json = JsonConvert.SerializeObject(loginRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Log dei dettagli della richiesta
                    TestResult = $"Invio richiesta POST a: {apiUrl}/api/auth/login\n";
                    TestResult += $"Payload:\n{JsonConvert.SerializeObject(loginRequest, Formatting.Indented)}\n\n";

                    var response = await _httpClient.PostAsync($"{apiUrl}/api/auth/login", content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    TestResult += $"Codice di stato: {(int)response.StatusCode} ({response.StatusCode})\n";
                    
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        try
                        {
                            // Prova a formattare la risposta JSON
                            var formattedJson = JsonConvert.SerializeObject(
                                JsonConvert.DeserializeObject(responseContent), 
                                Formatting.Indented);
                            TestResult += $"Risposta:\n{formattedJson}";
                        }
                        catch
                        {
                            TestResult += $"Risposta:\n{responseContent}";
                        }
                    }
                    else
                    {
                        TestResult += "Risposta vuota dal server";
                    }
                }
                catch (Exception ex)
                {
                    TestResult += $"Errore: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        TestResult += $"\nDettagli: {ex.InnerException.Message}";
                    }
                }
            }
        }

        private async Task CheckApiStatus()
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"];
            
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/api/auth");
                ApiStatus = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
                
                if (ApiStatus)
                {
                    ApiStatusMessage = $"API raggiungibile. Stato: {response.StatusCode}";
                }
                else
                {
                    ApiStatusMessage = $"API non risponde correttamente. Stato: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ApiStatus = false;
                ApiStatusMessage = $"Errore: {ex.Message}";
            }
        }
    }
}
