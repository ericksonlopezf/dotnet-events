// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Inbox;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Inbox;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;
using Xunit;
using EventId = EricksonLopez.Events.Identifiers.EventId;

namespace EricksonLopez.Events.Inbox.Tests;

[Trait("Category", "Unit")]
public sealed class IdempotentEventHandlerTests
{
    public sealed record SampleOrderPlaced(EventId Id, string OrderNumber, DateTimeOffset OccurredAt) : IIntegrationEvent;

    public sealed record SampleEnvelopeEvent(EventId Id, EventType Type, EventVersion Version, DateTimeOffset OccurredAt, EventMetadata Metadata) : IEvent, IEventEnvelope
    {
        public object GetPayload() => this;
    }

    public sealed class SampleOrderPlacedHandler : IEventHandler<SampleOrderPlaced>
    {
        public bool Handled { get; private set; }

        public ValueTask HandleAsync(SampleOrderPlaced eventInstance, CancellationToken cancellationToken = default)
        {
            Handled = true;
            return ValueTask.CompletedTask;
        }
    }

    private readonly IEventHandler<SampleOrderPlaced> _innerHandler = Substitute.For<IEventHandler<SampleOrderPlaced>>();
    private readonly IInboxConsumerFilter _inboxFilter = Substitute.For<IInboxConsumerFilter>();
    private readonly ILogger<IdempotentEventHandler<SampleOrderPlaced>> _logger = Substitute.For<ILogger<IdempotentEventHandler<SampleOrderPlaced>>>();

    [Fact]
    public void Constructor_NullInnerHandler_ThrowsArgumentNullException()
    {
        Action act = () => new IdempotentEventHandler<SampleOrderPlaced>(null!, _inboxFilter);
        act.Should().Throw<ArgumentNullException>().WithParameterName("innerHandler");
    }

    [Fact]
    public void Constructor_NullInboxFilter_ThrowsArgumentNullException()
    {
        Action act = () => new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("inboxFilter");
    }

    [Fact]
    public void Constructor_DefaultConsumerName_SetsFallbackConsumerName()
    {
        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter);
        sut.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_NullEventInstance_ThrowsArgumentNullException()
    {
        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter);
        Func<Task> act = async () => await sut.HandleAsync(null!);
        await act.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HandleAsync_WhenHandledTrue_ExecutesIdempotentlyAndInvokesInnerHandler()
    {
        var evt = new SampleOrderPlaced(EventId.New(), "ORD-123", DateTimeOffset.UtcNow);

        Func<CallInfo, ValueTask<bool>> callback = async callInfo =>
        {
            var handler = callInfo.Arg<Func<CancellationToken, ValueTask>>();
            var ct = callInfo.Arg<CancellationToken>();
            await handler(ct);
            return true;
        };

        _inboxFilter.ExecuteIdempotentlyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            Arg.Any<CancellationToken>())
            .Returns(callback);

        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter, "CustomConsumer", _logger);

        await sut.HandleAsync(evt);

        await _inboxFilter.Received(1).ExecuteIdempotentlyAsync(
            Arg.Is<string>(id => id.Contains(nameof(SampleOrderPlaced))),
            "CustomConsumer",
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            CancellationToken.None);

        await _innerHandler.Received(1).HandleAsync(evt, CancellationToken.None);

        _logger.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithEnvelopeEvent_DerivesMessageIdFromEnvelopeId()
    {
        var eventId = EventId.New();
        var envelopeEvent = new SampleEnvelopeEvent(eventId, EventType.From("orders.placed"), EventVersion.V1, DateTimeOffset.UtcNow, EventMetadata.Empty);

        var innerEnvelopeHandler = Substitute.For<IEventHandler<SampleEnvelopeEvent>>();
        var sut = new IdempotentEventHandler<SampleEnvelopeEvent>(innerEnvelopeHandler, _inboxFilter, "EnvelopeConsumer");

        _inboxFilter.ExecuteIdempotentlyAsync(
            eventId.ToString(),
            "EnvelopeConsumer",
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(true));

        await sut.HandleAsync(envelopeEvent);

        await _inboxFilter.Received(1).ExecuteIdempotentlyAsync(
            eventId.ToString(),
            "EnvelopeConsumer",
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_WhenHandledFalse_LogsDuplicateSkippedMessage()
    {
        var evt = new SampleOrderPlaced(EventId.New(), "ORD-DUPLICATE", DateTimeOffset.UtcNow);

        _logger.IsEnabled(LogLevel.Debug).Returns(true);

        _inboxFilter.ExecuteIdempotentlyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(false));

        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter, "OrderConsumer", _logger);

        await sut.HandleAsync(evt);

        await _innerHandler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default);

        var logCalls = _logger.ReceivedCalls().Where(c => c.GetMethodInfo().Name == "Log").ToList();
        logCalls.Should().ContainSingle();
        var args = logCalls[0].GetArguments();
        args[0].Should().Be(LogLevel.Debug);
        var formatted = args[2]?.ToString();
        formatted.Should().Contain("was skipped as a duplicate by consumer 'OrderConsumer'");
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_PropagatesTokenToFilter()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var evt = new SampleOrderPlaced(EventId.New(), "ORD-TOKEN", DateTimeOffset.UtcNow);

        _inboxFilter.ExecuteIdempotentlyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            token)
            .Returns(ValueTask.FromResult(true));

        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter);

        await sut.HandleAsync(evt, token);

        await _inboxFilter.Received(1).ExecuteIdempotentlyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            token);
    }

    [Fact]
    public async Task HandleAsync_WhenInnerHandlerThrows_PropagatesException()
    {
        var evt = new SampleOrderPlaced(EventId.New(), "ORD-ERR", DateTimeOffset.UtcNow);

        Func<CallInfo, ValueTask<bool>> callback = async callInfo =>
        {
            var handler = callInfo.Arg<Func<CancellationToken, ValueTask>>();
            await handler(CancellationToken.None);
            return true;
        };

        _inboxFilter.ExecuteIdempotentlyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            Arg.Any<CancellationToken>())
            .Returns(callback);

        _innerHandler.HandleAsync(evt, Arg.Any<CancellationToken>())
            .Returns<ValueTask>(_ => throw new InvalidOperationException("Handler execution failed"));

        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter);

        Func<Task> act = async () => await sut.HandleAsync(evt);

        await act.Should().ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Handler execution failed");
    }

    [Fact]
    public async Task HandleAsync_WhenInboxFilterThrows_PropagatesExceptionAndDoesNotInvokeInnerHandler()
    {
        var evt = new SampleOrderPlaced(EventId.New(), "ORD-FILTER-ERR", DateTimeOffset.UtcNow);
        var filterException = new TimeoutException("Database connection timeout during inbox deduplication check");

        _inboxFilter.ExecuteIdempotentlyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask>>(),
            Arg.Any<CancellationToken>())
            .Returns<ValueTask<bool>>(_ => throw filterException);

        var sut = new IdempotentEventHandler<SampleOrderPlaced>(_innerHandler, _inboxFilter);

        Func<Task> act = async () => await sut.HandleAsync(evt);

        var thrown = await act.Should().ThrowExactlyAsync<TimeoutException>();
        thrown.Which.Should().BeSameAs(filterException);
        await _innerHandler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default);
    }
}
