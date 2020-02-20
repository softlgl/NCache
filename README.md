# NCache
AOP框架使用AspectCore文档较少，使用时候多看看源码</br>
https://github.com/dotnetcore/AspectCore-Framework

Redis客户端驱动使用的是CSRedis,因为csredis配置比较灵活所以保留了默认的配置方式</br>
https://github.com/2881099/csredis

获取方式
```
Install-Package NCache -Version 1.1.0
```

NCache通过Attribute的方式可以对方法结果进行缓存，缓存key的规则为KeyPrefix:md5(json(方法参数))

示例代码 .net core 3.1
```
public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
```

```
public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPersonService, PersonService>();
            services.AddControllersWithViews();
            services.AddNCache(option =>
            {
                option.CacheType = CacheTypeEnum.Redis;
                option.RedisClient = new CSRedis.CSRedisClient("127.0.0.1:6379");
            });
        }
```

```
 public interface IPersonService
    {
        Person GetPerson(int id);
    }
```


```
public class PersonService : IPersonService
    {
        [Cache(KeyPrefix ="Person",Expiration =3600)]
        public Person GetPerson(int id)
        {
            Person person = new Person
            {
                Id=id,
                Name="liguoliang",
                Birthday=new DateTime(1992,12,11)
            };
            return person;
        }
    }
```
