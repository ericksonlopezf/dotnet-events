// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Identifiers;

[Trait("Category", "Adversarial")]
public sealed class IdentifierEqualityAndSortingInvariantsTests
{
    [Fact]
    public void TenantId_CompareToZero_MustImply_EqualsTrue_AndIdenticalHashCode()
    {
        // Demonstration of EVT-MOD-001:
        // In standard .NET invariants: if x.CompareTo(y) == 0, then x.Equals(y) MUST be true,
        // and EqualityComparer<T>.Default.GetHashCode(x) MUST equal EqualityComparer<T>.Default.GetHashCode(y).
        var tenantLower = new TenantId("tenant-alpha");
        var tenantUpper = new TenantId("TENANT-ALPHA");

        int comparison = tenantLower.CompareTo(tenantUpper);
        comparison.Should().Be(0, "CompareTo uses OrdinalIgnoreCase");

        // The fundamental invariant test:
        bool equals = tenantLower.Equals(tenantUpper);
        int hashLower = tenantLower.GetHashCode();
        int hashUpper = tenantUpper.GetHashCode();

        // If CompareTo says they are equal, Equals must agree!
        equals.Should().BeTrue("CompareTo == 0 must imply Equals == true to prevent SortedSet/Dictionary corruption.");
        hashLower.Should().Be(hashUpper, "Equal objects must produce identical hash codes.");
    }

    [Fact]
    public void EventType_CompareToZero_MustImply_EqualsTrue_AndIdenticalHashCode()
    {
        // Demonstration of EVT-MOD-002:
        var type1 = EventType.From("order.created");
        var type2 = EventType.From("ORDER.CREATED");

        int comparison = type1.CompareTo(type2);
        comparison.Should().Be(0, "CompareTo uses OrdinalIgnoreCase");

        bool equals = type1.Equals(type2);
        int hash1 = type1.GetHashCode();
        int hash2 = type2.GetHashCode();

        equals.Should().BeTrue("CompareTo == 0 must imply Equals == true for EventType.");
        hash1.Should().Be(hash2, "Equal event types must produce identical hash codes.");
    }

    [Fact]
    public void EventEnvelope_WhenExplicitVersionV1Passed_MustNotBeOverwrittenByRegistryV2()
    {
        // Demonstration of EVT-MOD-003:
        // If an event has Version 2 configured, but a caller explicitly constructs an envelope with EventVersion.V1,
        // the explicit version must NOT be overwritten!
        var sampleEvent = new VersionedSampleEvent(EventId.New(), DateTimeOffset.UtcNow);

        // Explicitly requesting V1:
        var envelope = new EventEnvelope<VersionedSampleEvent>(
            sampleEvent.Id,
            EventType.From("versioned.sample"),
            EventVersion.V1,
            sampleEvent.OccurredAt,
            sampleEvent,
            EventMetadata.Empty);

        envelope.Version.Value.Should().Be(1u, "Explicitly passed EventVersion.V1 must be preserved and not overwritten.");
    }

    [Attributes.EventVersion(2)]
    public sealed record VersionedSampleEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;
}
