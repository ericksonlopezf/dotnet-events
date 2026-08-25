// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EricksonLopez.Events.CloudEvents;

/// <summary>
/// Represents an immutable CloudEvents v1.0 specification event message.
/// </summary>
/// <typeparam name="TData">The type of the event payload data.</typeparam>
public sealed record CloudEvent<TData>
{
    /// <summary>
    /// Gets the CloudEvents specification version. Always "1.0".
    /// </summary>
    [JsonPropertyName("specversion")]
    public string SpecVersion => "1.0";

    /// <summary>
    /// Gets the unique identifier for the event.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; }

    /// <summary>
    /// Gets the context in which an event happened (URI-reference).
    /// </summary>
    [JsonPropertyName("source")]
    public Uri Source { get; init; }

    /// <summary>
    /// Gets the type of event related to the originating occurrence.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; }

    /// <summary>
    /// Gets the timestamp of when the occurrence happened.
    /// </summary>
    [JsonPropertyName("time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? Time { get; init; }

    /// <summary>
    /// Gets the content type of data value.
    /// </summary>
    [JsonPropertyName("datacontenttype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DataContentType { get; init; }

    /// <summary>
    /// Gets the URI that identifies the schema that data adheres to.
    /// </summary>
    [JsonPropertyName("dataschema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Uri? DataSchema { get; init; }

    /// <summary>
    /// Gets the domain-specific event payload.
    /// </summary>
    [JsonPropertyName("data")]
    public TData Data { get; init; }

    /// <summary>
    /// Gets the correlation identifier extension attribute.
    /// </summary>
    [JsonPropertyName("correlationid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the causation identifier extension attribute.
    /// </summary>
    [JsonPropertyName("causationid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CausationId { get; init; }

    /// <summary>
    /// Gets the tenant identifier extension attribute.
    /// </summary>
    [JsonPropertyName("tenantid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets any additional extension attributes.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?>? ExtensionAttributes { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudEvent{TData}"/> class.
    /// </summary>
    /// <param name="id">The unique event identifier.</param>
    /// <param name="source">The originating producer source URI.</param>
    /// <param name="type">The semantic event type name.</param>
    /// <param name="data">The event payload data.</param>
    /// <param name="time">The optional occurrence timestamp.</param>
    /// <param name="dataContentType">The optional data MIME content type.</param>
    /// <param name="dataSchema">The optional data schema URI.</param>
    /// <param name="correlationId">The optional correlation identifier.</param>
    /// <param name="causationId">The optional causation identifier.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> or <paramref name="type"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="data"/> is <see langword="null"/></exception>
    [JsonConstructor]
    public CloudEvent(
        string id,
        Uri source,
        string type,
        TData data,
        DateTimeOffset? time = null,
        string? dataContentType = "application/json",
        Uri? dataSchema = null,
        string? correlationId = null,
        string? causationId = null,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentNullException.ThrowIfNull(data);

        Id = id;
        Source = source;
        Type = type;
        Data = data;
        Time = time;
        DataContentType = dataContentType;
        DataSchema = dataSchema;
        CorrelationId = correlationId;
        CausationId = causationId;
        TenantId = tenantId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudEvent{TData}"/> class with explicit extension attributes.
    /// </summary>
    /// <param name="id">The unique event identifier.</param>
    /// <param name="source">The originating producer source URI.</param>
    /// <param name="type">The semantic event type name.</param>
    /// <param name="data">The event payload data.</param>
    /// <param name="time">The optional occurrence timestamp.</param>
    /// <param name="dataContentType">The optional data MIME content type.</param>
    /// <param name="dataSchema">The optional data schema URI.</param>
    /// <param name="correlationId">The optional correlation identifier.</param>
    /// <param name="causationId">The optional causation identifier.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="extensionAttributes">The optional dictionary of custom extension attributes.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> or <paramref name="type"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="data"/> is <see langword="null"/></exception>
    public CloudEvent(
        string id,
        Uri source,
        string type,
        TData data,
        DateTimeOffset? time,
        string? dataContentType,
        Uri? dataSchema,
        string? correlationId,
        string? causationId,
        string? tenantId,
        Dictionary<string, object?>? extensionAttributes)
        : this(id, source, type, data, time, dataContentType, dataSchema, correlationId, causationId, tenantId)
    {
        ExtensionAttributes = extensionAttributes;
    }
}
