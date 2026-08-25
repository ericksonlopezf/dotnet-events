// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.Envelopes;

using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using AwesomeAssertions;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventEnvelopeTests
{
    [EventName("orders.order-created")]
    [EventVersion(2)]
    [EventSource("orders.service")]
    private sealed record TestOrderCreated(EventId Id, string OrderNumber, DateTimeOffset OccurredAt) : IIntegrationEvent;

    private sealed record PlainEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;

    [Fact]
    public void EventEnvelope_Create_WithAnnotatedEvent_ShouldAutoResolveTypeVersionAndSource()
    {
        var id = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var ev = new TestOrderCreated(id, "ORD-999", now);

        var envelope = EventEnvelope.Create(ev);

        envelope.Id.Should().Be(id);
        envelope.Type.Should().Be(EventType.From("orders.order-created"));
        envelope.Version.Should().Be(EventVersion.From(2));
        envelope.OccurredAt.Should().Be(now);
        envelope.Payload.Should().Be(ev);
        envelope.Metadata.Source.Should().Be("orders.service");
        ((IEventEnvelope)envelope).GetPayload().Should().Be(ev);
    }

    [Fact]
    public void EventEnvelope_Create_WithExplicitTypeAndVersionOverrides_ShouldApplyOverrides()
    {
        var ev = new TestOrderCreated(EventId.New(), "ORD-123", DateTimeOffset.UtcNow);
        var customType = EventType.From("custom.override");
        var customVersion = EventVersion.From(5);

        var envelope = EventEnvelope.Create(ev, null, customType, customVersion);

        envelope.Type.Should().Be(customType);
        envelope.Version.Should().Be(customVersion);
    }

    [Fact]
    public void EventEnvelope_Create_WithExistingSourceInMetadata_ShouldNotOverwriteDescriptorSource()
    {
        var ev = new TestOrderCreated(EventId.New(), "ORD-123", DateTimeOffset.UtcNow);
        var meta = new EventMetadataBuilder().WithSource("custom-override-source").Build();

        var envelope = EventEnvelope.Create(ev, meta);

        envelope.Metadata.Source.Should().Be("custom-override-source");
    }

    [Fact]
    public void EventEnvelope_Create_WithNullEvent_ShouldThrowArgumentNullException()
    {
        Action act = () => EventEnvelope.Create<PlainEvent>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EventEnvelope_Constructor_WithEmptyDefaults_ShouldInferFromPayloadAndRegistry()
    {
        var ev = new TestOrderCreated(EventId.New(), "ORD-1", DateTimeOffset.UtcNow);
        var envelope = new EventEnvelope<TestOrderCreated>(
            EventId.Empty,
            default,
            default,
            default,
            ev,
            null);

        envelope.Id.Should().Be(ev.Id);
        envelope.Type.Should().Be(EventType.From("orders.order-created"));
        envelope.Version.Should().Be(EventVersion.From(2));
        envelope.OccurredAt.Should().Be(ev.OccurredAt);
        envelope.Metadata.Should().Be(EventMetadata.Empty);
    }

    [Fact]
    public void EventEnvelope_Constructor_WithExplicitCustomIdAndOccurredAt_ShouldRetainExplicitValues()
    {
        var ev = new TestOrderCreated(EventId.New(), "ORD-1", DateTimeOffset.UtcNow.AddMinutes(-10));
        var explicitId = EventId.New();
        var explicitOccurredAt = DateTimeOffset.UtcNow;

        var envelope = new EventEnvelope<TestOrderCreated>(
            explicitId,
            default,
            default,
            explicitOccurredAt,
            ev,
            null);

        envelope.Id.Should().Be(explicitId);
        envelope.OccurredAt.Should().Be(explicitOccurredAt);
    }

    [Fact]
    public void EventEnvelope_Constructor_WithNullPayload_ShouldThrowArgumentNullException()
    {
        Action act = () => new EventEnvelope<PlainEvent>(
            EventId.New(),
            EventType.From("test"),
            EventVersion.V1,
            DateTimeOffset.UtcNow,
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EventEnvelope_Create_WithPlainEvent_ShouldFallbackToConvention()
    {
        var ev = new PlainEvent(EventId.New(), DateTimeOffset.UtcNow);

        var envelope = EventEnvelope.Create(ev);

        envelope.Type.Should().Be(EventType.From(nameof(PlainEvent)));
        envelope.Version.Should().Be(EventVersion.V1);
    }

    [Fact]
    public void EventEnvelope_Wrap_WithCorrelationAndCausation_ShouldPopulateMetadata()
    {
        var ev = new PlainEvent(EventId.New(), DateTimeOffset.UtcNow);
        var correlationId = CorrelationId.New();
        var causationId = CausationId.From("CMD-1");
        var tenantId = TenantId.From("tenant-1");

        var envelope = EventEnvelope.Wrap(ev, correlationId, causationId, tenantId, "test.source");

        envelope.Metadata.CorrelationId.Should().Be(correlationId);
        envelope.Metadata.CausationId.Should().Be(causationId);
        envelope.Metadata.TenantId.Should().Be(tenantId);
        envelope.Metadata.Source.Should().Be("test.source");
    }

    [Fact]
    public void EventEnvelope_Wrap_WithNullOptionals_ShouldDefaultCorrectly()
    {
        var ev = new PlainEvent(EventId.New(), DateTimeOffset.UtcNow);

        var envelope = EventEnvelope.Wrap(ev);

        envelope.Metadata.CorrelationId.Should().Be(CorrelationId.Empty);
        envelope.Metadata.CausationId.Should().Be(CausationId.Empty);
        envelope.Metadata.TenantId.Should().Be(TenantId.Empty);
    }
}



