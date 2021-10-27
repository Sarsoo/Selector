using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Selector.Cache
{
    public interface ICache<TKey>
    {
        public Task<string> Get(TKey key);
        public Task<bool> Set(TKey key, string value);
    }

    /// <summary>
    /// Is this unnecessary?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface ICacheSerialiser<T, TKey>
    {
        public Task<bool> Write(TKey key, T obj, ICache<TKey> cache);
        public Task<T> Read(TKey key, ICache<TKey> cache);
    }
}
