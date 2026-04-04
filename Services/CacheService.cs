using CacheApi.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CacheApi.Services;

public class CacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _storage = new();

    public SetResponse Set(string key, string value, int ttlSeconds)
    {
        ValidateKey(key);
        ValidateValue(value);
        ValidateTtl(ttlSeconds);

        var now = DateTime.UtcNow;
        var entry = new CacheEntry
        {
            Key = key,
            Value = value,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(ttlSeconds)
        };

        _storage.AddOrUpdate(key, entry, (_, _) => entry);

        return new SetResponse
        {
            Key = key,
            ExpiresAt = entry.ExpiresAt
        };
    }

    public CacheEntry? Get(string key)
    {
        CleanupExpired();

        if (_storage.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return entry;
            }
            _storage.TryRemove(key, out _);
        }
        return null;
    }

    public bool Delete(string key)
    {
        CleanupExpired();
        return _storage.TryRemove(key, out _);
    }

    public StatsResponse GetStats()
    {
        CleanupExpired();

        var now = DateTime.UtcNow;
        var entries = _storage.Values.ToList();
        var total = entries.Count;
        var active = entries.Count(e => e.ExpiresAt > now);
        var expired = entries.Count(e => e.ExpiresAt <= now);
        var expiringSoon = entries.Count(e => e.ExpiresAt > now && e.ExpiresAt <= now.AddMinutes(5));

        return new StatsResponse
        {
            Total = total,
            Active = active,
            ExpiringSoon = expiringSoon,
            Expired = expired
        };
    }

    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _storage
            .Where(kvp => kvp.Value.ExpiresAt <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _storage.TryRemove(key, out _);
        }
    }

    private void ValidateKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 1 || key.Length > 100)
        {
            throw new ArgumentException("Key must be between 1 and 100 characters.");
        }
        if (!Regex.IsMatch(key, @"^[a-zA-Z0-9_]+$"))
        {
            throw new ArgumentException("Key must contain only Latin letters, digits, and underscores.");
        }
    }

    private void ValidateValue(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 1 || value.Length > 1000)
        {
            throw new ArgumentException("Value must be between 1 and 1000 characters.");
        }
    }

    private void ValidateTtl(int ttlSeconds)
    {
        if (ttlSeconds < 1 || ttlSeconds > 86400)
        {
            throw new ArgumentException("TTL must be between 1 and 86400 seconds (24 hours).");
        }
    }
}
