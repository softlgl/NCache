using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCache.Json;
using NCache.Repository;

namespace NCache
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNCache(this IServiceCollection services, Action<MemoryCacheOptions> cacheAction = null)
        {
            if (cacheAction != null)
            {
                // 交由容器创建并管理 IMemoryCache 生命周期（原先在容器外 new，不受 Dispose 管理）
                services.AddMemoryCache(cacheAction);
            }
            else
            {
                services.AddMemoryCache();
            }

            services.AddOptions<JsonOptions>();
            services.Replace(ServiceDescriptor.Singleton<ICacheRepository, MemoryCacheRepository>());
            return services;
        }

        public static IServiceCollection AddNCacheWithDistributedCache(this IServiceCollection services)
        {
            services.AddOptions<JsonOptions>();
            services.Replace(ServiceDescriptor.Singleton<ICacheRepository, DistributedCachRepository>());
            return services;
        }
    }
}
