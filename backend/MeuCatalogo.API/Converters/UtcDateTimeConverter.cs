using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeuCatalogo.API.Converters;

/// <summary>
/// JSON converter that ensures all DateTime values are serialized and deserialized as UTC
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, out var dateTime))
            {
                // Ensure the DateTime is treated as UTC
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle Unix timestamp
            var unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        }

        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Ensure the DateTime is in UTC before serializing
        var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

/// <summary>
/// JSON converter for nullable DateTime values that ensures UTC handling
/// </summary>
public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, out var dateTime))
            {
                // Ensure the DateTime is treated as UTC
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle Unix timestamp
            var unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        }

        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to DateTime?.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Ensure the DateTime is in UTC before serializing
        var utcValue = value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime();
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}
