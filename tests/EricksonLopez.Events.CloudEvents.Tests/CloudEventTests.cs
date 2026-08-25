// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.CloudEvents;
using EricksonLopez.Events.CloudEvents.Serialization;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.Events.CloudEvents.Tests;

[Trait("Category", "Unit")]
public sealed class CloudEventTests
{
    [Fact]
    public void Constructor_ValidArguments_ShouldInitializeProperties()
    {
        var source = new Uri("https://ordering.mycorp.internal");
        var data = new OrderCreatedIntegrationEvent(EventId.New(), Guid.NewGuid(), 149.99m, DateTimeOffset.UtcNow);

        var cloudEvent = new CloudEvent<OrderCreatedIntegrationEvent>(
            id: "evt-12345",
            source: source,
            type: "com.mycorp.order-created",
            data: data,
            correlationId: "corr-100",
            causationId: "caus-200",
            tenantId: "tenant-300");

        cloudEvent.SpecVersion.Should().Be("1.0");
        cloudEvent.Id.Should().Be("evt-12345");
        cloudEvent.Source.Should().Be(source);
        cloudEvent.Type.Should().Be("com.mycorp.order-created");
        cloudEvent.Data.Should().Be(data);
        cloudEvent.CorrelationId.Should().Be("corr-100");
        cloudEvent.CausationId.Should().Be("caus-200");
        cloudEvent.TenantId.Should().Be("tenant-300");
        cloudEvent.DataContentType.Should().Be("application/json");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidId_ShouldThrow(string? invalidId)
    {
        var source = new Uri("https://ordering.mycorp.internal");
        var data = new OrderCreatedIntegrationEvent(EventId.New(), Guid.NewGuid(), 149.99m, DateTimeOffset.UtcNow);

        Action act = () => new CloudEvent<OrderCreatedIntegrationEvent>(
            id: invalidId!,
            source: source,
            type: "com.mycorp.order-created",
            data: data);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidType_ShouldThrow(string? invalidType)
    {
        var source = new Uri("https://ordering.mycorp.internal");
        var data = new OrderCreatedIntegrationEvent(EventId.New(), Guid.NewGuid(), 149.99m, DateTimeOffset.UtcNow);

        Action act = () => new CloudEvent<OrderCreatedIntegrationEvent>(
            id: "evt-123",
            source: source,
            type: invalidType!,
            data: data);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToCloudEvent_FromEventEnvelope_ShouldMapCorrectly()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var orderEvent = new OrderCreatedIntegrationEvent(eventId, Guid.NewGuid(), 250.00m, now);

        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.From("corr-999"))
            .WithCausationId(CausationId.From("caus-888"))
            .WithTenantId(TenantId.From("tenant-777"))
            .WithSource("https://ordering.service.internal")
            .WithHeader("X-Custom-Env", "Production")
            .Build();

        var envelope = EventEnvelope.Create(orderEvent, metadata);

        var cloudEvent = envelope.ToCloudEvent(schemaBaseUri: new Uri("https://schemas.mycorp.internal/"));

        cloudEvent.SpecVersion.Should().Be("1.0");
        cloudEvent.Id.Should().Be(eventId.ToString());
        cloudEvent.Source.Should().Be(new Uri("https://ordering.service.internal"));
        cloudEvent.Type.Should().Be("ordering.order-created");
        cloudEvent.CorrelationId.Should().Be("corr-999");
        cloudEvent.CausationId.Should().Be("caus-888");
        cloudEvent.TenantId.Should().Be("tenant-777");
        cloudEvent.DataSchema.Should().Be(new Uri("https://schemas.mycorp.internal/schemas/ordering.order-created/v1"));
        cloudEvent.ExtensionAttributes.Should().NotBeNull();
        cloudEvent.ExtensionAttributes!.Should().ContainKey("xcustomenv");
        cloudEvent.ExtensionAttributes["xcustomenv"].Should().Be("Production");
    }

    [Fact]
    public void ToCloudEvent_WithFallbackDefaultSource_ShouldUseFallback()
    {
        var eventId = EventId.New();
        var simpleEvent = new SimpleNoSourceIntegrationEvent(eventId, DateTimeOffset.UtcNow);
        var envelope = EventEnvelope.Create(simpleEvent);

        var cloudEvent = envelope.ToCloudEvent();

        cloudEvent.Source.Should().Be(new Uri("urn:events:test.no-source"));
    }

    [Fact]
    public void RoundTrip_EventEnvelopeToCloudEventAndBack_ShouldPreserveData()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var orderEvent = new OrderCreatedIntegrationEvent(eventId, Guid.NewGuid(), 499.99m, now);

        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.From("corr-abc"))
            .WithCausationId(CausationId.From("caus-xyz"))
            .WithTenantId(TenantId.From("tenant-omega"))
            .WithSource("https://ordering.corp")
            .WithHeader("Region", "EU")
            .Build();

