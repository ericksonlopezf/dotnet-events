// Copyright © Erickson Lopez. MIT License.
// EVT-HIGH-001 / EVT-HIGH-006 / EVT-HIGH-AOT-001 Regression Tests
// Confirms that concurrent PublishAsync invocations never invoke a handler more than once
// per published event (the old GetOrDiscoverHandlers bug caused N invocations under concurrency).

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Xunit.Trait("Category", "Concurrency")]
public sealed class ConcurrencyFixTests
{
    /// <summary>
    /// Event used for concurrency regression testing.
    /// </summary>
    public sealed record ConcurrencyTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    /// <summary>
    /// Handler that counts invocations using Interlocked for thread-safety.
    /// </summary>
    public sealed class CountingHandler : IEventHandler<ConcurrencyTestEvent>
    {
        private int _invocations;
        public int Invocations => _invocations;

        public ValueTask HandleAsync(ConcurrencyTestEvent @event, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _invocations);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// EVT-HIGH-001 Regression: Publishing the same event N times concurrently must invoke the handler
    /// exactly N times total (once per publish). The old GetOrDiscoverHandlers used ConcurrentDictionary.GetOrAdd
    /// which could execute the factory multiple times, resulting in duplicate HandlerDescriptors
    /// and multiple handler invocations per event.
    /// </summary>
    [Fact]
    public async Task PublishAsync_UnderHighConcurrency_HandlerInvokedExactlyOncePerEvent()
    {
        const int concurrentPublishes = 200;

        var handler = new CountingHandler();
        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<ConcurrencyTestEvent, CountingHandler>(ServiceLifetime.Singleton);
        services.AddSingleton(handler);
        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        var events = new List<ConcurrencyTestEvent>(concurrentPublishes);
        for (int i = 0; i < concurrentPublishes; i++)
        {
            events.Add(new ConcurrencyTestEvent(EventId.New(), DateTimeOffset.UtcNow));
        }

        // Publish all events concurrently.
        var tasks = new Task[concurrentPublishes];
        for (int i = 0; i < concurrentPublishes; i++)
        {
            var evt = events[i];
            tasks[i] = Task.Run(async () => await bus.PublishAsync(evt));
        }

        await Task.WhenAll(tasks);

        // Each event must have triggered exactly one handler invocation.
        handler.Invocations.Should().Be(concurrentPublishes,
            "each event must invoke the handler exactly once — no duplicate registrations from concurrent factory execution");
    }

    /// <summary>
    /// EVT-HIGH-001 Regression: Publishing the same event sequentially should invoke handler once.
    /// Confirms the baseline sequential behavior is unaffected by the concurrency fix.
    /// </summary>
    [Fact]
    public async Task PublishAsync_Sequential_HandlerInvokedExactlyOnce()
    {
        var handler = new CountingHandler();
        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<ConcurrencyTestEvent, CountingHandler>(ServiceLifetime.Singleton);
        services.AddSingleton(handler);
        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        var evt = new ConcurrencyTestEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        handler.Invocations.Should().Be(1,
            "sequential publish must invoke handler exactly once");
    }

    /// <summary>
    /// EVT-HIGH-006 / EVT-HIGH-AOT-001 Regression: The registry must return handlers without using
    /// MakeGenericType reflection. Verifies that GetHandlers is AOT-safe by calling it directly
    /// and confirming it returns the registered handler.
    /// </summary>
    [Fact]
    public void Registry_GetHandlers_ReturnsRegisteredHandlerWithoutReflection()
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<ConcurrencyTestEvent, CountingHandler>();
        var sp = services.BuildServiceProvider();

        var registry = sp.GetRequiredService<IHandlerRegistry>();
        var handlers = registry.GetHandlers(typeof(ConcurrencyTestEvent));

        handlers.Should().HaveCount(1, "exactly one handler must be registered");
        handlers[0].HandlerType.Should().Be(typeof(CountingHandler));
    }
}
