using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCache.Redis.Repository;
using NCache.Repository;

namespace NCache.Redus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNCacheRedis(this IServiceCollection services, Func<CSRedis.CSRedisClient> redisFunc)
        {
            if (redisFunc == null)
            {
                throw new ArgumentNullException(nameof(redisFunc));
            }

            AddNCacheRedis(services, redisFunc.Invoke());
        }

        public static void AddNCacheRedis(this IServiceCollection services, CSRedis.CSRedisClient cSRedis)
        {
            if (cSRedis == null)
            {
                throw new ArgumentNullException(nameof(cSRedis));
            }

            RedisHelper.Initialization(cSRedis);
            services.Replace(ServiceDescriptor.Singleton<ICacheRepository, RedisCacheRepository>());
        }
    }
}
