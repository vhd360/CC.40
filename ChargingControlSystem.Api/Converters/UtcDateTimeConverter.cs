using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChargingControlSystem.Api.Converters;

/// <summary>
/// Custom JSON converter that ensures DateTime values are serialized as UTC with 'Z' suffix
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTimeString = reader.GetString();
        if (string.IsNullOrEmpty(dateTimeString))
            return default;
            
        var dateTime = DateTime.Parse(dateTimeString);
        
        // If Kind is Unspecified, assume UTC
        if (dateTime.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            
        // Convert to UTC if it's local
        return dateTime.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always write as UTC with 'Z' suffix
        var utcValue = value.Kind == DateTimeKind.Utc 
            ? value 
            : value.ToUniversalTime();
            
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

/// <summary>
/// Custom JSON converter for nullable DateTime that ensures values are serialized as UTC with 'Z' suffix
/// </summary>
public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTimeString = reader.GetString();
        if (string.IsNullOrEmpty(dateTimeString))
            return null;
            
        var dateTime = DateTime.Parse(dateTimeString);
        
        // If Kind is Unspecified, assume UTC
        if (dateTime.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            
        // Convert to UTC if it's local
        return dateTime.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }
        
        // Always write as UTC with 'Z' suffix
        var utcValue = value.Value.Kind == DateTimeKind.Utc 
            ? value.Value 
            : value.Value.ToUniversalTime();
            
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

