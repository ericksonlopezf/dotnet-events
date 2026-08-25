// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

namespace EricksonLopez.Events.CloudEvents;

/// <summary>
/// Provides extension methods for converting between <see cref="EventEnvelope{TEvent}"/> and <see cref="CloudEvent{TData}"/>.
/// </summary>
public static class CloudEventExtensions
{
    /// <summary>
    /// Converts a strongly-typed <see cref="EventEnvelope{TEvent}"/> into a <see cref="CloudEvent{TEvent}"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event payload.</typeparam>
    /// <param name="envelope">The event envelope instance to convert.</param>
    /// <param name="defaultSource">The fallback producer source URI used when metadata does not contain a valid source.</param>
    /// <param name="schemaBaseUri">The optional base URI for constructing the dataschema URI.</param>
    /// <returns>A new <see cref="CloudEvent{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="envelope"/> is <see langword="null"/></exception>
    public static CloudEvent<TEvent> ToCloudEvent<TEvent>(
        this EventEnvelope<TEvent> envelope,
        Uri? defaultSource = null,
        Uri? schemaBaseUri = null)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);

        Uri sourceUri;
        if (!string.IsNullOrWhiteSpace(envelope.Metadata.Source) &&
            Uri.TryCreate(envelope.Metadata.Source, UriKind.RelativeOrAbsolute, out var parsedSource))
        {
            sourceUri = parsedSource;
        }
        else
        {
            sourceUri = defaultSource ?? new Uri($"urn:events:{envelope.Type.Value.ToLowerInvariant()}");
        }

        Uri? dataSchema = null;
        if (schemaBaseUri != null)
        {
            dataSchema = new Uri(schemaBaseUri, $"schemas/{envelope.Type.Value}/v{envelope.Version.Value}");
        }

        Dictionary<string, object?>? extensionAttributes = null;
        if (envelope.Metadata.CustomHeaders.Count > 0)
        {
            extensionAttributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in envelope.Metadata.CustomHeaders)
            {
                // Extension attributes in CloudEvents specification must be lowercase alphanumeric
                string normalizedKey = key.ToLowerInvariant().Replace("-", string.Empty);
                extensionAttributes[normalizedKey] = value;
            }
        }

        return new CloudEvent<TEvent>(
            id: envelope.Id.ToString(),
            source: sourceUri,
            type: envelope.Type.Value,
            data: envelope.Payload,
            time: envelope.OccurredAt.ToUniversalTime(),
            dataContentType: envelope.Metadata.ContentType ?? "application/json",
            dataSchema: dataSchema,
            correlationId: envelope.Metadata.CorrelationId.IsEmpty ? null : envelope.Metadata.CorrelationId.Value,
            causationId: envelope.Metadata.CausationId.IsEmpty ? null : envelope.Metadata.CausationId.Value,
            tenantId: envelope.Metadata.TenantId.IsEmpty ? null : envelope.Metadata.TenantId.Value,
            extensionAttributes: extensionAttributes);
    }

    /// <summary>
    /// Converts a <see cref="CloudEvent{TEvent}"/> into a strongly-typed <see cref="EventEnvelope{TEvent}"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event payload.</typeparam>
    /// <param name="cloudEvent">The cloud event instance to convert.</param>
    /// <returns>A new <see cref="EventEnvelope{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cloudEvent"/> is <see langword="null"/></exception>
    public static EventEnvelope<TEvent> ToEventEnvelope<TEvent>(this CloudEvent<TEvent> cloudEvent)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(cloudEvent);

        var eventId = EventId.TryParse(cloudEvent.Id, null, out var parsedId)
            ? parsedId
            : cloudEvent.Data.Id;

        var eventType = EventType.From(cloudEvent.Type);

        var metadataBuilder = new EventMetadataBuilder()
            .WithSource(cloudEvent.Source.ToString())
            .WithContentType(cloudEvent.DataContentType ?? "application/json");

        if (!string.IsNullOrWhiteSpace(cloudEvent.CorrelationId))
        {
            metadataBuilder.WithCorrelationId(CorrelationId.From(cloudEvent.CorrelationId));
        }

        if (!string.IsNullOrWhiteSpace(cloudEvent.CausationId))
        {
            metadataBuilder.WithCausationId(CausationId.From(cloudEvent.CausationId));
        }

        if (!string.IsNullOrWhiteSpace(cloudEvent.TenantId))
        {
            metadataBuilder.WithTenantId(TenantId.From(cloudEvent.TenantId));
        }

        if (cloudEvent.ExtensionAttributes != null)
        {
            foreach (var (key, value) in cloudEvent.ExtensionAttributes)
            {
                if (value != null)
                {
                    metadataBuilder.WithHeader(key, value.ToString()!);
                }
            }
        }

        var metadata = metadataBuilder.Build();

        return new EventEnvelope<TEvent>(
            id: eventId,
            type: eventType,
            version: default,
            occurredAt: cloudEvent.Time ?? cloudEvent.Data.OccurredAt,
            payload: cloudEvent.Data,
            metadata: metadata);
    }
}
