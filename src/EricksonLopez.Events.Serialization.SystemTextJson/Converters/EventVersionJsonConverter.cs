// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="EventVersion"/>.
/// </summary>
public sealed class EventVersionJsonConverter : JsonConverter<EventVersion>
{
    /// <inheritdoc />
    public override EventVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetUInt32(out var val))
        {
            return new EventVersion(val == 0 ? 1 : val);
        }

        if (reader.TokenType == JsonTokenType.String && uint.TryParse(reader.GetString(), out var parsedVal))
        {
            return new EventVersion(parsedVal == 0 ? 1 : parsedVal);
        }

        throw new JsonException($"Expected numeric or string integer representation for {nameof(EventVersion)}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EventVersion value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteNumberValue(value.Value == 0 ? 1 : value.Value);
    }
}
