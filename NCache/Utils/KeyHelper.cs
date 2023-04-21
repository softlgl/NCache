using System;
using NCache.Json;

namespace NCache.Utils
{
    public static class KeyHelper
    {
        /// <summary>
        /// 构建缓存key
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static string KeyBuilder(string keyPrefix,params object[] paramters)
        {
            string paramStr = paramters.SerializeObject();
            return $"{keyPrefix}:{MD5Helper.Get32MD5(paramStr)}";
        }

        /// <summary>
        /// 过期时间基础上加随机数
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static int KeyExpirationRandom(int expiration)
        {
            if (expiration <= 60)
            {
                return expiration;
            }
            if (expiration <= 600)
            {
                return expiration += IntRandom(0,60);
            }
            if (expiration <= 3600)
            {
                return expiration += IntRandom(0, 300);
            }
            return expiration += IntRandom(0, 600);
        }

        /// <summary>
        /// 生成整数随机数
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int IntRandom(int minValue,int maxValue)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return random.Next(minValue, maxValue);
        }
    }
}
