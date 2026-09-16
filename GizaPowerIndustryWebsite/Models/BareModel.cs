namespace GizaPowerIndustryWebsite.Models
{
    public class BareModel
    {
        public string  ProductName { get; set; }
        public string StanderName { get; set; }
        public string ProductCode { get; set; }
        public string Area { get; set; }
        public string Areacmil { get; set; }
        public string NOWires { get; set; }
        public string NOWiresSteel { get; set; }
        public decimal Resistance { get; set; }
        public decimal Diameter { get; set; }
        public decimal BreakingLoad { get; set; }
        public decimal RatedCurrent { get; set; }
        public decimal ApproxWeight { get; set; }
        public string AreaUsingAWKCmil { get; set; }
        public string AreaUsingAWmm { get; set; }
        public decimal InsulationThickness { get; set; }
    }
}
