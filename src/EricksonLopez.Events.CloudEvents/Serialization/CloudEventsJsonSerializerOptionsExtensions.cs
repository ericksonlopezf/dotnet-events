// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;

namespace EricksonLopez.Events.CloudEvents.Serialization;

/// <summary>
/// Provides extension methods for configuring <see cref="JsonSerializerOptions"/> with CloudEvents options.
/// </summary>
public static class CloudEventsJsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures <see cref="JsonSerializerOptions"/> with CloudEvents v1.0 standard settings.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to configure.</param>
    /// <returns>The configured <see cref="JsonSerializerOptions"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    public static JsonSerializerOptions ConfigureForCloudEvents(this JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        return options;
    }
}
