using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using NCache.Json;
using NCache.Repository;
using NCache.Utils;
using System;
using System.Threading.Tasks;

namespace NCache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CachePutAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// key前缀
        /// </summary>
        public string KeyPrefix { get; set; }

        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public int Expiration { get; set; } = 60;

        public CachePutAttribute(string keyPrefix)
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
                var serializerOptions = JsonOptions.ResolveSerializerOptions(context.ServiceProvider);
                string stringValue = targetReturn.SerializeObject(serializerOptions);
                redisRepository.Set(cacheKey, stringValue, Expiration);
            }
        }
    }
}
