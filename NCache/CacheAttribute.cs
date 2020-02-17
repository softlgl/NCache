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
        /// 过期时间
        /// </summary>
        public int ExpirationSeconds { get; set; }

        [FromServiceContext]
        private ICacheRepository cacheRepository { get; set; }

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            string paramStr = JsonConvert.SerializeObject(context.Parameters);
            string key = $"KeyPrefix:{MD5Helper.Get32MD5(paramStr)}";
            string value = cacheRepository.get(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                context.ReturnValue = JsonConvert.DeserializeObject(value, context.ProxyMethod.ReturnType);
                return;
            }
            await next(context);
            object returnValue = context.ReturnValue;
            if (returnValue != null)
            {
                cacheRepository.set(key, JsonConvert.SerializeObject(returnValue), ExpirationSeconds);
                return;
            }
        }
    }
}
