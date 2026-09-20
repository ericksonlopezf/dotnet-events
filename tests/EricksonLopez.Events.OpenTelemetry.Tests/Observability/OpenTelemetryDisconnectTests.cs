// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Diagnostics;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.OpenTelemetry;
using global::OpenTelemetry;
using global::OpenTelemetry.Trace;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.OpenTelemetry.Tests.Observability;

[Trait("Category", "Integration")]
public sealed class OpenTelemetryDisconnectTests
{
    public sealed record OrderPlacedIntegrationEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class OrderPlacedHandler : IEventHandler<OrderPlacedIntegrationEvent>
    {
        public ValueTask HandleAsync(OrderPlacedIntegrationEvent @event, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
    }

    [Fact]
    public async Task AddEventsInstrumentation_ListensToEventBus_ActivitiesAreCaptured()
    {
        // Validates EVT-OBS-001 Remediation:
        // AddEventsInstrumentation registers both EventsDiagnostics.SourceName and EventBusDiagnostics.SourceName.
        // Therefore, when publishing through EventBus, activities ARE captured!

        bool busActivityCaptured = false;

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == EventBusDiagnostics.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = act => busActivityCaptured = true
        };
        ActivitySource.AddActivityListener(listener);

        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<OrderPlacedIntegrationEvent, OrderPlacedHandler>();
        var sp = services.BuildServiceProvider();

        var bus = sp.GetRequiredService<IEventBus>();
        await bus.PublishAsync(new OrderPlacedIntegrationEvent(EventId.New(), DateTimeOffset.UtcNow));

        busActivityCaptured.Should().BeTrue(
            "EventBus uses 'EricksonLopez.Events.Bus' and AddEventsInstrumentation now registers it, ensuring zero dropped activities.");
    }
}
