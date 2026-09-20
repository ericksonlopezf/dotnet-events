// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Events.CloudEvents;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Xunit;

namespace EricksonLopez.Events.CloudEvents.Tests;

[Trait("Category", "Unit")]
public sealed class CloudEventCollisionTests
{
    private sealed record CollisionTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [Fact]
    public void ToCloudEvent_WhenHeadersCollideWithDifferentValues_ShouldThrowInvalidOperationException()
    {
        // Validates EVT-SEC-002 Remediation:
        // Silently overwriting extension attributes on hyphen normalization collision is rejected.
        var evt = new CollisionTestEvent(EventId.New(), DateTimeOffset.UtcNow);
        var metadata = new EventMetadataBuilder()
            .WithHeader("X-Auth-Token", "SecretToken1")
            .WithHeader("xauthtoken", "SecretToken2")
            .Build();

        var envelope = EventEnvelope.Create(evt, metadata);

        Action act = () => envelope.ToCloudEvent();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CloudEvent extension attribute key collision*");
    }

    [Fact]
    public void ToCloudEvent_WhenHeadersNormalizeToSameKeyWithIdenticalValue_ShouldNotThrow()
    {
        var evt = new CollisionTestEvent(EventId.New(), DateTimeOffset.UtcNow);
        var metadata = new EventMetadataBuilder()
            .WithHeader("X-Auth-Token", "SameToken")
            .WithHeader("xauthtoken", "SameToken")
            .Build();

        var envelope = EventEnvelope.Create(evt, metadata);

        var cloudEvent = envelope.ToCloudEvent();
        cloudEvent.ExtensionAttributes.Should().NotBeNull();
        cloudEvent.ExtensionAttributes!["xauthtoken"].Should().Be("SameToken");
    }
}
