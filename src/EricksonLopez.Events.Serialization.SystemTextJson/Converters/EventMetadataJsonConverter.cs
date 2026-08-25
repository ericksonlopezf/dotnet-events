// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

/// <summary>
/// Provides Native AOT compatible JSON conversion for <see cref="EventMetadata"/>.
/// </summary>
public sealed class EventMetadataJsonConverter : JsonConverter<EventMetadata>
{
    /// <inheritdoc />
    public override EventMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject token when deserializing {nameof(EventMetadata)}.");
        }

        var correlationId = CorrelationId.Empty;
        var causationId = CausationId.Empty;
        var tenantId = TenantId.Empty;
        string? source = null;
        string? contentType = null;
        Dictionary<string, string>? customHeaders = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            var propName = reader.GetString();
            reader.Read();

            if (string.Equals(propName, "correlationId", StringComparison.OrdinalIgnoreCase))
            {
                var val = reader.GetString();
                if (val is not null) correlationId = new CorrelationId(val);
            }
            else if (string.Equals(propName, "causationId", StringComparison.OrdinalIgnoreCase))
            {
                var val = reader.GetString();
                if (val is not null) causationId = new CausationId(val);
            }
            else if (string.Equals(propName, "tenantId", StringComparison.OrdinalIgnoreCase))
            {
                var val = reader.GetString();
                if (val is not null) tenantId = new TenantId(val);
            }
            else if (string.Equals(propName, "source", StringComparison.OrdinalIgnoreCase))
            {
                source = reader.GetString();
            }
            else if (string.Equals(propName, "contentType", StringComparison.OrdinalIgnoreCase))
            {
                contentType = reader.GetString();
            }
            else if (string.Equals(propName, "customHeaders", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(propName, "headers", StringComparison.OrdinalIgnoreCase))
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    customHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }

                        var headerKey = reader.GetString()!;
                        reader.Read();
                        var headerVal = reader.GetString();
                        customHeaders[headerKey] = headerVal ?? string.Empty;
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
            else
            {
                reader.Skip();
            }
        }

        return new EventMetadata(
            correlationId,
            causationId,
            tenantId,
            source,
            contentType,
            customHeaders);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EventMetadata value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        if (!value.CorrelationId.IsEmpty)
        {
            writer.WriteString("correlationId", value.CorrelationId.Value);
        }

        if (!value.CausationId.IsEmpty)
        {
            writer.WriteString("causationId", value.CausationId.Value);
        }

        if (!value.TenantId.IsEmpty)
        {
            writer.WriteString("tenantId", value.TenantId.Value);
        }

        if (!string.IsNullOrEmpty(value.Source))
        {
            writer.WriteString("source", value.Source);
        }

        if (!string.IsNullOrEmpty(value.ContentType))
        {
            writer.WriteString("contentType", value.ContentType);
        }

        if (value.CustomHeaders.Count > 0)
        {
            writer.WriteStartObject("customHeaders");
            foreach (var kvp in value.CustomHeaders)
            {
                writer.WriteString(kvp.Key, kvp.Value);
            }
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}


