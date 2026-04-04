namespace CacheApi.Models;

public class SetRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int TtlSeconds { get; set; }
}
