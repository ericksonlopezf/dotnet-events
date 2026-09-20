// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Events.CloudEvents;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Xunit;

namespace EricksonLopez.Events.CloudEvents.Tests;

[Trait("Category", "Adversarial")]
public sealed class CloudEventNonGuidIdDropTests
{
    private sealed record NonGuidTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [Fact]
    public void EVT_CLD_001_ToEventEnvelope_WhenCloudEventHasNonGuidId_SilentlyDropsIdAndReplacesWithDataId()
    {
        // ARRANGE:
        // CloudEvents 1.0 Specification states: "id: Identifies the event. String. Required. Non-empty string."
        // A valid CloudEvents string ID that is NOT a GUID (e.g. from AWS, Kafka, or custom systems):
        string nonGuidCloudEventId = "order-created-partition-4-seq-99823";

        var originalEventId = EventId.New();
        var data = new NonGuidTestEvent(originalEventId, DateTimeOffset.UtcNow);

        var cloudEvent = new CloudEvent<NonGuidTestEvent>(
            id: nonGuidCloudEventId,
            source: new Uri("urn:source:orders"),
            type: "order.created",
            data: data);

        // ACT:
        var envelope = cloudEvent.ToEventEnvelope();

        // ASSERT:
        // Envelope Id falls back to data.Id (since EventId requires UUID structure),
        // and the original non-GUID string ID is preserved in custom headers:
        envelope.Id.Should().Be(originalEventId);
        envelope.Metadata.CustomHeaders.Should().ContainKey("cloudevents.id")
            .WhoseValue.Should().Be(nonGuidCloudEventId);
    }
}
