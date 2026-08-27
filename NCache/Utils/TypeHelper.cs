using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCache.Utils
{
    public static class TypeHelper
    {
        /// <summary>
        /// 判断类型是否为列表类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            // 数组和泛型 IEnumerable<> 都实现了非泛型 IEnumerable，一次判断即可
            return type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        }
    }
}
