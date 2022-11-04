using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NCache.Json
{
    public class JsonOptions
    {

        public static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions(DefaultSerializerOptions);

        public static JsonSerializerOptions ResolveSerializerOptions(IServiceProvider serviceProvider = null)
        {
            if (serviceProvider == null)
            {
                return DefaultSerializerOptions;
            }
            return serviceProvider.GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions ?? DefaultSerializerOptions;
        }
    }
}
