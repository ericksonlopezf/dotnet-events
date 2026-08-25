// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Metadata;

using System.Collections.Frozen;
using EricksonLopez.Events.Identifiers;

/// <summary>
/// Represents strongly-typed, immutable ambient metadata accompanying an event across system boundaries.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EventMetadata"/> is immutable. All mutation operations (e.g., <see cref="WithHeader"/>) return a new instance.
/// The <see cref="CustomHeaders"/> collection is never <see langword="null"/> and is case-insensitive.
/// </para>
/// <para>
/// To construct instances fluently, use <see cref="EventMetadataBuilder"/>.
/// </para>
/// </remarks>
/// <seealso cref="EventMetadataBuilder"/>
public sealed record EventMetadata
{
    private static readonly FrozenDictionary<string, string> EmptyHeaders =
        FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the empty default <see cref="EventMetadata"/> instance.
    /// </summary>
    public static readonly EventMetadata Empty = new(
        CorrelationId.Empty,
        CausationId.Empty,
        TenantId.Empty,
        null,
        null,
        EmptyHeaders);

    /// <summary>
    /// Gets the correlation identifier tracing the end-to-end business workflow.
    /// </summary>
    public CorrelationId CorrelationId { get; init; }

    /// <summary>
    /// Gets the causation identifier representing the direct trigger of this event.
    /// </summary>
    public CausationId CausationId { get; init; }

    /// <summary>
    /// Gets the optional tenant identifier for multi-tenant isolation.
    /// </summary>
    public TenantId TenantId { get; init; }

    /// <summary>
    /// Gets the originating source or service that emitted the event.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>Gets the MIME content type of the event payload (e.g., <c>application/json</c>).</summary>
    public string? ContentType { get; init; }

    /// <summary>Gets the case-insensitive, read-only collection of custom transport headers or extension attributes.</summary>
    /// <remarks>Never <see langword="null"/>. Returns an empty collection when no custom headers are set.</remarks>
    public IReadOnlyDictionary<string, string> CustomHeaders { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventMetadata"/> class with the specified metadata values.
    /// </summary>
    /// <param name="correlationId">The correlation identifier tracing the workflow.</param>
    /// <param name="causationId">The causation identifier representing the direct trigger.</param>
    /// <param name="tenantId">The tenant identifier for multi-tenant isolation.</param>
    /// <param name="source">The optional originating source or service name.</param>
    /// <param name="contentType">The optional payload content type.</param>
    /// <param name="customHeaders">The optional collection of custom headers.</param>
    public EventMetadata(
        CorrelationId correlationId,
        CausationId causationId,
        TenantId tenantId,
        string? source = null,
        string? contentType = null,
        IReadOnlyDictionary<string, string>? customHeaders = null)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
        TenantId = tenantId;
        Source = source;
        ContentType = contentType;
        CustomHeaders = customHeaders is not null
            ? (customHeaders as FrozenDictionary<string, string> ?? customHeaders.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase))
            : EmptyHeaders;
    }

    /// <summary>
    /// Creates a new <see cref="EventMetadata"/> instance with specified values.
    /// </summary>
    /// <param name="correlationId">The optional correlation identifier.</param>
    /// <param name="causationId">The optional causation identifier.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="source">The optional originating source.</param>
    /// <param name="contentType">The optional payload content type.</param>
    /// <param name="customHeaders">The optional collection of custom headers.</param>
    /// <returns>A new <see cref="EventMetadata"/> instance with empty default identifiers for any omitted parameters.</returns>
    public static EventMetadata Create(
        CorrelationId? correlationId = null,
        CausationId? causationId = null,
        TenantId? tenantId = null,
        string? source = null,
        string? contentType = null,
        IReadOnlyDictionary<string, string>? customHeaders = null) =>
        new(
            correlationId ?? CorrelationId.Empty,
            causationId ?? CausationId.Empty,
            tenantId ?? TenantId.Empty,
            source,
            contentType,
            customHeaders);

    /// <summary>
    /// Returns a new <see cref="EventMetadata"/> instance with the specified header added or replaced.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <returns>A new <see cref="EventMetadata"/> with the updated header.</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    public EventMetadata WithHeader(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var dict = new Dictionary<string, string>(CustomHeaders, StringComparer.OrdinalIgnoreCase)
        {
            [key] = value ?? string.Empty
        };

        return this with { CustomHeaders = dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) };
    }

    /// <summary>
    /// Attempts to retrieve a custom header value by its key.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">When this method returns, contains the retrieved header value if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the header was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetHeader(string key, out string? value)
    {
        if (CustomHeaders.TryGetValue(key, out var val))
        {
            value = val;
            return true;
        }

        value = null;
        return false;
    }
}



