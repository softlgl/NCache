using System;
using Microsoft.Extensions.Caching.Memory;
using NCache.Utils;

namespace NCache.Repository
{
    internal class MemoryCacheRepository:ICacheRepository
    {
        private IMemoryCache _memoryCache;
        public MemoryCacheRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Delete(string keyPrefix, params object[] paramters)
        {
            _memoryCache.Remove(KeyHelper.KeyBuilder(keyPrefix, paramters));
        }

        public string Get(string key)
        {
            return _memoryCache.Get<string>(key);
        }

        public void Set(string key, string data, int expireSeconds)
        {
            _memoryCache.Set(key, data, TimeSpan.FromSeconds(expireSeconds));
        }
    }
}
