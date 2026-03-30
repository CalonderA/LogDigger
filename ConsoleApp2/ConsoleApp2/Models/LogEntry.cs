namespace ConsoleApp2.Models
{
    public enum LogLevel
    {
        INFO,
        WARN,
        ERROR,
        DEBUG
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;

        public static LogEntry? Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var pattern = @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\] \[(INFO|WARN|ERROR|DEBUG)\] (.+)$";
            var match = System.Text.RegularExpressions.Regex.Match(line, pattern);

            if (!match.Success)
                return null;

            if (DateTime.TryParseExact(match.Groups[1].Value, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var timestamp))
            {
                if (Enum.TryParse<LogLevel>(match.Groups[2].Value, out var level))
                {
                    return new LogEntry
                    {
                        Timestamp = timestamp,
                        Level = level,
                        Message = match.Groups[3].Value
                    };
                }
            }

            return null;
        }
    }
}
