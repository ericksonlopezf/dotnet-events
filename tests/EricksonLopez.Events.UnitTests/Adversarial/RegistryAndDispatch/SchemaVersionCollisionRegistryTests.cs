// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Registry;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.RegistryAndDispatch;

[Trait("Category", "Adversarial")]
public sealed class SchemaVersionCollisionRegistryTests
{
    private sealed record OrderPlacedV1(EventId Id, DateTimeOffset OccurredAt, string OrderId) : IEvent;
    private sealed record OrderPlacedV2(EventId Id, DateTimeOffset OccurredAt, string OrderId, decimal Amount) : IEvent;

    [Fact]
    public void EventTypeRegistry_WhenMultipleVersionsOfSameEventTypeRegistered_MustPreserveBothVersions()
    {
        var eventType = EventType.From("sales.order-placed");

        var descV1 = new EventTypeDescriptor(typeof(OrderPlacedV1), eventType, EventVersion.From(1), "order-service");
        var descV2 = new EventTypeDescriptor(typeof(OrderPlacedV2), eventType, EventVersion.From(2), "order-service");

        var registry = new EventTypeRegistry(new[] { descV1, descV2 });

        // Both CLR types must resolve their respective descriptors:
        registry.TryGetDescriptor(typeof(OrderPlacedV1), out var resolvedV1).Should().BeTrue();
        resolvedV1!.Version.Value.Should().Be(1);
        resolvedV1.ClrType.Should().Be(typeof(OrderPlacedV1));

        registry.TryGetDescriptor(typeof(OrderPlacedV2), out var resolvedV2).Should().BeTrue();
        resolvedV2!.Version.Value.Should().Be(2);
        resolvedV2.ClrType.Should().Be(typeof(OrderPlacedV2));

        // When retrieving all descriptors:
        registry.GetAllDescriptors().Count.Should().Be(2);

        // Version-aware lookup allows resolving each version unambiguously:
        registry.TryGetDescriptor(eventType, EventVersion.From(1), out var versioned1).Should().BeTrue();
        versioned1!.Version.Value.Should().Be(1);
        versioned1.ClrType.Should().Be(typeof(OrderPlacedV1));

        registry.TryGetDescriptor(eventType, EventVersion.From(2), out var versioned2).Should().BeTrue();
        versioned2!.Version.Value.Should().Be(2);
        versioned2.ClrType.Should().Be(typeof(OrderPlacedV2));
    }
}
