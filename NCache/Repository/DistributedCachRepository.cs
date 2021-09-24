using System;
using Microsoft.Extensions.Caching.Distributed;
using NCache.Utils;

namespace NCache.Repository
{
    public class DistributedCachRepository: ICacheRepository
    {
        private readonly IDistributedCache _distributedCache;
        public DistributedCachRepository(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public void Delete(string keyPrefix, params object[] paramters)
        {
            _distributedCache.Remove(KeyHelper.KeyBuilder(keyPrefix, paramters));
        }

        public string Get(string key)
        {
            return _distributedCache.GetString(key);
        }

        public void Set(string key, string data, int expireSeconds)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
            _distributedCache.Set(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expireSeconds),
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expireSeconds)
            });
        }
    }
}
