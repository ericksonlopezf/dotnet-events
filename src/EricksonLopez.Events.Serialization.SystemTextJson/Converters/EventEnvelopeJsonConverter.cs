// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="EventEnvelope{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">The type of the event payload.</typeparam>
public sealed class EventEnvelopeJsonConverter<TEvent> : JsonConverter<EventEnvelope<TEvent>>
    where TEvent : IEvent
{
    /// <inheritdoc />
    public override EventEnvelope<TEvent> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject token when deserializing {nameof(EventEnvelope<TEvent>)}.");
        }

        var id = EventId.Empty;
        var type = default(EventType);
        var version = EventVersion.V1;
        var occurredAt = default(DateTimeOffset);
        TEvent? payload = default;
        EventMetadata? metadata = null;

        var payloadTypeInfo = (JsonTypeInfo<TEvent>)options.GetTypeInfo(typeof(TEvent));
        var metadataTypeInfo = (JsonTypeInfo<EventMetadata>)options.GetTypeInfo(typeof(EventMetadata));

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            var propName = reader.GetString();
            reader.Read();

            if (string.Equals(propName, "id", StringComparison.OrdinalIgnoreCase))
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (reader.TryGetGuid(out var guid))
                    {
                        id = new EventId(guid);
                    }
                }
            }
            else if (string.Equals(propName, "type", StringComparison.OrdinalIgnoreCase))
            {
                var val = reader.GetString();
                if (val is not null) type = new EventType(val);
            }
            else if (string.Equals(propName, "version", StringComparison.OrdinalIgnoreCase))
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (reader.TryGetUInt32(out var verVal))
                    {
                        version = new EventVersion(verVal == 0 ? 1 : verVal);
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    if (uint.TryParse(reader.GetString(), out var strVer))
                    {
                        version = new EventVersion(strVer == 0 ? 1 : strVer);
                    }
                }
            }
            else if (string.Equals(propName, "occurredAt", StringComparison.OrdinalIgnoreCase))
            {
                occurredAt = reader.GetDateTimeOffset();
            }
            else if (string.Equals(propName, "payload", StringComparison.OrdinalIgnoreCase))
            {
                payload = JsonSerializer.Deserialize(ref reader, payloadTypeInfo);
            }
            else if (string.Equals(propName, "metadata", StringComparison.OrdinalIgnoreCase))
            {
                metadata = JsonSerializer.Deserialize(ref reader, metadataTypeInfo);
            }
            else
            {
                reader.Skip();
            }
        }

        if (payload is null)
        {
            throw new JsonException($"Missing payload property when deserializing {nameof(EventEnvelope<TEvent>)}.");
        }

        return new EventEnvelope<TEvent>(
            id,
            type,
            version,
            occurredAt,
            payload,
            metadata);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EventEnvelope<TEvent> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        var payloadTypeInfo = (JsonTypeInfo<TEvent>)options.GetTypeInfo(typeof(TEvent));
        var metadataTypeInfo = (JsonTypeInfo<EventMetadata>)options.GetTypeInfo(typeof(EventMetadata));

        writer.WriteStartObject();

        writer.WriteString("id", value.Id.Value);
        writer.WriteString("type", value.Type.Value);
        writer.WriteNumber("version", value.Version.Value);
        writer.WriteString("occurredAt", value.OccurredAt);

        writer.WritePropertyName("payload");
        JsonSerializer.Serialize(writer, value.Payload, payloadTypeInfo);

        if (value.Metadata != EventMetadata.Empty)
        {
            writer.WritePropertyName("metadata");
            JsonSerializer.Serialize(writer, value.Metadata, metadataTypeInfo);
        }

        writer.WriteEndObject();
    }
}


