using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GizaPowerIndustryWebsite.Models.Metals;
using Microsoft.Extensions.Caching.Memory;

namespace GizaPowerIndustryWebsite.Services
{
    // Live quotes for the header widget: Copper (8831), Aluminum (49768), Lead (959207).
    // Primary source is Investing.com's public quote API so numbers match the LIVE_COMMODITIES widget.
    // MetalpriceAPI remains a fallback if Investing.com is unreachable.
    public class MetalsService : IMetalsService
    {
        private const string CacheKey = "GPI_Metals_Spot_Cache";
        private const double TroyOuncesPerPound = 7000.0 / 480.0;
        private const double TroyOuncesPerMetricTonne = 1_000_000.0 / 31.1034768;
        private const string Symbols = "XCU,ALU,XPB";

        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public MetalsService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<MetalsWidgetDto> GetRatesAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out MetalsWidgetDto? cached) && cached != null)
            {
                return cached;
            }

            await Gate.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(CacheKey, out cached) && cached != null)
                {
                    return cached;
                }

                var live = await FetchInvestingRatesAsync(cancellationToken)
                    ?? await FetchMetalpriceRatesAsync(cancellationToken);

                if (live == null)
                {
                    return new MetalsWidgetDto { Success = false };
                }

                var cacheSeconds = live.Success ? 45 : 15;
                _cache.Set(CacheKey, live, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheSeconds)
                });

                return live;
            }
            finally
            {
                Gate.Release();
            }
        }

        private async Task<MetalsWidgetDto?> FetchInvestingRatesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var copperTask = FetchInvestingInstrumentAsync("8831", "CU", "Copper", "USD/lb", 4, cancellationToken);
                var aluminumTask = FetchInvestingInstrumentAsync("49768", "AL", "Aluminum", "USD/t", 2, cancellationToken);
                var leadTask = FetchInvestingInstrumentAsync("959207", "PB", "Lead", "USD/t", 2, cancellationToken);
                await Task.WhenAll(copperTask, aluminumTask, leadTask);

                var copper = copperTask.Result;
                var aluminum = aluminumTask.Result;
                var lead = leadTask.Result;
                if (copper == null && aluminum == null && lead == null)
                {
                    return null;
                }

                return new MetalsWidgetDto
                {
                    Success = true,
                    Copper = copper,
                    Aluminum = aluminum,
                    Lead = lead,
                    LastUpdated = DateTime.UtcNow.ToString("HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<MetalQuoteDto?> FetchInvestingInstrumentAsync(
            string pairId,
            string symbol,
            string name,
            string unit,
            int decimals,
            CancellationToken cancellationToken)
        {
            var overview = await TryGetJsonAsync(
                $"https://api.investing.com/api/financialdata/{pairId}/overview",
                cancellationToken);
            var quote = overview != null ? MapFromOverview(overview, symbol, name, unit, decimals) : null;
            if (quote != null)
            {
                return quote;
            }

            var chart = await TryGetJsonAsync(
                $"https://api.investing.com/api/financialdata/{pairId}/historical/chart/?period=P1D&interval=PT5M&pointscount=24",
                cancellationToken);
            return chart != null ? MapFromChart(chart, symbol, name, unit, decimals) : null;
        }

        private async Task<JsonDocument?> TryGetJsonAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.Referrer = new Uri("https://www.investing.com/");
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            }
            catch
            {
                return null;
            }
        }

        private static MetalQuoteDto? MapFromOverview(JsonDocument doc, string symbol, string name, string unit, int decimals)
        {
            var root = doc.RootElement;
            var last = FindNumber(root, "last", "last_numeric", "lastPrice", "close");
            if (last == null)
            {
                return null;
            }

            var prev = FindNumber(root, "previousClose", "prevClose", "pc", "previous_close") ?? last.Value;
            var high = FindNumber(root, "high", "dayHigh", "last_high") ?? last.Value;
            var low = FindNumber(root, "low", "dayLow", "last_low") ?? last.Value;
            var change = FindNumber(root, "change", "chg", "change_val") ?? (last.Value - prev);
            var changePct = FindNumber(root, "changePercent", "chg_pct", "change_percent", "changePercentage")
                ?? (prev != 0 ? (last.Value - prev) / prev * 100 : 0);
            var time = FindString(root, "time", "last_time", "timestamp") ?? DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            return BuildQuote(symbol, name, unit, decimals, last.Value, prev, high, low, change, changePct, time);
        }

        private static MetalQuoteDto? MapFromChart(JsonDocument doc, string symbol, string name, string unit, int decimals)
        {
            if (!TryGetCandles(doc.RootElement, out var candles) || candles.Count == 0)
            {
                return null;
            }

            var last = candles[^1];
            var high = candles.Max(c => c.High);
            var low = candles.Min(c => c.Low);
            var prev = candles[0].Open;
            var change = last.Close - prev;
            var changePct = prev != 0 ? change / prev * 100 : 0;
            var time = DateTimeOffset.FromUnixTimeMilliseconds(last.Timestamp).UtcDateTime
                .ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            return BuildQuote(symbol, name, unit, decimals, last.Close, prev, high, low, change, changePct, time);
        }

        private static bool TryGetCandles(JsonElement root, out List<Candle> candles)
        {
            candles = new List<Candle>();
            JsonElement data = root;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataProp))
            {
                data = dataProp;
            }

            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("candles", out var nested))
            {
                data = nested;
            }

            if (data.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var item in data.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() >= 5)
                {
                    candles.Add(new Candle(
                        item[0].GetInt64(),
                        item[1].GetDouble(),
                        item[2].GetDouble(),
                        item[3].GetDouble(),
                        item[4].GetDouble()));
                }
            }

            return candles.Count > 0;
        }

        private static MetalQuoteDto BuildQuote(
            string symbol,
            string name,
            string unit,
            int decimals,
            double last,
            double prev,
            double high,
            double low,
            double change,
            double changePct,
            string time)
        {
            var date = DateTime.UtcNow;
            return new MetalQuoteDto
            {
                Symbol = symbol,
                Name = name,
                Price = Math.Round(last, decimals),
                Open = Math.Round(prev, decimals),
                High = Math.Round(high, decimals),
                Low = Math.Round(low, decimals),
                Change = Math.Round(change, decimals),
                ChangePercent = Math.Round(changePct, 2),
                Unit = unit,
                Date = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                PrevDate = date.AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                Time = time,
                Status = change >= 0 ? "up" : "down"
            };
        }

        private static double? FindNumber(JsonElement element, params string[] names)
        {
            if (element.ValueKind == JsonValueKind.Number && names.Length == 0)
            {
                return element.GetDouble();
            }

            if (element.ValueKind == JsonValueKind.String && double.TryParse(element.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        var nested = FindNumber(item, names);
                        if (nested != null) return nested;
                    }
                }
                return null;
            }

            foreach (var prop in element.EnumerateObject())
            {
                if (names.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        return prop.Value.GetDouble();
                    }
                    if (prop.Value.ValueKind == JsonValueKind.String &&
                        double.TryParse(prop.Value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        return value;
                    }
                }
            }

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                {
                    var nested = FindNumber(prop.Value, names);
                    if (nested != null) return nested;
                }
            }

            return null;
        }

        private static string? FindString(JsonElement element, params string[] names)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var prop in element.EnumerateObject())
            {
                if (names.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        return prop.Value.GetString();
                    }
                    if (prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        return prop.Value.ToString();
                    }
                }
            }

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    var nested = FindString(prop.Value, names);
                    if (!string.IsNullOrWhiteSpace(nested)) return nested;
                }
            }

            return null;
        }

        private async Task<MetalsWidgetDto?> FetchMetalpriceRatesAsync(CancellationToken cancellationToken)
        {
            var apiKey = _configuration["MetalpriceApi:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            var latestTask = FetchLatestAsync(apiKey, cancellationToken);
            var changeTask = FetchChangeAsync(apiKey, cancellationToken);
            await Task.WhenAll(latestTask, changeTask);

            var latest = latestTask.Result;
            if (latest == null || !latest.Success || latest.Rates == null)
            {
                return null;
            }

            var change = changeTask.Result;
            var stamp = DateTimeOffset.FromUnixTimeSeconds(latest.Timestamp);

            return new MetalsWidgetDto
            {
                Success = true,
                Copper = MapMetalpriceQuote(latest, change, "XCU", "CU", "Copper", TroyOuncesPerPound, "USD/lb", 4, stamp),
                Aluminum = MapMetalpriceQuote(latest, change, "ALU", "AL", "Aluminum", TroyOuncesPerMetricTonne, "USD/t", 2, stamp),
                Lead = MapMetalpriceQuote(latest, change, "XPB", "PB", "Lead", TroyOuncesPerMetricTonne, "USD/t", 2, stamp),
                LastUpdated = stamp.UtcDateTime.ToString("HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)
            };
        }

        private async Task<MetalpriceLatestResponse?> FetchLatestAsync(string apiKey, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"https://api.metalpriceapi.com/v1/latest?api_key={Uri.EscapeDataString(apiKey)}&base=USD&currencies={Symbols}";
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<MetalpriceLatestResponse>(JsonOptions, cancellationToken);
            }
            catch
            {
                return null;
            }
        }

        private async Task<MetalpriceChangeResponse?> FetchChangeAsync(string apiKey, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"https://api.metalpriceapi.com/v1/change?api_key={Uri.EscapeDataString(apiKey)}&base=USD&currencies={Symbols}&date_type=recent";
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<MetalpriceChangeResponse>(JsonOptions, cancellationToken);
            }
            catch
            {
                return null;
            }
        }

        private static MetalQuoteDto? MapMetalpriceQuote(
            MetalpriceLatestResponse latest,
            MetalpriceChangeResponse? change,
            string apiSymbol,
            string displaySymbol,
            string name,
            double troyOuncesPerUnit,
            string unit,
            int decimals,
            DateTimeOffset stamp)
        {
            if (latest.Rates == null || !latest.Rates.TryGetValue($"USD{apiSymbol}", out var pricePerOz))
            {
                return null;
            }

            var price = Math.Round(pricePerOz * troyOuncesPerUnit, decimals);
            var previous = price;
            double changeAmount = 0;
            double changePercent = 0;

            if (change?.Rates != null
                && change.Rates.TryGetValue(apiSymbol, out var rate)
                && rate.StartRate > 0
                && rate.EndRate > 0)
            {
                var startPrice = (1.0 / rate.StartRate) * troyOuncesPerUnit;
                var endPrice = (1.0 / rate.EndRate) * troyOuncesPerUnit;
                previous = Math.Round(startPrice, decimals);
                changeAmount = Math.Round(endPrice - startPrice, decimals);
                changePercent = startPrice != 0
                    ? Math.Round((endPrice - startPrice) / startPrice * 100, 2)
                    : 0;
            }

            return BuildQuote(
                displaySymbol,
                name,
                unit,
                decimals,
                price,
                previous,
                price,
                price,
                changeAmount,
                changePercent,
                stamp.UtcDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        }

        private sealed record Candle(long Timestamp, double Open, double High, double Low, double Close);

        private class MetalpriceLatestResponse
        {
            public bool Success { get; set; }
            public string? Base { get; set; }
            public long Timestamp { get; set; }
            public Dictionary<string, double>? Rates { get; set; }
        }

        private class MetalpriceChangeResponse
        {
            public bool Success { get; set; }
            public Dictionary<string, MetalpriceChangeRate>? Rates { get; set; }
        }

        private class MetalpriceChangeRate
        {
            [JsonPropertyName("start_rate")]
            public double StartRate { get; set; }

            [JsonPropertyName("end_rate")]
            public double EndRate { get; set; }

            public double Change { get; set; }

            [JsonPropertyName("change_pct")]
            public double ChangePct { get; set; }
        }
    }
}
