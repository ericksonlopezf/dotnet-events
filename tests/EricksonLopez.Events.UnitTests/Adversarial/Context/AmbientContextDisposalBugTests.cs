// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Context;

[Trait("Category", "Adversarial")]
public sealed class AmbientContextDisposalBugTests
{
    private sealed record ContextTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    // Custom publisher that relies on IEventPublisher.PublishEnvelopeAsync DEFAULT INTERFACE METHOD (DIM)
    private sealed class DimEventPublisher : IEventPublisher
    {
        public bool HandlerObservedNullContext { get; private set; }

        public async ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
            where TEvent : IEvent
        {
            // Simulate an asynchronous dispatch across an await boundary
            await Task.Yield();

            // Check if ambient context is still alive
            if (EventContext.Current == null)
            {
                HandlerObservedNullContext = true;
            }
        }
    }

    [Fact]
    public async Task EVT_CTX_001_DefaultInterfaceMethod_PublishEnvelopeAsync_PreservesScopeUntilTaskCompletes()
    {
        var tcs = new TaskCompletionSource<bool>();
        var publisher = new DeferredEventPublisher(tcs);
        var evt = new ContextTestEvent(EventId.New(), DateTimeOffset.UtcNow);
        var envelope = EventEnvelope.Create(evt, EventMetadata.Create(tenantId: TenantId.From("tenant-abc")));

        // Invoke the Default Interface Method via interface
        var vt = ((IEventPublisher)publisher).PublishEnvelopeAsync(envelope);

        // Inside PublishAsync before await:
        publisher.ObservedContextBeforeAwait.Should().NotBeNull();
        publisher.ObservedContextBeforeAwait!.Id.Should().Be(evt.Id);

        // Complete the pending task
        tcs.SetResult(true);
        await vt;

        // Inside PublishAsync after await:
        // Proves that the scope in PublishEnvelopeAsync was not prematurely disposed!
        publisher.ObservedContextAfterAwait.Should().NotBeNull();
        publisher.ObservedContextAfterAwait!.Id.Should().Be(evt.Id);
    }

    private sealed class DeferredEventPublisher(TaskCompletionSource<bool> tcs) : IEventPublisher
    {
        public IEventEnvelope? ObservedContextBeforeAwait { get; private set; }
        public IEventEnvelope? ObservedContextAfterAwait { get; private set; }

        public async ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
            where TEvent : IEvent
        {
            ObservedContextBeforeAwait = EventContext.Current;
            await tcs.Task;
            ObservedContextAfterAwait = EventContext.Current;
        }
    }

    [Fact]
    public async Task EVT_CTX_002_InMemoryEventPublisher_PublishAsync_SetsAmbientContext()
    {
        var inMemPublisher = new InMemoryEventPublisher();
        var evt = new ContextTestEvent(EventId.New(), DateTimeOffset.UtcNow);

        IEventEnvelope? observedContext = null;

        var handler = new ActionHandler<ContextTestEvent>(e =>
        {
            observedContext = EventContext.Current;
        });

        inMemPublisher.Subscribe(handler);

        await inMemPublisher.PublishAsync(evt);

        // In InMemoryEventPublisher, PublishAsync now establishes an ephemeral envelope context!
        observedContext.Should().NotBeNull();
        observedContext!.Id.Should().Be(evt.Id);
    }

    private sealed class ActionHandler<T>(Action<T> action) : IEventHandler<T> where T : IEvent
    {
        public ValueTask HandleAsync(T eventInstance, CancellationToken cancellationToken = default)
        {
            action(eventInstance);
            return ValueTask.CompletedTask;
        }
    }
}
