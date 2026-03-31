using Microsoft.Extensions.Caching.Memory;

namespace OrderManagement.Application.Cache;

public static class CachePolicies
{
    public static MemoryCacheEntryOptions Customer => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    };

    public static MemoryCacheEntryOptions Product => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public static MemoryCacheEntryOptions Order => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    };

    public static MemoryCacheEntryOptions User => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
        // SlidingExpiration = TimeSpan.FromMinutes(10) - Future
    };
}
