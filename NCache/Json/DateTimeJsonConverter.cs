using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NCache.Json
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        private readonly string _dateFormatString;

        public DateTimeJsonConverter()
        {
            _dateFormatString = "yyyy-MM-dd HH:mm:ss";
        }

        public DateTimeJsonConverter(string dateFormatString)
        {
            _dateFormatString = dateFormatString;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_dateFormatString));
        }
    }
}
