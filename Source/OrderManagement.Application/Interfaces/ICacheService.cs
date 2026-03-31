namespace OrderManagement.Application.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    Task RemoveAsync(string key);

    Task RemoveByPatternAsync(string pattern);

    Task<bool> ExistsAsync(string key);

}
