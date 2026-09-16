using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GizaPowerIndustryWebsite.Diagnostics
{
    /// <summary>
    /// Simple in‑memory collector for request performance data.
    /// Thread‑safe and suitable for development / profiling scenarios.
    /// </summary>
    public class PerformanceReport
    {
        private readonly ConcurrentBag<PerformanceEntry> _entries = new();

        public void AddEntry(string method, string path, TimeSpan duration)
        {
            _entries.Add(new PerformanceEntry
            {
                Timestamp = DateTime.UtcNow,
                HttpMethod = method,
                Path = path,
                Duration = duration
            });
        }

        /// <summary>
        /// Returns a snapshot of the collected entries.
        /// </summary>
        public IReadOnlyCollection<PerformanceEntry> GetEntries() => _entries.ToArray();

        /// <summary>
        /// Generates a simple CSV representation of the data.
        /// </summary>
        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,Method,Path,DurationMs");
            foreach (var e in _entries)
            {
                sb.AppendLine($"{e.Timestamp:o},{e.HttpMethod},{e.Path},{e.Duration.TotalMilliseconds}");
            }
            return sb.ToString();
        }
    }

    public class PerformanceEntry
    {
        public DateTime Timestamp { get; set; }
        public string HttpMethod { get; set; }
        public string Path { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
