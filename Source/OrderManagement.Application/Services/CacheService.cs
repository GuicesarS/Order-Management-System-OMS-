using Microsoft.Extensions.Caching.Memory;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Application.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly HashSet<string> _keys = new();

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    public Task<T?> GetAsync<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue) 
            options.AbsoluteExpirationRelativeToNow = expiration;

        _memoryCache.Set(key, value, options);

        _keys.Add(key);

        return Task.CompletedTask;
    }
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var result = await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            if (expiration.HasValue)
                entry.AbsoluteExpirationRelativeToNow = expiration;

            var value = await factory();

            return value;
        });

        _keys.Add(key);

        return result ?? throw new InvalidOperationException($"Cache factory returned null for key: {key}");
    }

    public Task RemoveAsync(string key)
    {
       _memoryCache.Remove(key);

        _keys.Remove(key);

        return Task.CompletedTask;
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var keysToRemove = _keys.Where(k => k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var key in keysToRemove)
        {
           await RemoveAsync(key);
        }
    }

    public Task<bool> ExistsAsync(string key)
    {
        var exists = _memoryCache.TryGetValue(key, out _);
        return Task.FromResult(exists);

    }

}
