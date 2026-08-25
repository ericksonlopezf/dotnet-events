// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Diagnostics;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Metadata;

/// <summary>
/// Provides zero-cost OpenTelemetry instrumentation, distributed tracing, and metrics for event operations.
/// </summary>
public static class EventsDiagnostics
{
    /// <summary>
    /// Gets the name of the instrumentation source.
    /// </summary>
    public const string SourceName = "EricksonLopez.Events";

    /// <summary>
    /// Gets the version of the instrumentation source.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>Gets the <see cref="System.Diagnostics.ActivitySource"/> used for distributed tracing of event operations.</summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, Version);

    /// <summary>Gets the <see cref="System.Diagnostics.Metrics.Meter"/> used for recording event metrics.</summary>
    public static readonly Meter Meter = new(SourceName, Version);

    private static readonly Counter<long> s_eventsPublished =
        Meter.CreateCounter<long>("events.published.count", description: "Number of events published");

    private static readonly Counter<long> s_eventsHandled =
        Meter.CreateCounter<long>("events.handled.count", description: "Number of events processed by handlers");

    private static readonly Histogram<double> s_handlingDuration =
        Meter.CreateHistogram<double>("events.handling.duration", unit: "ms", description: "Duration of event handler execution");

    /// <summary>
    /// Starts an activity for an event publication if tracing listeners are active.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event being published.</typeparam>
    /// <param name="event">The event instance being published.</param>
    /// <param name="metadata">The optional ambient metadata associated with the event.</param>
    /// <returns>An active <see cref="Activity"/> if listeners are configured; otherwise, <see langword="null"/>.</returns>
    public static Activity? StartPublishActivity<TEvent>(TEvent @event, EventMetadata? metadata = null)
        where TEvent : IEvent
    {
        var activity = ActivitySource.StartActivity($"Event.Publish {typeof(TEvent).Name}", ActivityKind.Producer);
        if (activity is not null)
        {
            activity.SetTag("messaging.system", "ericksonlopez.events");
            activity.SetTag("messaging.event.id", @event.Id.ToString());
            activity.SetTag("messaging.event.type", typeof(TEvent).FullName);

            if (metadata is not null)
            {
                if (!metadata.CorrelationId.IsEmpty)
                {
                    activity.SetTag("messaging.correlation_id", metadata.CorrelationId.Value);
                }

                if (!metadata.CausationId.IsEmpty)
                {
                    activity.SetTag("messaging.causation_id", metadata.CausationId.Value);
                }

                if (!metadata.TenantId.IsEmpty)
                {
                    activity.SetTag("messaging.tenant_id", metadata.TenantId.Value);
                }
            }
        }

        return activity;
    }

    /// <summary>
    /// Records a published event metric.
    /// </summary>
    /// <param name="eventType">The type name of the event.</param>
    public static void RecordEventPublished(string eventType) =>
        s_eventsPublished.Add(1, new KeyValuePair<string, object?>("event.type", eventType));

    /// <summary>
    /// Records a handled event metric with its duration and outcome.
    /// </summary>
    /// <param name="eventType">The type name of the event.</param>
    /// <param name="durationMs">The execution duration in milliseconds.</param>
    /// <param name="success"><see langword="true"/> if handling succeeded; otherwise, <see langword="false"/>.</param>
    public static void RecordEventHandled(string eventType, double durationMs, bool success)
    {
        s_eventsHandled.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("event.success", success));

        s_handlingDuration.Record(durationMs, new KeyValuePair<string, object?>("event.type", eventType));
    }
}


