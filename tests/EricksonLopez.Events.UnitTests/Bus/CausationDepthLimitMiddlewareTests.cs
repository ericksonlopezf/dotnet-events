// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Bus;

[Trait("Category", "Unit")]
public sealed class CausationDepthLimitMiddlewareTests
{
    private sealed record TestDepthEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [Fact]
    public void Constructor_WithInvalidMaxDepth_ThrowsArgumentOutOfRangeException()
    {
        var actZero = () => new CausationDepthLimitMiddleware(0);
        actZero.Should().Throw<ArgumentOutOfRangeException>();

        var actNeg = () => new CausationDepthLimitMiddleware(-5);
        actNeg.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ThrowsArgumentNullException()
    {
        var middleware = new CausationDepthLimitMiddleware(10);
        var evt = new TestDepthEvent(EventId.New(), DateTimeOffset.UtcNow);

        var act = async () => await middleware.InvokeAsync(evt, null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoEnvelopeContext_ProceedsToNext()
    {
        var middleware = new CausationDepthLimitMiddleware(10);
        var evt = new TestDepthEvent(EventId.New(), DateTimeOffset.UtcNow);
        bool nextCalled = false;

        await middleware.InvokeAsync(evt, (@event, ct) =>
        {
            nextCalled = true;
            return ValueTask.CompletedTask;
        }, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenDepthIsBelowMax_ProceedsToNext()
    {
        var middleware = new CausationDepthLimitMiddleware(5);
        var evt = new TestDepthEvent(EventId.New(), DateTimeOffset.UtcNow);

        var metadata = new EventMetadataBuilder()
            .WithHeader(CausationDepthLimitMiddleware.CausationDepthHeaderName, "3")
            .Build();

        var envelope = EventEnvelope.Create(evt, metadata);
        bool nextCalled = false;

        using (EventContext.SetCurrent(envelope))
        {
            await middleware.InvokeAsync(evt, (@event, ct) =>
            {
                nextCalled = true;
                return ValueTask.CompletedTask;
            }, CancellationToken.None);
        }

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenDepthEqualsOrExceedsMax_ThrowsInvalidOperationException()
    {
        // EVT-MED-SEC-001 / ATK-001 Remediation Verification:
        // After Fix 6, depth is tracked via AsyncLocal — NOT read from envelope headers.
        // To test that the depth limit is enforced, we simulate actual recursion by nesting
        // InvokeAsync calls from within the next delegate, incrementing the AsyncLocal counter.
        // An attacker cannot bypass this by injecting X-Causation-Depth: 0 in headers.

        var middleware = new CausationDepthLimitMiddleware(maxDepth: 3);
        var evt = new TestDepthEvent(EventId.New(), DateTimeOffset.UtcNow);

        int depthReached = 0;

        // Simulate 3 nested calls — at depth 3 the guard should throw.
        async ValueTask RecursiveNext(TestDepthEvent @event, CancellationToken ct)
        {
            depthReached++;
            await middleware.InvokeAsync(@event, RecursiveNext, ct);
        }

        var act = async () => await middleware.InvokeAsync(evt, RecursiveNext, CancellationToken.None);
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*Causation depth limit (3) exceeded*");
        depthReached.Should().Be(3, "Depth counter should have reached the limit.");
    }

    [Fact]
    public async Task InvokeAsync_HeaderInjection_CannotBypassDepthLimit()
    {
        // ATK-001 Verification: Injecting X-Causation-Depth: 0 in headers cannot reset AsyncLocal counter.
        // The middleware only reads from AsyncLocal, never from incoming headers.
        var middleware = new CausationDepthLimitMiddleware(maxDepth: 2);
        var evt = new TestDepthEvent(EventId.New(), DateTimeOffset.UtcNow);

        // Create envelope with X-Causation-Depth: 0 to attempt bypass
        var metadata = new EventMetadataBuilder()
            .WithHeader(CausationDepthLimitMiddleware.CausationDepthHeaderName, "0") // injection attempt
            .Build();
        var envelope = EventEnvelope.Create(evt, metadata);

        int depthReached = 0;

        // Simulate nested invocations — counter should still accumulate via AsyncLocal.
        async ValueTask RecursiveNextWithInjection(TestDepthEvent @event, CancellationToken ct)
        {
            depthReached++;
            // Re-set the header to 0 each level to simulate continuous injection
            var injectedMetadata = new EventMetadataBuilder()
                .WithHeader(CausationDepthLimitMiddleware.CausationDepthHeaderName, "0")
                .Build();
            using (EventContext.SetCurrent(EventEnvelope.Create(@event, injectedMetadata)))
            {
                await middleware.InvokeAsync(@event, RecursiveNextWithInjection, ct);
            }
        }

        using (EventContext.SetCurrent(envelope))
        {
            var act = async () => await middleware.InvokeAsync(evt, RecursiveNextWithInjection, CancellationToken.None);
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.WithMessage("*Causation depth limit (2) exceeded*");
        }

        depthReached.Should().BePositive("Depth limit was still enforced despite header injection attempts.");
    }
}
