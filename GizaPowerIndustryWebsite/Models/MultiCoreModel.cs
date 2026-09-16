namespace GizaPowerIndustryWebsite.Models
{
    public class MultiCoreRowModel
    {
        public string ProductRang { get; set; }
        public string ShortDescraption { get; set; }
        public string VoltageGrade { get; set; }
        public string CoductorType { get; set; }   // "Copper" / "Aluminum"
        public string Type { get; set; }
        public string ProductCode { get; set; }
        public string? PahseArea { get; set; }      // Phase cross‑section
        public string? NeutralArea { get; set; }    // Neutral cross‑section (nullable)
        public string MaxResistanceDC1 { get; set; }
        public string MaxResistanceDC2 { get; set; }
        public decimal? CurrantRateGround { get; set; }
        public decimal? CurrantRateDuct { get; set; }
        public decimal? CurrantRateAir { get; set; }
        public decimal ApproxDiameter { get; set; }
        public decimal ApproxWeight { get; set; }
        public string ProductShort { get; set; }
    }
}
