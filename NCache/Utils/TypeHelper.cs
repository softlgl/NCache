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
            if (type.IsArray)
            {
                return true;
            }
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return true;
            }
            foreach (var it in type.GetInterfaces())
            {
                if (it.IsGenericType && typeof(IEnumerable<>) == it.GetGenericTypeDefinition())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
