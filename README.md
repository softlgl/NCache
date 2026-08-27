# NCache

基于 AOP 的声明式缓存框架。通过 Attribute 对方法结果进行缓存，支持 **Redis**、**本地内存缓存**以及 **IDistributedCache** 三种存储方式。

缓存 key 规则为 `KeyPrefix:md5(json(方法参数))`。

## 特性

- **声明式缓存**：通过 `Cacheable` / `CachePut` / `CacheEvict` 三个特性即可声明缓存行为，业务代码零侵入
- **缓存集中防御**：
  - 防**击穿**：对相同 key 的并发请求使用分段锁合并执行，并做双重检查
  - 防**雪崩**：过期时间在设定值上叠加随机偏移，避免大量 key 同时失效
- **支持返回类型**：`Cacheable` / `CachePut` 可缓存同步返回值、`Task<T>`、`ValueTask<T>`（`Cacheable` 不允许返回 `void` 或非泛型 `Task`，会抛异常）；`CacheEvict` 对返回类型无任何限制，`void` 方法亦可使用
- **可扩展**：实现 `ICacheRepository` 即可接入自定义缓存存储

### 三种特性说明

| 特性 | 说明 |
| --- | --- |
| `[Cacheable(keyPrefix)]` | 读缓存。方法执行前先查缓存，命中则直接返回；未命中则执行方法并写入缓存 |
| `[CachePut(keyPrefix)]` | 写缓存。方法执行成功后，将返回值写入缓存（适用于不需要先查的场景） |
| `[CacheEvict(keyPrefix)]` | 逐出缓存。方法执行成功后，无条件删除对应缓存（适用于删除/更新操作） |

`Cacheable` / `CachePut` 的 `Expiration`（过期时间，秒，默认 60）与 `KeyPrefix` 均可通过特性参数配置；`CacheEvict` 仅支持 `KeyPrefix`。

## 相关依赖

AOP 基于 AspectCore，Redis 客户端使用 CSRedis：

- https://github.com/dotnetcore/AspectCore-Framework
- https://github.com/2881099/csredis

## 获取方式

```bash
Install-Package NCache -Version 1.3.0
```

```bash
# 使用 Redis 存储时额外安装
Install-Package NCache.Redis -Version 1.3.0
```

## 快速开始

将服务注册到容器，并使用 AspectCore 的动态代理工厂：

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
```

### 本地内存缓存

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IPersonService, PersonService>();
    services.AddControllersWithViews();
    services.AddNCache();
}
```

`AddNCache()` 支持可选参数自定义 `MemoryCacheOptions`：

```csharp
services.AddNCache(opts => opts.SizeLimit = 10000);
```

### Redis 缓存

```csharp
// 方式一：传入 client 实例
services.AddNCacheRedis(new CSRedis.CSRedisClient("127.0.0.1:6379"));

// 方式二：传入创建 client 的工厂，便于配置与生命周期管理
services.AddNCacheRedis(() => new CSRedis.CSRedisClient("127.0.0.1:6379"));
```

### 结合 IDistributedCache

```csharp
// 直接复用已有的 IDistributedCache 实例，可自行注册本地缓存或 Redis 缓存
services.AddDistributedMemoryCache().AddNCacheWithDistributedCache();
```

## Demo 示例

```csharp
public interface IPersonService
{
    Person GetPerson(int id);

    void AddPerson(Person person);

    Task<Person> UpdatePerson(int id, Person person);
}
```

```csharp
public class PersonService : IPersonService
{
    [Cacheable("Person", Expiration = 600)]
    public Person GetPerson(int id)
    {
        Person person = new Person
        {
            Id = id,
            Name = "liguoliang",
            Birthday = new DateTime(1992, 12, 11)
        };
        return person;
    }

    [CachePut("PersonAdd", Expiration = 3600)]
    public void AddPerson(Person person)
    {
    }

    [Cacheable("PersonUpdate", Expiration = 3600)]
    public async Task<Person> UpdatePerson(int id, Person person)
    {
        await Task.Delay(10);
        return person;
    }
}
```