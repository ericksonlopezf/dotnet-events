// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Reliability;

[Trait("Category", "Adversarial")]
public sealed class ExecutionStrategyContractDivergenceTests
{
    public sealed record DivergenceEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class CustomBusinessException : Exception
    {
        public CustomBusinessException(string message) : base(message) { }
    }

    public sealed class FailingHandler : IEventHandler<DivergenceEvent>
    {
        public ValueTask HandleAsync(DivergenceEvent eventInstance, CancellationToken cancellationToken = default)
        {
            throw new CustomBusinessException("Business rule violated");
        }
    }

    public sealed class SecondHandler : IEventHandler<DivergenceEvent>
    {
        public static bool Executed { get; set; }

        public ValueTask HandleAsync(DivergenceEvent eventInstance, CancellationToken cancellationToken = default)
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }

        public static void Reset() => Executed = false;
    }

    public sealed class OperationCanceledHandler : IEventHandler<DivergenceEvent>
    {
        public ValueTask HandleAsync(DivergenceEvent eventInstance, CancellationToken cancellationToken = default)
        {
            // Simulate an internal HTTP timeout that throws OperationCanceledException with an internal token
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            throw new OperationCanceledException(cts.Token);
        }
    }

    [Fact]
    public async Task FailFast_SequentialThrowsRawException_WhereasParallelThrowsEventDispatchException()
    {
        // Sequential Bus
        var seqServices = new ServiceCollection();
        seqServices.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ErrorPolicy = ErrorHandlingPolicy.FailFast;
        });
        seqServices.AddEventHandler<DivergenceEvent, FailingHandler>();
        var seqSp = seqServices.BuildServiceProvider();
        var seqBus = seqSp.GetRequiredService<IEventBus>();

        // Parallel Bus with 2 handlers
        var parServices = new ServiceCollection();
        parServices.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Parallel;
            opts.ErrorPolicy = ErrorHandlingPolicy.FailFast;
        });
        parServices.AddEventHandler<DivergenceEvent, FailingHandler>();
        parServices.AddEventHandler<DivergenceEvent, SecondHandler>();
        var parSp = parServices.BuildServiceProvider();
        var parBus = parSp.GetRequiredService<IEventBus>();

        var evt = new DivergenceEvent(EventId.New(), DateTimeOffset.UtcNow);

        // Sequential throws raw CustomBusinessException
        Func<Task> seqAct = async () => await seqBus.PublishAsync(evt);
        await seqAct.Should().ThrowAsync<CustomBusinessException>();

        // Parallel under FailFast also throws raw CustomBusinessException (EVT-REL-001 harmonized)
        Func<Task> parAct = async () => await parBus.PublishAsync(evt);
        await parAct.Should().ThrowAsync<CustomBusinessException>();
    }

    [Fact]
    public async Task SequentialMode_AggregateAndContinue_InternalHandlerTimeout_MustNotDropSubsequentHandlers()
    {
        SecondHandler.Reset();

        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue;
        });
        services.AddEventHandler<DivergenceEvent, OperationCanceledHandler>();
        services.AddEventHandler<DivergenceEvent, SecondHandler>();
        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        var evt = new DivergenceEvent(EventId.New(), DateTimeOffset.UtcNow);

        // Act: Handler 1 throws OperationCanceledException (from internal timeout).
        // Under AggregateAndContinue, the caller passed CancellationToken.None! The caller did NOT cancel!
        Func<Task> act = async () => await bus.PublishAsync(evt, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<EventDispatchException>();
        ex.Which.InnerExceptions.Should().ContainSingle(e => e is OperationCanceledException);
        SecondHandler.Executed.Should().BeTrue("Downstream handlers must execute under AggregateAndContinue even if upstream handler throws internal OperationCanceledException.");
    }
}
