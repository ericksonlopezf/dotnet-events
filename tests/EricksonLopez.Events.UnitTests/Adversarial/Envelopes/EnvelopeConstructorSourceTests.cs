// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Envelopes;

[Trait("Category", "Adversarial")]
public sealed class EnvelopeConstructorSourceTests
{
    [EventName("catalog.item-added")]
    [EventVersion(1)]
    [EventSource("catalog-service")]
    private sealed record CatalogItemAddedEvent(EventId Id, DateTimeOffset OccurredAt, string Sku) : IIntegrationEvent;

    [Fact]
    public void EVT_MOD_003_EventEnvelope_DirectConstructor_PopulatesSourceAttribute_ConsistentWithFactoryMethod()
    {
        // Verified Remediation for EVT-MOD-003:
        // Both EventEnvelope.Create and direct constructor new EventEnvelope<T>(...)
        // resolve descriptor.Source ("catalog-service") from [EventSource] attribute when metadata.Source is null.

        var evt = new CatalogItemAddedEvent(EventId.New(), DateTimeOffset.UtcNow, "SKU-100");

        // Factory method populates source from attribute:
        var factoryEnvelope = EventEnvelope.Create(evt);
        factoryEnvelope.Metadata.Source.Should().Be("catalog-service", "EventEnvelope.Create populates source from [EventSource]");

        // Direct constructor also populates source from attribute:
        var constructorEnvelope = new EventEnvelope<CatalogItemAddedEvent>(
            evt.Id,
            default(EventType),
            default,
            default,
            evt,
            metadata: null);

        // REMEDIATION VERIFICATION:
        constructorEnvelope.Metadata.Source.Should().Be("catalog-service", "EVT-MOD-003: Direct constructor now resolves [EventSource] attribute consistently with EventEnvelope.Create.");
    }
}
