// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Outbox;
using EricksonLopez.Outbox.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace EricksonLopez.Events.Outbox.Tests;

[Trait("Category", "Unit")]
public sealed class OutboxEventPublisherTests
{
    private readonly IOutbox _outbox = Substitute.For<IOutbox>();
    private readonly IOutboxTransactionProvider _transactionProvider = Substitute.For<IOutboxTransactionProvider>();
    private readonly IOutboxTransactionContext _txContext = Substitute.For<IOutboxTransactionContext>();

    private sealed record OrderPlacedEvent(string OrderId, decimal Amount) : IIntegrationEvent
    {
        public EventId Id { get; init; } = EventId.New();
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    }

    [Fact]
    public void Constructor_WhenOutboxNull_ThrowsArgumentNullException()
    {
        var act = () => new OutboxEventPublisher(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_WhenEventNull_ThrowsArgumentNullException()
    {
        var sut = new OutboxEventPublisher(_outbox, _transactionProvider);
        Func<Task> act = async () => await sut.PublishAsync<OrderPlacedEvent>(null!);
        await act.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_WhenNoTransactionContext_ThrowsInvalidOperationException()
    {
        _transactionProvider.CurrentTransaction.Returns((IOutboxTransactionContext?)null);
        var sut = new OutboxEventPublisher(_outbox, _transactionProvider);
        var evt = new OrderPlacedEvent("ORD-123", 99.99m);

        Func<Task> act = async () => await sut.PublishAsync(evt);

        var thrown = await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        thrown.WithMessage($"Cannot store event '{nameof(OrderPlacedEvent)}' ({evt.Id}) into the outbox because no active transaction context was provided by '{_transactionProvider.GetType().Name}'.");
    }

    [Fact]
    public async Task PublishAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var sut = new OutboxEventPublisher(_outbox, _transactionProvider);
        var evt = new OrderPlacedEvent("ORD-CANCELLED", 10.0m);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () => await sut.PublishAsync(evt, cts.Token);

        await act.Should().ThrowExactlyAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PublishAsync_WithCancelledToken_EvenIfTransactionContextNull_ThrowsOperationCanceledExceptionBeforeCheckingTransaction()
    {
        _transactionProvider.CurrentTransaction.Returns((IOutboxTransactionContext?)null);
        var sut = new OutboxEventPublisher(_outbox, _transactionProvider);
        var evt = new OrderPlacedEvent("ORD-CANCELLED-NOTX", 10.0m);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () => await sut.PublishAsync(evt, cts.Token);

        await act.Should().ThrowExactlyAsync<OperationCanceledException>();
        _outbox.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_WhenTransactionContextActive_StoresEventInOutbox()
    {
        _transactionProvider.CurrentTransaction.Returns(_txContext);
        var sut = new OutboxEventPublisher(_outbox, _transactionProvider);
        var evt = new OrderPlacedEvent("ORD-123", 99.99m);
        using var cts = new CancellationTokenSource();

        await sut.PublishAsync(evt, cts.Token);

        await _outbox.Received(1).StoreAsync(
            evt,
            _txContext,
            Arg.Is<OutboxMessageMetadata>(m => m.MessageType == typeof(OrderPlacedEvent).FullName),
            null,
            cts.Token);
    }

    [Fact]
    public async Task PublishAsync_WhenOutboxStoreThrows_ShouldPropagateExceptionWithoutModifying()
    {
        _transactionProvider.CurrentTransaction.Returns(_txContext);
        var expectedException = new InvalidOperationException("Persistence failure during store operation");
        _outbox.StoreAsync(
            Arg.Any<object>(),
            Arg.Any<IOutboxTransactionContext>(),
            Arg.Any<OutboxMessageMetadata>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValueTask(Task.FromException(expectedException)));

        var sut = new OutboxEventPublisher(_outbox, _transactionProvider);
        var evt = new OrderPlacedEvent("ORD-ERR-999", 50.0m);

        Func<Task> act = async () => await sut.PublishAsync(evt);

        var thrown = await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(expectedException);
    }

    [Fact]
    public void AddOutboxEventPublisher_WhenServicesNull_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;
        var act = () => services.AddOutboxEventPublisher();

        var ex = act.Should().ThrowExactly<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddOutboxEventPublisher_WithCustomProvider_WhenServicesNull_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;
        var act = () => services.AddOutboxEventPublisher<CustomTransactionProvider>();

        var ex = act.Should().ThrowExactly<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddOutboxEventPublisher_RegistersServiceInServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_outbox);

        services.AddOutboxEventPublisher();

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetService<IEventPublisher>();
        var txProvider = provider.GetService<IOutboxTransactionProvider>();

        publisher.Should().NotBeNull();
        publisher.Should().BeOfType<OutboxEventPublisher>();
        txProvider.Should().NotBeNull();
        txProvider.Should().BeOfType<NullOutboxTransactionProvider>();
    }

    private sealed class CustomTransactionProvider : IOutboxTransactionProvider
    {
        public IOutboxTransactionContext? CurrentTransaction => null;
    }

    [Fact]
    public void AddOutboxEventPublisher_WithCustomProvider_RegistersCustomProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_outbox);

        services.AddOutboxEventPublisher<CustomTransactionProvider>();

        var provider = services.BuildServiceProvider();
        var txProvider = provider.GetService<IOutboxTransactionProvider>();
        var publisher = provider.GetService<IEventPublisher>();

        txProvider.Should().NotBeNull();
        txProvider.Should().BeOfType<CustomTransactionProvider>();
        publisher.Should().NotBeNull();
        publisher.Should().BeOfType<OutboxEventPublisher>();
    }

    [Fact]
    public void NullOutboxTransactionProvider_Instance_ShouldReturnSingletonAndNullTransaction()
    {
        var provider = NullOutboxTransactionProvider.Instance;
        provider.Should().NotBeNull();
        provider.CurrentTransaction.Should().BeNull();
        NullOutboxTransactionProvider.Instance.Should().BeSameAs(provider);
    }

    [Fact]
    public async Task Constructor_WithSingleOutboxParameter_UsesNullOutboxTransactionProviderByDefault()
    {
        var sut = new OutboxEventPublisher(_outbox);
        var evt = new OrderPlacedEvent("ORD-DEFAULT-PROVIDER", 15.5m);

        Func<Task> act = async () => await sut.PublishAsync(evt);

        // Since NullOutboxTransactionProvider returns null for CurrentTransaction, PublishAsync throws InvalidOperationException
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }
}
