using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NCache.Json;
using NCache.Repository;
using NCache.Utils;
using System;
using System.Text.Json;
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

        private JsonSerializerOptions _serializerOptions;

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await next(context);

            string cacheKey = KeyHelper.KeyBuilder(KeyPrefix, context.Parameters);
            ICacheRepository cacheRepository = context.ServiceProvider.GetRequiredService<ICacheRepository>();
            object targetReturn = context.IsAsync() ? await context.UnwrapAsyncReturnValue() : context.ReturnValue;
            if (targetReturn != null)
            {
                string stringValue = targetReturn.SerializeObject(GetSerializerOptions(context));
                // 过期时间加随机偏移，防止同一时刻大量key集中失效（缓存雪崩）
                cacheRepository.Set(cacheKey, stringValue, KeyHelper.KeyExpirationRandom(Expiration));
            }
        }

        private JsonSerializerOptions GetSerializerOptions(AspectContext context)
        {
            return _serializerOptions ??= JsonOptions.ResolveSerializerOptions(context.ServiceProvider);
        }
    }
}
