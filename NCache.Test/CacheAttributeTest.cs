using System;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCache.Extensions;
using NCache.Test.Model;
using NCache.Test.Service;
using Xunit;

namespace NCache.Test
{
    public class CacheAttributeTest
    {
        [Fact]
        public void CacheAttribute()
        {
            IServiceCollection services =new ServiceCollection();
            services.AddSingleton<IPersonService, PersonService>();
            services.AddNCache(option =>
            {
                option.cacheType = CacheTypeEnum.Redis;
                option.redisClient = new CSRedis.CSRedisClient("127.0.0.1:6379");
            });
            services.ConfigureDynamicProxy();
            IServiceProvider serviceProvider = services.BuildDynamicProxyProvider();
            IPersonService personService = serviceProvider.GetService<IPersonService>();
            Person person = personService.GetPerson(1);
            personService.AddPerson(person);
        }
    }
}
