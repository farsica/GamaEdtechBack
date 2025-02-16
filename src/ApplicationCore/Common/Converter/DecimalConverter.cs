namespace GamaEdtech.Backend.Common.Converter
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType == JsonTokenType.String && decimal.TryParse(reader.GetString(), out var number)
                ? number
                : reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out number)
                ? number
                : throw new ArgumentException("TokenType");

        public override void Write([NotNull] Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteNumberValue(value);

        public override bool CanConvert(Type typeToConvert) => Type.GetTypeCode(typeToConvert) == TypeCode.Decimal;
    }
}
