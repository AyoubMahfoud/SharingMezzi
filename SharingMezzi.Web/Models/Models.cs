using System.ComponentModel.DataAnnotations;

namespace SharingMezzi.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public UserRole Ruolo { get; set; }
        public decimal Credito { get; set; }
        public int PuntiEco { get; set; }
        public decimal CreditoMinimo { get; set; }
        public UserStatus Stato { get; set; }
        public DateTime? DataSospensione { get; set; }
        public string? MotivoSospensione { get; set; }
        public DateTime DataRegistrazione { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // English properties for admin compatibility
        public string FirstName { get => Nome; set => Nome = value; }
        public string LastName { get => Cognome; set => Cognome = value; }
        public string Role { get => Ruolo.ToString(); set => Ruolo = Enum.Parse<UserRole>(value); }
        public bool IsActive { get => Stato == UserStatus.Attivo; set => Stato = value ? UserStatus.Attivo : UserStatus.Sospeso; }
        public decimal Balance { get => Credito; set => Credito = value; }
        public int TotalTrips { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class Vehicle
    {
        public int Id { get; set; }
        public string Modello { get; set; } = string.Empty;
        public VehicleType Tipo { get; set; }
        public bool IsElettrico { get; set; }
        public VehicleStatus Stato { get; set; }
        public int? LivelloBatteria { get; set; }
        public decimal TariffaPerMinuto { get; set; }
        public decimal TariffaFissa { get; set; }
        public DateTime? UltimaManutenzione { get; set; }
        public int? ParcheggioId { get; set; }
        public Parking? Parcheggio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Parking
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Indirizzo { get; set; } = string.Empty;
        public int Capienza { get; set; }
        public int PostiLiberi { get; set; }
        public int PostiOccupati { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Slot> Slots { get; set; } = new();
    }

    public class Slot
    {
        public int Id { get; set; }
        public int ParcheggioId { get; set; }
        public Parking? Parcheggio { get; set; }
        public int Numero { get; set; }
        public SlotStatus Stato { get; set; }
        public int? MezzoId { get; set; }
        public Vehicle? Mezzo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Trip
    {
        public int Id { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime? Fine { get; set; }
        public int DurataMinuti { get; set; }
        public decimal CostoTotale { get; set; }
        public TripStatus Stato { get; set; }
        public int UtenteId { get; set; }
        public User? Utente { get; set; }
        public int MezzoId { get; set; }
        public Vehicle? Mezzo { get; set; }
        public int ParcheggioPartenzaId { get; set; }
        public Parking? ParcheggioPartenza { get; set; }
        public int? ParcheggioDestinazioneId { get; set; }
        public Parking? ParcheggioDestinazione { get; set; }
        public decimal? DistanzaPercorsa { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? PuntiEcoAssegnati { get; set; }

        // Italian properties for compatibility
        public DateTime DataInizio => Inizio;
        public DateTime? DataFine => Fine;
    }

    public class Recharge
    {
        public int Id { get; set; }
        public int UtenteId { get; set; }
        public User? Utente { get; set; }
        public decimal Importo { get; set; }
        public PaymentMethod MetodoPagamento { get; set; }
        public PaymentStatus StatoPagamento { get; set; }
        public DateTime DataRicarica { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Added properties for compatibility with views
        public PaymentStatus Stato { get => StatoPagamento; set => StatoPagamento = value; }
        public decimal SaldoFinale { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Cognome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "La password e la conferma password non corrispondono.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        [Required]
        [Display(Name = "Accetto i termini e condizioni")]
        public bool AcceptTerms { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public User? User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime Expires { get; set; } // Compatibilità per il vecchio codice
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RechargeRequest
    {
        [Required]
        public decimal Importo { get; set; }

        [Required]
        public PaymentMethod MetodoPagamento { get; set; }

        public string? PaymentDetails { get; set; }
    }

    public class MaintenanceRequest
    {
        [Required]
        public int MezzoId { get; set; }

        [Required]
        public string Descrizione { get; set; } = string.Empty;

        public string? Categoria { get; set; }
    }

    public enum UserRole
    {
        User = 0,
        Admin = 1,
        Amministratore = 1  // Alias per Admin per compatibilità con backend
    }

    public enum UserStatus
    {
        Attivo = 0,
        Sospeso = 1,
        Cancellato = 2
    }

    public enum VehicleType
    {
        Bicicletta = 0,
        Scooter = 1,
        Auto = 2,
        Monopattino = 3,
        EBike = 4
    }

    public enum VehicleStatus
    {
        Disponibile = 0,
        InUso = 1,
        Manutenzione = 2,
        Fuori_Servizio = 3
    }

    public enum SlotStatus
    {
        Libero = 0,
        Occupato = 1,
        Fuori_Servizio = 2
    }

    public enum TripStatus
    {
        InCorso = 0,
        Completata = 1,
        Annullata = 2
    }

    public enum PaymentMethod
    {
        CreditoWallet = 0,
        CartaCredito = 1,
        PayPal = 2,
        Bonifico = 3
    }

    public enum PaymentStatus
    {
        Completato = 0,
        InAttesa = 1,
        Fallito = 2,
        Annullato = 3
    }

    public class UserStatistics
    {
        public int UserId { get; set; }
        public int TotalTrips { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastLogin { get; set; }
        public TimeSpan AverageDistance { get; set; }
        public int EcoPoints { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
