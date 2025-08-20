using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharingMezzi.Web.Models;
using SharingMezzi.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharingMezzi.Web.Pages.Admin
{
    public class ParkingsModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IParkingService _parkingService;

        public List<Parking> Parkings { get; set; } = new List<Parking>();
        public int TotalParkings { get; set; }
        public int AvailableSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? ErrorMessage { get; set; }

        public ParkingsModel(IAuthService authService, IParkingService parkingService)
        {
            _authService = authService;
            _parkingService = parkingService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verifica autenticazione manuale
            if (!_authService.IsAuthenticated())
            {
                return RedirectToPage("/Login");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser?.Ruolo != UserRole.Admin)
            {
                return RedirectToPage("/AccessDenied");
            }

            try
            {
                await LoadParkingsData();
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nel caricamento dei dati: {ex.Message}";
                // Carica dati di fallback
                LoadFallbackData();
            }

            return Page();
        }

        private async Task LoadParkingsData()
        {
            try
            {
                var parkings = await _parkingService.GetAllParkingsAsync();
                if (parkings != null)
                {
                    Parkings = parkings.Select(p => new Parking
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Indirizzo = p.Indirizzo,
                        Capienza = p.Capienza,
                        PostiLiberi = p.PostiLiberi,
                        PostiOccupati = p.PostiOccupati,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Slots = p.Slots?.Select(s => new Slot
                        {
                            Id = s.Id,
                            Numero = s.Numero,
                            Stato = s.Stato,
                            MezzoId = s.MezzoId
                        }).ToList() ?? new List<Slot>()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento parcheggi: {ex.Message}");
                LoadFallbackData();
            }
        }

        private void CalculateStatistics()
        {
            if (Parkings?.Any() == true)
            {
                TotalParkings = Parkings.Count;
                AvailableSlots = Parkings.Sum(p => p.PostiLiberi);
                OccupiedSlots = Parkings.Sum(p => p.PostiOccupati);
                TotalRevenue = OccupiedSlots * 2.50m; // Stima ricavi
            }
        }

        private void LoadFallbackData()
        {
            // Dati di esempio per la visualizzazione
            Parkings = new List<Parking>
            {
                new Parking
                {
                    Id = 1,
                    Nome = "Parcheggio Centrale",
                    Indirizzo = "Via Roma 123, Milano",
                    Capienza = 50,
                    PostiLiberi = 35,
                    PostiOccupati = 15,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now,
                    Slots = Enumerable.Range(1, 50).Select(i => new Slot
                    {
                        Id = i,
                        Numero = i,
                        Stato = i <= 35 ? SlotStatus.Libero : SlotStatus.Occupato,
                        MezzoId = i > 35 ? i : null
                    }).ToList()
                },
                new Parking
                {
                    Id = 2,
                    Nome = "Stazione Nord",
                    Indirizzo = "Piazza Garibaldi 45, Milano",
                    Capienza = 30,
                    PostiLiberi = 20,
                    PostiOccupati = 10,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    UpdatedAt = DateTime.Now,
                    Slots = Enumerable.Range(1, 30).Select(i => new Slot
                    {
                        Id = i + 50,
                        Numero = i,
                        Stato = i <= 20 ? SlotStatus.Libero : SlotStatus.Occupato,
                        MezzoId = i > 20 ? i + 50 : null
                    }).ToList()
                }
            };

            CalculateStatistics();
        }
    }
}
