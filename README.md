# NCache
AOP框架使用AspectCore文档较少，使用时候多看看源码</br>
https://github.com/dotnetcore/AspectCore-Framework

Redis客户端驱动使用的是CSRedis,因为csredis配置比较灵活所以保留了默认的配置方式</br>
https://github.com/2881099/csredis

### 获取方式
```
Install-Package NCache -Version 1.3.0
```

NCache通过Attribute的方式可以对方法结果进行缓存，缓存key的规则为KeyPrefix:md5(json(方法参数)),目前支持Redis和本地缓存两种方式

### 示例代码 .net core 3.1
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
```
#### 本地缓存
```csharp
public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPersonService, PersonService>();
            services.AddControllersWithViews();
            services.AddNCache();
        }
```
#### Redis缓存
```
Install-Package NCache.Redis -Version 1.3.0
```
```csharp
//使用redis缓存
services.AddNCacheRedis(new CSRedis.CSRedisClient("127.0.0.1:6379"));
```
#### 结合IDistributedCache
```csharp
//如果直接使用使用下面的方式,则会直接IDistributedCache的实例
//可自行给IDistributedCache注册本地缓存或Redis缓存
services.AddDistributedMemoryCache().AddNCacheWithDistributedCache();
```
#### Demo示例
```csharp
 public interface IPersonService
    {
        Person GetPerson(int id);

        void AddPerson(Person person);

        Task<Person> UpdatePerson(int id,Person person);
    }
```

```csharp
 public class PersonService : IPersonService
    {
        public PersonService()
        {
        }

        [Cacheable("Person",Expiration=600)]
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

        [CachePut("PersonAdd", Expiration  = 3600)]
        public void AddPerson(Person person)
        {

        }

        [Cacheable("PersonUpdate", Expiration  = 3600)]
        public async Task<Person> UpdatePerson(int id,Person person)
        {
            await Task.Delay(10);
            return person;
        }
    }
```
