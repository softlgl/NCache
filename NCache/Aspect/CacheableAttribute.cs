using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NCache.Json;
using NCache.Repository;
using NCache.Utils;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NCache
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheableAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// key前缀
        /// </summary>
        public string KeyPrefix { get; set; }

        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public int Expiration { get; set; } = 60;

        public CacheableAttribute(string keyPrefix)
        {
            KeyPrefix = keyPrefix;
        }

        private JsonSerializerOptions _serializerOptions;

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            bool isAsync = context.IsAsync();
            if (context.ServiceMethod.ReturnType == typeof(void) || isAsync && !context.ServiceMethod.ReturnType.IsGenericType)
            {
                throw new ArgumentException("method cannot return void");
            }

            string cacheKey = KeyHelper.KeyBuilder(KeyPrefix, context.Parameters);
            ICacheRepository cacheRepository = context.ServiceProvider.GetRequiredService<ICacheRepository>();

            string cachedValue = cacheRepository.Get(cacheKey);
            if (!string.IsNullOrWhiteSpace(cachedValue))
            {
                SetReturnValueFromCache(context, cachedValue);
                return;
            }

            // 缓存未命中：对相同key的并发请求合并执行，防止缓存击穿
            SemaphoreSlim keyLock = CacheAspectUtils.GetKeyLock(cacheKey);
            bool locked = await keyLock.WaitAsync(CacheAspectUtils.KeyLockTimeout);
            try
            {
                if (locked)
                {
                    // 双重检查：等锁期间可能已被其它请求写入
                    cachedValue = cacheRepository.Get(cacheKey);
                    if (!string.IsNullOrWhiteSpace(cachedValue))
                    {
                        SetReturnValueFromCache(context, cachedValue);
                        return;
                    }
                }

                await next(context);

                object targetReturn = isAsync ? await context.UnwrapAsyncReturnValue() : context.ReturnValue;
                if (targetReturn != null)
                {
                    string stringValue = targetReturn.SerializeObject(GetSerializerOptions(context));
                    // 过期时间加随机偏移，防止同一时刻大量key集中失效（缓存雪崩）
                    cacheRepository.Set(cacheKey, stringValue, KeyHelper.KeyExpirationRandom(Expiration));
                }
            }
            finally
            {
                if (locked)
                {
                    keyLock.Release();
                }
            }
        }

        private void SetReturnValueFromCache(AspectContext context, string cachedValue)
        {
            AsyncReturnMeta returnMeta = CacheAspectUtils.GetReturnMeta(context.ServiceMethod.ReturnType);
            object returnValue = JsonSerializer.Deserialize(cachedValue, returnMeta.ResultType, GetSerializerOptions(context));
            if (returnMeta.WrapFunc != null)
            {
                context.ReturnValue = returnMeta.WrapFunc(returnValue!);
            }
            else
            {
                context.ReturnValue = returnValue;
            }
        }

        private JsonSerializerOptions GetSerializerOptions(AspectContext context)
        {
            return _serializerOptions ??= JsonOptions.ResolveSerializerOptions(context.ServiceProvider);
        }
    }
}
