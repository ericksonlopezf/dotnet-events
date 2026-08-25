// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="EventId"/>.
/// </summary>
public sealed class EventIdJsonConverter : JsonConverter<EventId>
{
    /// <inheritdoc />
    public override EventId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return EventId.Empty;
        }

        if (reader.TokenType == JsonTokenType.String && reader.TryGetGuid(out var guid))
        {
            return new EventId(guid);
        }

        throw new JsonException($"Expected string UUID representation for {nameof(EventId)}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EventId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value);
    }
}
