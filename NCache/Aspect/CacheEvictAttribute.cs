using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using NCache.Repository;
using System;
using System.Threading.Tasks;

namespace NCache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CacheEvictAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// key前缀
        /// </summary>
        public string KeyPrefix { get; set; }

        public CacheEvictAttribute(string keyPrefix)
        {
            KeyPrefix = keyPrefix;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await next(context);

            // 方法执行成功后无条件删除缓存，不再依赖返回值非null（否则void方法永远无法逐出）
            ICacheRepository cacheRepository = context.ServiceProvider.GetRequiredService<ICacheRepository>();
            cacheRepository.Delete(KeyPrefix, context.Parameters);
        }
    }
}
