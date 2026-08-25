// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="CorrelationId"/>.
/// </summary>
public sealed class CorrelationIdJsonConverter : JsonConverter<CorrelationId>
{
    /// <inheritdoc />
    public override CorrelationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return CorrelationId.Empty;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string or null for {nameof(CorrelationId)}.");
        }

        return new CorrelationId(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CorrelationId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value);
    }
}
