// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.OpenTelemetry.Tests;

using System.Collections.Generic;
using System.Diagnostics;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Diagnostics;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.UnitTests.Common;
using global::OpenTelemetry;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Trace;
using Xunit;

public sealed record SampleOrderPlacedEvent(
    EventId Id,
    DateTimeOffset OccurredAt,
    string OrderNumber) : IEvent;

[Trait("Category", "Integration")]
public class EventsOpenTelemetryTests
{
    [Fact]
    public void AddEventsInstrumentation_TracerProvider_ConfiguresSource()
    {
        // Arrange
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddEventsInstrumentation()
            .Build();

        // Assert
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddEventsInstrumentation_TracerProvider_NullBuilder_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((TracerProviderBuilder)null!).AddEventsInstrumentation();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public void AddEventsInstrumentation_MeterProvider_NullBuilder_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((MeterProviderBuilder)null!).AddEventsInstrumentation();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public void AddEventsInstrumentation_MeterProvider_ConfiguresMeter()
    {
        // Arrange
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddEventsInstrumentation()
            .Build();

        // Assert
        meterProvider.Should().NotBeNull();
    }

    [Fact]
    public void EventsDiagnostics_StartPublishActivity_WhenListenerActive_GeneratesValidActivityWithTags()
    {
        // Arrange
        using var activityScope = new ActivityTestScope(EventsDiagnostics.SourceName);

        var @event = new SampleOrderPlacedEvent(
            EventId.New(),
            DateTimeOffset.UtcNow,
            "ORD-999");
        var metadata = EventMetadata.Create(
            correlationId: CorrelationId.New(),
            causationId: CausationId.From("CAUSE-123"),
            tenantId: TenantId.From("TENANT-456"));

        // Act
        using (var activity = EventsDiagnostics.StartPublishActivity(@event, metadata))
        {
            activity?.Stop();
        }

        // Assert
        activityScope.StoppedActivities.Should().NotBeEmpty();
        var recorded = activityScope.StoppedActivities[0];
        recorded.DisplayName.Should().Contain(nameof(SampleOrderPlacedEvent));
        recorded.GetTagItem("messaging.system").Should().Be("ericksonlopez.events");
        recorded.GetTagItem("messaging.correlation_id").Should().Be(metadata.CorrelationId.Value);
        recorded.GetTagItem("messaging.causation_id").Should().Be("CAUSE-123");
        recorded.GetTagItem("messaging.tenant_id").Should().Be("TENANT-456");
    }

    [Fact]
    public void EVT_SEC_005_StartPublishActivity_WithTagRedactor_SanitizesSensitiveTags()
    {
        // Verified Remediation for EVT-SEC-005:
        // TagRedactor enables scrubbing of PII (tenant.id) before they hit APM.

        using var activityScope = new ActivityTestScope(EricksonLopez.Events.Bus.Diagnostics.EventBusDiagnostics.SourceName);

        // Temporarily assign a redactor
        var originalRedactor = EricksonLopez.Events.Bus.Diagnostics.EventBusDiagnostics.TagRedactor;
        try
        {
            EricksonLopez.Events.Bus.Diagnostics.EventBusDiagnostics.TagRedactor = (key, value) =>
            {
                if (key == "tenant.id") return "***REDACTED***";
                return value;
            };

            var @event = new SampleOrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow, "ORD-999");
            var metadata = EventMetadata.Create(tenantId: TenantId.From("SECRET-TENANT-ID"));

            var envelope = EricksonLopez.Events.Envelopes.EventEnvelope.Create(@event, metadata);

            // Act
            using (EricksonLopez.Events.Context.EventContext.SetCurrent(envelope))
            {
                using (var activity = EricksonLopez.Events.Bus.Diagnostics.EventBusDiagnostics.StartPublishActivity(@event))
                {
                    activity?.Stop();
                }
            }

            // Assert
            activityScope.StoppedActivities.Should().NotBeEmpty();
            var recorded = activityScope.StoppedActivities[0];

            // The redactor should have replaced the value
            recorded.GetTagItem("tenant.id").Should().Be("***REDACTED***", "EVT-SEC-005: TagRedactor must sanitize tags before they are recorded in the OpenTelemetry Activity.");
        }
        finally
        {
            EricksonLopez.Events.Bus.Diagnostics.EventBusDiagnostics.TagRedactor = originalRedactor;
        }
    }

    [Fact]
    public void EventsDiagnostics_RecordEventMetrics_WhenCalled_RecordsMeasurements()
    {
        using var meterScope = new MeterTestScope(EventsDiagnostics.SourceName);

        EventsDiagnostics.RecordEventPublished(nameof(SampleOrderPlacedEvent));
        EventsDiagnostics.RecordEventHandled(nameof(SampleOrderPlacedEvent), 15.5, true);

        meterScope.LongMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "events.published.count" &&
            (string?)m.Tags["event.type"] == nameof(SampleOrderPlacedEvent) &&
            m.Value == 1);

        meterScope.LongMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "events.handled.count" &&
            (string?)m.Tags["event.type"] == nameof(SampleOrderPlacedEvent) &&
            (bool?)m.Tags["event.success"] == true &&
            m.Value == 1);

        meterScope.DoubleMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "events.handling.duration" &&
            (string?)m.Tags["event.type"] == nameof(SampleOrderPlacedEvent) &&
            m.Value == 15.5);
    }
}



