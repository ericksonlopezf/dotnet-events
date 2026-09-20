// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Lifecycle;

[Trait("Category", "Adversarial")]
public sealed class HandlerScopeLifecycleTests
{
    public sealed record OrderPlacedEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class ScopedSession
    {
        public Guid SessionId { get; } = Guid.NewGuid();
    }

    public sealed class OrderHandler1 : IEventHandler<OrderPlacedEvent>
    {
        public ScopedSession Session { get; }
        public static Guid LastObservedSessionId { get; set; }

        public OrderHandler1(ScopedSession session)
        {
            Session = session;
        }

        public ValueTask HandleAsync(OrderPlacedEvent eventInstance, CancellationToken cancellationToken = default)
        {
            LastObservedSessionId = Session.SessionId;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class OrderHandler2 : IEventHandler<OrderPlacedEvent>
    {
        public ScopedSession Session { get; }
        public static Guid LastObservedSessionId { get; set; }

        public OrderHandler2(ScopedSession session)
        {
            Session = session;
        }

        public ValueTask HandleAsync(OrderPlacedEvent eventInstance, CancellationToken cancellationToken = default)
        {
            LastObservedSessionId = Session.SessionId;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task ParallelExecutionStrategy_ConsistentlyCreatesIsolatedScopes_ForBothOneAndMultipleHandlers()
    {
        // Validates EVT-LIF-001 Remediation:
        // ParallelExecutionStrategy consistently isolates scoped dependencies in child scopes,
        // eliminating the asymmetric single-handler ambient leak bug.

        // Setup 1: 1 handler registered
        var services1 = new ServiceCollection();
        services1.AddScoped<ScopedSession>();
        services1.AddEventBus(opts => opts.ExecutionMode = EventExecutionMode.Parallel);
        services1.AddEventHandler<OrderPlacedEvent, OrderHandler1>(ServiceLifetime.Scoped);

        var sp1 = services1.BuildServiceProvider();
        using (var callerScope = sp1.CreateScope())
        {
            var callerSession = callerScope.ServiceProvider.GetRequiredService<ScopedSession>();
            var bus = callerScope.ServiceProvider.GetRequiredService<IEventBus>();

            await bus.PublishAsync(new OrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow));

            // Now with EVT-LIF-001 fixed: Even with 1 handler in Parallel mode, an isolated scope is created!
            OrderHandler1.LastObservedSessionId.Should().NotBe(callerSession.SessionId,
                "EVT-LIF-001 Remediation: Parallel mode consistently isolates handlers in dedicated child scopes.");
        }

        // Setup 2: 2 handlers registered
        var services2 = new ServiceCollection();
        services2.AddScoped<ScopedSession>();
        services2.AddEventBus(opts => opts.ExecutionMode = EventExecutionMode.Parallel);
        services2.AddEventHandler<OrderPlacedEvent, OrderHandler1>(ServiceLifetime.Scoped);
        services2.AddEventHandler<OrderPlacedEvent, OrderHandler2>(ServiceLifetime.Scoped);

        var sp2 = services2.BuildServiceProvider();
        using (var callerScope = sp2.CreateScope())
        {
            var callerSession = callerScope.ServiceProvider.GetRequiredService<ScopedSession>();
            var bus = callerScope.ServiceProvider.GetRequiredService<IEventBus>();

            await bus.PublishAsync(new OrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow));

            OrderHandler1.LastObservedSessionId.Should().NotBe(callerSession.SessionId);
            OrderHandler2.LastObservedSessionId.Should().NotBe(callerSession.SessionId);
        }
    }

    [Fact]
    public async Task SequentialExecutionStrategy_WhenScopePolicyReuseAmbientScope_ReusesCallerScope()
    {
        // When explicitly configuring HandlerScopePolicy.ReuseAmbientScope in Sequential mode:
        var services = new ServiceCollection();
        services.AddScoped<ScopedSession>();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ScopePolicy = HandlerScopePolicy.ReuseAmbientScope;
        });
        services.AddEventHandler<OrderPlacedEvent, OrderHandler1>(ServiceLifetime.Scoped);

        var sp = services.BuildServiceProvider();
        using var callerScope = sp.CreateScope();
        var callerSession = callerScope.ServiceProvider.GetRequiredService<ScopedSession>();
        var bus = callerScope.ServiceProvider.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new OrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow));

        OrderHandler1.LastObservedSessionId.Should().Be(callerSession.SessionId,
            "When ScopePolicy is ReuseAmbientScope in Sequential mode, the caller's ambient session is reused.");
    }

    [Fact]
    public async Task SequentialExecutionStrategy_WhenCreatePerHandler_CreatesChildScopePerHandler()
    {
        // Validates EVT-LIF-001 Remediation for Sequential Execution:
        // Sequential mode can now create isolated child scopes when configured with HandlerScopePolicy.CreatePerHandler.
        var services = new ServiceCollection();
        services.AddScoped<ScopedSession>();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ScopePolicy = HandlerScopePolicy.CreatePerHandler;
        });
        services.AddEventHandler<OrderPlacedEvent, OrderHandler1>(ServiceLifetime.Scoped);

        var sp = services.BuildServiceProvider();
        using var callerScope = sp.CreateScope();
        var callerSession = callerScope.ServiceProvider.GetRequiredService<ScopedSession>();
        var bus = callerScope.ServiceProvider.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new OrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow));

        OrderHandler1.LastObservedSessionId.Should().NotBe(callerSession.SessionId,
            "When ScopePolicy is CreatePerHandler, Sequential execution creates an isolated child scope.");
    }

    [Fact]
    public async Task SequentialExecutionStrategy_AlwaysReusesCallerScope_AcrossAllHandlers_ByDefaultForSequential()
    {
        // Default behavior for Sequential mode is ReuseAmbientScope (matching standard in-process mediation).
        var services = new ServiceCollection();
        services.AddScoped<ScopedSession>();
        services.AddEventBus(opts => opts.ExecutionMode = EventExecutionMode.Sequential);
        services.AddEventHandler<OrderPlacedEvent, OrderHandler1>(ServiceLifetime.Scoped);
        services.AddEventHandler<OrderPlacedEvent, OrderHandler2>(ServiceLifetime.Scoped);

        var sp = services.BuildServiceProvider();
        using (var callerScope = sp.CreateScope())
        {
            var callerSession = callerScope.ServiceProvider.GetRequiredService<ScopedSession>();
            var bus = callerScope.ServiceProvider.GetRequiredService<IEventBus>();

            await bus.PublishAsync(new OrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow));

            OrderHandler1.LastObservedSessionId.Should().Be(callerSession.SessionId);
            OrderHandler2.LastObservedSessionId.Should().Be(callerSession.SessionId);
        }
    }
}
