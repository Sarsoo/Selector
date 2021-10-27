using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Selector.Cache
{
    public class RedisCache : ICache<string>
    {
        private readonly IDatabaseAsync Db;

        public RedisCache(
            IDatabaseAsync db
        ) {
            Db = db;
        }

        public async Task<string> Get(string key) => (await Db.StringGetAsync(key)).ToString();

        public async Task<bool> Set(string key, string value) => await Db.StringSetAsync(key, value);
    }
}
