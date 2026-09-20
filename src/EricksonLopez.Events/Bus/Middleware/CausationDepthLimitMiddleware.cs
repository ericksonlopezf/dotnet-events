// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.Events.Bus.Middleware;

/// <summary>
/// Provides a middleware that guards against distributed causality loops or runaway reentrancy chains
/// by tracking causation depth monotonically using <see cref="System.Threading.AsyncLocal{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// The causation depth is maintained internally via an <see cref="System.Threading.AsyncLocal{T}"/>
/// counter that increments with each nested event publication. It is never read from or written to
/// envelope headers on the incoming side — only propagated outward for cross-service correlation.
/// </para>
/// <para>
/// This design prevents ATK-001: an attacker injecting <c>X-Causation-Depth: 0</c> in
/// <see cref="EricksonLopez.Events.Metadata.EventMetadata.CustomHeaders"/> cannot reset the depth counter
/// because the middleware always uses the <see cref="System.Threading.AsyncLocal{T}"/> value, not the header.
/// </para>
/// </remarks>
public sealed class CausationDepthLimitMiddleware : IEventMiddleware
{
    /// <summary>
    /// The default header name used to convey causality depth across transport boundaries.
    /// </summary>
    public const string CausationDepthHeaderName = "X-Causation-Depth";

    // EVT-MED-SEC-001 / ATK-001 FIX:
    // Depth is tracked monotonically via AsyncLocal, not read from headers.
    // This prevents header injection attacks that could reset the counter.
    private readonly AsyncLocal<int> _causationDepth = new();

    /// <summary>
    /// Gets the maximum allowed causation depth before publication is rejected. The default is 16.
    /// </summary>
    public int MaxDepth { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CausationDepthLimitMiddleware"/> class.
    /// </summary>
    /// <param name="maxDepth">The maximum allowable causation depth. Must be greater than zero.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxDepth"/> is less than or equal to zero</exception>
    public CausationDepthLimitMiddleware(int maxDepth = 16)
    {
        if (maxDepth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth), "Max depth must be greater than zero.");
        }

        MaxDepth = maxDepth;
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(nextHandler);

        // EVT-MED-SEC-001 / ATK-001 FIX:
        // Read depth from AsyncLocal — immune to header injection.
        // Previously, the middleware read X-Causation-Depth from the envelope's CustomHeaders,
        // which allowed an attacker to inject "X-Causation-Depth: 0" to reset the counter
        // each level, effectively bypassing the MaxDepth limit and enabling infinite event chains.
        int currentDepth = _causationDepth.Value;

        if (currentDepth >= MaxDepth)
        {
            throw new InvalidOperationException(
                $"Causation depth limit ({MaxDepth}) exceeded for event '{typeof(TEvent).Name}'. " +
                $"Current depth: {currentDepth}. Possible distributed cyclic cascade. " +
                "Increase CausationDepthLimitMiddleware.MaxDepth or break the event chain.");
        }

        // Increment the counter for this async context before invoking next.
        _causationDepth.Value = currentDepth + 1;

        // Propagate the depth outward in envelope headers for cross-service correlation (read-only intent).
        // This is for observability only — incoming depth header is never read by this middleware.
        IDisposable? depthScope = null;
        if (Context.EventContext.Current is { } activeEnvelope)
        {
            var updatedMetadata = activeEnvelope.Metadata.WithHeader(
                CausationDepthHeaderName,
                (currentDepth + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
            var updatedEnvelope = Envelopes.EventEnvelope.Create(eventInstance, updatedMetadata);
            depthScope = Context.EventContext.SetCurrent(updatedEnvelope);
        }

        try
        {
            await nextHandler(eventInstance, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // Restore previous depth in this async context.
            _causationDepth.Value = currentDepth;
            depthScope?.Dispose();
        }
    }
}