        var originalEnvelope = EventEnvelope.Create(orderEvent, metadata);
        var cloudEvent = originalEnvelope.ToCloudEvent();
        var reconstructedEnvelope = cloudEvent.ToEventEnvelope();

        reconstructedEnvelope.Id.Should().Be(originalEnvelope.Id);
        reconstructedEnvelope.Type.Should().Be(originalEnvelope.Type);
        reconstructedEnvelope.Payload.Should().Be(originalEnvelope.Payload);
        reconstructedEnvelope.Metadata.CorrelationId.Value.Should().Be("corr-abc");
        reconstructedEnvelope.Metadata.CausationId.Value.Should().Be("caus-xyz");
        reconstructedEnvelope.Metadata.TenantId.Value.Should().Be("tenant-omega");
        reconstructedEnvelope.Metadata.Source.Should().Be("https://ordering.corp/");
        reconstructedEnvelope.Metadata.CustomHeaders.Should().ContainKey("region");
    }

    [Fact]
    public void JsonSerialization_CloudEvent_ShouldSerializeAndDeserialize()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var orderEvent = new OrderCreatedIntegrationEvent(eventId, Guid.NewGuid(), 99.00m, now);

        var envelope = EventEnvelope.Wrap(
            orderEvent,
            CorrelationId.From("c-1"),
            CausationId.From("c-2"),
            TenantId.From("t-1"),
            "https://test.source");

        var cloudEvent = envelope.ToCloudEvent();

        var options = new JsonSerializerOptions().ConfigureForCloudEvents();
        options.AddEventsConverters();

        string json = JsonSerializer.Serialize(cloudEvent, options);

        json.Should().Contain("\"specversion\":\"1.0\"");
        json.Should().Contain("\"type\":\"ordering.order-created\"");
        json.Should().Contain("\"correlationid\":\"c-1\"");
        json.Should().Contain("\"causationid\":\"c-2\"");
        json.Should().Contain("\"tenantid\":\"t-1\"");

        var deserialized = JsonSerializer.Deserialize<CloudEvent<OrderCreatedIntegrationEvent>>(json, options);

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(cloudEvent.Id);
        deserialized.Type.Should().Be(cloudEvent.Type);
        deserialized.CorrelationId.Should().Be("c-1");
        deserialized.CausationId.Should().Be("c-2");
        deserialized.TenantId.Should().Be("t-1");
    }

    [Property]
    public Property CloudEvent_Roundtrip_ArbitraryGuidsAndPayloads_ShouldPreserveIdentity()
    {
        return Prop.ForAll(Arb.Default.Guid(), Arb.Default.Guid(), (eventGuid, orderGuid) =>
        {
            var eventId = EventId.From(eventGuid);
            var now = DateTimeOffset.UtcNow;
            var payload = new OrderCreatedIntegrationEvent(eventId, orderGuid, 123.45m, now);

            var metadata = new EventMetadataBuilder()
                .WithCorrelationId($"corr_{eventGuid:N}")
                .WithCausationId($"caus_{orderGuid:N}")
                .WithTenantId("tenant-pbt")
                .WithSource("https://ordering.pbt.internal")
                .Build();

            var originalEnvelope = EventEnvelope.Create(payload, metadata);
            var cloudEvent = originalEnvelope.ToCloudEvent();
            var reconstructed = cloudEvent.ToEventEnvelope();

            return reconstructed.Id == originalEnvelope.Id &&
                   reconstructed.Type == originalEnvelope.Type &&
                   reconstructed.Payload.OrderId == orderGuid &&
                   reconstructed.Metadata.CorrelationId.Value == $"corr_{eventGuid:N}" &&
                   reconstructed.Metadata.CausationId.Value == $"caus_{orderGuid:N}" &&
                   reconstructed.Metadata.TenantId.Value == "tenant-pbt";
        });
    }

    [Fact]
    public void ToCloudEvent_WhenEnvelopeNull_ThrowsArgumentNullException()
    {
        EventEnvelope<OrderCreatedIntegrationEvent> envelope = null!;
        Action act = () => envelope.ToCloudEvent();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToEventEnvelope_WhenCloudEventNull_ThrowsArgumentNullException()
    {
        CloudEvent<OrderCreatedIntegrationEvent> cloudEvent = null!;
        Action act = () => cloudEvent.ToEventEnvelope();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToCloudEvent_WhenSourceIsEmptyOrMissing_FallsBackToDefaultSourceOrUrn()
    {
        var eventId = EventId.New();
        var simpleEvent = new SimpleNoSourceIntegrationEvent(eventId, DateTimeOffset.UtcNow);
        var envelope = EventEnvelope.Create(simpleEvent);
        var customDefaultSource = new Uri("https://fallback.corp/events");

        var cloudEvent1 = envelope.ToCloudEvent(defaultSource: customDefaultSource);
        cloudEvent1.Source.Should().Be(customDefaultSource);

        var cloudEvent2 = envelope.ToCloudEvent();
        cloudEvent2.Source.Should().Be(new Uri("urn:events:test.no-source"));
    }

    [Fact]
    public void ToEventEnvelope_WhenIdIsNotGuid_FallsBackToDataEventId()
    {
        var dataEventId = EventId.New();
        var payload = new OrderCreatedIntegrationEvent(dataEventId, Guid.NewGuid(), 20m, DateTimeOffset.UtcNow);
        var cloudEvent = new CloudEvent<OrderCreatedIntegrationEvent>(
            id: "not-a-valid-guid-string",
            source: new Uri("https://orders.service.internal"),
            type: "ordering.order-created",
            data: payload,
            time: null,
            dataContentType: null,
            dataSchema: null,
            correlationId: null,
            causationId: null,
            tenantId: null,
            extensionAttributes: new System.Collections.Generic.Dictionary<string, object?>
            {
                { "validheader", "value1" },
                { "nullheader", null }
            });

        var envelope = cloudEvent.ToEventEnvelope();
        envelope.Id.Should().Be(dataEventId);
        envelope.OccurredAt.Should().Be(payload.OccurredAt);
        envelope.Metadata.ContentType.Should().Be("application/json");
        envelope.Metadata.CorrelationId.IsEmpty.Should().BeTrue();
        envelope.Metadata.CausationId.IsEmpty.Should().BeTrue();
        envelope.Metadata.TenantId.IsEmpty.Should().BeTrue();
        envelope.Metadata.CustomHeaders.Should().ContainKey("validheader");
        envelope.Metadata.CustomHeaders.Should().NotContainKey("nullheader");
    }

    [Fact]
    public void ToEventEnvelope_WhenExtensionAttributesNull_ShouldInitializeEmptyHeaders()
    {
        var dataEventId = EventId.New();
        var payload = new OrderCreatedIntegrationEvent(dataEventId, Guid.NewGuid(), 20m, DateTimeOffset.UtcNow);
        var cloudEvent = new CloudEvent<OrderCreatedIntegrationEvent>(
            id: dataEventId.ToString(),
            source: new Uri("https://orders.service.internal"),
            type: "ordering.order-created",
            data: payload,
            time: null,
            dataContentType: "application/json",
            dataSchema: null,
            correlationId: null,
            causationId: null,
            tenantId: null,
            extensionAttributes: null);

        var envelope = cloudEvent.ToEventEnvelope();
        envelope.Metadata.CustomHeaders.Should().BeEmpty();
    }
}

[EventName("ordering.order-created")]
[EventVersion(1)]
[EventSource("https://ordering.service.internal")]
public sealed record OrderCreatedIntegrationEvent(
    EventId Id,
    Guid OrderId,
    decimal Total,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

[EventName("test.no-source")]
[EventVersion(1)]
public sealed record SimpleNoSourceIntegrationEvent(
    EventId Id,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

