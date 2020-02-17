using System;
namespace NCache.Repository
{
    public class RedisCacheRepository:ICacheRepository
    {
        public RedisCacheRepository()
        {
        }

        public void delete(string key)
        {
            RedisHelper.Del(key);
        }

        public string get(string key)
        {
            return RedisHelper.Get(key);
        }

        public void set(string key, string data, int expireSeconds)
        {
            RedisHelper.Set(key, data, expireSeconds);
        }
    }
}
