using System.Globalization;
using Newtonsoft.Json;

namespace DeviceMgmt.Web.Serialization;

/// <summary>
/// 将 long 以 JSON 字符串输出，避免前端 JSON.parse 对超过 2^53-1 的整数精度丢失（雪花 Id）。
/// </summary>
public sealed class LongAsStringJsonConverter : JsonConverter<long>
{
    public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(CultureInfo.InvariantCulture));
    }

    public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return ReadLong(reader);
    }

    internal static long ReadLong(JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
                var s = reader.Value as string;
                return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0L;
            case JsonToken.Integer:
                return Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
            case JsonToken.Float:
                return Convert.ToInt64(Convert.ToDecimal(reader.Value, CultureInfo.InvariantCulture));
            case JsonToken.Null:
                return 0L;
            default:
                return 0L;
        }
    }
}

public sealed class NullableLongAsStringJsonConverter : JsonConverter<long?>
{
    public override void WriteJson(JsonWriter writer, long? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(value.Value.ToString(CultureInfo.InvariantCulture));
    }

    public override long? ReadJson(JsonReader reader, Type objectType, long? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        return LongAsStringJsonConverter.ReadLong(reader);
    }
}
