namespace GizaPowerIndustryWebsite.Models
{
    public class HighVoltageModel
    {
        public string ProductRang { get; set; }
        public string ShortDescraption { get; set; }
        public string VoltageGrade { get; set; }
        public string CoductorType { get; set; }
        public string ProductCode { get; set; }
        public string Area { get; set; }          // might be string or decimal
        public decimal? MaxResistanceDC20 { get; set; }
        public decimal? MaxResistanceDC90Flat { get; set; }
        public decimal? MaxResistanceDC90Trefoil { get; set; }
        public decimal? Operation { get; set; }    // capacitance
        public decimal? InductanceFlat { get; set; }
        public decimal? InductanceTrefoil { get; set; }
        public decimal? ScreenCoductorThickness { get; set; }
        public decimal? InsulationThickness { get; set; }
        public decimal? InsulationScreenThickness { get; set; }
        public decimal? OuterSheathThickness { get; set; }
        public decimal? ApproxDiameter { get; set; }
        public decimal? ApproxWeight { get; set; }

        // For the second table
        public string LayingTerfoilTypeEarthing { get; set; }
        public decimal? LayingTerfoilDirectBurial { get; set; }
        public decimal? LayingTerfoilInAir { get; set; }
        public string LayingFlatTypeEarthing { get; set; }
        public decimal? LayingFlatDirectBurial { get; set; }
        public decimal? LayingFlatInAir { get; set; }
    }
}
