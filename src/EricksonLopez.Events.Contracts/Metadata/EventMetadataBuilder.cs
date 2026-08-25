// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Metadata;

using System.Collections.Frozen;
using EricksonLopez.Events.Identifiers;

/// <summary>
/// Provides a fluent builder for creating immutable <see cref="EventMetadata"/> instances.
/// </summary>
public sealed class EventMetadataBuilder
{
    private CorrelationId _correlationId = CorrelationId.Empty;
    private CausationId _causationId = CausationId.Empty;
    private TenantId _tenantId = TenantId.Empty;
    private string? _source;
    private string? _contentType;
    private readonly Dictionary<string, string> _customHeaders = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Configures the correlation identifier for the event metadata.</summary>
    /// <param name="correlationId">The correlation identifier tracing the end-to-end business workflow.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithCorrelationId(CorrelationId correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    /// <summary>Configures the correlation identifier for the event metadata from a string value.</summary>
    /// <param name="correlationId">The correlation identifier string.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithCorrelationId(string correlationId)
    {
        _correlationId = CorrelationId.From(correlationId);
        return this;
    }

    /// <summary>Configures the causation identifier representing the direct trigger of the event.</summary>
    /// <param name="causationId">The causation identifier of the event that caused the current event.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithCausationId(CausationId causationId)
    {
        _causationId = causationId;
        return this;
    }

    /// <summary>Configures the causation identifier for the event metadata from a string value.</summary>
    /// <param name="causationId">The causation identifier string.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithCausationId(string causationId)
    {
        _causationId = CausationId.From(causationId);
        return this;
    }

    /// <summary>Configures the tenant identifier for multi-tenant event routing and isolation.</summary>
    /// <param name="tenantId">The tenant identifier to associate with the event.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>Configures the tenant identifier for the event metadata from a string value.</summary>
    /// <param name="tenantId">The tenant identifier string.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithTenantId(string tenantId)
    {
        _tenantId = TenantId.From(tenantId);
        return this;
    }

    /// <summary>Configures the originating source of the event.</summary>
    /// <param name="source">The source identifier or URI string (e.g., a service name or absolute URI).</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithSource(string? source)
    {
        _source = source;
        return this;
    }

    /// <summary>Configures the MIME content type of the event payload.</summary>
    /// <param name="contentType">The MIME content type string (e.g., <c>application/json</c>).</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventMetadataBuilder WithContentType(string? contentType)
    {
        _contentType = contentType;
        return this;
    }

    /// <summary>Adds or replaces a custom header entry in the event metadata.</summary>
    /// <param name="key">The header key. Must not be <see langword="null"/>, empty, or whitespace.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    public EventMetadataBuilder WithHeader(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _customHeaders[key] = value ?? string.Empty;
        return this;
    }

    /// <summary>Builds and returns the configured immutable <see cref="EventMetadata"/> instance.</summary>
    /// <returns>A new immutable <see cref="EventMetadata"/> instance containing all configured values.</returns>
    public EventMetadata Build()
    {
        var headers = _customHeaders.Count > 0
            ? _customHeaders.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
            : FrozenDictionary<string, string>.Empty;

        return new EventMetadata(
            _correlationId,
            _causationId,
            _tenantId,
            _source,
            _contentType,
            headers);
    }
}



