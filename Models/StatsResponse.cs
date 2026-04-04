namespace CacheApi.Models;

public class StatsResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int ExpiringSoon { get; set; }
    public int Expired { get; set; }
}
