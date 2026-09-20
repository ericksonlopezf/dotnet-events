// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using AwesomeAssertions;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.UnitTests.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using EventId = EricksonLopez.Events.Identifiers.EventId;

[Xunit.Trait("Category", "Unit")]
public sealed class ExecutionStrategiesTests
{
    private sealed record TestStrategyEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class DummyHandler1;
    public sealed class DummyHandler2;
    public sealed class DummyHandler3;

    private sealed class TestLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        public int WarningLogCount { get; private set; }
        public List<string> WarningMessages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == Microsoft.Extensions.Logging.LogLevel.Warning)
            {
                WarningLogCount++;
                WarningMessages.Add(state?.ToString() ?? "");
            }
        }
    }

    #region SequentialExecutionStrategy Tests

    [Theory]
    [InlineData(0, "handlers")]
    [InlineData(1, "eventInstance")]
    [InlineData(2, "serviceProvider")]
    [InlineData(3, "options")]
    public async Task SequentialStrategy_WithNullArguments_ShouldThrowArgumentNullException(int nullArgIndex, string expectedParamName)
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);
        var handlers = Array.Empty<HandlerDescriptor>();

        Func<Task> act = nullArgIndex switch
        {
            0 => async () => await strategy.ExecuteAsync<TestStrategyEvent>(null!, evt, sp, options, CancellationToken.None),
            1 => async () => await strategy.ExecuteAsync<TestStrategyEvent>(handlers, null!, sp, options, CancellationToken.None),
            2 => async () => await strategy.ExecuteAsync<TestStrategyEvent>(handlers, evt, null!, options, CancellationToken.None),
            _ => async () => await strategy.ExecuteAsync<TestStrategyEvent>(handlers, evt, sp, null!, CancellationToken.None)
        };

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.WithParameterName(expectedParamName);
    }

    [Fact]
    public async Task SequentialStrategy_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            strategy.ExecuteAsync(new[] { desc }, evt, sp, options, cts.Token).AsTask());
    }

    [Fact]
    public async Task SequentialStrategy_WithCancellationTriggeredDuringFirstHandler_ShouldCancelAndNotExecuteSecondHandler()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var h1 = new DummyHandler1();
        var h2 = new DummyHandler2();
        sp.GetService(typeof(DummyHandler1)).Returns(h1);
        sp.GetService(typeof(DummyHandler2)).Returns(h2);

        using var cts = new CancellationTokenSource();
        bool h1Executed = false;
        bool h2Executed = false;

        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
        {
            h1Executed = true;
            cts.Cancel(); // Cancel token while first handler runs
            return ValueTask.CompletedTask;
        });

        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) =>
        {
            h2Executed = true;
            return ValueTask.CompletedTask;
        });

        var options = new EventBusOptions { ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue };
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        Func<Task> act = async () => await strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, cts.Token);

        // Must throw OperationCanceledException and NOT wrap it in EventDispatchException
        await act.Should().ThrowAsync<OperationCanceledException>();

        h1Executed.Should().BeTrue();
        h2Executed.Should().BeFalse("second handler must never be invoked when token is canceled mid-execution");
    }

    [Fact]
    public async Task SequentialStrategy_WithNullServiceResolution_ShouldSkipGracefully()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns((object?)null);
        var logger = new TestLogger<SequentialExecutionStrategy>();
        sp.GetService(typeof(ILogger<SequentialExecutionStrategy>)).Returns(logger);

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);
        bool invokerCalled = false;
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
        {
            invokerCalled = true;
            return ValueTask.CompletedTask;
        });

        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        sp.GetService(typeof(DummyHandler2)).Returns((object?)null);

        await strategy.ExecuteAsync(new[] { desc, desc2 }, evt, sp, options, CancellationToken.None);

        invokerCalled.Should().BeFalse();
        logger.WarningLogCount.Should().Be(2);
        logger.WarningMessages[0].Should().Contain("Skipping handler.");
        logger.WarningMessages[1].Should().Contain("Skipping handler.");
        sp.Received(1).GetService(typeof(ILogger<SequentialExecutionStrategy>));
    }

    [Fact]
    public async Task SequentialStrategy_WithNullServiceResolution_AndNoLogger_ShouldSkipSilently()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns((object?)null);
        sp.GetService(typeof(ILogger<SequentialExecutionStrategy>)).Returns((object?)null);

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => ValueTask.CompletedTask);

        await strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None);
    }

    [Fact]
    public async Task SequentialStrategy_WithMultipleHandlers_ShouldExecuteSequentiallyInOrder()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var h1 = new DummyHandler1();
        var h2 = new DummyHandler2();
        sp.GetService(typeof(DummyHandler1)).Returns(h1);
        sp.GetService(typeof(DummyHandler2)).Returns(h2);

        var executionLog = new List<string>();
        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), async (_, _, ct) =>
        {
            await Task.Delay(5, ct).ConfigureAwait(false);
            executionLog.Add("h1");
        });
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h2");
            return ValueTask.CompletedTask;
        });

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, CancellationToken.None);

        executionLog.Should().ContainInOrder("h1", "h2");
    }

    [Fact]
    public async Task SequentialStrategy_WithFailFast_ShouldRethrowImmediatelyAndStopSubsequentHandlers()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());
        sp.GetService(typeof(DummyHandler2)).Returns(new DummyHandler2());

        var executionLog = new List<string>();
        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h1");
            throw new InvalidOperationException("Handler 1 failed");
        });
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h2");
            return ValueTask.CompletedTask;
        });

        var options = new EventBusOptions { ErrorPolicy = ErrorHandlingPolicy.FailFast };
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var act = () => strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler 1 failed");
        executionLog.Should().Equal("h1");
    }

    [Fact]
    public async Task SequentialStrategy_WithAggregateAndContinue_ShouldExecuteAllHandlersAndThrowAggregate()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());
        sp.GetService(typeof(DummyHandler2)).Returns(new DummyHandler2());
        sp.GetService(typeof(DummyHandler3)).Returns(new DummyHandler3());

        var executionLog = new List<string>();
        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h1");
            throw new InvalidOperationException("Handler 1 failed");
        });
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h2");
            return ValueTask.CompletedTask;
        });
        var desc3 = new HandlerDescriptor(typeof(DummyHandler3), typeof(object), (_, _, _) =>
        {
            executionLog.Add("h3");
            throw new ArgumentException("Handler 3 failed");
        });

        var options = new EventBusOptions { ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue };
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var act = () => strategy.ExecuteAsync(new[] { desc1, desc2, desc3 }, evt, sp, options, CancellationToken.None).AsTask();

        var ex = await act.Should().ThrowAsync<EventDispatchException>();
        ex.Which.EventType.Should().Be(typeof(TestStrategyEvent));
        ex.Which.InnerExceptions.Should().HaveCount(2);
        executionLog.Should().Equal("h1", "h2", "h3");
    }

    #endregion

    #region ParallelExecutionStrategy Tests

    [Theory]
    [InlineData(0, "handlers")]
    [InlineData(1, "eventInstance")]
    [InlineData(2, "serviceProvider")]
    [InlineData(3, "options")]
    public async Task ParallelStrategy_WithNullArguments_ShouldThrowArgumentNullException(int nullArgIndex, string expectedParamName)
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);
        var handlers = Array.Empty<HandlerDescriptor>();

        Func<Task> act = nullArgIndex switch
        {
            0 => async () => await strategy.ExecuteAsync<TestStrategyEvent>(null!, evt, sp, options, CancellationToken.None),
            1 => async () => await strategy.ExecuteAsync<TestStrategyEvent>(handlers, null!, sp, options, CancellationToken.None),
            2 => async () => await strategy.ExecuteAsync<TestStrategyEvent>(handlers, evt, null!, options, CancellationToken.None),
            _ => async () => await strategy.ExecuteAsync<TestStrategyEvent>(handlers, evt, sp, null!, CancellationToken.None)
        };

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.WithParameterName(expectedParamName);
    }

    [Fact]
    public async Task ParallelStrategy_WithEmptyHandlers_ShouldCompleteImmediately()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(Array.Empty<HandlerDescriptor>(), evt, sp, options, CancellationToken.None);
    }

    [Fact]
    public async Task ParallelStrategy_WithSingleHandler_ShouldExecuteDirectly()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var h1 = new DummyHandler1();
        sp.GetService(typeof(DummyHandler1)).Returns(h1);

        bool executed = false;
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
        {
            executed = true;
            return ValueTask.CompletedTask;
        });

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None);

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ParallelStrategy_WithSingleHandler_NullService_ShouldSkipGracefully()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns((object?)null);
        var logger = new TestLogger<ParallelExecutionStrategy>();
        sp.GetService(typeof(ILogger<ParallelExecutionStrategy>)).Returns(logger);

        bool executed = false;
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
        {
            executed = true;
            return ValueTask.CompletedTask;
        });

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None);

        executed.Should().BeFalse();
        logger.WarningLogCount.Should().Be(1);
        logger.WarningMessages[0].Should().Contain("Skipping handler.");
        sp.Received(1).GetService(typeof(ILogger<ParallelExecutionStrategy>));
    }

    [Fact]
    public async Task ParallelStrategy_WithMultipleHandlers_IncludingNullService_ShouldExecuteConcurrently()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var h1 = new DummyHandler1();
        sp.GetService(typeof(DummyHandler1)).Returns(h1);
        sp.GetService(typeof(DummyHandler2)).Returns((object?)null);
        sp.GetService(typeof(DummyHandler3)).Returns((object?)null);
        var logger = new TestLogger<ParallelExecutionStrategy>();
        sp.GetService(typeof(ILogger<ParallelExecutionStrategy>)).Returns(logger);

        var executed = new List<string>();
        var lockObj = new object();

        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), async (_, _, _) =>
        {
            await Task.Delay(10, CancellationToken.None);
            lock (lockObj) { executed.Add("h1"); }
        });
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) =>
        {
            lock (lockObj) { executed.Add("h2"); }
            return ValueTask.CompletedTask;
        });
        var desc3 = new HandlerDescriptor(typeof(DummyHandler3), typeof(object), (_, _, _) =>
        {
            lock (lockObj) { executed.Add("h3"); }
            return ValueTask.CompletedTask;
        });

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(new[] { desc1, desc2, desc3 }, evt, sp, options, CancellationToken.None);

        executed.Should().HaveCount(1);
        executed.Should().Contain("h1");
        executed.Should().NotContain("h2");
        executed.Should().NotContain("h3");

        logger.WarningLogCount.Should().Be(2);
        logger.WarningMessages[0].Should().Contain("Skipping handler.");
        logger.WarningMessages[1].Should().Contain("Skipping handler.");
        sp.Received(1).GetService(typeof(ILogger<ParallelExecutionStrategy>));
    }

    [Fact]
    public async Task ParallelStrategy_WhenHandlersFail_ShouldCollectExceptionsAndThrowEventDispatchException()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());
        sp.GetService(typeof(DummyHandler2)).Returns(new DummyHandler2());

        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), async (_, _, _) =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Parallel error 1");
        });
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), async (_, _, _) =>
        {
            await Task.Yield();
            throw new ArgumentException("Parallel error 2");
        });

        var options = new EventBusOptions { ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue };
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var act = () => strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, CancellationToken.None).AsTask();

        var ex = await act.Should().ThrowAsync<EventDispatchException>();
        ex.Which.EventType.Should().Be(typeof(TestStrategyEvent));
        ex.Which.InnerExceptions.Should().HaveCount(2);
    }

    [Fact]
    public async Task ParallelStrategy_WhenTasksAreCanceled_ShouldRethrowOperationCanceledException()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());
        sp.GetService(typeof(DummyHandler2)).Returns(new DummyHandler2());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) =>
            new ValueTask(Task.FromCanceled(cts.Token)));
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) =>
            new ValueTask(Task.FromCanceled(cts.Token)));

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var act = () => strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SequentialStrategy_AsyncContinuation_ShouldUseConfigureAwaitFalse()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => new ValueTask(tcs.Task));

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var prevSyncContext = SynchronizationContext.Current;
        var trackingContext = new TrackingSynchronizationContext();
        Task executeTask;
        try
        {
            SynchronizationContext.SetSynchronizationContext(trackingContext);
            executeTask = strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None).AsTask();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevSyncContext);
        }

        _ = Task.Run(() => tcs.SetResult());
        await executeTask;

        trackingContext.PostCount.Should().Be(0);
    }

    [Fact]
    public async Task ParallelStrategy_SingleHandler_AsyncContinuation_ShouldUseConfigureAwaitFalse()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => new ValueTask(tcs.Task));

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var prevSyncContext = SynchronizationContext.Current;
        var trackingContext = new TrackingSynchronizationContext();
        Task executeTask;
        try
        {
            SynchronizationContext.SetSynchronizationContext(trackingContext);
            executeTask = strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None).AsTask();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevSyncContext);
        }

        _ = Task.Run(() => tcs.SetResult());
        await executeTask;

        trackingContext.PostCount.Should().Be(0);
    }

    [Fact]
    public async Task ParallelStrategy_MultipleHandlers_AsyncContinuation_ShouldUseConfigureAwaitFalse()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns(new DummyHandler1());
        sp.GetService(typeof(DummyHandler2)).Returns(new DummyHandler2());

        var tcs1 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs2 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => new ValueTask(tcs1.Task));
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) => new ValueTask(tcs2.Task));

        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        var prevSyncContext = SynchronizationContext.Current;
        var trackingContext = new TrackingSynchronizationContext();
        Task executeTask;
        try
        {
            SynchronizationContext.SetSynchronizationContext(trackingContext);
            executeTask = strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, CancellationToken.None).AsTask();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevSyncContext);
        }

        _ = Task.Run(() =>
        {
            tcs1.SetResult();
            tcs2.SetResult();
        });
        await executeTask;

        trackingContext.PostCount.Should().Be(0);
    }

    [Fact]
    public async Task SequentialStrategy_WhenHandlerThrowsOperationCanceledExceptionWithoutCallerCancellation_ShouldAggregateUnderContinueOnError()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var h1 = new DummyHandler1();
        sp.GetService(typeof(DummyHandler1)).Returns(h1);

        var desc = new HandlerDescriptor(
            typeof(DummyHandler1),
            typeof(object),
            (_, _, _) => throw new OperationCanceledException("Handler cancelled directly"));

        var options = new EventBusOptions { ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue };
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        Func<Task> act = async () => await strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<EventDispatchException>();
        ex.Which.InnerExceptions.Should().ContainSingle(e => e is OperationCanceledException && e.Message == "Handler cancelled directly");
    }

    [Fact]
    public async Task SequentialStrategy_WhenCallerTokenIsCanceled_ShouldPropagateOperationCanceledExceptionDirectly()
    {
        var strategy = new SequentialExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        var h1 = new DummyHandler1();
        sp.GetService(typeof(DummyHandler1)).Returns(h1);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var desc = new HandlerDescriptor(
            typeof(DummyHandler1),
            typeof(object),
            (_, _, ct) => { ct.ThrowIfCancellationRequested(); return ValueTask.CompletedTask; });

        var options = new EventBusOptions { ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue };
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        Func<Task> act = async () => await strategy.ExecuteAsync(new[] { desc }, evt, sp, options, cts.Token);

        await act.Should().ThrowExactlyAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ParallelStrategy_SingleHandler_WhenUnresolvedAndNoLogger_ShouldSkipSilently()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns((object?)null);
        sp.GetService(typeof(ILogger<ParallelExecutionStrategy>)).Returns((object?)null);

        var desc = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(new[] { desc }, evt, sp, options, CancellationToken.None);
    }

    [Fact]
    public async Task ParallelStrategy_MultipleHandlers_WhenUnresolvedAndNoLogger_ShouldSkipSilently()
    {
        var strategy = new ParallelExecutionStrategy();
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(DummyHandler1)).Returns((object?)null);
        sp.GetService(typeof(DummyHandler2)).Returns((object?)null);
        sp.GetService(typeof(ILogger<ParallelExecutionStrategy>)).Returns((object?)null);

        var desc1 = new HandlerDescriptor(typeof(DummyHandler1), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        var desc2 = new HandlerDescriptor(typeof(DummyHandler2), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        var options = new EventBusOptions();
        var evt = new TestStrategyEvent(EventId.New(), DateTimeOffset.UtcNow);

        await strategy.ExecuteAsync(new[] { desc1, desc2 }, evt, sp, options, CancellationToken.None);
    }

    #endregion
}




