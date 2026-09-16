namespace GizaPowerIndustryWebsite.Models
{
    public class TechnicalRowModel
    {
        public string ProductRang { get; set; }
        public string ShortDescraption { get; set; }
        public string VoltageGrade { get; set; }
        public string CoductorType { get; set; }   // "Copper" or "Aluminum"
        public string Type { get; set; }           // "Stranded", "Flexible", etc.
        public string ProductCode { get; set; }
        public decimal Area { get; set; }
        public decimal MaxResistanceDC1 { get; set; }
        public decimal MaxResistanceDC2 { get; set; }
        public decimal? GroundFlat { get; set; }
        public decimal? GroundTrefoil { get; set; }
        public decimal? Groundduct { get; set; }
        public decimal? AirFlatseperated { get; set; }
        public decimal? AirFlattouched { get; set; }
        public decimal? AirTrefoil { get; set; }
        public decimal ApproxDiameter { get; set; }
        public decimal ApproxWeight { get; set; }
        public string ProductShort { get; set; }
        public string ProductHeader { get; set; }
        
        public decimal CurrentRateFree { get; set; }
        public decimal CurrentRateInPipe { get; set; }

    }
}
