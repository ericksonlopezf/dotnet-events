// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Concurrency;

[Trait("Category", "Concurrency")]
public sealed class ConcurrencyAndScopeTests
{
    public sealed record StockReservedEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class ScopedStateService
    {
        private int _concurrencyCount;
        public int MaxConcurrentObserved { get; private set; }

        public async Task SimulateDbWorkAsync()
        {
            int count = Interlocked.Increment(ref _concurrencyCount);
            lock (this)
            {
                if (count > MaxConcurrentObserved) MaxConcurrentObserved = count;
            }
            await Task.Delay(20);
            Interlocked.Decrement(ref _concurrencyCount);
        }
    }

    public sealed class ScopedHandlerA : IEventHandler<StockReservedEvent>
    {
        private readonly ScopedStateService _state;
        public ScopedHandlerA(ScopedStateService state) => _state = state;

        public async ValueTask HandleAsync(StockReservedEvent @event, CancellationToken cancellationToken = default) =>
            await _state.SimulateDbWorkAsync();
    }

    public sealed class ScopedHandlerB : IEventHandler<StockReservedEvent>
    {
        private readonly ScopedStateService _state;
        public ScopedHandlerB(ScopedStateService state) => _state = state;

        public async ValueTask HandleAsync(StockReservedEvent @event, CancellationToken cancellationToken = default) =>
            await _state.SimulateDbWorkAsync();
    }

    [Fact]
    public async Task ParallelExecutionStrategy_WithScopedHandlers_IsolatesScopedInstancesConcurrently()
    {
        // Validates EVT-CONC-001 Remediation:
        // ParallelExecutionStrategy creates an isolated IServiceScope per concurrent handler task.
        // Handlers no longer collide on the same ScopedStateService instance.
        var services = new ServiceCollection();
        services.AddScoped<ScopedStateService>();
        services.AddEventBus(opts => opts.ExecutionMode = EventExecutionMode.Parallel);
        services.AddEventHandler<StockReservedEvent, ScopedHandlerA>(ServiceLifetime.Scoped);
        services.AddEventHandler<StockReservedEvent, ScopedHandlerB>(ServiceLifetime.Scoped);

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<ScopedStateService>();
        var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new StockReservedEvent(EventId.New(), DateTimeOffset.UtcNow));

        // The parent scope's service was never touched concurrently by the parallel handlers!
        scopedService.MaxConcurrentObserved.Should().Be(0,
            "ParallelExecutionStrategy creates isolated child scopes per handler, preventing concurrent access to scoped dependencies.");
    }

    [Fact]
    public async Task ParallelExecutionStrategy_RespectsErrorPolicyFailFast()
    {
        // Validates EVT-REL-001 Remediation:
        // ParallelExecutionStrategy honors ErrorHandlingPolicy.FailFast and rethrows the original exception directly.
        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Parallel;
            opts.ErrorPolicy = ErrorHandlingPolicy.FailFast;
        });

        services.AddEventHandler<StockReservedEvent, FailingHandler>(ServiceLifetime.Transient);
        services.AddEventHandler<StockReservedEvent, ScopedHandlerA>(ServiceLifetime.Transient);
        services.AddTransient<ScopedStateService>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        Func<Task> act = async () => await bus.PublishAsync(new StockReservedEvent(EventId.New(), DateTimeOffset.UtcNow));

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Simulated handler crash");
    }

    private sealed class FailingHandler : IEventHandler<StockReservedEvent>
    {
        public async ValueTask HandleAsync(StockReservedEvent @event, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            throw new InvalidOperationException("Simulated handler crash");
        }
    }

    [Fact]
    public async Task Concurrency_Stress_100PublishersConcurrently_EventBus_ShouldMaintainState()
    {
        // Concurrency test: 100 concurrent tasks publishing through EventBus
        var services = new ServiceCollection();
        var counter = new ConcurrentBag<Guid>();
        services.AddSingleton(counter);
        services.AddEventBus();
        services.AddEventHandler<StockReservedEvent, ConcurrentCountingHandler>(ServiceLifetime.Transient);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        var tasks = new Task[100];
        for (int i = 0; i < 100; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                var evt = new StockReservedEvent(EventId.New(), DateTimeOffset.UtcNow);
                await bus.PublishAsync(evt);
            });
        }

        await Task.WhenAll(tasks);
        counter.Count.Should().Be(100);
    }

    private sealed class ConcurrentCountingHandler : IEventHandler<StockReservedEvent>
    {
        private readonly ConcurrentBag<Guid> _bag;
        public ConcurrentCountingHandler(ConcurrentBag<Guid> bag) => _bag = bag;

        public ValueTask HandleAsync(StockReservedEvent @event, CancellationToken cancellationToken = default)
        {
            _bag.Add(@event.Id.Value);
            return ValueTask.CompletedTask;
        }
    }
}
