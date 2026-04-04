namespace CacheApi.Models;

public class SetResponse
{
    public string Key { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
