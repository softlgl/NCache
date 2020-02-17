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
        public CacheTypeEnum cacheType { get; set; }

        /// <summary>
        /// CsRedis实例
        /// </summary>
        public CSRedis.CSRedisClient redisClient { get; set; }
    }
}
