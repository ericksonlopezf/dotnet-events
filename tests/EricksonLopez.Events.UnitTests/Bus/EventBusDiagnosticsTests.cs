// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.Bus;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.UnitTests.Common;
using Xunit;

[Collection("Diagnostics")]
[Xunit.Trait("Category", "Unit")]
public sealed class EventBusDiagnosticsTests
{
    private sealed record SampleTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [Fact]
    public void EventBusDiagnostics_Constants_ShouldMatchExpectedValues()
    {
        EventBusDiagnostics.SourceName.Should().Be("EricksonLopez.Events.Bus");
        EventBusDiagnostics.SourceVersion.Should().Be("2.0.0");
    }

    [Fact]
    public void EventBusDiagnostics_StartPublishActivity_WithoutListener_ShouldReturnNullOrInactiveActivity()
    {
        var evt = new SampleTestEvent(EventId.New(), DateTimeOffset.UtcNow);
        var activity = EventBusDiagnostics.StartPublishActivity(evt);
        activity?.Dispose();
    }

    [Fact]
    public void EventBusDiagnostics_StartPublishActivity_WithActiveListener_ShouldPopulateTagsAndOperationName()
    {
        using var activityScope = new ActivityTestScope(EventBusDiagnostics.SourceName);

        var evt = new SampleTestEvent(EventId.New(), DateTimeOffset.UtcNow);
        using var activity = EventBusDiagnostics.StartPublishActivity(evt);

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("EventBus.Publish");
        activity.Kind.Should().Be(ActivityKind.Internal);
        activity.TagObjects.Should().Contain(kv => kv.Key == "event.type" && (string?)kv.Value == typeof(SampleTestEvent).FullName);
        activity.TagObjects.Should().Contain(kv => kv.Key == "event.name" && (string?)kv.Value == nameof(SampleTestEvent));

        activityScope.StartedActivities.Should().ContainSingle(a => a.OperationName == "EventBus.Publish");
    }

    [Fact]
    public void EventBusDiagnostics_RecordPublish_Instruments_ShouldHaveExpectedMetadata()
    {
        using var meterScope = new MeterTestScope(EventBusDiagnostics.SourceName);

        // Trigger instrument creation
        EventBusDiagnostics.RecordPublish("DummyEvent", 1.0, 1, true);

        var published = meterScope.PublishedInstruments.Single(i => i.Name == "eventbus.events.published");
        published.Unit.Should().Be("{event}");
        published.Description.Should().Be("Number of events published through the EventBus");

        var failed = meterScope.PublishedInstruments.Single(i => i.Name == "eventbus.events.failed");
        failed.Unit.Should().Be("{event}");
        failed.Description.Should().Be("Number of events that failed during dispatch");

        var duration = meterScope.PublishedInstruments.Single(i => i.Name == "eventbus.dispatch.duration");
        duration.Unit.Should().Be("ms");
        duration.Description.Should().Be("Duration of event dispatch including all handlers in milliseconds");

        var handlers = meterScope.PublishedInstruments.Single(i => i.Name == "eventbus.handlers.executed");
        handlers.Unit.Should().Be("{handler}");
        handlers.Description.Should().Be("Number of individual event handlers executed");
    }

    [Fact]
    public void EventBusDiagnostics_RecordPublish_WithSuccessAndFailure_ShouldRecordExpectedMetrics()
    {
        using var meterScope = new MeterTestScope(EventBusDiagnostics.SourceName);

        // 1. Success with 2 handlers
        EventBusDiagnostics.RecordPublish("OrderPlacedEvent", 25.5, 2, success: true);

        // 2. Failure with 0 handlers
        EventBusDiagnostics.RecordPublish("OrderFailedEvent", 10.0, 0, success: false);

        meterScope.RecordObservableInstruments();

        // Assert published
        var published = meterScope.LongMeasurements.Where(m => m.InstrumentName == "eventbus.events.published").ToList();
        published.Should().HaveCount(2);

        var publishedPlaced = published.Single(m => (string?)m.Tags["event.type"] == "OrderPlacedEvent");
        publishedPlaced.Value.Should().Be(1);
        publishedPlaced.Tags["status"].Should().Be("success");

        var publishedFailed = published.Single(m => (string?)m.Tags["event.type"] == "OrderFailedEvent");
        publishedFailed.Value.Should().Be(1);
        publishedFailed.Tags["status"].Should().Be("failed");

        // Assert duration
        var durations = meterScope.DoubleMeasurements.Where(m => m.InstrumentName == "eventbus.dispatch.duration").ToList();
        durations.Should().HaveCount(2);
        durations.Single(m => (string?)m.Tags["event.type"] == "OrderPlacedEvent").Value.Should().Be(25.5);
        durations.Single(m => (string?)m.Tags["event.type"] == "OrderFailedEvent").Value.Should().Be(10.0);

        // Assert failed
        var failed = meterScope.LongMeasurements.Where(m => m.InstrumentName == "eventbus.events.failed").ToList();
        failed.Should().ContainSingle();
        failed[0].Value.Should().Be(1);
        failed[0].Tags["event.type"].Should().Be("OrderFailedEvent");

        // Assert executed handlers: only events with handlerCount > 0 must record to eventbus.handlers.executed
        var handlers = meterScope.LongMeasurements
            .Where(m => m.InstrumentName == "eventbus.handlers.executed")
            .ToList();
        handlers.Should().ContainSingle();
        handlers[0].Value.Should().Be(2);
        handlers[0].Tags["event.type"].Should().Be("OrderPlacedEvent");
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    public void EventBusDiagnostics_RecordPublish_ExecutedHandlers_ShouldOnlyRecordWhenCountGreaterThanZero(int handlerCount, bool shouldRecord)
    {
        using var meterScope = new MeterTestScope(EventBusDiagnostics.SourceName);

        EventBusDiagnostics.RecordPublish("HandlerCountTestEvent", 5.0, handlerCount, success: true);

        meterScope.RecordObservableInstruments();

        var handlers = meterScope.LongMeasurements
            .Where(m => m.InstrumentName == "eventbus.handlers.executed")
            .ToList();

        if (shouldRecord)
        {
            handlers.Should().ContainSingle();
            handlers[0].Value.Should().Be(handlerCount);
            handlers[0].Tags["event.type"].Should().Be("HandlerCountTestEvent");
        }
        else
        {
            handlers.Should().BeEmpty();
        }
    }
}




