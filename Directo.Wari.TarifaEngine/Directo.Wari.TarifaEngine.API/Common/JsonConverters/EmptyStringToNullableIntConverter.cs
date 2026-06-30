using System.Text.Json;
using System.Text.Json.Serialization;

namespace Directo.Wari.TarifaEngine.API.Common.JsonConverters
{
    public class EmptyStringToNullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (int.TryParse(value, out var result))
                    return result;

                throw new JsonException($"Invalid number: {value}");
            }

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            if (reader.TokenType == JsonTokenType.Null)
                return null;

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
