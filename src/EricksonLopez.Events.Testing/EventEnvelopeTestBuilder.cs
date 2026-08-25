// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

namespace EricksonLopez.Events.Testing;

/// <summary>
/// Provides a fluent test builder for constructing synthetic <see cref="EventEnvelope{TEvent}"/> and <see cref="EventMetadata"/> instances in unit tests.
/// </summary>
/// <typeparam name="TEvent">The type of the event payload.</typeparam>
public sealed class EventEnvelopeTestBuilder<TEvent> where TEvent : IEvent
{
    private readonly TEvent _payload;
    private EventId? _id;
    private EventType? _type;
    private EventVersion? _version;
    private DateTimeOffset? _occurredAt;
    private CorrelationId? _correlationId;
    private CausationId? _causationId;
    private TenantId? _tenantId;
    private string? _source;
    private string? _contentType;
    private readonly Dictionary<string, string> _customHeaders = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="EventEnvelopeTestBuilder{TEvent}"/> class wrapping the specified payload.
    /// </summary>
    /// <param name="payload">The event payload.</param>
    /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/></exception>
    public EventEnvelopeTestBuilder(TEvent payload)
    {
        _payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    /// <summary>
    /// Sets the event identifier.
    /// </summary>
    /// <param name="id">The event identifier.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithId(EventId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the semantic event type name.
    /// </summary>
    /// <param name="eventType">The semantic event type name.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithType(string eventType)
    {
        _type = EventType.From(eventType);
        return this;
    }

    /// <summary>
    /// Sets the schema contract version.
    /// </summary>
    /// <param name="version">The contract version.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithVersion(uint version)
    {
        _version = EventVersion.From(version);
        return this;
    }

    /// <summary>
    /// Sets the timestamp when the event occurred.
    /// </summary>
    /// <param name="occurredAt">The occurrence timestamp.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithOccurredAt(DateTimeOffset occurredAt)
    {
        _occurredAt = occurredAt;
        return this;
    }

    /// <summary>
    /// Sets the correlation identifier in the metadata.
    /// </summary>
    /// <param name="correlationId">The correlation identifier string.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithCorrelationId(string correlationId)
    {
        _correlationId = CorrelationId.From(correlationId);
        return this;
    }

    /// <summary>
    /// Sets the causation identifier in the metadata.
    /// </summary>
    /// <param name="causationId">The causation identifier string.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithCausationId(string causationId)
    {
        _causationId = CausationId.From(causationId);
        return this;
    }

    /// <summary>
    /// Sets the tenant identifier in the metadata.
    /// </summary>
    /// <param name="tenantId">The tenant identifier string.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithTenantId(string tenantId)
    {
        _tenantId = TenantId.From(tenantId);
        return this;
    }

    /// <summary>
    /// Sets the originating source identifier or URI in the metadata.
    /// </summary>
    /// <param name="source">The source identifier string.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithSource(string source)
    {
        _source = source;
        return this;
    }

    /// <summary>
    /// Sets the payload MIME content type in the metadata.
    /// </summary>
    /// <param name="contentType">The MIME content type string.</param>
    /// <returns>The current builder instance.</returns>
    public EventEnvelopeTestBuilder<TEvent> WithContentType(string contentType)
    {
        _contentType = contentType;
        return this;
    }

    /// <summary>
    /// Adds a custom header to the event metadata.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public EventEnvelopeTestBuilder<TEvent> WithHeader(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        _customHeaders[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the configured <see cref="EventEnvelope{TEvent}"/> instance.
    /// </summary>
    /// <returns>A new <see cref="EventEnvelope{TEvent}"/> instance.</returns>
    public EventEnvelope<TEvent> Build()
    {
        var metaBuilder = new EventMetadataBuilder();

        if (_correlationId.HasValue)
        {
            metaBuilder.WithCorrelationId(_correlationId.Value);
        }

        if (_causationId.HasValue)
        {
            metaBuilder.WithCausationId(_causationId.Value);
        }

        if (_tenantId.HasValue)
        {
            metaBuilder.WithTenantId(_tenantId.Value);
        }

        if (!string.IsNullOrEmpty(_source))
        {
            metaBuilder.WithSource(_source);
        }

        if (!string.IsNullOrEmpty(_contentType))
        {
            metaBuilder.WithContentType(_contentType);
        }

        foreach (var (k, v) in _customHeaders)
        {
            metaBuilder.WithHeader(k, v);
        }

        var metadata = metaBuilder.Build();

        return new EventEnvelope<TEvent>(
            id: _id ?? _payload.Id,
            type: _type ?? EventType.From(typeof(TEvent).Name),
            version: _version ?? EventVersion.V1,
            occurredAt: _occurredAt ?? _payload.OccurredAt,
            payload: _payload,
            metadata: metadata);
    }
}
