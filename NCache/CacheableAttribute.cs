using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NCache.Json;
using NCache.Repository;
using NCache.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            bool isAsync = context.IsAsync();
            if (context.ServiceMethod.ReturnType == typeof(void) || isAsync && !context.ServiceMethod.ReturnType.IsGenericType)
            {
                throw new ArgumentException("method cannot return void");
            }

            var serializerOptions = JsonOptions.ResolveSerializerOptions(context.ServiceProvider);
            string cacheKey = CacheAspectUtils.GenerateKey(context.Parameters);
            if (!string.IsNullOrWhiteSpace(KeyPrefix))
            {
                cacheKey = $"{KeyPrefix}:{cacheKey}";
            }

            ICacheRepository redisRepository = context.ServiceProvider.GetRequiredService<ICacheRepository>();
            string redisValue = redisRepository!.Get(cacheKey);
            if (!string.IsNullOrWhiteSpace(redisValue))
            {
                Type returnType = isAsync ? context.ServiceMethod.ReturnType.GetGenericArguments().First() : context.ServiceMethod.ReturnType;
                var returnValue = JsonSerializer.Deserialize(redisValue, returnType, serializerOptions);
                if (isAsync)
                {
                    var callFunc = context.ServiceMethod.ReturnType.GetTypeInfo().IsTaskWithResult() ? CacheAspectUtils.TaskResultFunc(returnType) :
                        CacheAspectUtils.ValueTaskResultFunc(returnType);
                    context.ReturnValue = callFunc(returnValue!);
                }
                else
                {
                    context.ReturnValue = returnValue;
                }
                return;
            }

            await next(context);

            if (string.IsNullOrWhiteSpace(redisValue))
            {
                object targetReturn = context.IsAsync() ? await context.UnwrapAsyncReturnValue() : context.ReturnValue;
                if (targetReturn != null)
                {
                    string stringValue = targetReturn.SerializeObject(serializerOptions);
                    redisRepository.Set(cacheKey, stringValue, Expiration);
                }
            }
        }
    }
}
