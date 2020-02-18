using System;
using System.Threading.Tasks;
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
        public void GetCache()
        {
            IServiceProvider serviceProvider = BuildServiceProvider();
            IPersonService personService = serviceProvider.GetService<IPersonService>();
            Person person = personService.GetPerson(1);
            personService.AddPerson(person);
        }

        [Fact]
        public void HasNotReturnValue()
        {
            IServiceProvider serviceProvider = BuildServiceProvider();
            IPersonService personService = serviceProvider.GetService<IPersonService>();
            Person person = new Person
            {
                Id = 2,
                Name = "liguoliang",
                Birthday = new DateTime(1992, 12, 11)
            };
            personService.AddPerson(person);
        }

        [Fact]
        public void ReturnTaskValue()
        {
            IServiceProvider serviceProvider = BuildServiceProvider();
            IPersonService personService = serviceProvider.GetService<IPersonService>();
            Person person = new Person
            {
                Id = 1,
                Name = "liguoliang",
                Birthday = new DateTime(1992, 12, 11)
            };
            Task<Person> task = personService.UpdatePerson(1,person);
            person = task.Result;
        }

        private static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IPersonService, PersonService>();
            services.AddNCache(option =>
            {
                option.cacheType = CacheTypeEnum.Redis;
                option.redisClient = new CSRedis.CSRedisClient("127.0.0.1:6379");
            });
            services.ConfigureDynamicProxy();
            return services.BuildDynamicProxyProvider(); ;
        }
    }
}