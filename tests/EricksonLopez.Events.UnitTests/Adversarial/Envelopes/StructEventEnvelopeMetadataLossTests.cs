// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
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
public sealed class StructEventEnvelopeMetadataLossTests
{
    public readonly record struct StructOrderEvent(EventId Id, DateTimeOffset OccurredAt, string OrderNumber) : IEvent;

    public sealed class StructOrderHandler : IEventHandler<StructOrderEvent>
    {
        public static string? ObservedTenantId { get; set; }
        public static string? ObservedCorrelationId { get; set; }
        public static string? ObservedHeader { get; set; }

        public ValueTask HandleAsync(StructOrderEvent eventInstance, CancellationToken cancellationToken = default)
        {
            ObservedTenantId = EventContext.TenantId?.Value;
            ObservedCorrelationId = EventContext.CorrelationId?.Value;
            ObservedHeader = EventContext.Current?.Metadata.TryGetHeader("X-Custom", out var h) == true ? h : null;
            return ValueTask.CompletedTask;
        }

        public static void Reset()
        {
            ObservedTenantId = null;
            ObservedCorrelationId = null;
            ObservedHeader = null;
        }
    }

    [Fact]
    public async Task PublishEnvelopeAsync_WithStructEvent_LosesMetadataDueToBoxingReferenceEqualsFailure()
    {
        StructOrderHandler.Reset();

        var registry = new global::EricksonLopez.Events.Bus.Registry.HandlerRegistry();
        registry.Register(
            typeof(StructOrderEvent),
            new global::EricksonLopez.Events.Bus.Registry.HandlerDescriptor(
                typeof(StructOrderHandler),
                typeof(IEventHandler<StructOrderEvent>),
                static (inst, evt, ct) => ((StructOrderHandler)inst).HandleAsync((StructOrderEvent)evt, ct)));

        var services = new ServiceCollection();
        services.AddSingleton<StructOrderHandler>();
        var sp = services.BuildServiceProvider();

        var bus = new EventBus(registry, sp, new EventBusOptions());

        var evt = new StructOrderEvent(EventId.New(), DateTimeOffset.UtcNow, "ORD-12345");
        var metadata = new EventMetadataBuilder()
            .WithTenantId(TenantId.From("tenant-enterprise"))
            .WithCorrelationId(CorrelationId.From("corr-99999"))
            .WithHeader("X-Custom", "AuditValue")
            .Build();

        var envelope = EventEnvelope.Create(evt, metadata);

        // Publish custom envelope with tenant and correlation metadata
        await bus.PublishEnvelopeAsync(envelope);

        // In EventBus.cs line 89:
        // if (Context.EventContext.Current == null || !ReferenceEquals(Context.EventContext.Current.GetPayload(), eventInstance))
        // Because StructOrderEvent is a struct, ReferenceEquals(Current.GetPayload(), eventInstance) checks reference equality on two boxed copies, returning false!
        // It creates an ephemeral envelope and overwrites the active context with empty metadata!
        // This test proves whether the metadata survives or is stripped:
        StructOrderHandler.ObservedTenantId.Should().Be("tenant-enterprise", "Ambient tenant metadata must not be lost for struct events.");
        StructOrderHandler.ObservedCorrelationId.Should().Be("corr-99999", "Ambient correlation metadata must not be lost for struct events.");
        StructOrderHandler.ObservedHeader.Should().Be("AuditValue", "Custom transport headers must not be lost for struct events.");
    }
}
