using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using NCache.Repository;
using NCache.Utils;
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

            string cacheKey = CacheAspectUtils.GenerateKey(context.Parameters);
            if (!string.IsNullOrWhiteSpace(KeyPrefix))
            {
                cacheKey = $"{KeyPrefix}:{cacheKey}";
            }

            ICacheRepository redisRepository = context.ServiceProvider.GetRequiredService<ICacheRepository>();
            object targetReturn = context.IsAsync() ? await context.UnwrapAsyncReturnValue() : context.ReturnValue;
            if (targetReturn != null)
            {
                 redisRepository.Delete(cacheKey);
            }
        }
    }
}
