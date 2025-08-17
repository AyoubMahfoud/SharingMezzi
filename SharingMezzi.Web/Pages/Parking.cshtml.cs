using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;
using System.Text.Json;

namespace SharingMezzi.Web.Pages
{
    public class ParkingModel : PageModel
    {
        private readonly IParkingService _parkingService;
        private readonly IAuthService _authService;
        private readonly ILogger<ParkingModel> _logger;

        public ParkingModel(
            IParkingService parkingService, 
            IAuthService authService,
            ILogger<ParkingModel> logger)
        {
            _parkingService = parkingService;
            _authService = authService;
            _logger = logger;
        }

        public List<Parking> Parkings { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool IsLoading { get; set; } = true;
        public string ParkingsJson { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("=== PARKING PAGE LOAD START ===");
                
                // Verifica autenticazione (opzionale per visualizzazione)
                var isAuthenticated = _authService.IsAuthenticated();
                _logger.LogInformation("User authenticated: {IsAuth}", isAuthenticated);

                // Carica i parcheggi dal backend
                Parkings = await _parkingService.GetParkingsAsync();
                
                // Se i dati non sono completi, usa valori di fallback
                if (Parkings == null || !Parkings.Any())
                {
                    _logger.LogWarning("No parkings received from backend, using fallback data");
                    Parkings = GetFallbackParkings();
                }
                else
                {
                    // Verifica e correggi eventuali dati mancanti
                    foreach (var parking in Parkings)
                    {
                        // Log dei dati ricevuti
                        _logger.LogInformation("Parking {Id} - {Nome}: Capienza={Capienza}, PostiLiberi={PostiLiberi}, PostiOccupati={PostiOccupati}",
                            parking.Id, parking.Nome, parking.Capienza, parking.PostiLiberi, parking.PostiOccupati);
                        
                        // Validazione e correzione dati
                        if (parking.Capienza <= 0)
                        {
                            _logger.LogWarning("Invalid Capienza for parking {Id}, setting default", parking.Id);
                            parking.Capienza = 20; // Default
                        }
                        
                        // Se PostiLiberi + PostiOccupati > Capienza, correggilo
                        if (parking.PostiLiberi + parking.PostiOccupati > parking.Capienza)
                        {
                            _logger.LogWarning("Invalid slots count for parking {Id}, adjusting", parking.Id);
                            parking.PostiLiberi = parking.Capienza - parking.PostiOccupati;
                        }
                        
                        // Se entrambi sono 0 ma c'è capienza, assumiamo tutto libero
                        if (parking.PostiLiberi == 0 && parking.PostiOccupati == 0 && parking.Capienza > 0)
                        {
                            _logger.LogWarning("No slot data for parking {Id}, assuming all free", parking.Id);
                            parking.PostiLiberi = parking.Capienza;
                        }
                    }
                }
                
                _logger.LogInformation("Loaded {Count} parkings", Parkings.Count);
                
                // Serializza per JavaScript
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                
                ParkingsJson = JsonSerializer.Serialize(Parkings.Select(p => new 
                {
                    id = p.Id,
                    nome = p.Nome,
                    indirizzo = p.Indirizzo,
                    capienza = p.Capienza,
                    postiLiberi = p.PostiLiberi,
                    postiOccupati = p.PostiOccupati
                }), options);
                
                // Log delle statistiche
                var totalSpots = Parkings.Sum(p => p.Capienza);
                var availableSpots = Parkings.Sum(p => p.PostiLiberi);
                var occupiedSpots = Parkings.Sum(p => p.PostiOccupati);
                
                _logger.LogInformation("Parking stats - Total: {Total}, Available: {Available}, Occupied: {Occupied}", 
                    totalSpots, availableSpots, occupiedSpots);

                if (!Parkings.Any())
                {
                    ErrorMessage = "Nessun parcheggio disponibile al momento.";
                }

                IsLoading = false;
                _logger.LogInformation("=== PARKING PAGE LOAD SUCCESS ===");
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parking page");
                ErrorMessage = $"Errore nel caricamento dei parcheggi: {ex.Message}";
                IsLoading = false;
                Parkings = GetFallbackParkings();
                ParkingsJson = JsonSerializer.Serialize(Parkings);
                return Page();
            }
        }

        private List<Parking> GetFallbackParkings()
        {
            // Dati di fallback basati su quello che hai nel DB
            return new List<Parking>
            {
                new Parking 
                { 
                    Id = 1, 
                    Nome = "Centro Storico", 
                    Indirizzo = "Piazza Castello 1",
                    Capienza = 25,
                    PostiLiberi = 20,
                    PostiOccupati = 5
                },
                new Parking 
                { 
                    Id = 2, 
                    Nome = "Politecnico", 
                    Indirizzo = "Corso Duca Abruzzi 24",
                    Capienza = 40,
                    PostiLiberi = 30,
                    PostiOccupati = 10
                },
                new Parking 
                { 
                    Id = 3, 
                    Nome = "Porta Nuova", 
                    Indirizzo = "Piazza Carlo Felice 1",
                    Capienza = 30,
                    PostiLiberi = 25,
                    PostiOccupati = 5
                }
            };
        }

        public async Task<IActionResult> OnGetRefreshAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing parkings data");
                
