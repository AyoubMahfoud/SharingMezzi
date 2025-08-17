using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharingMezzi.Infrastructure.Database;
using SharingMezzi.Core.Entities;

namespace SharingMezzi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly SharingMezziContext _context;
        private readonly ILogger<PublicController> _logger;

        public PublicController(SharingMezziContext context, ILogger<PublicController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottieni mezzi disponibili per homepage pubblica
        /// </summary>
        [HttpGet("mezzi/disponibili")]
        public async Task<IActionResult> GetMezziDisponibili()
        {
            try
            {
                var mezzi = await _context.Mezzi
                    .Where(m => m.Stato == StatoMezzo.Disponibile)
                    .Include(m => m.Parcheggio)
                    .Select(m => new {
                        m.Id,
                        m.Modello,
                        m.Tipo,
                        m.TariffaPerMinuto,
                        m.TariffaFissa,
                        Stato = m.Stato.ToString(),
                        m.LivelloBatteria,
                        IsElettrico = m.Tipo == TipoMezzo.BiciElettrica || m.Tipo == TipoMezzo.Monopattino,
                        ParcheggioAttualeId = m.Parcheggio != null ? m.Parcheggio.Id : (int?)null,
                        NomeParcheggio = m.Parcheggio != null ? m.Parcheggio.Nome : "Non parcheggiato"
                    })
                    .ToListAsync();

                _logger.LogInformation($"Restituiti {mezzi.Count} mezzi disponibili per homepage");
                return Ok(mezzi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero mezzi disponibili pubblici");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottieni parcheggi per homepage pubblica
        /// </summary>
        [HttpGet("parcheggi")]
        public async Task<IActionResult> GetParcheggi()
        {
            try
            {
                var parcheggi = await _context.Parcheggi
                    .Include(p => p.Mezzi.Where(m => m.Stato == StatoMezzo.Disponibile))
                    .Select(p => new {
                        p.Id,
                        p.Nome,
                        p.Indirizzo,
                        // Nota: Le coordinate potrebbero non essere nella entità Parcheggio
                        // Se non ci sono, le impostiamo a valori di default per Torino
                        Latitude = 45.0703, // Default: Torino centro
                        Longitude = 7.6869, // Default: Torino centro
                        Capacita = p.Slots != null ? p.Slots.Count : 20, // Usa slot count o default
                        PostiLiberi = p.Slots != null ? p.Slots.Count(s => s.Stato == StatoSlot.Libero) : 15,
                        MezziPresenti = p.Mezzi.Count(m => m.Stato == StatoMezzo.Disponibile),
                        Mezzi = p.Mezzi.Where(m => m.Stato == StatoMezzo.Disponibile).Select(m => new {
                            m.Id,
                            m.Modello,
                            Tipo = m.Tipo.ToString(),
                            Stato = m.Stato.ToString()
                        })
                    })
                    .ToListAsync();

                _logger.LogInformation($"Restituiti {parcheggi.Count} parcheggi per homepage");
                return Ok(parcheggi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero parcheggi pubblici");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottieni statistiche pubbliche per homepage
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetPublicStats()
        {
            try
            {
                var mezziDisponibili = await _context.Mezzi.CountAsync(m => m.Stato == StatoMezzo.Disponibile);
                var totaleParcheggi = await _context.Parcheggi.CountAsync();
                var corsaAttive = await _context.Corse.CountAsync(c => c.Stato == StatoCorsa.InCorso);
                var totaleUtenti = await _context.Utenti.CountAsync();

                var stats = new {
                    MezziDisponibili = mezziDisponibili,
                    TotaleParcheggi = totaleParcheggi,
                    CorseAttive = corsaAttive,
                    TotaleUtenti = totaleUtenti,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Restituite statistiche pubbliche per homepage");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche pubbliche");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Test endpoint per verificare connettività
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "API Backend funzionante!", 
                timestamp = DateTime.UtcNow,
                endpoint = "public/test"
            });
        }
    }
}