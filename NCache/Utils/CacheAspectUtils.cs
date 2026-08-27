using AspectCore.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCache.Utils
{
    public class CacheAspectUtils
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _asTaskFuncCache = new ConcurrentDictionary<Type, Func<object, object>>();
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _asValueTaskFuncCache = new ConcurrentDictionary<Type, Func<object, object>>();
        private static readonly ConcurrentDictionary<Type, AsyncReturnMeta> _returnMetaCache = new ConcurrentDictionary<Type, AsyncReturnMeta>();

        /// <summary>
        /// key锁等待超时时间，超时后放弃合并直接执行，避免同key嵌套调用时死锁
        /// </summary>
        public static readonly TimeSpan KeyLockTimeout = TimeSpan.FromSeconds(5);

        private const int KeyLockCount = 256;
        private static readonly SemaphoreSlim[] _keyLocks = CreateKeyLocks();

        private static SemaphoreSlim[] CreateKeyLocks()
        {
            var locks = new SemaphoreSlim[KeyLockCount];
            for (int i = 0; i < KeyLockCount; i++)
            {
                locks[i] = new SemaphoreSlim(1, 1);
            }
            return locks;
        }

        /// <summary>
        /// 按key哈希获取分条锁，用于合并相同key的并发请求，防止缓存击穿
        /// </summary>
        public static SemaphoreSlim GetKeyLock(string cacheKey)
        {
            return _keyLocks[(cacheKey.GetHashCode() & 0x7FFFFFFF) % KeyLockCount];
        }

        /// <summary>
        /// 获取方法返回类型的元信息（缓存反射结果，避免热路径上每次调用都重复反射）
        /// </summary>
        internal static AsyncReturnMeta GetReturnMeta(Type returnType)
        {
            return _returnMetaCache.GetOrAdd(returnType, type => new AsyncReturnMeta(type));
        }

        public static Func<object, object> TaskResultFunc(Type returnType)
        {
            var func = _asTaskFuncCache.GetOrAdd(returnType, type =>
            {
                var resultMethod = typeof(Task).GetMethod("FromResult")!.MakeGenericMethod(returnType);
                ParameterExpression source = Expression.Parameter(typeof(object), "result");
                var instanceCast = Expression.Convert(source, returnType);
                var callExpr = Expression.Call(resultMethod, instanceCast);
                var expr = Expression.Lambda<Func<object, object>>(callExpr, source).Compile();
                return expr;
            });
            return func;
        }

        public static Func<object, object> ValueTaskResultFunc(Type returnType)
        {
            var func = _asValueTaskFuncCache.GetOrAdd(returnType, type =>
            {
                var vauleType = typeof(ValueTask<>).MakeGenericType(returnType);
                ParameterExpression source = Expression.Parameter(typeof(object), "result");
                UnaryExpression instanceCast = Expression.Convert(source, returnType);
                var newExpr = Expression.New(vauleType.GetConstructor(new[] { returnType }), instanceCast);
                var convertBody = Expression.Convert(newExpr, typeof(object));
                var expr = Expression.Lambda<Func<object, object>>(convertBody, source).Compile();
                return expr;
            });
            return func;
        }

        public static string GenerateKey(params object[] paramters)
        {
            List<string> list = new List<string>();
            foreach (var item in paramters)
            {
                if (item != null && item.GetType() != typeof(string) && item.GetType().IsEnumerable())
                {
                    var objs = (IEnumerable)item;
                    foreach (var obj in objs)
                    {
                        list.Add(obj.ToString());
                    }
                    continue;
                }
                list.Add(item?.ToString());
            }
            return list.Any() ? string.Join('|', list.Where(i => !string.IsNullOrWhiteSpace(i))) : "";
        }
    }

    /// <summary>
    /// 方法返回类型元信息
    /// </summary>
    internal sealed class AsyncReturnMeta
    {
        /// <summary>
        /// 实际返回值类型（Task/ValueTask 的泛型参数，同步方法为方法返回类型本身）
        /// </summary>
        public Type ResultType { get; }

        /// <summary>
        /// 将结果包装为 Task/ValueTask 的委托；同步方法为 null
        /// </summary>
        public Func<object, object> WrapFunc { get; }

        public AsyncReturnMeta(Type returnType)
        {
            if (returnType.IsGenericType
                && (returnType.GetGenericTypeDefinition() == typeof(Task<>)
                    || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
            {
                ResultType = returnType.GetGenericArguments()[0];
                WrapFunc = returnType.GetGenericTypeDefinition() == typeof(Task<>)
                    ? CacheAspectUtils.TaskResultFunc(ResultType)
                    : CacheAspectUtils.ValueTaskResultFunc(ResultType);
            }
            else
            {
                ResultType = returnType;
                WrapFunc = null;
            }
        }
    }
}