                var parkings = await _parkingService.GetParkingsAsync();
                
                if (parkings == null || !parkings.Any())
                {
                    parkings = GetFallbackParkings();
                }
                
                return new JsonResult(new { 
                    success = true, 
                    data = parkings.Select(p => new {
                        id = p.Id,
                        nome = p.Nome,
                        indirizzo = p.Indirizzo,
                        capienza = p.Capienza,
                        postiLiberi = p.PostiLiberi,
                        postiOccupati = p.PostiOccupati
                    }),
                    stats = new {
                        total = parkings.Count,
                        totalCapacity = parkings.Sum(p => p.Capienza),
                        availableSpots = parkings.Sum(p => p.PostiLiberi),
                        occupiedSpots = parkings.Sum(p => p.PostiOccupati),
                        occupancyRate = parkings.Sum(p => p.Capienza) > 0 ? 
                            Math.Round((double)parkings.Sum(p => p.PostiOccupati) / parkings.Sum(p => p.Capienza) * 100, 1) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing parkings");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            try
            {
                _logger.LogInformation("Loading details for parking {Id}", id);
                
                var parking = await _parkingService.GetParkingAsync(id);
                
                if (parking == null)
                {
                    // Prova con i dati di fallback
                    parking = GetFallbackParkings().FirstOrDefault(p => p.Id == id);
                    
                    if (parking == null)
                    {
                        return new JsonResult(new { success = false, message = "Parcheggio non trovato" });
                    }
                }
                
                // Prova a caricare gli slot
                var slots = await _parkingService.GetParkingSlotsAsync(id);
                
                return new JsonResult(new { 
                    success = true, 
                    data = new {
                        Id = parking.Id,
                        Nome = parking.Nome,
                        Indirizzo = parking.Indirizzo,
                        Capienza = parking.Capienza,
                        PostiLiberi = parking.PostiLiberi,
                        PostiOccupati = parking.PostiOccupati
                    },
                    slots = (slots?.Select(s => (object)new {
                        id = s.Id,
                        numero = s.Numero,
                        stato = s.Stato.ToString(),
                        isOccupied = s.Stato == SlotStatus.Occupato
                    }).ToList()) ?? new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parking details for {Id}", id);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostReserveAsync([FromBody] ReserveRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to reserve spot in parking {Id}", request.Id);
                
                if (!_authService.IsAuthenticated())
                {
                    return new JsonResult(new { success = false, message = "Devi effettuare l'accesso per prenotare" });
                }

                var parking = await _parkingService.GetParkingAsync(request.Id);
                if (parking == null)
                {
                    return new JsonResult(new { success = false, message = "Parcheggio non trovato" });
                }

                if (parking.PostiLiberi <= 0)
                {
                    return new JsonResult(new { success = false, message = "Parcheggio pieno" });
                }

                // Simula la prenotazione (il backend dovrebbe gestire questo)
                var success = await _parkingService.ReserveParkingSlotAsync(request.Id);
                
                if (success)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = "Posto prenotato con successo!",
                        slotNumber = $"P{request.Id}-{DateTime.Now.Ticks % 100}",
                        parkingName = parking.Nome
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Errore nella prenotazione. Riprova." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving parking slot");
                return new JsonResult(new { 
                    success = false, 
                    message = "Errore durante la prenotazione" 
                });
            }
        }

        // Classe per la richiesta di prenotazione
        public class ReserveRequest
        {
            public int Id { get; set; }
        }

        // Metodi helper per le statistiche
        public int GetTotalParkings() => Parkings.Count;
        public int GetTotalCapacity() => Parkings.Sum(p => p.Capienza);
        public int GetAvailableSpots() => Parkings.Sum(p => p.PostiLiberi);
        public int GetOccupiedSpots() => Parkings.Sum(p => p.PostiOccupati);
        public double GetOccupancyRate() => GetTotalCapacity() > 0 ? 
            Math.Round((double)GetOccupiedSpots() / GetTotalCapacity() * 100, 1) : 0;

        // Metodi per lo stato visuale
        public string GetAvailabilityStatus(Parking parking)
        {
            if (parking.Capienza == 0) return "Dati non disponibili";
            
            var percentage = (double)parking.PostiLiberi / parking.Capienza * 100;
            
            return percentage switch
            {
                > 50 => "Molti posti disponibili",
                > 20 => "Pochi posti disponibili", 
                > 0 => "Quasi pieno",
                _ => "Parcheggio pieno"
            };
        }

        public string GetAvailabilityClass(Parking parking)
        {
            if (parking.Capienza == 0) return "bg-secondary";
            
            var percentage = (double)parking.PostiLiberi / parking.Capienza * 100;
            
            return percentage switch
            {
                > 50 => "bg-success",
                > 20 => "bg-warning",
                _ => "bg-danger"
            };
        }

        public string GetAvailabilityIcon(Parking parking)
        {
            if (parking.Capienza == 0) return "fas fa-question-circle text-secondary";
            
            var percentage = (double)parking.PostiLiberi / parking.Capienza * 100;
            
            return percentage switch
            {
                > 50 => "fas fa-check-circle text-success",
                > 20 => "fas fa-exclamation-triangle text-warning",
                _ => "fas fa-times-circle text-danger"
            };
        }
    }
}