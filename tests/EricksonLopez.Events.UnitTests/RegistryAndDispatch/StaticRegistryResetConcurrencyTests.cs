// Copyright © Erickson Lopez. MIT License.
// EVT-HIGH-003 Regression Tests
// Confirms that StaticEventTypeRegistry.Reset() is atomic — no race condition between Reset
// and a concurrent SetCurrent call that could throw "already initialized" spuriously.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.RegistryAndDispatch;

using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Registry;
using Xunit;

[Collection("DiagnosticsAndStaticRegistry")]
[Xunit.Trait("Category", "Concurrency")]
public sealed class StaticRegistryResetConcurrencyTests : IDisposable
{
    public sealed record ResetTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public void Dispose()
    {
        // Always clean up after each test
        try { StaticEventTypeRegistry.Reset(); } catch { }
    }

    /// <summary>
    /// EVT-HIGH-003 Regression: Reset() followed immediately by SetCurrent() on another thread
    /// must NOT throw "already initialized". The fix reverses the order: Interlocked.Exchange to 0
    /// BEFORE setting s_current = Empty, so SetCurrent sees s_isInitialized=0 and proceeds safely.
    /// </summary>
    [Fact]
    public async Task Reset_ConcurrentWithSetCurrent_NeverThrowsAlreadyInitializedException()
    {
        const int iterations = 100;
        var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        for (int i = 0; i < iterations; i++)
        {
            // Ensure initialized state before each iteration
            StaticEventTypeRegistry.Reset();
            StaticEventTypeRegistry.SetCurrent(EventTypeRegistry.Empty, allowOverride: true);

            var resetTask = Task.Run(() =>
            {
                try { StaticEventTypeRegistry.Reset(); }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already initialized"))
                {
                    errors.Add(ex);
                }
            });

            var setCurrentTask = Task.Run(() =>
            {
                try { StaticEventTypeRegistry.SetCurrent(EventTypeRegistry.Empty, allowOverride: true); }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already initialized"))
                {
                    errors.Add(ex);
                }
            });

            await Task.WhenAll(resetTask, setCurrentTask);
        }

        errors.Should().BeEmpty(
            "Reset() must atomically reset the initialized flag before clearing s_current, " +
            "so concurrent SetCurrent() never sees a spurious 'already initialized' state");
    }

    /// <summary>
    /// EVT-HIGH-003 Regression: After Reset(), SetCurrent must succeed without allowOverride=true.
    /// The fixed Reset() leaves the registry in a clean state where the initialization flag is 0.
    /// </summary>
    [Fact]
    public void Reset_ThenSetCurrent_WithoutAllowOverride_Succeeds()
    {
        StaticEventTypeRegistry.Reset();

        // This must NOT throw — Reset() cleared the initialized flag first.
        var act = () => StaticEventTypeRegistry.SetCurrent(EventTypeRegistry.Empty, allowOverride: false);
        act.Should().NotThrow<InvalidOperationException>(
            "after Reset(), s_isInitialized is 0, so SetCurrent with allowOverride=false must succeed");
    }

    /// <summary>
    /// Basic: Reset leaves the registry in the Empty state.
    /// </summary>
    [Fact]
    public void Reset_LeavesRegistryInEmptyState()
    {
        StaticEventTypeRegistry.SetCurrent(EventTypeRegistry.Empty, allowOverride: true);
        StaticEventTypeRegistry.Reset();

        StaticEventTypeRegistry.Current.Should().BeSameAs(EventTypeRegistry.Empty,
            "after Reset(), Current must be the Empty registry");
    }
}
