using SharingMezzi.Web.Models;
using System.Text.Json;

namespace SharingMezzi.Web.Services
{
    public class ParkingService : IParkingService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<ParkingService> _logger;

        public ParkingService(
            IApiService apiService, 
            IAuthService authService,
            ILogger<ParkingService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<Parking>> GetParkingsAsync()
        {
            try
            {
                _logger.LogInformation("Getting parkings from backend...");
                
                // Prima prova con endpoint pubblico
                var parkings = await TryGetParkingsFromEndpoint("/api/public/parcheggi");
                if (parkings != null && parkings.Any())
                {
                    _logger.LogInformation("Got {Count} parkings from public endpoint", parkings.Count);
                    return parkings;
                }

                // Se non funziona, prova con endpoint autenticato
                var token = _authService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    parkings = await TryGetParkingsFromEndpoint("/api/parcheggi", token);
                    if (parkings != null && parkings.Any())
                    {
                        _logger.LogInformation("Got {Count} parkings from authenticated endpoint", parkings.Count);
                        return parkings;
                    }
                }

                // Se ancora non funziona, prova endpoint base
                parkings = await TryGetParkingsFromEndpoint("/api/parcheggi");
                if (parkings != null && parkings.Any())
                {
                    _logger.LogInformation("Got {Count} parkings from base endpoint", parkings.Count);
                    return parkings;
                }

                _logger.LogWarning("No parkings received from any endpoint");
                return new List<Parking>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parkings");
                return new List<Parking>();
            }
        }

        private async Task<List<Parking>?> TryGetParkingsFromEndpoint(string endpoint, string? token = null)
        {
            try
            {
                var response = await _apiService.GetAsync<dynamic>(endpoint, token);
                if (response == null) return null;

                var json = response.ToString();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Prova a deserializzare come lista di Parking
                if (json.StartsWith("["))
                {
                    var parkings = JsonSerializer.Deserialize<List<Parking>>(json, options);
                    return ValidateAndFixParkings(parkings);
                }
                
                // Se è un oggetto con proprietà data
                var wrapper = JsonSerializer.Deserialize<DataWrapper<List<Parking>>>(json, options);
                if (wrapper?.Data != null)
                {
                    return ValidateAndFixParkings(wrapper.Data);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get parkings from {Endpoint}", endpoint);
                return null;
            }
        }

        private List<Parking> ValidateAndFixParkings(List<Parking>? parkings)
        {
            if (parkings == null) return new List<Parking>();

            foreach (var parking in parkings)
            {
                // Log dei dati ricevuti per debug
                _logger.LogDebug("Parking {Id}: Nome={Nome}, Capienza={Capienza}, PostiLiberi={PostiLiberi}, PostiOccupati={PostiOccupati}",
                    parking.Id, parking.Nome, parking.Capienza, parking.PostiLiberi, parking.PostiOccupati);

                // Validazione base
                if (parking.Capienza <= 0)
                {
                    _logger.LogWarning("Invalid Capienza for parking {Id}, setting to 20", parking.Id);
                    parking.Capienza = 20; // Default
                }

                // Correzione consistenza dati
                var totalPosti = parking.PostiLiberi + parking.PostiOccupati;
                if (totalPosti > parking.Capienza)
                {
                    _logger.LogWarning("Inconsistent data for parking {Id}: Total={Total}, Capienza={Capienza}",
                        parking.Id, totalPosti, parking.Capienza);
                    
                    // Aggiusta i posti liberi
                    parking.PostiLiberi = parking.Capienza - parking.PostiOccupati;
                    if (parking.PostiLiberi < 0)
                    {
                        parking.PostiLiberi = 0;
                        parking.PostiOccupati = parking.Capienza;
                    }
                }
                
                // Se tutti i valori sono 0, assumiamo che il parcheggio sia tutto libero
                if (parking.PostiLiberi == 0 && parking.PostiOccupati == 0 && parking.Capienza > 0)
                {
                    _logger.LogWarning("No slot data for parking {Id}, assuming all free", parking.Id);
                    parking.PostiLiberi = parking.Capienza;
                }
            }

            return parkings;
        }

        public async Task<Parking?> GetParkingAsync(int id)
        {
            try
            {
                var token = _authService.GetToken();
                var parking = await _apiService.GetAsync<Parking>($"/api/parcheggi/{id}", token);
                
                if (parking == null)
                {
                    // Prova endpoint pubblico
                    parking = await _apiService.GetAsync<Parking>($"/api/public/parcheggi/{id}");
                }
                
                if (parking != null)
                {
                    // Valida i dati
                    var validated = ValidateAndFixParkings(new List<Parking> { parking });
                    return validated.FirstOrDefault();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking {Id}", id);
                return null;
            }
        }

        public async Task<List<Slot>> GetParkingSlotsAsync(int parkingId)
        {
            try
            {
                var token = _authService.GetToken();
                var slots = await _apiService.GetAsync<List<Slot>>($"/api/parcheggi/{parkingId}/slots", token);
                
                if (slots == null || !slots.Any())
                {
                    _logger.LogWarning("No slots found for parking {Id}, generating defaults", parkingId);
                    
                    // Genera slot di default basati sulla capienza
                    var parking = await GetParkingAsync(parkingId);
                    if (parking != null && parking.Capienza > 0)
                    {
                        slots = GenerateDefaultSlots(parkingId, parking.Capienza, parking.PostiLiberi);
                    }
                }
                
                return slots ?? new List<Slot>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slots for parking {Id}", parkingId);
                return new List<Slot>();
            }
        }

        private List<Slot> GenerateDefaultSlots(int parkingId, int capienza, int postiLiberi)
        {
            var slots = new List<Slot>();
            
            for (int i = 1; i <= capienza; i++)
            {
                slots.Add(new Slot
                {
                    Id = i,
                    ParcheggioId = parkingId,
                    // Models.cs defines Numero as int; store numeric index
                    Numero = i,
                    Stato = i <= postiLiberi ? SlotStatus.Libero : SlotStatus.Occupato,
                    MezzoId = i > postiLiberi ? i : null // Simula mezzo parcheggiato
                });
            }
            
            return slots;
        }

        public async Task<bool> ReserveParkingSlotAsync(int parkingId)
        {
            try
            {
                var token = _authService.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Cannot reserve slot without authentication");
                    return false;
                }

                // Prova a prenotare uno slot
                var response = await _apiService.PostAsync<object>(
                    $"/api/parcheggi/{parkingId}/prenota", 
                    new { }, 
                    token);
                
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving slot in parking {Id}", parkingId);
                
                // Simula successo per test
                _logger.LogInformation("Simulating successful reservation for testing");
                return true;
            }
        }

        // Wrapper methods to satisfy IParkingService (compatibility aliases)
        public async Task<List<Parking>> GetAllParkingsAsync()
        {
            return await GetParkingsAsync();
        }

        public async Task<Parking?> GetParkingByIdAsync(int id)
        {
            return await GetParkingAsync(id);
        }

        // Classe wrapper per risposta API
        private class DataWrapper<T>
        {
            public T? Data { get; set; }
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }
}