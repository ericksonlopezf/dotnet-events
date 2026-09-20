// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.RegistryAndDispatch;

using System.Diagnostics;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Diagnostics;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;
using EricksonLopez.Events.UnitTests.Common;
using Xunit;

[Collection("DiagnosticsAndStaticRegistry")]
[Xunit.Trait("Category", "Unit")]
public sealed class RegistryAndDispatchTests
{
    private sealed record UserRegistered(EventId Id, string Email, DateTimeOffset OccurredAt) : IDomainEvent;
    private sealed record InvoiceIssued(EventId Id, decimal Amount, DateTimeOffset OccurredAt) : IIntegrationEvent;

    [Fact]
    public void EventTypeRegistry_Empty_ShouldBeEmpty()
    {
        var empty = EventTypeRegistry.Empty;
        empty.GetAllDescriptors().Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, "descriptors")]
    [InlineData(1, "clrType")]
    [InlineData(2, "descriptor")]
    public void EventTypeRegistry_NullValidations_ShouldThrow(int nullArgIndex, string expectedParamName)
    {
        Action act = nullArgIndex switch
        {
            0 => () => new EventTypeRegistry(null!),
            1 => () => EventTypeRegistry.Empty.TryGetDescriptor((Type)null!, out _),
            _ => () => EventTypeRegistry.CreateBuilder().Register(null!)
        };

        act.Should().Throw<ArgumentNullException>().WithParameterName(expectedParamName);
    }

    [Fact]
    public void EventTypeDescriptor_For_ShouldCreateWithDefaultOrExplicitValues()
    {
        var defaultDesc = EventTypeDescriptor.For<UserRegistered>(EventType.From("users.registered"));
        defaultDesc.ClrType.Should().Be(typeof(UserRegistered));
        defaultDesc.EventType.Should().Be(EventType.From("users.registered"));
        defaultDesc.Version.Should().Be(EventVersion.V1);
        defaultDesc.Source.Should().BeNull();

        var explicitDesc = EventTypeDescriptor.For<InvoiceIssued>(
            EventType.From("billing.invoices"),
            EventVersion.From(2),
            "billing-service");
        explicitDesc.ClrType.Should().Be(typeof(InvoiceIssued));
        explicitDesc.EventType.Should().Be(EventType.From("billing.invoices"));
        explicitDesc.Version.Should().Be(EventVersion.From(2));
        explicitDesc.Source.Should().Be("billing-service");
    }

    [Fact]
    public void EventTypeRegistryBuilder_ShouldRegisterAndLookupDescriptors()
    {
        var customDescriptor = new EventTypeDescriptor(
            typeof(UserRegistered),
            EventType.From("users.custom-registered"),
            EventVersion.From(3),
            "custom-source");

        var registry = EventTypeRegistry.CreateBuilder()
            .Register(customDescriptor)
            .Register<UserRegistered>("users.user-registered", 1, "identity.service")
            .Register<InvoiceIssued>("billing.invoice-issued", 2, "billing.service")
            .Build();

        registry.GetAllDescriptors().Should().HaveCount(3);

        registry.TryGetDescriptor(EventType.From("users.user-registered"), out var desc1).Should().BeTrue();
        desc1!.ClrType.Should().Be(typeof(UserRegistered));
        desc1.Version.Should().Be(EventVersion.V1);
        desc1.Source.Should().Be("identity.service");

        registry.TryGetDescriptor<InvoiceIssued>(out var desc2).Should().BeTrue();
        desc2!.EventType.Should().Be(EventType.From("billing.invoice-issued"));
        desc2.Version.Should().Be(EventVersion.From(2));

        registry.TryGetDescriptor(typeof(InvoiceIssued), out var desc3).Should().BeTrue();
        desc3.Should().Be(desc2);

        registry.TryGetDescriptor(EventType.From("non.existent"), out var missing).Should().BeFalse();
        missing.Should().BeNull();

        registry.TryGetDescriptor(typeof(string), out var missingClr).Should().BeFalse();
        missingClr.Should().BeNull();
    }

    [Fact]
    public void StaticEventTypeRegistry_CurrentAndCaching_ShouldWork()
    {
        StaticEventTypeRegistry.Reset();
        var original = StaticEventTypeRegistry.Current;

        var customRegistry = EventTypeRegistry.CreateBuilder()
            .Register<UserRegistered>("users.custom", 3, "custom.src")
            .Build();

        StaticEventTypeRegistry.SetCurrent(customRegistry, allowOverride: true);
        StaticEventTypeRegistry.Current.Should().BeSameAs(customRegistry);

        StaticEventTypeRegistry.Reset();
        StaticEventTypeRegistry.Current.Should().BeSameAs(EventTypeRegistry.Empty);

        StaticEventTypeRegistry.SetCurrent(original, allowOverride: true);

        var descriptor = StaticEventTypeRegistry.GetDescriptor<UserRegistered>();
        descriptor.ClrType.Should().Be(typeof(UserRegistered));
        descriptor.EventType.Should().Be(EventType.From(nameof(UserRegistered)));
        descriptor.Version.Should().Be(EventVersion.V1);

        StaticEventTypeRegistry.GetEventType<UserRegistered>().Should().Be(EventType.From(nameof(UserRegistered)));
        StaticEventTypeRegistry.GetVersion<UserRegistered>().Should().Be(EventVersion.V1);
    }

    [Fact]
    public void EventsDiagnostics_WithActiveListener_ShouldEmitActivityAndTags()
    {
        using var activityScope = new ActivityTestScope(EventsDiagnostics.SourceName);

        var ev = new UserRegistered(EventId.New(), "test@user.com", DateTimeOffset.UtcNow);
        var meta = new EventMetadataBuilder()
            .WithCorrelationId("corr-123")
            .WithCausationId("caus-456")
            .WithTenantId("tenant-789")
            .Build();

        using var activity = EventsDiagnostics.StartPublishActivity(ev, meta);
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be($"Event.Publish {nameof(UserRegistered)}");
        activity.Tags.Should().Contain(t => t.Key == "messaging.system" && t.Value == "ericksonlopez.events");
        activity.Tags.Should().Contain(t => t.Key == "messaging.event.id" && t.Value == ev.Id.ToString());
        activity.Tags.Should().Contain(t => t.Key == "messaging.event.type" && t.Value == typeof(UserRegistered).FullName);
        activity.Tags.Should().Contain(t => t.Key == "messaging.correlation_id" && t.Value == "corr-123");
        activity.Tags.Should().Contain(t => t.Key == "messaging.causation_id" && t.Value == "caus-456");
        activity.Tags.Should().Contain(t => t.Key == "messaging.tenant_id" && t.Value == "tenant-789");

        activityScope.StartedActivities.Should().ContainSingle(a => a.DisplayName == $"Event.Publish {nameof(UserRegistered)}");
    }

    [Fact]
    public void EventsDiagnostics_WithoutListener_ShouldReturnNullActivity()
    {
        var ev = new UserRegistered(EventId.New(), "test@user.com", DateTimeOffset.UtcNow);
        using var activity = EventsDiagnostics.StartPublishActivity(ev);
    }

    [Fact]
    public void EventsDiagnostics_Metrics_ShouldRecordValuesAndTags()
    {
        using var meterScope = new MeterTestScope(EventsDiagnostics.SourceName);

        const string testEventType = "MetricsDiagnosticTestEvent";
        EventsDiagnostics.RecordEventPublished(testEventType);
        EventsDiagnostics.RecordEventHandled(testEventType, 25.5, true);
        EventsDiagnostics.RecordEventHandled(testEventType, 10.0, false);

        var pubInst = meterScope.PublishedInstruments.Single(i => i.Name == "events.published.count");
        pubInst.Description.Should().Be("Number of events published");

        var handledInst = meterScope.PublishedInstruments.Single(i => i.Name == "events.handled.count");
        handledInst.Description.Should().Be("Number of events processed by handlers");

        var durInst = meterScope.PublishedInstruments.Single(i => i.Name == "events.handling.duration");
        durInst.Unit.Should().Be("ms");
        durInst.Description.Should().Be("Duration of event handler execution");

        meterScope.LongMeasurements.Should().Contain(m =>
            m.InstrumentName == "events.published.count" &&
            m.Value == 1 &&
            (string?)m.Tags["event.type"] == testEventType);

        meterScope.LongMeasurements.Should().Contain(m =>
            m.InstrumentName == "events.handled.count" &&
            m.Value == 1 &&
            (string?)m.Tags["event.type"] == testEventType &&
            (bool?)m.Tags["event.success"] == true);

        meterScope.LongMeasurements.Should().Contain(m =>
            m.InstrumentName == "events.handled.count" &&
            m.Value == 1 &&
            (string?)m.Tags["event.type"] == testEventType &&
            (bool?)m.Tags["event.success"] == false);

        meterScope.DoubleMeasurements.Should().Contain(m =>
            m.InstrumentName == "events.handling.duration" &&
            m.Value == 25.5 &&
            (string?)m.Tags["event.type"] == testEventType);
    }

    [Theory]
    [InlineData(0, "handler")]
    [InlineData(1, "handler")]
    [InlineData(2, "handler")]
    [InlineData(3, "handler")]
    [InlineData(4, "eventInstance")]
    public async Task InMemoryEventPublisher_NullValidations_ShouldThrow(int nullArgIndex, string expectedParamName)
    {
        var publisher = new InMemoryEventPublisher();

        Func<Task> act = nullArgIndex switch
        {
            0 => () => { publisher.Subscribe<UserRegistered>((IEventHandler<UserRegistered>)null!); return Task.CompletedTask; }
            ,
            1 => () => { publisher.Unsubscribe<UserRegistered>((IEventHandler<UserRegistered>)null!); return Task.CompletedTask; }
            ,
            2 => () => { publisher.Subscribe<UserRegistered>((IEnvelopeEventHandler<UserRegistered>)null!); return Task.CompletedTask; }
            ,
            3 => () => { publisher.Unsubscribe<UserRegistered>((IEnvelopeEventHandler<UserRegistered>)null!); return Task.CompletedTask; }
            ,
            _ => async () => await publisher.PublishAsync<UserRegistered>(null!)
        };

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.WithParameterName(expectedParamName);
    }

    [Fact]
    public async Task InMemoryEventPublisher_Publish_ToMultipleSubscribers_InvokesAllHandlersAndRecordsMetrics()
    {
        using var meterScope = new MeterTestScope(EventsDiagnostics.SourceName);

        var publisher = new InMemoryEventPublisher();
        var handler1 = new TestHandler<UserRegistered>();
        var handler2 = new TestHandler<UserRegistered>();
        var handler3 = new TestHandler<UserRegistered>();

        publisher.Subscribe(handler1);
        publisher.Subscribe(handler2);
        publisher.Subscribe(handler3);

        var ev = new UserRegistered(EventId.New(), "user@example.com", DateTimeOffset.UtcNow);

        await publisher.PublishAsync(ev);

        handler1.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
        handler2.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
        handler3.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);

        meterScope.LongMeasurements.Should().Contain(m =>
            m.InstrumentName == "events.published.count" &&
            (string?)m.Tags["event.type"] == nameof(UserRegistered) &&
            m.Value == 1);

        meterScope.LongMeasurements.Where(m =>
            m.InstrumentName == "events.handled.count" &&
            (string?)m.Tags["event.type"] == nameof(UserRegistered) &&
            (bool?)m.Tags["event.success"] == true).Should().HaveCount(3);

        meterScope.DoubleMeasurements.Where(m =>
            m.InstrumentName == "events.handling.duration" &&
            (string?)m.Tags["event.type"] == nameof(UserRegistered)).Should().HaveCount(3);
    }

    [Fact]
    public async Task InMemoryEventPublisher_Unsubscribe_MiddleHandler_RemovesMiddleAndRetainsOthers()
    {
        var publisher = new InMemoryEventPublisher();
        var handler1 = new TestHandler<UserRegistered>();
        var handler2 = new TestHandler<UserRegistered>();
        var handler3 = new TestHandler<UserRegistered>();

        publisher.Subscribe(handler1);
        publisher.Subscribe(handler2);
        publisher.Subscribe(handler3);

        // Remove middle handler (index 1) to test CopyOnWriteList array copy on both sides
        publisher.Unsubscribe(handler2);

        var ev = new UserRegistered(EventId.New(), "user2@example.com", DateTimeOffset.UtcNow);
        await publisher.PublishAsync(ev);

        handler1.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
        handler2.HandledEvents.Should().BeEmpty();
        handler3.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
    }

    [Fact]
    public async Task InMemoryEventPublisher_Unsubscribe_FirstHandler_RemovesFirstAndRetainsOthers()
    {
        var publisher = new InMemoryEventPublisher();
        var handler1 = new TestHandler<UserRegistered>();
        var handler2 = new TestHandler<UserRegistered>();

        publisher.Subscribe(handler1);
        publisher.Subscribe(handler2);

        // Remove first handler (index 0)
        publisher.Unsubscribe(handler1);

        var ev = new UserRegistered(EventId.New(), "user3@example.com", DateTimeOffset.UtcNow);
        await publisher.PublishAsync(ev);

        handler1.HandledEvents.Should().BeEmpty();
        handler2.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
    }

    [Fact]
    public async Task InMemoryEventPublisher_Unsubscribe_LastHandler_RemovesLastAndRetainsOthers()
    {
        var publisher = new InMemoryEventPublisher();
        var handler1 = new TestHandler<UserRegistered>();
        var handler2 = new TestHandler<UserRegistered>();

        publisher.Subscribe(handler1);
        publisher.Subscribe(handler2);

        // Remove last handler (index 1)
        publisher.Unsubscribe(handler2);

        var ev = new UserRegistered(EventId.New(), "user4@example.com", DateTimeOffset.UtcNow);
        await publisher.PublishAsync(ev);

        handler1.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
        handler2.HandledEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task InMemoryEventPublisher_Unsubscribe_NonExistentHandler_DoesNotThrowOrAffectOthers()
    {
        var publisher = new InMemoryEventPublisher();
        var handler1 = new TestHandler<UserRegistered>();
        var handlerUnregistered = new TestHandler<UserRegistered>();

        publisher.Subscribe(handler1);

        // Unsubscribe not-found handler should not throw or affect registered handlers
        Action act = () => publisher.Unsubscribe(handlerUnregistered);
        act.Should().NotThrow();

        var ev = new UserRegistered(EventId.New(), "user5@example.com", DateTimeOffset.UtcNow);
        await publisher.PublishAsync(ev);

        handler1.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
    }

    [Fact]
    public async Task InMemoryEventPublisher_WhenNoSubscribers_ShouldCompleteGracefully()
    {
        var publisher = new InMemoryEventPublisher();
        var ev = new UserRegistered(EventId.New(), "nobody@example.com", DateTimeOffset.UtcNow);

        await publisher.PublishAsync(ev);
    }

    [Fact]
    public async Task InMemoryEventPublisher_WhenHandlerThrows_ShouldPropagateExceptionAndRecordFailureMetric()
    {
        using var meterScope = new MeterTestScope(EventsDiagnostics.SourceName);

        var publisher = new InMemoryEventPublisher();
        var throwingHandler = new ThrowingHandler<UserRegistered>();
        publisher.Subscribe(throwingHandler);

        var ev = new UserRegistered(EventId.New(), "error@example.com", DateTimeOffset.UtcNow);

        var act = async () => await publisher.PublishAsync(ev);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler execution failed");

        meterScope.LongMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "events.handled.count" &&
            (string?)m.Tags["event.type"] == nameof(UserRegistered) &&
            (bool?)m.Tags["event.success"] == false);
    }

    [Fact]
    public async Task InMemoryEventPublisher_WithCancellationToken_ShouldHonorCancellation()
    {
        var publisher = new InMemoryEventPublisher();
        var handler = new TestHandler<UserRegistered>();
        publisher.Subscribe(handler);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ev = new UserRegistered(EventId.New(), "user@example.com", DateTimeOffset.UtcNow);

        var act = async () => await publisher.PublishAsync(ev, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task InMemoryEventPublisher_WhenAllSubscribersUnsubscribed_CancelledTokenShouldNotThrow()
    {
        var publisher = new InMemoryEventPublisher();
        var handler1 = new TestHandler<UserRegistered>();
        var handler2 = new TestHandler<UserRegistered>();
        publisher.Subscribe(handler1);
        publisher.Subscribe(handler2);

        publisher.Unsubscribe(handler1);
        publisher.Unsubscribe(handler2);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ev = new UserRegistered(EventId.New(), "user@example.com", DateTimeOffset.UtcNow);

        var act = async () => await publisher.PublishAsync(ev, cts.Token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InMemoryEventPublisher_AsyncContinuation_ShouldUseConfigureAwaitFalse()
    {
        var publisher = new InMemoryEventPublisher();
        var asyncYieldHandler = new AsyncYieldHandler<UserRegistered>();
        publisher.Subscribe(asyncYieldHandler);

        var prevSyncContext = SynchronizationContext.Current;
        var trackingContext = new TrackingSynchronizationContext();
        Task publishTask;
        try
        {
            SynchronizationContext.SetSynchronizationContext(trackingContext);

            var ev = new UserRegistered(EventId.New(), "yield@example.com", DateTimeOffset.UtcNow);
            publishTask = publisher.PublishAsync(ev).AsTask();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevSyncContext);
        }

        _ = Task.Run(() => asyncYieldHandler.Tcs.SetResult());

        await publishTask;

        trackingContext.PostCount.Should().Be(0);
    }

    [Fact]
    public async Task InMemoryEventPublisher_ConcurrentSubscribeUnsubscribeAndPublish_ShouldBeThreadSafe()
    {
        var publisher = new InMemoryEventPublisher();
        var handlers = Enumerable.Range(0, 50).Select(_ => new TestHandler<UserRegistered>()).ToArray();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var ev = new UserRegistered(EventId.New(), "concurrent@example.com", DateTimeOffset.UtcNow);

        var publishTask = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                await publisher.PublishAsync(ev);
                await Task.Yield();
            }
        });

        var subscribeTask = Task.Run(async () =>
        {
            var rnd = new Random(42);
            while (!cts.IsCancellationRequested)
            {
                var h = handlers[rnd.Next(handlers.Length)];
                publisher.Subscribe(h);
                await Task.Yield();
                publisher.Unsubscribe(h);
            }
        });

        await Task.WhenAll(publishTask, subscribeTask);
    }

    private sealed class AsyncYieldHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        public TaskCompletionSource Tcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default)
        {
            await Tcs.Task.ConfigureAwait(false);
        }
    }

    private sealed class TestHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        public List<TEvent> HandledEvents { get; } = new();

        public ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default)
        {
            HandledEvents.Add(eventInstance);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        public ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Handler execution failed");
        }
    }
}





