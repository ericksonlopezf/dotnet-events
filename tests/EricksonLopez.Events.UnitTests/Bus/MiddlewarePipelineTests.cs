// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class MiddlewarePipelineTests
{
    private sealed record TestMiddlewareEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    private sealed class LoggingMiddleware : IEventMiddleware
    {
        private readonly List<string> _log;
        private readonly string _name;

        public LoggingMiddleware(List<string> log, string name)
        {
            _log = log;
            _name = name;
        }

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> nextHandler, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            _log.Add($"{_name}:before");
            await nextHandler(eventInstance, cancellationToken);
            _log.Add($"{_name}:after");
        }
    }

    private sealed class ShortCircuitMiddleware : IEventMiddleware
    {
        private readonly List<string> _log;

        public ShortCircuitMiddleware(List<string> log)
        {
            _log = log;
        }

        public ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> nextHandler, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            _log.Add("short-circuit");
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public void Build_WithNullTerminal_ShouldThrowArgumentNullException()
    {
        var act = () => MiddlewarePipeline.Build<TestMiddlewareEvent>(null!, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("terminal");
    }

    [Fact]
    public async Task Build_WithNullOrEmptyMiddlewares_ShouldReturnTerminalDirectly()
    {
        bool terminalExecuted = false;
        EventMiddlewareDelegate<TestMiddlewareEvent> terminal = (evt, ct) =>
        {
            terminalExecuted = true;
            return ValueTask.CompletedTask;
        };

        var pipeline1 = MiddlewarePipeline.Build(null!, terminal);
        var pipeline2 = MiddlewarePipeline.Build(Array.Empty<IEventMiddleware>(), terminal);

        var evt = new TestMiddlewareEvent(EventId.New(), DateTimeOffset.UtcNow);

        await pipeline1(evt, CancellationToken.None);
        terminalExecuted.Should().BeTrue();

        terminalExecuted = false;
        await pipeline2(evt, CancellationToken.None);
        terminalExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Build_WithMultipleMiddlewares_ShouldExecuteInPipelineOrder()
    {
        var log = new List<string>();
        var m1 = new LoggingMiddleware(log, "m1");
        var m2 = new LoggingMiddleware(log, "m2");

        EventMiddlewareDelegate<TestMiddlewareEvent> terminal = (evt, ct) =>
        {
            log.Add("terminal");
            return ValueTask.CompletedTask;
        };

        var pipeline = MiddlewarePipeline.Build(new IEventMiddleware[] { m1, m2 }, terminal);
        var evt = new TestMiddlewareEvent(EventId.New(), DateTimeOffset.UtcNow);

        await pipeline(evt, CancellationToken.None);

        log.Should().Equal("m1:before", "m2:before", "terminal", "m2:after", "m1:after");
    }

    [Fact]
    public async Task Build_WithThreeOrMoreMiddlewares_ShouldNestAndUnwindInExactPipelineOrder()
    {
        var log = new List<string>();
        var m1 = new LoggingMiddleware(log, "m1");
        var m2 = new LoggingMiddleware(log, "m2");
        var m3 = new LoggingMiddleware(log, "m3");

        EventMiddlewareDelegate<TestMiddlewareEvent> terminal = (evt, ct) =>
        {
            log.Add("terminal");
            return ValueTask.CompletedTask;
        };

        var pipeline = MiddlewarePipeline.Build(new IEventMiddleware[] { m1, m2, m3 }, terminal);
        var evt = new TestMiddlewareEvent(EventId.New(), DateTimeOffset.UtcNow);

        await pipeline(evt, CancellationToken.None);

        log.Should().Equal(
            "m1:before",
            "m2:before",
            "m3:before",
            "terminal",
            "m3:after",
            "m2:after",
            "m1:after");
    }

    [Fact]
    public async Task Build_WithShortCircuitMiddleware_ShouldNotCallTerminal()
    {
        var log = new List<string>();
        var m1 = new LoggingMiddleware(log, "m1");
        var m2 = new ShortCircuitMiddleware(log);

        EventMiddlewareDelegate<TestMiddlewareEvent> terminal = (evt, ct) =>
        {
            log.Add("terminal");
            return ValueTask.CompletedTask;
        };

        var pipeline = MiddlewarePipeline.Build(new IEventMiddleware[] { m1, m2 }, terminal);
        var evt = new TestMiddlewareEvent(EventId.New(), DateTimeOffset.UtcNow);

        await pipeline(evt, CancellationToken.None);

        log.Should().Equal("m1:before", "short-circuit", "m1:after");
        log.Should().NotContain("terminal");
    }

    [Fact]
    public async Task Build_WithMiddlewareThrowingException_ShouldAbortPipelineAndPropagate()
    {
        var log = new List<string>();
        var m1 = new LoggingMiddleware(log, "m1");

        var throwingMiddleware = Substitute.For<IEventMiddleware>();
        var expectedEx = new InvalidOperationException("Middleware crashed");
        throwingMiddleware.InvokeAsync(
            Arg.Any<TestMiddlewareEvent>(),
            Arg.Any<EventMiddlewareDelegate<TestMiddlewareEvent>>(),
            Arg.Any<CancellationToken>())
            .Returns<ValueTask>(_ => throw expectedEx);

        bool terminalCalled = false;
        EventMiddlewareDelegate<TestMiddlewareEvent> terminal = (_, _) =>
        {
            terminalCalled = true;
            return ValueTask.CompletedTask;
        };

        var pipeline = MiddlewarePipeline.Build(new[] { m1, throwingMiddleware }, terminal);
        var evt = new TestMiddlewareEvent(EventId.New(), DateTimeOffset.UtcNow);

        Func<Task> act = async () => await pipeline(evt, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(expectedEx);
        terminalCalled.Should().BeFalse();
        log.Should().Equal("m1:before");
    }

    [Fact]
    public async Task Build_WithCancelledCancellationToken_ShouldPropagateToMiddlewaresAndTerminal()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        CancellationToken capturedToken = default;
        EventMiddlewareDelegate<TestMiddlewareEvent> terminal = (_, ct) =>
        {
            capturedToken = ct;
            ct.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        };

        var pipeline = MiddlewarePipeline.Build(Array.Empty<IEventMiddleware>(), terminal);
        var evt = new TestMiddlewareEvent(EventId.New(), DateTimeOffset.UtcNow);

        Func<Task> act = async () => await pipeline(evt, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        capturedToken.IsCancellationRequested.Should().BeTrue();
    }
}




