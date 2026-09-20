// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Lifecycle;

[Trait("Category", "Adversarial")]
public sealed class SourceGeneratedHandlerResolutionStormTests
{
    public sealed record StormEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class StormHandler1 : IEventHandler<StormEvent>, IDisposable
    {
        public static int InstancesCreated { get; set; }
        public static int Invocations { get; set; }
        public StormHandler1() => InstancesCreated++;
        public ValueTask HandleAsync(StormEvent eventInstance, CancellationToken cancellationToken = default)
        {
            Invocations++;
            return ValueTask.CompletedTask;
        }
        public void Dispose() { }
    }

    public sealed class StormHandler2 : IEventHandler<StormEvent>, IDisposable
    {
        public static int InstancesCreated { get; set; }
        public static int Invocations { get; set; }
        public StormHandler2() => InstancesCreated++;
        public ValueTask HandleAsync(StormEvent eventInstance, CancellationToken cancellationToken = default)
        {
            Invocations++;
            return ValueTask.CompletedTask;
        }
        public void Dispose() { }
    }

    public sealed class StormHandler3 : IEventHandler<StormEvent>, IDisposable
    {
        public static int InstancesCreated { get; set; }
        public static int Invocations { get; set; }
        public StormHandler3() => InstancesCreated++;
        public ValueTask HandleAsync(StormEvent eventInstance, CancellationToken cancellationToken = default)
        {
            Invocations++;
            return ValueTask.CompletedTask;
        }
        public void Dispose() { }
    }

    [Fact]
    public async Task EVT_LFC_001_WhenHandlersRegisteredAsInterfaceOnly_CausesN2ZombieInstantiationStorm()
    {
        // Reset counters
        StormHandler1.InstancesCreated = 0;
        StormHandler1.Invocations = 0;
        StormHandler2.InstancesCreated = 0;
        StormHandler2.Invocations = 0;
        StormHandler3.InstancesCreated = 0;
        StormHandler3.Invocations = 0;

        // Reproduce exactly what EventIncrementalGenerator emits:
        // services.Add(new ServiceDescriptor(typeof(IEventHandler<TEvent>), typeof(THandler), lifetime));
        // Note: It does NOT register typeof(THandler) directly!
        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ScopePolicy = HandlerScopePolicy.CreatePerHandler;
        });

        // 3 handlers registered via interface only (source generator pattern):
        services.AddTransient<IEventHandler<StormEvent>, StormHandler1>();
        services.AddSingleton(new HandlerRegistrationToken(
            typeof(StormEvent),
            new HandlerDescriptor(typeof(StormHandler1), typeof(IEventHandler<StormEvent>), (inst, evt, ct) => ((IEventHandler<StormEvent>)inst).HandleAsync((StormEvent)evt, ct))));

        services.AddTransient<IEventHandler<StormEvent>, StormHandler2>();
        services.AddSingleton(new HandlerRegistrationToken(
            typeof(StormEvent),
            new HandlerDescriptor(typeof(StormHandler2), typeof(IEventHandler<StormEvent>), (inst, evt, ct) => ((IEventHandler<StormEvent>)inst).HandleAsync((StormEvent)evt, ct))));

        services.AddTransient<IEventHandler<StormEvent>, StormHandler3>();
        services.AddSingleton(new HandlerRegistrationToken(
            typeof(StormEvent),
            new HandlerDescriptor(typeof(StormHandler3), typeof(IEventHandler<StormEvent>), (inst, evt, ct) => ((IEventHandler<StormEvent>)inst).HandleAsync((StormEvent)evt, ct))));

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // ACT: Publish a single event
        await bus.PublishAsync(new StormEvent(EventId.New(), DateTimeOffset.UtcNow));

        // ASSERT: Each handler was invoked exactly ONCE:
        StormHandler1.Invocations.Should().Be(1);
        StormHandler2.Invocations.Should().Be(1);
        StormHandler3.Invocations.Should().Be(1);

        // CRITICAL DEFECT DEMONSTRATION:
        // Because HandlerResolutionHelper falls back to GetService<IEnumerable<IEventHandler<TEvent>>>()
        // inside each handler's resolution:
        // For 3 handlers, resolution of Handler 1 creates [H1, H2, H3]
        // Resolution of Handler 2 creates [H1, H2, H3]
        // Resolution of Handler 3 creates [H1, H2, H3]
        // Total instances created = 3 * 3 = 9 instances instead of 3!
        int totalInstances = StormHandler1.InstancesCreated + StormHandler2.InstancesCreated + StormHandler3.InstancesCreated;
        totalInstances.Should().Be(9,
            "EVT-LFC-001 Defect: HandlerResolutionHelper creates N^2 handler instances (9 instead of 3)!");
    }
}
