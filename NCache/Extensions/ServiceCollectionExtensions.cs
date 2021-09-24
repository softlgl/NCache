using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NCache.Repository;

namespace NCache.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNCache(this IServiceCollection services, Action<CacheOption> cacheAction)
        {
            CacheOption cacheOption = new CacheOption();
            cacheAction.Invoke(cacheOption);
            switch (cacheOption.CacheType)
            {
                case CacheTypeEnum.Local:
                    MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
                    services.AddSingleton<IMemoryCache>(memoryCache);
                    services.AddSingleton<ICacheRepository, MemoryCacheRepository>();
                    break;
                case CacheTypeEnum.Redis:
                    if (cacheOption.RedisClient == null)
                    {
                        throw new ArgumentNullException(nameof(cacheOption.RedisClient));
                    }
                    CSRedis.CSRedisClient cSRedis = cacheOption.RedisClient;
                    RedisHelper.Initialization(cSRedis);
                    services.AddSingleton<ICacheRepository, RedisCacheRepository>();
                    break;
            }
        }

        public static IServiceCollection AddNCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacheRepository, DistributedCachRepository>();
            return services;
        }
    }
}
