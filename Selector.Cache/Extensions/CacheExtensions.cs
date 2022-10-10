using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Selector.Cache;

public static class CacheExtensions
{
    public static async Task<int?> GetTrackDuration(this IDatabaseAsync cache, string trackId)
    {
        return (int?) await cache?.HashGetAsync(Key.Track(trackId), Key.Duration);
    }

    public static async Task SetTrackDuration(this IDatabaseAsync cache, string trackId, int duration, TimeSpan? expiry = null)
    {
        var trackCacheKey = Key.Track(trackId);

        await cache?.HashSetAsync(trackCacheKey, Key.Duration, duration);

        if(expiry is not null)
        {
            await cache?.KeyExpireAsync(trackCacheKey, expiry);
        }
    }
}

