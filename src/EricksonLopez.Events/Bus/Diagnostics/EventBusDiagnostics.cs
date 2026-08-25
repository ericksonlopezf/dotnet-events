// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Bus.Diagnostics;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using EricksonLopez.Events.Contracts;

/// <summary>
/// Provides OpenTelemetry ActivitySource and Metrics instruments for the EventBus.
/// </summary>
public static class EventBusDiagnostics
{
    /// <summary>
    /// Gets the canonical name of the EventBus telemetry source and meter.
    /// </summary>
    public const string SourceName = "EricksonLopez.Events.Bus";

    /// <summary>
    /// Gets the current version of the EventBus telemetry source.
    /// </summary>
    public const string SourceVersion = "1.0.0";

    internal static readonly ActivitySource ActivitySource = new(SourceName, SourceVersion);
    internal static readonly Meter Meter = new(SourceName, SourceVersion);

    private static readonly Counter<long> PublishedEventsCounter = Meter.CreateCounter<long>(
        "eventbus.events.published",
        unit: "{event}",
        description: "Number of events published through the EventBus");

    private static readonly Counter<long> FailedEventsCounter = Meter.CreateCounter<long>(
        "eventbus.events.failed",
        unit: "{event}",
        description: "Number of events that failed during dispatch");

    private static readonly Histogram<double> DispatchDurationHistogram = Meter.CreateHistogram<double>(
        "eventbus.dispatch.duration",
        unit: "ms",
        description: "Duration of event dispatch including all handlers in milliseconds");

    private static readonly Counter<long> ExecutedHandlersCounter = Meter.CreateCounter<long>(
        "eventbus.handlers.executed",
        unit: "{handler}",
        description: "Number of individual event handlers executed");

    /// <summary>
    /// Starts an OpenTelemetry activity for event publication.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event being published.</typeparam>
    /// <param name="eventInstance">The event instance to trace.</param>
    /// <returns>An active <see cref="Activity"/> if listeners are configured; otherwise, <see langword="null"/>.</returns>
    public static Activity? StartPublishActivity<TEvent>(TEvent eventInstance) where TEvent : IEvent
    {
        var activity = ActivitySource.StartActivity("EventBus.Publish", ActivityKind.Internal);
        if (activity != null && activity.IsAllDataRequested)
        {
            activity.SetTag("event.type", typeof(TEvent).FullName);
            activity.SetTag("event.name", typeof(TEvent).Name);
        }
        return activity;
    }

    /// <summary>
    /// Records telemetry metrics for a completed event publication.
    /// </summary>
    /// <param name="eventType">The type name of the event.</param>
    /// <param name="durationMs">The elapsed dispatch time in milliseconds.</param>
    /// <param name="handlerCount">The number of handlers executed.</param>
    /// <param name="success"><see langword="true"/> if dispatch completed successfully; otherwise, <see langword="false"/>.</param>
    public static void RecordPublish(string eventType, double durationMs, int handlerCount, bool success)
    {
        PublishedEventsCounter.Add(1, new KeyValuePair<string, object?>("event.type", eventType), new KeyValuePair<string, object?>("status", success ? "success" : "failed"));
        DispatchDurationHistogram.Record(durationMs, new KeyValuePair<string, object?>("event.type", eventType));

        if (!success)
        {
            FailedEventsCounter.Add(1, new KeyValuePair<string, object?>("event.type", eventType));
        }

        if (handlerCount > 0)
        {
            ExecutedHandlersCounter.Add(handlerCount, new KeyValuePair<string, object?>("event.type", eventType));
        }
    }
}


