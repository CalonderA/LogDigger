using ConsoleApp2.Models;
using ConsoleApp2.Services;

namespace ConsoleApp2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string logFilePath;

                if (args.Length == 0)
                {
                    logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "app.log");
                    Console.WriteLine($"No log file path provided, using default: {logFilePath}");
                }
                else
                {
                    logFilePath = args[0];
                }

                if (!File.Exists(logFilePath))
                {
                    Console.WriteLine($"Error: Log file not found at path: {logFilePath}");
                    Environment.Exit(1);
                    return;
                }

                var processor = new LogProcessor();
                Console.WriteLine($"Processing log file: {logFilePath}");

                var logEntries = await processor.ReadLogFileAsync(logFilePath);
                Console.WriteLine($"Parsed {logEntries.Count} log entries");

                var report = processor.GenerateReport(logEntries);
                
                var reportPath = Path.Combine(Path.GetDirectoryName(logFilePath)!, "report.json");
                await processor.SaveReportAsync(report, reportPath);

                Console.WriteLine($"Report generated: {reportPath}");
                Console.WriteLine();
                Console.WriteLine("Report content:");
                Console.WriteLine(await File.ReadAllTextAsync(reportPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
