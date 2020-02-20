using System;
using Newtonsoft.Json;

namespace NCache.Utils
{
    public class KeyHelper
    {
        /// <summary>
        /// 构建缓存key
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static string KeyBuilder(string keyPrefix,params object[] paramters)
        {
            string paramStr = JsonConvert.SerializeObject(paramters);
            return $"{keyPrefix}:{MD5Helper.Get32MD5(paramStr)}";
        }
    }
}
