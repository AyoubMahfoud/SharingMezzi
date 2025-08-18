namespace SharingMezzi.Core.DTOs
{
    public class GrowthStatisticsDto
    {
        public decimal VehicleGrowth { get; set; }
        public decimal UserGrowth { get; set; }
        public decimal TripGrowth { get; set; }
        public decimal RevenueGrowth { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
