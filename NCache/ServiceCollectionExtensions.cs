using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCache.Repository;

namespace NCache
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNCache(this IServiceCollection services, Action<MemoryCacheOptions> cacheAction = null)
        {
            MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions();

            if (cacheAction != null)
            {
                cacheAction.Invoke(memoryCacheOptions);
            }

            MemoryCache memoryCache = new MemoryCache(memoryCacheOptions);
            services.TryAddSingleton<IMemoryCache>(memoryCache);
            services.Replace(ServiceDescriptor.Singleton<ICacheRepository, MemoryCacheRepository>());
        }

        public static IServiceCollection AddNCacheWithDistributedCache(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<ICacheRepository, DistributedCachRepository>());
            return services;
        }
    }
}
