// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="CausationId"/>.
/// </summary>
public sealed class CausationIdJsonConverter : JsonConverter<CausationId>
{
    /// <inheritdoc />
    public override CausationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return CausationId.Empty;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string or null for {nameof(CausationId)}.");
        }

        return new CausationId(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CausationId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value);
    }
}
