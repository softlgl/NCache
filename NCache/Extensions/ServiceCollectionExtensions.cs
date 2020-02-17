using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NCache.Repository;

namespace NCache.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSingleton<TService, TServiceImpl>(this IServiceCollection services, string name)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            Dictionary<string, TService> typeDics = serviceProvider.GetService<Dictionary<string, TService>>();
        }

        public static void AddNCache(this IServiceCollection services, Action<CacheOption> cacheAction)
        {
            CacheOption cacheOption = new CacheOption();
            cacheAction.Invoke(cacheOption);
            switch (cacheOption.cacheType)
            {
                case CacheTypeEnum.Local:
                case CacheTypeEnum.Redis:
                    if (cacheOption.redisClient==null)
                    {
                        throw new ArgumentNullException(nameof(cacheOption.redisClient));
                    }
                    CSRedis.CSRedisClient cSRedis = cacheOption.redisClient;
                    RedisHelper.Initialization(cSRedis);
                    services.AddSingleton<ICacheRepository, RedisCacheRepository>();
                    break;
            }
        }
    }
}
