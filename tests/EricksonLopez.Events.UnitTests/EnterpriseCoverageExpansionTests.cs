// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests;

public sealed record CoverageTestEvent : IEvent
{
    public EventId Id { get; init; } = EventId.New();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class DummyEventSubscriber : IEventSubscriber
{
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent { }
    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent { }
}

public sealed class DummyEnvelopeHandler<TEvent> : IEnvelopeEventHandler<TEvent> where TEvent : IEvent
{
    public IEventEnvelope<TEvent>? ReceivedEnvelope { get; private set; }

    public ValueTask HandleAsync(IEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default)
    {
        ReceivedEnvelope = envelope;
        return ValueTask.CompletedTask;
    }
}

public sealed class DummyExecutionTracker : IEventExecutionTracker
{
    public bool Completed { get; set; }

    public bool IsCompleted(EventId eventId, Type handlerType) => Completed;

    public void MarkCompleted(EventId eventId, Type handlerType) => Completed = true;
}

public class EnterpriseCoverageExpansionTests
{
    [Fact]
    public void IEventSubscriber_DefaultEnvelopeMethods_ThrowNotSupportedException()
    {
        IEventSubscriber subscriber = new DummyEventSubscriber();
        var handler = new DummyEnvelopeHandler<CoverageTestEvent>();

        var actSubscribe = () => subscriber.Subscribe(handler);
        var actUnsubscribe = () => subscriber.Unsubscribe(handler);

        actSubscribe.Should().Throw<NotSupportedException>();
        actUnsubscribe.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public async Task EventPublisherExtensions_PublishAsync_WithEventEnvelope_DispatchesSuccessfully()
    {
        var publisher = new InMemoryEventPublisher();
        var handler = new DummyEnvelopeHandler<CoverageTestEvent>();
        publisher.Subscribe(handler);

        var evt = new CoverageTestEvent();
        var envelope = EventEnvelope.Create(evt);

        await publisher.PublishAsync(envelope);

        handler.ReceivedEnvelope.Should().NotBeNull();
        handler.ReceivedEnvelope!.Payload.Id.Should().Be(evt.Id);

        publisher.Unsubscribe(handler);
    }

    [Fact]
    public async Task EventPublisherExtensions_PublishAsync_WithIEventEnvelope_DispatchesSuccessfully()
    {
        var publisher = new InMemoryEventPublisher();
        var handler = new DummyEnvelopeHandler<CoverageTestEvent>();
        publisher.Subscribe(handler);

        var evt = new CoverageTestEvent();
        IEventEnvelope<CoverageTestEvent> envelope = EventEnvelope.Create(evt);

        await publisher.PublishAsync(envelope);

        handler.ReceivedEnvelope.Should().NotBeNull();
        handler.ReceivedEnvelope!.Payload.Id.Should().Be(evt.Id);
    }

    [Fact]
    public async Task EventPublisherExtensions_ThrowsArgumentNullException_OnNullArguments()
    {
        IEventPublisher nullPublisher = null!;
        var envelope = EventEnvelope.Create(new CoverageTestEvent());

        var act1 = async () => await nullPublisher.PublishAsync(envelope);
        await act1.Should().ThrowAsync<ArgumentNullException>();

        var publisher = new InMemoryEventPublisher();
        EventEnvelope<CoverageTestEvent> nullEnvelope = null!;
        var act2 = async () => await publisher.PublishAsync(nullEnvelope);
        await act2.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void EventContext_AmbientPropertiesAndTracker_WorkAsExpected()
    {
        // 1. Without active context
        EventContext.Current.Should().BeNull();
        EventContext.Metadata.Should().BeNull();
        EventContext.TenantId.Should().BeNull();
        EventContext.CorrelationId.Should().BeNull();
        EventContext.CausationId.Should().BeNull();
        EventContext.ExecutionTracker.Should().BeNull();
        EventContext.IsHandlerCompleted(EventId.New(), typeof(EnterpriseCoverageExpansionTests)).Should().BeFalse();

        // 2. With ExecutionTracker
        var tracker = new DummyExecutionTracker();
        using (EventContext.SetExecutionTracker(tracker))
        {
            EventContext.ExecutionTracker.Should().BeSameAs(tracker);
            EventContext.IsHandlerCompleted(EventId.New(), typeof(EnterpriseCoverageExpansionTests)).Should().BeFalse();
            EventContext.MarkHandlerCompleted(EventId.New(), typeof(EnterpriseCoverageExpansionTests));
            tracker.Completed.Should().BeTrue();
            EventContext.IsHandlerCompleted(EventId.New(), typeof(EnterpriseCoverageExpansionTests)).Should().BeTrue();
        }
        EventContext.ExecutionTracker.Should().BeNull();

        // 3. With Active Envelope Context
        var metadata = new EventMetadataBuilder()
            .WithTenantId(TenantId.From("tenant-enterprise"))
            .WithCorrelationId(CorrelationId.New())
            .WithCausationId(CausationId.From(Guid.NewGuid()))
            .Build();

        var envelope = EventEnvelope.Create(new CoverageTestEvent(), metadata);
        using (EventContext.SetCurrent(envelope))
        {
            EventContext.Current.Should().BeSameAs(envelope);
            EventContext.Metadata.Should().Be(metadata);
            EventContext.TenantId.Should().Be(TenantId.From("tenant-enterprise"));
            EventContext.CorrelationId.Should().Be(metadata.CorrelationId);
            EventContext.CausationId.Should().Be(metadata.CausationId);
        }

        EventContext.Current.Should().BeNull();
    }

    [Fact]
    public void InMemoryEventPublisher_EnvelopeSubscribeAndUnsubscribe_GuardsNull()
    {
        var publisher = new InMemoryEventPublisher();
        IEnvelopeEventHandler<CoverageTestEvent> nullHandler = null!;

        var actSub = () => publisher.Subscribe(nullHandler);
        var actUnsub = () => publisher.Unsubscribe(nullHandler);

        actSub.Should().Throw<ArgumentNullException>();
        actUnsub.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StaticEventTypeRegistry_UnregisteredType_ReturnsFallbackDescriptors()
    {
        var descriptor = StaticEventTypeRegistry.GetDescriptor<CoverageTestEvent>();
        descriptor.Should().NotBeNull();
        descriptor.EventType.Value.Should().Contain(nameof(CoverageTestEvent));

        var eventType = StaticEventTypeRegistry.GetEventType<CoverageTestEvent>();
        eventType.Value.Should().Contain(nameof(CoverageTestEvent));

        var version = StaticEventTypeRegistry.GetVersion<CoverageTestEvent>();
        version.Value.Should().Be(1);
    }

    [Fact]
    public void TenantId_GuidInteroperability_OperatesCorrectly()
    {
        var guid = Guid.NewGuid();
        var tenant = TenantId.From(guid);
        tenant.Value.Should().Be(guid.ToString("D"));

        var explicitTenant = (TenantId)guid;
        explicitTenant.Should().Be(tenant);

        var success = tenant.TryToGuid(out var parsedGuid);
        success.Should().BeTrue();
        parsedGuid.Should().Be(guid);

        var invalidTenant = new TenantId("not-a-guid");
        invalidTenant.TryToGuid(out var emptyGuid).Should().BeFalse();
        emptyGuid.Should().Be(Guid.Empty);
    }

    [Fact]
    public void EventBusDiagnostics_WithAmbientEnvelope_EnrichesActivity()
    {
        using var listener = new System.Diagnostics.ActivityListener
        {
            ShouldListenTo = source => source.Name == EventBusDiagnostics.SourceName,
            Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _) =>
                System.Diagnostics.ActivitySamplingResult.AllDataAndRecorded
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);

        var metadata = new EventMetadataBuilder()
            .WithTenantId(TenantId.From("tenant-test"))
            .WithCorrelationId(CorrelationId.New())
            .WithCausationId(CausationId.From(Guid.NewGuid()))
            .Build();

        var evt = new CoverageTestEvent();
        var envelope = EventEnvelope.Create(evt, metadata);

        using (EventContext.SetCurrent(envelope))
        {
            using var activity = EventBusDiagnostics.StartPublishActivity(evt);
            activity.Should().NotBeNull();
            activity!.GetTagItem("tenant.id").Should().Be("tenant-test");
            activity.GetTagItem("correlation.id").Should().Be(metadata.CorrelationId.Value);
            activity.GetTagItem("causation.id").Should().Be(metadata.CausationId.Value);
        }
    }

    private sealed class CoverageEnvelopeHandler : IEnvelopeEventHandler<CoverageTestEvent>
    {
        public ValueTask HandleAsync(IEventEnvelope<CoverageTestEvent> @event, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
    }

    [Fact]
    public void HandlerResolutionHelper_EnvelopeHandlerResolution_ResolvesDirectAndFallback()
    {
        var handler = new CoverageEnvelopeHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IEnvelopeEventHandler<CoverageTestEvent>>(handler);
        var provider = services.BuildServiceProvider();

        var descriptor = new EricksonLopez.Events.Bus.Registry.HandlerDescriptor(
            typeof(CoverageEnvelopeHandler),
            typeof(IEnvelopeEventHandler<CoverageTestEvent>),
            (sp, env, ct) => ValueTask.CompletedTask);

        var fallbackCache = new Dictionary<Type, object?>();
        var resolved = EricksonLopez.Events.Bus.Execution.HandlerResolutionHelper.ResolveHandler<CoverageTestEvent>(
            provider,
            descriptor,
            fallbackCache);

        resolved.Should().BeSameAs(handler);
        fallbackCache.Should().ContainKey(typeof(CoverageEnvelopeHandler));

        // Test second resolution using cache
        var fromCache = EricksonLopez.Events.Bus.Execution.HandlerResolutionHelper.ResolveHandler<CoverageTestEvent>(
            provider,
            descriptor,
            fallbackCache);

        fromCache.Should().BeSameAs(handler);
    }
}
