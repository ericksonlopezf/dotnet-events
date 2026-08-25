// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Common;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Diagnostics")]
[Xunit.Trait("Category", "Unit")]
public sealed class TestInfrastructureTests
{
    public sealed record SampleInfraEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class SampleInfraHandler : IEventHandler<SampleInfraEvent>
    {
        public bool Handled { get; private set; }

        public ValueTask HandleAsync(SampleInfraEvent @event, CancellationToken cancellationToken = default)
        {
            Handled = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public void TrackingSynchronizationContext_Post_IncrementsCountAndInvokesCallback()
    {
        var context = new TrackingSynchronizationContext();
        bool callbackExecuted = false;

        context.Post(_ => { callbackExecuted = true; }, null);

        context.PostCount.Should().Be(1);
        callbackExecuted.Should().BeTrue();
    }

    [Fact]
    public void ActivityTestScope_CapturesStartedAndStoppedActivities()
    {
        const string sourceName = "Test.Activity.Source";
        using var source = new ActivitySource(sourceName);
        using var scope = new ActivityTestScope(sourceName);

        using (var act = source.StartActivity("CustomActivity"))
        {
            act.Should().NotBeNull();
            scope.StartedActivities.Should().ContainSingle(a => a.OperationName == "CustomActivity");
        }

        scope.StoppedActivities.Should().ContainSingle(a => a.OperationName == "CustomActivity");
    }

    [Fact]
    public void MeterTestScope_CapturesLongAndDoubleMeasurements()
    {
        const string meterName = "Test.Meter.Source";
        using var meter = new Meter(meterName);
        using var scope = new MeterTestScope(meterName);

        var counter = meter.CreateCounter<long>("test.counter");
        var histogram = meter.CreateHistogram<double>("test.histogram");

        counter.Add(10, new KeyValuePair<string, object?>("tag1", "val1"));
        histogram.Record(42.5, new KeyValuePair<string, object?>("tag2", "val2"));
        scope.RecordObservableInstruments();

        scope.LongMeasurements.Should().ContainSingle(m => m.InstrumentName == "test.counter" && m.Value == 10 && (string?)m.Tags["tag1"] == "val1");
        scope.DoubleMeasurements.Should().ContainSingle(m => m.InstrumentName == "test.histogram" && m.Value == 42.5 && (string?)m.Tags["tag2"] == "val2");
    }

    [Fact]
    public async Task EventBusTestFixture_BuildsAndDispatchesCorrectly()
    {
        using var fixture = new EventBusTestFixture()
            .WithOptions(opts => opts.MaxReentrancyDepth = 20)
            .WithEventHandler<SampleInfraEvent, SampleInfraHandler>(ServiceLifetime.Singleton);

        var bus = fixture.GetBus();
        var handler = fixture.GetService<SampleInfraHandler>();

        var evt = new SampleInfraEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        handler.Handled.Should().BeTrue();
    }
}





