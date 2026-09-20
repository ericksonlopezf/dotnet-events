// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Envelopes;

[Trait("Category", "Adversarial")]
public sealed class EnvelopeMetadataDispatchLossTests
{
    public sealed record TenantPaymentProcessed(EventId Id, DateTimeOffset OccurredAt, decimal Amount) : IEvent;

    public sealed class PaymentHandler : IEventHandler<TenantPaymentProcessed>
    {
        public static TenantPaymentProcessed? ReceivedEvent { get; set; }
        public static TenantId? ObservedTenantId { get; set; }
        public static CorrelationId? ObservedCorrelationId { get; set; }
        public static CausationId? ObservedCausationId { get; set; }
        public static string? ObservedCustomHeader { get; set; }

        public ValueTask HandleAsync(TenantPaymentProcessed eventInstance, CancellationToken cancellationToken = default)
        {
            ReceivedEvent = eventInstance;
            ObservedTenantId = EventContext.TenantId;
            ObservedCorrelationId = EventContext.CorrelationId;
            ObservedCausationId = EventContext.CausationId;
            if (EventContext.Metadata is { } meta && meta.TryGetHeader("X-Custom-Header", out var headerVal))
            {
                ObservedCustomHeader = headerVal;
            }

            return ValueTask.CompletedTask;
        }
    }

    public sealed class EnvelopePaymentHandler : IEnvelopeEventHandler<TenantPaymentProcessed>
    {
        public static IEventEnvelope<TenantPaymentProcessed>? ReceivedEnvelope { get; set; }

        public ValueTask HandleAsync(IEventEnvelope<TenantPaymentProcessed> envelope, CancellationToken cancellationToken = default)
        {
            ReceivedEnvelope = envelope;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task PublishAsync_WithEventEnvelope_PreservesMetadataAndAmbientContextDuringDispatch()
    {
        // Verification of EVT-ARC-001 Remediation:
        // When publishing an EventEnvelope<TEvent>, metadata is preserved via EventContext
        // and accessible directly inside IEventHandler<TEvent>.
        PaymentHandler.ReceivedEvent = null;
        PaymentHandler.ObservedTenantId = null;
        PaymentHandler.ObservedCorrelationId = null;
        PaymentHandler.ObservedCausationId = null;
        PaymentHandler.ObservedCustomHeader = null;

        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<TenantPaymentProcessed, PaymentHandler>();

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var originalEvent = new TenantPaymentProcessed(EventId.New(), DateTimeOffset.UtcNow, 1500m);
        var expectedCorrelation = CorrelationId.New();
        var metadata = new EventMetadataBuilder()
            .WithTenantId(TenantId.From("tenant-omega"))
            .WithCorrelationId(expectedCorrelation)
            .WithCausationId(CausationId.From("COMMAND-999"))
            .WithHeader("X-Custom-Header", "SecretToken")
            .Build();

        var envelope = EventEnvelope.Create(originalEvent, metadata);

        // Publish through the extension method:
        await publisher.PublishAsync(envelope);

        // Verify the handler received the payload:
        PaymentHandler.ReceivedEvent.Should().NotBeNull();
        PaymentHandler.ReceivedEvent!.Amount.Should().Be(1500m);

        // Verify all ambient metadata was preserved and observed inside the handler:
        PaymentHandler.ObservedTenantId.Should().Be(TenantId.From("tenant-omega"));
        PaymentHandler.ObservedCorrelationId.Should().Be(expectedCorrelation);
        PaymentHandler.ObservedCausationId.Should().Be(CausationId.From("COMMAND-999"));
        PaymentHandler.ObservedCustomHeader.Should().Be("SecretToken");

        // Verify ambient context does not leak outside the publish scope:
        EventContext.Current.Should().BeNull();
    }

    [Fact]
    public async Task PublishAsync_WithEnvelopeHandler_DirectlyReceivesTypedEnvelope()
    {
        EnvelopePaymentHandler.ReceivedEnvelope = null;

        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEnvelopeEventHandler<TenantPaymentProcessed, EnvelopePaymentHandler>();

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var originalEvent = new TenantPaymentProcessed(EventId.New(), DateTimeOffset.UtcNow, 2500m);
        var metadata = new EventMetadataBuilder()
            .WithTenantId(TenantId.From("tenant-delta"))
            .WithCausationId(CausationId.From("TRIGGER-42"))
            .Build();

        var envelope = EventEnvelope.Create(originalEvent, metadata);

        await publisher.PublishAsync(envelope);

        EnvelopePaymentHandler.ReceivedEnvelope.Should().NotBeNull();
        EnvelopePaymentHandler.ReceivedEnvelope!.Payload.Amount.Should().Be(2500m);
        EnvelopePaymentHandler.ReceivedEnvelope!.Metadata.TenantId.Should().Be(TenantId.From("tenant-delta"));
        EnvelopePaymentHandler.ReceivedEnvelope!.Metadata.CausationId.Should().Be(CausationId.From("TRIGGER-42"));

        EventContext.Current.Should().BeNull();
    }
}
