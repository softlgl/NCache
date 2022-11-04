using System.Text;
using System.Text.Json;

namespace NCache.Json
{
    public static class JsonSerializerExtensions
    {
        public static string SerializeObject(this object obj, JsonSerializerOptions? serializerOptions = null)
        {
            serializerOptions ??= JsonOptions.DefaultSerializerOptions;
            return JsonSerializer.Serialize(obj, serializerOptions);
        }

        public static byte[] SerializeBytes(this object obj, JsonSerializerOptions? serializerOptions = null)
        {
            serializerOptions ??= JsonOptions.DefaultSerializerOptions;
            return JsonSerializer.SerializeToUtf8Bytes(obj, serializerOptions);
        }

        public static T DeserializeObject<T>(this string value, JsonSerializerOptions? serializerOptions = null)
        {
            serializerOptions ??= JsonOptions.DefaultSerializerOptions;
            return JsonSerializer.Deserialize<T>(value, serializerOptions);
        }

        public static T DeserializeBytes<T>(this byte[] data, JsonSerializerOptions? serializerOptions = null)
        {
            serializerOptions ??= JsonOptions.DefaultSerializerOptions;
            return Encoding.UTF8.GetString(data).DeserializeObject<T>(serializerOptions);
        }
    }
}