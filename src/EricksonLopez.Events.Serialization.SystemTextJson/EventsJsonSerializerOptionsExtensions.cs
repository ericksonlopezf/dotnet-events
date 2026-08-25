// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Serialization.SystemTextJson;

using System.Text.Json;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides extension methods for configuring <see cref="JsonSerializerOptions"/> with event serialization converters.
/// </summary>
public static class EventsJsonSerializerOptionsExtensions
{
    /// <summary>
    /// Adds Native AOT compatible event serialization converters to the options instance.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> instance to configure.</param>
    /// <returns>The configured <see cref="JsonSerializerOptions"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    public static JsonSerializerOptions AddEventsConverters(this JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Converters.Add(new EventIdJsonConverter());
        options.Converters.Add(new EventTypeJsonConverter());
        options.Converters.Add(new EventVersionJsonConverter());
        options.Converters.Add(new CorrelationIdJsonConverter());
        options.Converters.Add(new CausationIdJsonConverter());
        options.Converters.Add(new TenantIdJsonConverter());
        options.Converters.Add(new EventMetadataJsonConverter());

        return options;
    }

    /// <summary>
    /// Creates a new <see cref="JsonSerializerOptions"/> instance configured with default event serialization converters.
    /// </summary>
    /// <returns>A new <see cref="JsonSerializerOptions"/> instance configured for event serialization.</returns>
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return options.AddEventsConverters();
    }
}


