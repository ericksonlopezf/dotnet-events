// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="EventType"/>.
/// </summary>
public sealed class EventTypeJsonConverter : JsonConverter<EventType>
{
    /// <inheritdoc />
    public override EventType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            return !string.IsNullOrEmpty(str) ? new EventType(str) : default;
        }

        throw new JsonException($"Expected string representation for {nameof(EventType)}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EventType value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value);
    }
}
