// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;

namespace EricksonLopez.Events.Bus.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Provides extension methods for registering <see cref="IEventBus"/> and related eventing services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class EventBusServiceCollectionExtensions
{
    /// <summary>
    /// Registers core event bus services into the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">An optional delegate to configure <see cref="EventBusOptions"/>.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <remarks>
    /// <para>
    /// This method registers the following services:
    /// <list type="bullet">
    /// <item><see cref="IHandlerRegistry"/> as a singleton, populated from registered <see cref="HandlerRegistrationToken"/> instances.</item>
    /// <item><see cref="IEventBus"/> as scoped (resolved once per DI scope, typically per HTTP request).</item>
    /// <item><see cref="IEventPublisher"/> as scoped, delegating to the same <see cref="IEventBus"/> instance.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        Action<EventBusOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new EventBusOptions();
        configure?.Invoke(options);
        options.Validate();

        services.TryAddSingleton(options);
        services.TryAddSingleton<IHandlerRegistry>(sp =>
        {
            var registry = new HandlerRegistry();
            var tokens = sp.GetServices<HandlerRegistrationToken>();
            foreach (var token in tokens)
            {
                registry.Register(token.EventType, token.Descriptor);
            }
            return registry;
        });
        services.TryAddScoped<IEventBus, EventBus>();
        services.TryAddScoped<IEventPublisher>(sp => sp.GetRequiredService<IEventBus>());

        return services;
    }

    /// <summary>
    /// Registers a strongly typed event handler into the service collection and handler registry.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to handle.</typeparam>
    /// <typeparam name="THandler">The type of the event handler implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="lifetime">The service lifetime for the handler. The default is <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddEventHandler<TEvent, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        services.Add(new ServiceDescriptor(typeof(IEventHandler<TEvent>), sp => sp.GetRequiredService<THandler>(), lifetime));

        // Register static dispatch delegate into HandlerRegistry without reflection in hot path
        HandlerInvoker<TEvent> typedInvoker = static (handlerInstance, eventInstance, ct) =>
            ((IEventHandler<TEvent>)handlerInstance).HandleAsync(eventInstance, ct);

        var descriptor = new HandlerDescriptor(
            typeof(THandler),
            typeof(IEventHandler<TEvent>),
            static (handlerInstance, eventInstance, ct) =>
                ((IEventHandler<TEvent>)handlerInstance).HandleAsync((TEvent)eventInstance, ct),
            typedInvoker);

        // Use post-configuration to populate singleton registry
        services.AddSingleton(new HandlerRegistrationToken(typeof(TEvent), descriptor));

        return services;
    }

    /// <summary>
    /// Registers a strongly typed envelope event handler into the service collection and handler registry.
    /// </summary>
    /// <typeparam name="TEvent">The type of event contained in the envelope to handle.</typeparam>
    /// <typeparam name="THandler">The type of the envelope event handler implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="lifetime">The service lifetime for the handler. The default is <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddEnvelopeEventHandler<TEvent, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEvent : IEvent
        where THandler : class, IEnvelopeEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        services.Add(new ServiceDescriptor(typeof(IEnvelopeEventHandler<TEvent>), sp => sp.GetRequiredService<THandler>(), lifetime));

        HandlerInvoker<TEvent> typedInvoker = static (handlerInstance, eventInstance, ct) =>
        {
            var envelope = Context.EventContext.Current as Envelopes.IEventEnvelope<TEvent>
                ?? new Envelopes.EventEnvelope<TEvent>(
                    eventInstance.Id,
                    default,
                    default,
                    eventInstance.OccurredAt,
                    eventInstance,
                    Context.EventContext.Metadata);

            return ((IEnvelopeEventHandler<TEvent>)handlerInstance).HandleAsync(envelope, ct);
        };

        var descriptor = new HandlerDescriptor(
            typeof(THandler),
            typeof(IEnvelopeEventHandler<TEvent>),
            static (handlerInstance, eventInstance, ct) =>
            {
                var typedEvent = (TEvent)eventInstance;
                var envelope = Context.EventContext.Current as Envelopes.IEventEnvelope<TEvent>
                    ?? new Envelopes.EventEnvelope<TEvent>(
                        typedEvent.Id,
                        default,
                        default,
                        typedEvent.OccurredAt,
                        typedEvent,
                        Context.EventContext.Metadata);

                return ((IEnvelopeEventHandler<TEvent>)handlerInstance).HandleAsync(envelope, ct);
            },
            typedInvoker);

        services.AddSingleton(new HandlerRegistrationToken(typeof(TEvent), descriptor));

        return services;
    }

    /// <summary>
    /// Registers an event middleware into the pipeline with the specified lifetime.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the event middleware implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="lifetime">
    /// The service lifetime for the middleware. The default is <see cref="ServiceLifetime.Transient"/>.
    /// </param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="lifetime"/> is <see cref="ServiceLifetime.Singleton"/> and <typeparamref name="TMiddleware"/>
    /// is not annotated with <c>[ThreadSafe]</c> or contains mutable fields — Singleton middlewares are shared
    /// across all concurrent requests and must be fully thread-safe.
    /// </exception>
    /// <remarks>
    /// <para>
    /// EVT-HIGH-004: Singleton lifetime for stateful middlewares is dangerous because the middleware instance
    /// is shared across all concurrent handlers. For example, <see cref="CausationDepthLimitMiddleware"/>
    /// uses <see cref="System.Threading.AsyncLocal{T}"/> and is safe as Singleton; however, any middleware
    /// with mutable instance fields that is not thread-safe must use <see cref="ServiceLifetime.Transient"/>
    /// or <see cref="ServiceLifetime.Scoped"/>.
    /// </para>
    /// <para>
    /// Use <see cref="ServiceLifetime.Transient"/> (default) for stateful middlewares.
    /// Use <see cref="ServiceLifetime.Singleton"/> only for stateless or <see cref="System.Threading.AsyncLocal{T}"/>-based middlewares.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddEventMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] TMiddleware>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : class, IEventMiddleware
    {
        ArgumentNullException.ThrowIfNull(services);

        // EVT-HIGH-004 FIX: Warn (via exception) on Singleton middleware registration.
        // Singleton middlewares are shared across concurrent requests. If a middleware has
        // mutable instance state (non-AsyncLocal fields), it will cause data races.
        // We validate that the Singleton path is used consciously by checking for common
        // dangerous patterns: writable instance fields that are not AsyncLocal<T>.
        if (lifetime == ServiceLifetime.Singleton)
        {
            var type = typeof(TMiddleware);
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
            {
                // AsyncLocal<T> fields are safe (per-async-context) even as Singleton.
                if (field.IsInitOnly || IsAsyncLocalField(field))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Middleware '{type.FullName}' cannot be registered as Singleton because it contains " +
                    $"mutable instance field '{field.Name}' of type '{field.FieldType.Name}'. " +
                    $"Singleton middlewares are shared across all concurrent event dispatches and must be fully thread-safe. " +
                    $"Either use ServiceLifetime.Transient (default), ServiceLifetime.Scoped, " +
                    $"or make the field readonly/AsyncLocal<T>.");
            }
        }

        services.Add(new ServiceDescriptor(typeof(IEventMiddleware), typeof(TMiddleware), lifetime));
        return services;
    }

    private static bool IsAsyncLocalField(System.Reflection.FieldInfo field)
    {
        var fieldType = field.FieldType;
        return fieldType.IsGenericType &&
               fieldType.GetGenericTypeDefinition() == typeof(AsyncLocal<>);
    }
}
