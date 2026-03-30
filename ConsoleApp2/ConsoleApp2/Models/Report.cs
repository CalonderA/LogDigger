using System.Text.Json.Serialization;

namespace ConsoleApp2.Models
{
    public class Report
    {
        [JsonPropertyName("generatedAt")]
        public string GeneratedAt { get; set; } = string.Empty;

        [JsonPropertyName("totalErrors")]
        public int TotalErrors { get; set; }

        [JsonPropertyName("totalWarns")]
        public int TotalWarns { get; set; }

        [JsonPropertyName("topIssues")]
        public List<TopIssue> TopIssues { get; set; } = new();
    }

    public class TopIssue
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;
    }
}
