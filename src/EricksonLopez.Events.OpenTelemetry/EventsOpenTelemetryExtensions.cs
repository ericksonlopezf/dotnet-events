// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.OpenTelemetry;

using EricksonLopez.Events.Diagnostics;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Trace;

/// <summary>
/// Provides extension methods for registering event instrumentation with OpenTelemetry.
/// </summary>
public static class EventsOpenTelemetryExtensions
{
    /// <summary>
    /// Adds event distributed tracing instrumentation to the tracer provider.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="TracerProviderBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static TracerProviderBuilder AddEventsInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSource(EventsDiagnostics.SourceName);
    }

    /// <summary>
    /// Adds event metrics instrumentation to the meter provider.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="MeterProviderBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static MeterProviderBuilder AddEventsInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddMeter(EventsDiagnostics.SourceName);
    }
}


