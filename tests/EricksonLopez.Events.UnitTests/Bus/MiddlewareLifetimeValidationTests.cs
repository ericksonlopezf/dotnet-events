// Copyright © Erickson Lopez. MIT License.
// EVT-HIGH-004 Regression Tests — Middleware Singleton Lifetime Validation
// Confirms that registering a Singleton middleware with mutable instance fields throws,
// while stateless or AsyncLocal-based middlewares are allowed.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using AwesomeAssertions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class MiddlewareLifetimeValidationTests
{
    // ── Mutable instance field ── Singleton NOT allowed ──────────────────────
    private sealed class MutableStateMiddleware : IEventMiddleware
    {
        private int _counter; // mutable, non-AsyncLocal → unsafe as Singleton

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> next, CancellationToken ct)
            where TEvent : IEvent
        {
            _counter++;
            await next(eventInstance, ct);
        }
    }

    // ── Another mutable field type (non-readonly) ── also NOT allowed as Singleton ──────────
    private sealed class MutableListMiddleware : IEventMiddleware
    {
        // Non-readonly mutable field — the reference itself can be reassigned → unsafe as Singleton.
        // Note: a `readonly List<string>` would be IsInitOnly=true and correctly skipped by the validator.
        private System.Collections.Generic.List<string>? _log; // non-readonly, replaceable field

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> next, CancellationToken ct)
            where TEvent : IEvent
        {
            _log ??= new();
            _log.Add(typeof(TEvent).Name);
            await next(eventInstance, ct);
        }
    }


    // ── AsyncLocal field only ── Singleton IS allowed ─────────────────────────
    private sealed class AsyncLocalMiddleware : IEventMiddleware
    {
        private readonly AsyncLocal<int> _depth = new(); // safe — per-async-context

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> next, CancellationToken ct)
            where TEvent : IEvent
        {
            _depth.Value++;
            await next(eventInstance, ct);
            _depth.Value--;
        }
    }

    // ── No instance fields ── Singleton IS allowed ────────────────────────────
    private sealed class StatelessMiddleware : IEventMiddleware
    {
        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> next, CancellationToken ct)
            where TEvent : IEvent
        {
            await next(eventInstance, ct);
        }
    }

    // ── Readonly-only field (immutable) ── Singleton IS allowed ───────────────
    private sealed class ReadonlyFieldMiddleware : IEventMiddleware
    {
        private readonly string _name = "test"; // readonly → safe

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> next, CancellationToken ct)
            where TEvent : IEvent
        {
            _ = _name;
            await next(eventInstance, ct);
        }
    }

    /// <summary>
    /// EVT-HIGH-004 Regression: Middleware with a mutable (non-readonly, non-AsyncLocal) instance field
    /// must be rejected when registered as Singleton.
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Singleton_WithMutableIntField_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<MutableStateMiddleware>(ServiceLifetime.Singleton);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be registered as Singleton*mutable instance field*_counter*");
    }

    /// <summary>
    /// EVT-HIGH-004: Mutable reference-type fields (e.g., List) are also rejected as Singleton.
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Singleton_WithMutableListField_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<MutableListMiddleware>(ServiceLifetime.Singleton);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be registered as Singleton*mutable instance field*");
    }

    /// <summary>
    /// EVT-HIGH-004: AsyncLocal-based middleware is explicitly allowed as Singleton.
    /// AsyncLocal provides per-async-context isolation even when shared.
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Singleton_WithAsyncLocalField_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<AsyncLocalMiddleware>(ServiceLifetime.Singleton);
        act.Should().NotThrow(
            "AsyncLocal<T> fields are safe for Singleton because each async context has its own value");
    }

    /// <summary>
    /// EVT-HIGH-004: Stateless middleware (no fields) is always safe as Singleton.
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Singleton_WithNoFields_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<StatelessMiddleware>(ServiceLifetime.Singleton);
        act.Should().NotThrow("stateless middleware has no instance state and is always thread-safe");
    }

    /// <summary>
    /// EVT-HIGH-004: Middleware with only readonly fields is safe as Singleton (immutable state).
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Singleton_WithReadonlyFieldOnly_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<ReadonlyFieldMiddleware>(ServiceLifetime.Singleton);
        act.Should().NotThrow("readonly fields are immutable and cannot cause data races");
    }

    /// <summary>
    /// Transient lifetime is always allowed, regardless of state.
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Transient_WithMutableField_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<MutableStateMiddleware>(ServiceLifetime.Transient);
        act.Should().NotThrow("Transient middleware gets a new instance per resolve — no sharing");
    }

    /// <summary>
    /// Scoped lifetime is always allowed, regardless of state.
    /// </summary>
    [Fact]
    public void AddEventMiddleware_Scoped_WithMutableField_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEventMiddleware<MutableStateMiddleware>(ServiceLifetime.Scoped);
        act.Should().NotThrow("Scoped middleware is isolated per DI scope — no cross-request sharing");
    }
}
