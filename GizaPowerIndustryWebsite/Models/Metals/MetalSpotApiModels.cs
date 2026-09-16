using System.Text.Json.Serialization;

namespace GizaPowerIndustryWebsite.Models.Metals
{
    public class MetalSpotApiResponse
    {
        [JsonPropertyName("status")]

        public string? Status { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("metal")]
        public string? Metal { get; set; }

        [JsonPropertyName("rate")]
        public MetalSpotRate? Rate { get; set; }
    }

    public class MetalSpotRate
    {
        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("ask")]
        public double Ask { get; set; }

        [JsonPropertyName("bid")]
        public double Bid { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("change")]
        public double Change { get; set; }

        [JsonPropertyName("change_percent")]
        public double ChangePercent { get; set; }
    }

    public class MetalQuoteDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string PrevDate { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = "up";
    }

    public class MetalsWidgetDto
    {
        public bool Success { get; set; }
        public MetalQuoteDto? Copper { get; set; }
        public MetalQuoteDto? Aluminum { get; set; }
        public MetalQuoteDto? Lead { get; set; }
        public string LastUpdated { get; set; } = string.Empty;
    }
}
