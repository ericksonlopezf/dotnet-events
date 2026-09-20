// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Testing;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.Events.Testing.Tests;

[Trait("Category", "Unit")]
public sealed class TestingUtilitiesTests
{
    private sealed record TestOrderCreated(EventId Id, string CustomerId, decimal Amount, DateTimeOffset OccurredAt) : IDomainEvent;
    private sealed record TestPaymentCompleted(EventId Id, Guid PaymentId, DateTimeOffset OccurredAt) : IIntegrationEvent;

    #region FakeEventPublisher Tests

    [Fact]
    public async Task FakeEventPublisher_PublishAsync_ShouldRecordEventsAndEnvelopes()
    {
        var publisher = new FakeEventPublisher();
        var ev1 = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        var ev2 = new TestPaymentCompleted(EventId.New(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        await publisher.PublishAsync(ev1);
        await publisher.PublishAsync(ev2);

        publisher.Count.Should().Be(2);
        publisher.PublishedEvents.Should().HaveCount(2);
        publisher.GetEvents<TestOrderCreated>().Should().ContainSingle().Which.Should().Be(ev1);
        publisher.GetEvents<TestPaymentCompleted>().Should().ContainSingle().Which.Should().Be(ev2);
        publisher.GetSingleEvent<TestOrderCreated>().Should().Be(ev1);

        publisher.ShouldHavePublished<TestOrderCreated>();
        publisher.ShouldHavePublished<TestOrderCreated>(e => e.CustomerId == "cust-1");
        publisher.ShouldHavePublishedCount<TestOrderCreated>(1);
        publisher.ShouldHavePublishedCount<TestPaymentCompleted>(1);
        publisher.ShouldNotHavePublished<TestOrderCreated>(e => e.CustomerId == "non-existent");
    }

    [Fact]
    public async Task FakeEventPublisher_NullAndCancellationValidations_ShouldThrow()
    {
        var publisher = new FakeEventPublisher();

        var actNull = async () => await publisher.PublishAsync<TestOrderCreated>(null!);
        var exNull = (await actNull.Should().ThrowExactlyAsync<ArgumentNullException>()).Which;
        exNull.ParamName.Should().Be("eventInstance");

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ev = new TestOrderCreated(EventId.New(), "c-1", 50m, DateTimeOffset.UtcNow);
        var actCancelled = async () => await publisher.PublishAsync(ev, cts.Token);
        await actCancelled.Should().ThrowExactlyAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FakeEventPublisher_SimulatedFailure_ShouldThrowConfiguredTimes()
    {
        var publisher = new FakeEventPublisher();
        var customEx = new InvalidOperationException("Simulated network drop");
        publisher.SimulateFailure(customEx, failureCount: 2);

        var ev = new TestOrderCreated(EventId.New(), "c-1", 50m, DateTimeOffset.UtcNow);

        var act1 = async () => await publisher.PublishAsync(ev);
        var act2 = async () => await publisher.PublishAsync(ev);
        var act3 = async () => await publisher.PublishAsync(ev);
        var act4 = async () => await publisher.PublishAsync(ev);

        await act1.Should().ThrowAsync<InvalidOperationException>().WithMessage("Simulated network drop");
        await act2.Should().ThrowAsync<InvalidOperationException>().WithMessage("Simulated network drop");
        await act3.Should().NotThrowAsync();
        await act4.Should().NotThrowAsync();

        publisher.Count.Should().Be(2);
    }

    [Fact]
    public void FakeEventPublisher_SimulateFailure_WithInvalidParameters_ShouldThrow()
    {
        var publisher = new FakeEventPublisher();

        var actNullEx = () => publisher.SimulateFailure(null!, 1);
        var exNull = actNullEx.Should().ThrowExactly<ArgumentNullException>().Which;
        exNull.ParamName.Should().Be("exception");

        var actZero = () => publisher.SimulateFailure(new InvalidOperationException("zero"), 0);
        var exZero = actZero.Should().ThrowExactly<ArgumentOutOfRangeException>().Which;
        exZero.ParamName.Should().Be("failureCount");
        exZero.Message.Should().Contain("Failure count must be greater than zero.");

        var actNegative = () => publisher.SimulateFailure(new InvalidOperationException("neg"), -1);
        var exNegative = actNegative.Should().ThrowExactly<ArgumentOutOfRangeException>().Which;
        exNegative.ParamName.Should().Be("failureCount");
    }

    [Fact]
    public void FakeEventPublisher_AssertionFailures_ShouldThrowDescriptiveExceptions()
    {
        var publisher = new FakeEventPublisher();

        Action act1 = () => publisher.ShouldHavePublished<TestOrderCreated>();
        act1.Should().Throw<InvalidOperationException>().WithMessage("*Expected event of type 'TestOrderCreated'*");

        Action act2 = () => publisher.GetSingleEvent<TestOrderCreated>();
        act2.Should().Throw<InvalidOperationException>().WithMessage("*none were published*");

        Action actCount = () => publisher.ShouldHavePublishedCount<TestOrderCreated>(1);
        actCount.Should().Throw<InvalidOperationException>().WithMessage("*Expected 1 events of type 'TestOrderCreated'*");

        var ev1 = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        var ev2 = new TestOrderCreated(EventId.New(), "cust-2", 200m, DateTimeOffset.UtcNow);
        _ = publisher.PublishAsync(ev1);

        Action actNotPublished = () => publisher.ShouldNotHavePublished<TestOrderCreated>();
        actNotPublished.Should().Throw<InvalidOperationException>().WithMessage("*Expected no events of type 'TestOrderCreated'*");

        Action actNotPublishedPred = () => publisher.ShouldNotHavePublished<TestOrderCreated>(e => e.CustomerId == "cust-1");
        actNotPublishedPred.Should().Throw<InvalidOperationException>().WithMessage("*Expected no events of type 'TestOrderCreated' matching predicate*");

        Action actShouldHavePublishedPredNone = () => publisher.ShouldHavePublished<TestOrderCreated>(e => e.CustomerId == "unknown");
        actShouldHavePublishedPredNone.Should().Throw<InvalidOperationException>().WithMessage("*Expected event of type 'TestOrderCreated' matching the predicate to have been published, but none matched.*");

        _ = publisher.PublishAsync(ev2);
        Action actGetSingleMultiple = () => publisher.GetSingleEvent<TestOrderCreated>();
        actGetSingleMultiple.Should().Throw<InvalidOperationException>().WithMessage("*Expected exactly one event of type 'TestOrderCreated', but 2 were published.*");
    }

    [Fact]
    public void FakeEventPublisher_NullPredicatesAndCounts_ShouldThrowArgumentExceptions()
    {
        var publisher = new FakeEventPublisher();

        var actGetEvents = () => publisher.GetEvents<TestOrderCreated>((Func<TestOrderCreated, bool>)null!);
        actGetEvents.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("predicate");

        var actShouldHavePublished = () => publisher.ShouldHavePublished<TestOrderCreated>((Func<TestOrderCreated, bool>)null!);
        actShouldHavePublished.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("predicate");

        var actShouldNotHavePublished = () => publisher.ShouldNotHavePublished<TestOrderCreated>((Func<TestOrderCreated, bool>)null!);
        actShouldNotHavePublished.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("predicate");

        var actCountNegative = () => publisher.ShouldHavePublishedCount<TestOrderCreated>(-1);
        var exCount = actCountNegative.Should().ThrowExactly<ArgumentOutOfRangeException>().Which;
        exCount.ParamName.Should().Be("expectedCount");
        exCount.Message.Should().Contain("Expected count cannot be negative.");
    }

    [Fact]
    public void FakeEventPublisher_ValidAssertions_ShouldReturnChainedInstance()
    {
        var publisher = new FakeEventPublisher();
        publisher.ShouldHavePublishedCount<TestOrderCreated>(0).Should().BeSameAs(publisher);

        var ev = new TestOrderCreated(EventId.New(), "cust-chain", 50m, DateTimeOffset.UtcNow);
        _ = publisher.PublishAsync(ev);

        var filtered = publisher.GetEvents<TestOrderCreated>(e => e.CustomerId == "cust-chain");
        filtered.Should().ContainSingle().Which.Should().Be(ev);

        var emptyFiltered = publisher.GetEvents<TestOrderCreated>(e => e.CustomerId == "cust-other");
        emptyFiltered.Should().BeEmpty();

        var chain1 = publisher.ShouldHavePublished<TestOrderCreated>(e => e.CustomerId == "cust-chain");
        chain1.Should().BeSameAs(publisher);

        var chain2 = publisher.ShouldNotHavePublished<TestPaymentCompleted>();
        chain2.Should().BeSameAs(publisher);

        var chain3 = publisher.ShouldNotHavePublished<TestOrderCreated>(e => e.CustomerId == "cust-other");
        chain3.Should().BeSameAs(publisher);

        var chain4 = publisher.ShouldHavePublishedCount<TestOrderCreated>(1);
        chain4.Should().BeSameAs(publisher);
    }

    [Fact]
    public void FakeEventPublisher_ShouldHavePublishedPredicate_ShouldShortCircuitOnFirstMatch()
    {
        var publisher = new FakeEventPublisher();
        var ev1 = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        var ev2 = new TestOrderCreated(EventId.New(), "cust-2", 200m, DateTimeOffset.UtcNow);
        var ev3 = new TestOrderCreated(EventId.New(), "cust-3", 300m, DateTimeOffset.UtcNow);

        _ = publisher.PublishAsync(ev1);
        _ = publisher.PublishAsync(ev2);
        _ = publisher.PublishAsync(ev3);

        int evaluations = 0;
        publisher.ShouldHavePublished<TestOrderCreated>(e =>
        {
            evaluations++;
            return true;
        });

        evaluations.Should().Be(1);
    }

    [Fact]
    public void FakeEventPublisher_Reset_ShouldClearAll()
    {
        var publisher = new FakeEventPublisher();
        var ev = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        _ = publisher.PublishAsync(ev);
        publisher.SimulateFailure(new InvalidOperationException("boom"), 5);

        publisher.Count.Should().Be(1);
        publisher.Reset();
        publisher.Count.Should().Be(0);
        publisher.PublishedEvents.Should().BeEmpty();

        // After reset, publish succeeds with no failure thrown
        var act = async () => await publisher.PublishAsync(ev);
        act.Should().NotThrowAsync();
        publisher.Count.Should().Be(1);
    }

    #endregion

    #region TestEventHandler Tests

    [Fact]
    public async Task TestEventHandler_InvocationAndCustomBehaviors_ShouldWork()
    {
        var handler = new TestEventHandler<TestOrderCreated>();
        var ev = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);

        handler.WasInvoked.Should().BeFalse();
        handler.InvocationCount.Should().Be(0);
        handler.LastEvent.Should().BeNull();
        handler.HandledEvents.Should().BeEmpty();
        handler.ExecutionTimestamps.Should().BeEmpty();

        await handler.HandleAsync(ev);

        handler.WasInvoked.Should().BeTrue();
        handler.InvocationCount.Should().Be(1);
        handler.LastEvent.Should().Be(ev);
        handler.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
        handler.ExecutionTimestamps.Should().HaveCount(1);
    }

    [Fact]
    public async Task TestEventHandler_NullAndCancellationValidations_ShouldThrow()
    {
        var handler = new TestEventHandler<TestOrderCreated>();

        var actNull = async () => await handler.HandleAsync(null!);
        var exNull = (await actNull.Should().ThrowExactlyAsync<ArgumentNullException>()).Which;
        exNull.ParamName.Should().Be("eventInstance");

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ev = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        var actCancelled = async () => await handler.HandleAsync(ev, cts.Token);
        await actCancelled.Should().ThrowExactlyAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TestEventHandler_WithDelayAndCallback_ShouldExecute()
    {
        bool callbackExecuted = false;
        var handler = new TestEventHandler<TestOrderCreated>()
            .WithDelay(TimeSpan.FromMilliseconds(40))
            .WithCallback(e => callbackExecuted = true);

        var ev = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        var sw = Stopwatch.StartNew();
        await handler.HandleAsync(ev);
        sw.Stop();

        callbackExecuted.Should().BeTrue();
        sw.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(20));
    }

    [Fact]
    public void TestEventHandler_WithInvalidDelay_ShouldThrowArgumentOutOfRangeException()
    {
        var handler = new TestEventHandler<TestOrderCreated>();

        var actZero = () => handler.WithDelay(TimeSpan.Zero);
        var exZero = actZero.Should().ThrowExactly<ArgumentOutOfRangeException>().Which;
        exZero.ParamName.Should().Be("delay");
        exZero.Message.Should().Contain("Delay duration must be greater than zero.");

        var actNegative = () => handler.WithDelay(TimeSpan.FromMilliseconds(-10));
        var exNegative = actNegative.Should().ThrowExactly<ArgumentOutOfRangeException>().Which;
        exNegative.ParamName.Should().Be("delay");
        exNegative.Message.Should().Contain("Delay duration must be greater than zero.");
    }

    [Fact]
    public async Task TestEventHandler_WithoutDelay_ShouldExecuteNormally()
    {
        var handler = new TestEventHandler<TestOrderCreated>();
        var ev = new TestOrderCreated(EventId.New(), "cust-no-delay", 10m, DateTimeOffset.UtcNow);

        await handler.HandleAsync(ev);

        handler.WasInvoked.Should().BeTrue();
        handler.InvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task TestEventHandler_WithAsyncCallback_ShouldExecute()
    {
        bool asyncCallbackExecuted = false;
        var handler = new TestEventHandler<TestOrderCreated>()
            .WithCallback((evt, ct) =>
            {
                asyncCallbackExecuted = true;
                return ValueTask.CompletedTask;
            });

        var ev = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        await handler.HandleAsync(ev);

        asyncCallbackExecuted.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "exception")]
    [InlineData(1, "callback")]
    [InlineData(2, "asyncCallback")]
    public void TestEventHandler_WithNullCallbacksOrExceptions_ShouldThrowArgumentNullException(int nullArgIndex, string expectedParamName)
    {
        var handler = new TestEventHandler<TestOrderCreated>();

        Action act = nullArgIndex switch
        {
            0 => () => handler.WithException(null!),
            1 => () => handler.WithCallback((Action<TestOrderCreated>)null!),
            _ => () => handler.WithCallback((Func<TestOrderCreated, CancellationToken, ValueTask>)null!)
        };

        act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be(expectedParamName);
    }

    [Fact]
    public async Task TestEventHandler_WithException_ShouldThrow()
    {
        var handler = new TestEventHandler<TestOrderCreated>()
            .WithException(new FormatException("Custom handler error"));

        var ev = new TestOrderCreated(EventId.New(), "cust-1", 100m, DateTimeOffset.UtcNow);
        var act = async () => await handler.HandleAsync(ev);

        await act.Should().ThrowAsync<FormatException>().WithMessage("Custom handler error");
        handler.WasInvoked.Should().BeTrue();
        handler.HandledEvents.Should().ContainSingle().Which.Should().Be(ev);
    }

    [Fact]
    public async Task TestEventHandler_Reset_ShouldClearAll()
    {
        var handler = new TestEventHandler<TestOrderCreated>()
            .WithDelay(TimeSpan.FromMilliseconds(50))
            .WithException(new InvalidOperationException("fail"))
            .WithCallback(e => { });

        var ev = new TestOrderCreated(EventId.New(), "cust-reset", 10m, DateTimeOffset.UtcNow);
        try
        {
            await handler.HandleAsync(ev);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        handler.WasInvoked.Should().BeTrue();
        handler.Reset();

        handler.WasInvoked.Should().BeFalse();
        handler.InvocationCount.Should().Be(0);
        handler.LastEvent.Should().BeNull();
        handler.HandledEvents.Should().BeEmpty();
        handler.ExecutionTimestamps.Should().BeEmpty();

        // After reset, no delay, exception or callback remains
        await handler.HandleAsync(ev);
        handler.WasInvoked.Should().BeTrue();
    }

    #endregion

    #region EventTestBuilder Tests

    [Fact]
    public void EventTestBuilder_WhenPayloadNull_ShouldThrowArgumentNullException()
    {
        var act = () => EventTestBuilder.For<TestOrderCreated>(null!);
        var ex = act.Should().ThrowExactly<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("payload");
    }

    [Fact]
    public void EventEnvelopeTestBuilder_WithInvalidHeaders_ShouldThrow()
    {
        var ev = new TestOrderCreated(EventId.New(), "cust-builder", 350m, DateTimeOffset.UtcNow);
        var builder = EventTestBuilder.For(ev);

        var actKeyNull = () => builder.WithHeader(null!, "val");
        actKeyNull.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("key");

        var actKeyEmpty = () => builder.WithHeader(string.Empty, "val");
        actKeyEmpty.Should().ThrowExactly<ArgumentException>().Which.ParamName.Should().Be("key");

        var actKeyWhitespace = () => builder.WithHeader("   ", "val");
        actKeyWhitespace.Should().ThrowExactly<ArgumentException>().Which.ParamName.Should().Be("key");

        var actValNull = () => builder.WithHeader("key", null!);
        actValNull.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("value");
    }

    [Fact]
    public void EventTestBuilder_DefaultBuild_ShouldUsePayloadDefaults()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var ev = new TestOrderCreated(eventId, "cust-builder", 350m, now);

        var envelope = EventTestBuilder.For(ev).Build();

        envelope.Id.Should().Be(eventId);
        envelope.Type.Value.Should().Be(nameof(TestOrderCreated));
        envelope.Version.Should().Be(EventVersion.V1);
        envelope.OccurredAt.Should().Be(now);
        envelope.Payload.Should().Be(ev);
        envelope.Metadata.Should().Be(EventMetadata.Empty);
    }

    [Fact]
    public void EventTestBuilder_ShouldConstructSyntheticEnvelopes()
    {
        var originalId = EventId.New();
        var originalTime = DateTimeOffset.UtcNow.AddHours(-1);
        var ev = new TestOrderCreated(originalId, "cust-builder", 350m, originalTime);

        var explicitId = EventId.New();
        var explicitTime = DateTimeOffset.UtcNow;

        var envelope = EventTestBuilder.For(ev)
            .WithId(explicitId)
            .WithType("ordering.order-created")
            .WithVersion(3)
            .WithOccurredAt(explicitTime)
            .WithCorrelationId("corr-123")
            .WithCausationId("caus-456")
            .WithTenantId("tenant-789")
            .WithSource("https://ordering.service.internal")
            .WithContentType("application/json")
            .WithHeader("X-Custom", "Val")
            .Build();

        envelope.Id.Should().Be(explicitId);
        envelope.Id.Should().NotBe(originalId);
        envelope.Type.Value.Should().Be("ordering.order-created");
        envelope.Version.Value.Should().Be(3u);
        envelope.OccurredAt.Should().Be(explicitTime);
        envelope.OccurredAt.Should().NotBe(originalTime);
        envelope.Payload.Should().Be(ev);
        envelope.Metadata.CorrelationId.Value.Should().Be("corr-123");
        envelope.Metadata.CausationId.Value.Should().Be("caus-456");
        envelope.Metadata.TenantId.Value.Should().Be("tenant-789");
        envelope.Metadata.Source.Should().Be("https://ordering.service.internal");
        envelope.Metadata.ContentType.Should().Be("application/json");
        envelope.Metadata.CustomHeaders.Should().ContainKey("x-custom");
    }

    #endregion

    #region Property-Based Testing (FsCheck)

    [Property]
    public Property FakeEventPublisher_ArbitraryEvents_ShouldPreserveExactCountAndIdentity()
    {
        return Prop.ForAll(Arb.Default.PositiveInt(), countGen =>
        {
            var publisher = new FakeEventPublisher();
            int count = Math.Min(countGen.Item, 20); // Cap to 20 for test performance

            for (int i = 0; i < count; i++)
            {
                var ev = new TestOrderCreated(EventId.New(), $"cust-{i}", i * 10m, DateTimeOffset.UtcNow);
                _ = publisher.PublishAsync(ev);
            }

            return publisher.Count == count &&
                   publisher.GetEvents<TestOrderCreated>().Count == count &&
                   publisher.PublishedEvents.Count == count;
        });
    }

    #endregion
}
