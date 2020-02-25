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
        //nuget api key:oy2hsntv453eeor4icyavb3brqkom7fu7auzxxzem3mghq
        [Fact]
        public void GetCache()
        {
            IServiceProvider serviceProvider = BuildServiceProvider();
            IPersonService personService = serviceProvider.GetService<IPersonService>();
            Person person = personService.GetPerson(1);
            person = personService.GetPerson(1);
            System.Threading.Thread.Sleep(61*1000);
            person = personService.GetPerson(1);
            Assert.True(person != null && person.Id == 1);
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
                option.CacheType = CacheTypeEnum.Local;
                //option.CacheType = CacheTypeEnum.Redis;
                //option.RedisClient = new CSRedis.CSRedisClient("127.0.0.1:6379");
            });
            //services.ConfigureDynamicProxy();
            return services.BuildDynamicProxyProvider();
        }
    }
}
