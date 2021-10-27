using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using SpotifyAPI.Web;

namespace Selector.Cache
{
    public static class JsonSerialiser
    {
        public static async Task<T> Read<T>(this ICache<string> cache, string key)
            => JsonSerializer.Deserialize<T>(await cache.Get(key));

        public static async Task<bool> Write<T>(this ICache<string> cache, string key, T obj)
            => await cache.Set(key, JsonSerializer.Serialize(obj));
    }
}
