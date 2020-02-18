using System;
namespace NCache
{
    public class CacheOption
    {
        public CacheOption()
        {
        }
        /// <summary>
        /// 缓存类型
        /// </summary>
        public CacheTypeEnum CacheType { get; set; }

        /// <summary>
        /// CsRedis实例
        /// </summary>
        public CSRedis.CSRedisClient RedisClient { get; set; }
    }
}
