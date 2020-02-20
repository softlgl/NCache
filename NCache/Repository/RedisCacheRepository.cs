using System;
using NCache.Utils;

namespace NCache.Repository
{
    internal class RedisCacheRepository:ICacheRepository
    {
        public RedisCacheRepository()
        {
        }

        public void Delete(string keyPrefix, params object[] paramters)
        {
            RedisHelper.Del(KeyHelper.KeyBuilder(keyPrefix, paramters));
        }

        public string Get(string key)
        {
            return RedisHelper.Get(key);
        }

        public void Set(string key, string data, int expireSeconds)
        {
            RedisHelper.Set(key, data, expireSeconds);
        }
    }
}
