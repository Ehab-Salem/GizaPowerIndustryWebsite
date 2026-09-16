namespace GizaPowerIndustryWebsite.Models
{
    public class MediumTechincalModel
    {
        public string ProductRang { get; set; }
        public string ShortDescraption { get; set; }
        public string VoltageGrade { get; set; }
        public string CoductorType { get; set; }   // "Copper" / "Aluminum"
        public string ProductCode { get; set; }
        public decimal Area { get; set; }

        // Common columns
        public decimal MaxResistanceDC20 { get; set; }
        public decimal? Operation { get; set; }       // Capacitance (nullable for single-core)
        public decimal? Inductance { get; set; }
        public decimal? InductanceFlat { get; set; }
        public decimal? InductanceTrefoil { get; set; }

        // Single-core specific
        public decimal? MaxResistanceDC90Flat { get; set; }
        public decimal? MaxResistanceDC90Trefoil { get; set; }
        public decimal? CurrentRateGRoundFlat { get; set; }
        public decimal? CurrentRateGRoundTrefoil { get; set; }
        public decimal? CurrentRateGRoundDuct { get; set; }
        public decimal? CurrentRateAirFlat { get; set; }
        public decimal? CurrentRateAirTrefoil { get; set; }

        // Multi-core specific
        public decimal? MaxResistanceDC90 { get; set; }   // single value for multi-core
        public decimal? CurrentRateGRound { get; set; }
        public decimal? CurrentRateGRoundAir { get; set; }

        public decimal ApproxDiameter { get; set; }
        public decimal ApproxWeight { get; set; }
        public string ProductShort { get; set; }
    }
}
