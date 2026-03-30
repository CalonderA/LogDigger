using ConsoleApp2.Models;

namespace ConsoleApp2.Services
{
    public class LogProcessor
    {
        public async Task<List<LogEntry>> ReadLogFileAsync(string filePath)
        {
            var logEntries = new List<LogEntry>();

            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                
                foreach (var line in lines)
                {
                    var entry = LogEntry.Parse(line);
                    if (entry != null)
                    {
                        logEntries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read log file: {ex.Message}", ex);
            }

            return logEntries;
        }

        public Report GenerateReport(List<LogEntry> logEntries)
        {
            var filteredEntries = logEntries.Where(e => e.Level == LogLevel.ERROR || e.Level == LogLevel.WARN).ToList();
            
            var errorEntries = filteredEntries.Where(e => e.Level == LogLevel.ERROR).ToList();
            var warnEntries = filteredEntries.Where(e => e.Level == LogLevel.WARN).ToList();

            var errorGroups = errorEntries.GroupBy(e => e.Message)
                .Select(g => new { Message = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault();

            var warnGroups = warnEntries.GroupBy(e => e.Message)
                .Select(g => new { Message = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault();

            var report = new Report
            {
                GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                TotalErrors = errorEntries.Count,
                TotalWarns = warnEntries.Count,
                TopIssues = new List<TopIssue>()
            };

            if (errorGroups != null)
            {
                report.TopIssues.Add(new TopIssue
                {
                    Message = errorGroups.Message,
                    Count = errorGroups.Count,
                    Level = "ERROR"
                });
            }

            if (warnGroups != null)
            {
                report.TopIssues.Add(new TopIssue
                {
                    Message = warnGroups.Message,
                    Count = warnGroups.Count,
                    Level = "WARN"
                });
            }

            return report;
        }

        public async Task SaveReportAsync(Report report, string outputPath)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(outputPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save report: {ex.Message}", ex);
            }
        }
    }
}
