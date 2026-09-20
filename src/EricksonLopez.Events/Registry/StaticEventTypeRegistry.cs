// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;

namespace EricksonLopez.Events.Registry;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

/// <summary>
/// Provides statically cached access to event descriptors with zero runtime overhead.
/// </summary>
public static class StaticEventTypeRegistry
{
    private static volatile IEventTypeRegistry s_current = EventTypeRegistry.Empty;
    private static int s_isInitialized;
    private static volatile bool s_isFrozen;

    /// <summary>
    /// Gets a value indicating whether the registry has been frozen against runtime modification.
    /// </summary>
    public static bool IsFrozen => s_isFrozen;

    /// <summary>
    /// Freezes the global registry, preventing any subsequent calls to <see cref="SetCurrent"/> or <see cref="Reset"/>.
    /// </summary>
    public static void Freeze()
    {
        s_isFrozen = true;
    }

    /// <summary>
    /// Gets or sets the global active <see cref="IEventTypeRegistry"/>.
    /// </summary>
    /// <remarks>
    /// Setting this property initializes the global registry. Re-assigning it after initialization
    /// throws <see cref="InvalidOperationException"/> to prevent unauthorized in-process registry poisoning.
    /// In test scenarios, use <see cref="SetCurrent(IEventTypeRegistry, bool)"/> with allowOverride: true or <see cref="Reset"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The registry was already initialized and cannot be overwritten directly.</exception>
    public static IEventTypeRegistry Current
    {
        get => s_current;
        set => SetCurrent(value, allowOverride: false);
    }

    /// <summary>
    /// Sets the global active <see cref="IEventTypeRegistry"/>.
    /// </summary>
    /// <param name="registry">The registry to set.</param>
    /// <param name="allowOverride">Whether to allow overriding an already initialized registry (intended for test suites).</param>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The registry is already initialized and <paramref name="allowOverride"/> is <see langword="false"/>, or the registry is frozen.</exception>
    public static void SetCurrent(IEventTypeRegistry registry, bool allowOverride = false)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (s_isFrozen)
        {
            throw new InvalidOperationException("StaticEventTypeRegistry is frozen and cannot be modified.");
        }

        if (!allowOverride && Interlocked.CompareExchange(ref s_isInitialized, 1, 0) != 0)
        {
            throw new InvalidOperationException(
                "StaticEventTypeRegistry is already initialized. Overwriting the global registry is disallowed for security reasons unless explicitly authorized via allowOverride: true.");
        }

        s_current = registry;
        Interlocked.Exchange(ref s_isInitialized, 1);
    }

    /// <summary>
    /// Resets the global registry to its default empty state (intended strictly for unit testing isolation).
    /// </summary>
    /// <exception cref="InvalidOperationException">The registry is frozen and cannot be reset.</exception>
    public static void Reset()
    {
        if (s_isFrozen)
        {
            throw new InvalidOperationException("StaticEventTypeRegistry is frozen and cannot be modified.");
        }

        // EVT-HIGH-003 FIX: Reset s_isInitialized to 0 BEFORE clearing s_current.
        // The previous order (s_current = Empty, then Interlocked.Exchange) created a race:
        // a concurrent thread calling SetCurrent would observe s_isInitialized=1 (not yet reset)
        // and throw "already initialized", even though Reset() was in progress.
        // Correct order ensures that once s_isInitialized=0, SetCurrent can proceed safely.
        Interlocked.Exchange(ref s_isInitialized, 0);
        s_current = EventTypeRegistry.Empty;
    }

    /// <summary>
    /// Retrieves the statically cached descriptor for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>The resolved <see cref="EventTypeDescriptor"/> for <typeparamref name="TEvent"/>.</returns>
    public static EventTypeDescriptor GetDescriptor<TEvent>() where TEvent : IEvent
    {
        if (s_current.TryGetDescriptor<TEvent>(out var descriptor))
        {
            return descriptor;
        }

        return Cache<TEvent>.Descriptor;
    }

    /// <summary>
    /// Retrieves the semantic event type for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>The semantic <see cref="EventType"/> for <typeparamref name="TEvent"/>.</returns>
    public static EventType GetEventType<TEvent>() where TEvent : IEvent =>
        GetDescriptor<TEvent>().EventType;

    /// <summary>
    /// Retrieves the schema contract version for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>The schema <see cref="EventVersion"/> for <typeparamref name="TEvent"/>.</returns>
    public static EventVersion GetVersion<TEvent>() where TEvent : IEvent =>
        GetDescriptor<TEvent>().Version;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Intentional fallback reflection path when compile-time Source Generator is not used.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Intentional fallback reflection path when compile-time Source Generator is not used.")]
    private static class Cache<TEvent> where TEvent : IEvent
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Intentional fallback reflection path when compile-time Source Generator is not used.")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Intentional fallback reflection path when compile-time Source Generator is not used.")]
        internal static readonly EventTypeDescriptor Descriptor = ResolveDescriptor();

        [RequiresUnreferencedCode("Fallback reflection path reading attributes. For 100% Native AOT compatibility, use EricksonLopez.Events.Generators source generator.")]
        [RequiresDynamicCode("Fallback reflection path reading attributes. For 100% Native AOT compatibility, use EricksonLopez.Events.Generators source generator.")]
        private static EventTypeDescriptor ResolveDescriptor()
        {
            var type = typeof(TEvent);
            var nameAttr = type.GetCustomAttribute<EventNameAttribute>();
            var versionAttr = type.GetCustomAttribute<EventVersionAttribute>();
            var sourceAttr = type.GetCustomAttribute<EventSourceAttribute>();

            var eventType = nameAttr is not null
                ? nameAttr.AsEventType()
                : EventType.From(type.Name);

            var version = versionAttr is not null
                ? versionAttr.AsEventVersion()
                : EventVersion.V1;

            var source = sourceAttr?.Source;

            return new EventTypeDescriptor(type, eventType, version, source);
        }
    }
}


