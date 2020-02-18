using System;
namespace NCache.Repository
{
    public class RedisCacheRepository:ICacheRepository
    {
        public RedisCacheRepository()
        {
        }

        public void Delete(string key)
        {
            RedisHelper.Del(key);
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
