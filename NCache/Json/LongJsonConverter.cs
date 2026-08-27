using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NCache.Json
{
    public class LongJsonConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            if (reader.TokenType == JsonTokenType.String
                && long.TryParse(reader.GetString(), out long result))
            {
                return result;
            }
            // 解析失败抛出异常，避免静默返回0掩盖脏数据
            throw new JsonException($"无法将 JsonToken({reader.TokenType}) 转换为 long");
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
