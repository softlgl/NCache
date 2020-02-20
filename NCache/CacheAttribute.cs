using System;
using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using NCache.Repository;
using NCache.Utils;
using Newtonsoft.Json;

namespace NCache
{
    public class CacheAttribute: AbstractInterceptorAttribute
    {
        /// <summary>
        /// key前缀
        /// </summary>
        public string KeyPrefix { get; set; }

        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public int Expiration { get; set; } = 60;

        /// <summary>
        /// 属性注入
        /// </summary>
        [FromServiceContext]
        private ICacheRepository CacheRepository { get; set; }

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            //构建缓存key
            string key = KeyHelper.KeyBuilder(KeyPrefix, context.Parameters);
            string value = CacheRepository.Get(key);
            //缓存存在
            if (!string.IsNullOrWhiteSpace(value))
            {
                context.ReturnValue = JsonConvert.DeserializeObject(value, context.ProxyMethod.ReturnType);
                return;
            }
            await next(context);
            //放入缓存
            object returnValue = context.ReturnValue;
            if (returnValue != null)
            {
                //处理task情况
                Type type = returnValue.GetType();
                if (typeof(Task).IsAssignableFrom(type))
                {
                    var resultProperty = type.GetProperty("Result");
                    object taskValue = resultProperty.GetValue(returnValue);
                    if (taskValue != null)
                    {
                        CacheRepository.Set(key, JsonConvert.SerializeObject(taskValue), Expiration);
                    }
                    return;
                }
                CacheRepository.Set(key, JsonConvert.SerializeObject(returnValue), Expiration);
            }
        }
    }
}
