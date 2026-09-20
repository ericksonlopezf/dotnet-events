// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.UnitTests.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using EventId = EricksonLopez.Events.Identifiers.EventId;

[Collection("Diagnostics")]
[Xunit.Trait("Category", "Unit")]
public sealed class EventBusTests
{
    public sealed record SampleBusEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;
    public sealed record CascadeEvent(int Depth, EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class SampleHandler;

    [Theory]
    [InlineData(0, "registry")]
    [InlineData(1, "serviceProvider")]
    [InlineData(2, "options")]
    public void Constructor_WithNullArgument_ShouldThrowArgumentNullException(int nullArgIndex, string expectedParamName)
    {
        var registry = nullArgIndex == 0 ? null! : Substitute.For<IHandlerRegistry>();
        var sp = nullArgIndex == 1 ? null! : Substitute.For<IServiceProvider>();
        var options = nullArgIndex == 2 ? null! : new EventBusOptions();

        var act = () => new EventBus(registry, sp, options);
        act.Should().Throw<ArgumentNullException>().WithParameterName(expectedParamName);
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithNullEvent_ShouldThrowArgumentNullException()
    {
        var registry = Substitute.For<IHandlerRegistry>();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions();
        var bus = new EventBus(registry, sp, options);

        await Assert.ThrowsAsync<ArgumentNullException>(() => bus.PublishAsync<SampleBusEvent>(null!).AsTask());
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        var registry = Substitute.For<IHandlerRegistry>();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions();
        var bus = new EventBus(registry, sp, options);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await Assert.ThrowsAsync<OperationCanceledException>(() => bus.PublishAsync(evt, cts.Token).AsTask());
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithNoHandlers_AndThrowOnUnregisteredFalse_ShouldCompleteCleanlyAndRecordMetric()
    {
        using var meterScope = new MeterTestScope(EventBusDiagnostics.SourceName);

        var registry = Substitute.For<IHandlerRegistry>();
        registry.GetHandlers(typeof(SampleBusEvent)).Returns(Array.Empty<HandlerDescriptor>());

        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions { ThrowOnUnregisteredEvent = false };
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        meterScope.LongMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "eventbus.events.published" &&
            (string?)m.Tags["status"] == "success" &&
            (string?)m.Tags["event.type"] == nameof(SampleBusEvent));
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithNoHandlers_AndThrowOnUnregisteredTrue_ShouldThrowInvalidOperationException()
    {
        var registry = Substitute.For<IHandlerRegistry>();
        registry.GetHandlers(typeof(SampleBusEvent)).Returns(Array.Empty<HandlerDescriptor>());

        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions { ThrowOnUnregisteredEvent = true };
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        Func<Task> act = async () => await bus.PublishAsync(evt);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"No handlers registered for event type '{typeof(SampleBusEvent).FullName}'*");
    }

    [Fact]
    public async Task EventBus_PublishAsync_WhenReentrancyDepthReachesLimit_ShouldThrowInvalidOperationExceptionImmediately()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions { MaxReentrancyDepth = 1 };

        EventBus bus = null!;
        var handler = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handler);

        int executionCount = 0;
        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, evt, ct) =>
        {
            Interlocked.Increment(ref executionCount);
            var cascade = (CascadeEvent)evt;
            // Immediate nested publication will exceed depth 1
            await bus.PublishAsync(new CascadeEvent(cascade.Depth + 1, EventId.New(), DateTimeOffset.UtcNow), ct).ConfigureAwait(false);
        });

        registry.Register(typeof(CascadeEvent), desc);
        bus = new EventBus(registry, sp, options);

        Func<Task> act = async () => await bus.PublishAsync(new CascadeEvent(1, EventId.New(), DateTimeOffset.UtcNow));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*maximum reentrancy depth limit (1) exceeded*");

        // The handler must have executed exactly once (at depth 0), and failed on the recursive call at depth 1
        executionCount.Should().Be(1);

        // Subsequent call must succeed because depth was restored in finally
        var normalEvt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(normalEvt);
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithReentrancyDepthGreaterThanOne_ShouldAllowNestedCallsUntilConfiguredLimitAndUnwindCorrectly()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions { MaxReentrancyDepth = 2 };

        EventBus bus = null!;
        var handler = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handler);

        int executionCount = 0;
        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, evt, ct) =>
        {
            Interlocked.Increment(ref executionCount);
            var cascade = (CascadeEvent)evt;
            // Recursively publish nested event
            await bus.PublishAsync(new CascadeEvent(cascade.Depth + 1, EventId.New(), DateTimeOffset.UtcNow), ct).ConfigureAwait(false);
        });

        registry.Register(typeof(CascadeEvent), desc);
        bus = new EventBus(registry, sp, options);

        // depth=0 (0 < 2 -> ok) -> publishes depth=1 (1 < 2 -> ok) -> publishes depth=2 (limit reached, 2 >= 2 -> throws)
        Func<Task> act = async () => await bus.PublishAsync(new CascadeEvent(0, EventId.New(), DateTimeOffset.UtcNow));

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*maximum reentrancy depth limit (2) exceeded*");

        // The handler executed at depth 0 and depth 1 (total 2 executions), and failed at depth 2
        executionCount.Should().Be(2);

        // Subsequent publication must succeed completely because depth counter was cleanly unwound in finally
        var normalEvt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        Func<Task> actSubsequent = async () => await bus.PublishAsync(normalEvt);
        await actSubsequent.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithSequentialMode_ShouldExecuteSequentiallyAndRecordSuccessMetric()
    {
        using var meterScope = new MeterTestScope(EventBusDiagnostics.SourceName);

        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        var executionLog = new List<string>();
        var desc1 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, _, ct) =>
        {
            await Task.Delay(10, ct).ConfigureAwait(false);
            executionLog.Add("h1");
        });
        var desc2 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h2");
            return ValueTask.CompletedTask;
        });

        registry.Register(typeof(SampleBusEvent), desc1);
        registry.Register(typeof(SampleBusEvent), desc2);

        var seqOptions = new EventBusOptions { ExecutionMode = EventExecutionMode.Sequential };
        var seqBus = new EventBus(registry, sp, seqOptions);

        await seqBus.PublishAsync(new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow));

        executionLog.Should().Equal("h1", "h2");

        meterScope.LongMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "eventbus.events.published" &&
            (string?)m.Tags["status"] == "success" &&
            (string?)m.Tags["event.type"] == nameof(SampleBusEvent));
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithParallelMode_ShouldExecuteConcurrently()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        var h1Started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var h2Started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseH1 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var completedHandlers = new List<string>();
        var lockObj = new object();

        var desc1 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, _, ct) =>
        {
            h1Started.TrySetResult();
            // Await until h2 has started concurrently, then wait for release signal
            await h2Started.Task.ConfigureAwait(false);
            await releaseH1.Task.ConfigureAwait(false);
            lock (lockObj) { completedHandlers.Add("h1"); }
        });
        var desc2 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, _, ct) =>
        {
            h2Started.TrySetResult();
            // Await until h1 has started concurrently, proving simultaneous execution
            await h1Started.Task.ConfigureAwait(false);
            lock (lockObj) { completedHandlers.Add("h2"); }
            // Once h2 finishes recording its completion, release h1 to complete
            releaseH1.TrySetResult();
        });

        registry.Register(typeof(SampleBusEvent), desc1);
        registry.Register(typeof(SampleBusEvent), desc2);

        var parOptions = new EventBusOptions { ExecutionMode = EventExecutionMode.Parallel };
        var parBus = new EventBus(registry, sp, parOptions);

        await parBus.PublishAsync(new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow));

        // Handler h2 completes deterministically before handler h1 is released, proving concurrent execution
        lock (lockObj)
        {
            completedHandlers.Should().Equal("h2", "h1");
        }
    }

    private sealed class LoggingTestMiddleware : IEventMiddleware
    {
        private readonly List<string> _log;
        public LoggingTestMiddleware(List<string> log) => _log = log;

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> nextHandler, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            _log.Add("middleware:before");
            await nextHandler(eventInstance, cancellationToken).ConfigureAwait(false);
            _log.Add("middleware:after");
        }
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithMiddlewarePipeline_ShouldWrapDispatchExecution()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        var log = new List<string>();
        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) =>
        {
            log.Add("handler");
            return ValueTask.CompletedTask;
        });
        registry.Register(typeof(SampleBusEvent), desc);

        var middleware = new LoggingTestMiddleware(log);
        var options = new EventBusOptions();
        var bus = new EventBus(registry, sp, options, new[] { middleware });

        await bus.PublishAsync(new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow));

        log.Should().Equal("middleware:before", "handler", "middleware:after");
    }

    [Fact]
    public async Task EventBus_PublishAsync_WhenHandlerThrows_ShouldRecordDiagnosticsAndLoggerAndRethrow()
    {
        using var activityScope = new ActivityTestScope(EventBusDiagnostics.SourceName);
        using var meterScope = new MeterTestScope(EventBusDiagnostics.SourceName);

        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) =>
            throw new InvalidOperationException("Handler crash"));
        registry.Register(typeof(SampleBusEvent), desc);

        var logger = Substitute.For<ILogger<EventBus>>();
        var options = new EventBusOptions();
        var bus = new EventBus(registry, sp, options, null, logger);

        Func<Task> act = async () => await bus.PublishAsync(new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler crash");

        AssertPublishActivityFailed(activityScope, "Handler crash", typeof(InvalidOperationException));

        // Verify specific logger call and message
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<Microsoft.Extensions.Logging.EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error occurred while dispatching event") && o.ToString()!.Contains(nameof(SampleBusEvent))),
            Arg.Is<Exception>(e => e.Message == "Handler crash"),
            Arg.Any<Func<object, Exception?, string>>());

        meterScope.LongMeasurements.Should().ContainSingle(m =>
            m.InstrumentName == "eventbus.events.failed" &&
            (string?)m.Tags["event.type"] == nameof(SampleBusEvent));
    }

    [Fact]
    public async Task EventBus_PublishAsync_AsyncContinuation_ShouldUseConfigureAwaitFalse()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) => new ValueTask(tcs.Task));
        registry.Register(typeof(SampleBusEvent), desc);

        var options = new EventBusOptions();
        var bus = new EventBus(registry, sp, options);

        var prevSyncContext = SynchronizationContext.Current;
        var trackingContext = new TrackingSynchronizationContext();
        Task publishTask;
        try
        {
            SynchronizationContext.SetSynchronizationContext(trackingContext);
            var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
            publishTask = bus.PublishAsync(evt).AsTask();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevSyncContext);
        }

        _ = Task.Run(() => tcs.SetResult());
        await publishTask;

        trackingContext.PostCount.Should().Be(0);
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithNullResolvedHandler_Sequential_ShouldLogWarningAndSkip()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<ILogger<EricksonLopez.Events.Bus.Execution.SequentialExecutionStrategy>>();
        sp.GetService(typeof(ILogger<EricksonLopez.Events.Bus.Execution.SequentialExecutionStrategy>)).Returns(logger);
        sp.GetService(typeof(SampleHandler)).Returns((object?)null);

        bool invoked = false;
        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });
        registry.Register(typeof(SampleBusEvent), desc);

        var options = new EventBusOptions { ExecutionMode = EventExecutionMode.Sequential };
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        invoked.Should().BeFalse();
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<Microsoft.Extensions.Logging.EventId>(),
            Arg.Is<object>(state => state != null &&
                                    state.ToString()!.Contains(typeof(SampleHandler).FullName!) &&
                                    state.ToString()!.Contains(nameof(SampleBusEvent))),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithNullResolvedHandler_Parallel_ShouldLogWarningAndSkip()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<ILogger<EricksonLopez.Events.Bus.Execution.ParallelExecutionStrategy>>();
        sp.GetService(typeof(ILogger<EricksonLopez.Events.Bus.Execution.ParallelExecutionStrategy>)).Returns(logger);
        sp.GetService(typeof(SampleHandler)).Returns((object?)null);

        bool invoked = false;
        var desc1 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });
        var desc2 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, _, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });
        registry.Register(typeof(SampleBusEvent), desc1);
        registry.Register(typeof(SampleBusEvent), desc2);

        var options = new EventBusOptions { ExecutionMode = EventExecutionMode.Parallel };
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        invoked.Should().BeFalse();
        logger.Received(2).Log(
            LogLevel.Warning,
            Arg.Any<Microsoft.Extensions.Logging.EventId>(),
            Arg.Is<object>(state => state != null &&
                                    state.ToString()!.Contains(typeof(SampleHandler).FullName!) &&
                                    state.ToString()!.Contains(nameof(SampleBusEvent))),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithEnvelopeExtension_ShouldPublishSuccessfully()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        SampleBusEvent? handledEvent = null;
        var desc = new HandlerDescriptor(typeof(SampleHandler), typeof(object), (_, evt, _) =>
        {
            handledEvent = (SampleBusEvent)evt;
            return ValueTask.CompletedTask;
        });
        registry.Register(typeof(SampleBusEvent), desc);

        var options = new EventBusOptions();
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        var envelope = EventEnvelope.Create(evt);

        await bus.PublishAsync(envelope);

        handledEvent.Should().BeSameAs(evt);
    }

    [Fact]
    public async Task EventBus_PublishAsync_Parallel_WithCancelledTokenMidExecution_ShouldThrow()
    {
        var registry = new HandlerRegistry();
        var sp = Substitute.For<IServiceProvider>();
        var handlerInstance = new SampleHandler();
        sp.GetService(typeof(SampleHandler)).Returns(handlerInstance);

        using var cts = new CancellationTokenSource();

        var desc1 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, _, ct) =>
        {
            await Task.Yield();
            cts.Cancel();
            ct.ThrowIfCancellationRequested();
        });
        var desc2 = new HandlerDescriptor(typeof(SampleHandler), typeof(object), async (_, _, ct) =>
        {
            await Task.Delay(50, ct);
        });

        registry.Register(typeof(SampleBusEvent), desc1);
        registry.Register(typeof(SampleBusEvent), desc2);

        var options = new EventBusOptions { ExecutionMode = EventExecutionMode.Parallel };
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        var act = async () => await bus.PublishAsync(evt, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EventBus_PublishAsync_UnderConcurrentLoad_ShouldNotThrow()
    {
        var registry = Substitute.For<IHandlerRegistry>();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions { ExecutionMode = EventExecutionMode.Parallel };
        var bus = new EventBus(registry, sp, options);

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);

        var tasks = Enumerable.Range(0, 1000).Select(_ => bus.PublishAsync(evt).AsTask());
        var act = async () => await Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();
    }

    // Integration tests have been moved to EventBusIntegrationTests.cs

    private static EventBus CreateDefaultBus(out IHandlerRegistry registry, out IServiceProvider sp, EventExecutionMode mode = EventExecutionMode.Sequential)
    {
        registry = Substitute.For<IHandlerRegistry>();
        sp = Substitute.For<IServiceProvider>();
        return new EventBus(registry, sp, new EventBusOptions { ExecutionMode = mode });
    }
    private static void AssertPublishActivityFailed(ActivityTestScope activityScope, string errorMessage, Type exceptionType)
    {
        activityScope.StoppedActivities.Should().NotBeEmpty();
        var stoppedActivity = activityScope.StoppedActivities.Single(a => a.OperationName == "EventBus.Publish");
        stoppedActivity.Status.Should().Be(ActivityStatusCode.Error);
        stoppedActivity.StatusDescription.Should().Be(errorMessage);
        stoppedActivity.Events.Should().Contain(e => e.Name == "exception");

        var exEvent = stoppedActivity.Events.First(e => e.Name == "exception");
        exEvent.Tags.Should().Contain(kv => kv.Key == "exception.message" && (string?)kv.Value == errorMessage);
        exEvent.Tags.Should().Contain(kv => kv.Key == "exception.type" && (string?)kv.Value == exceptionType.FullName);
    }
}




