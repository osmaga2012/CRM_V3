using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.Dtos.Converters;

/// <summary>
/// Converter para manejar números que vienen del servidor y deben ser convertidos a strings
/// Ejemplo: 123 -> "123", 123.45 -> "123.45"
/// </summary>
public class NumberToStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Convert number to string
            if (reader.TryGetInt64(out var longValue))
            {
                return longValue.ToString();
            }
            else if (reader.TryGetDouble(out var doubleValue))
            {
                return doubleValue.ToString();
            }
            else
            {
                // Fallback: try to read as decimal or return string representation
                try
                {
                    return reader.GetDecimal().ToString();
                }
                catch
                {
                    // Last resort: return null if unable to convert
                    return null;
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            writer.WriteStringValue(value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
