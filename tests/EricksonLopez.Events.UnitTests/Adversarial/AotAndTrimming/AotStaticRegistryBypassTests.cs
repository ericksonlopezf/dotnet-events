// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Registry;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.AotAndTrimming;

[Collection("DiagnosticsAndStaticRegistry")]
[Trait("Category", "Unit")]
public sealed class AotStaticRegistryBypassTests
{
    [EventName("custom.order-created")]
    [EventVersion(5)]
    [EventSource("test-source")]
    private sealed record TestAotEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [Fact]
    public void StaticEventTypeRegistry_GetEventType_RespectsStaticEventTypeRegistryCurrent()
    {
        // Validates EVT-AOT-001 Remediation:
        // StaticEventTypeRegistry.GetDescriptor<TEvent> queries Current first, enabling zero-reflection
        // compiled descriptors in Native AOT scenarios.

        var original = StaticEventTypeRegistry.Current;
        try
        {
            var customDescriptor = new EventTypeDescriptor(
                typeof(TestAotEvent),
                EventType.From("overridden.by.generator"),
                EventVersion.From(99),
                "custom-source");

            var customRegistry = new EventTypeRegistry(new[] { customDescriptor });
            StaticEventTypeRegistry.SetCurrent(customRegistry, allowOverride: true);

            var resolvedType = StaticEventTypeRegistry.GetEventType<TestAotEvent>();
            var resolvedVersion = StaticEventTypeRegistry.GetVersion<TestAotEvent>();

            resolvedType.Value.Should().Be("overridden.by.generator",
                "StaticEventTypeRegistry now queries Current first, honoring AOT compiled descriptors.");
            resolvedVersion.Value.Should().Be(99);
        }
        finally
        {
            StaticEventTypeRegistry.SetCurrent(original, allowOverride: true);
        }
    }
}
