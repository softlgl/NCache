using System;
using Microsoft.Extensions.Caching.Memory;

namespace NCache.Repository
{
    public class MemoryCacheRepository:ICacheRepository
    {
        private IMemoryCache _memoryCache;
        public MemoryCacheRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Delete(string key)
        {
            _memoryCache.Remove(key);
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
