// Copyright © Erickson Lopez. MIT License.
using System;

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

    /// <summary>
    /// Gets or sets the global active <see cref="IEventTypeRegistry"/>.
    /// </summary>
    /// <remarks>
    /// Setting this property to <see langword="null"/> resets it to <see cref="EventTypeRegistry.Empty"/>.
    /// The property is volatile and safe to read from multiple threads; however, replacing the registry
    /// during active event publishing may result in inconsistent descriptor resolution for in-flight operations.
    /// Prefer setting this once at application startup.
    /// </remarks>
    public static IEventTypeRegistry Current
    {
        get => s_current;
        set => s_current = value ?? EventTypeRegistry.Empty;
    }

    /// <summary>
    /// Retrieves the statically cached descriptor for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>The resolved <see cref="EventTypeDescriptor"/> for <typeparamref name="TEvent"/>.</returns>
    public static EventTypeDescriptor GetDescriptor<TEvent>() where TEvent : IEvent =>
        Cache<TEvent>.Descriptor;

    /// <summary>
    /// Retrieves the semantic event type for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>The semantic <see cref="EventType"/> for <typeparamref name="TEvent"/>.</returns>
    public static EventType GetEventType<TEvent>() where TEvent : IEvent =>
        Cache<TEvent>.Descriptor.EventType;

    /// <summary>
    /// Retrieves the schema contract version for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>The schema <see cref="EventVersion"/> for <typeparamref name="TEvent"/>.</returns>
    public static EventVersion GetVersion<TEvent>() where TEvent : IEvent =>
        Cache<TEvent>.Descriptor.Version;

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


