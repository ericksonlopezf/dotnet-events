// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;

namespace EricksonLopez.Events.Bus.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
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
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        Action<EventBusOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new EventBusOptions();
        configure?.Invoke(options);

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
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        services.Add(new ServiceDescriptor(typeof(IEventHandler<TEvent>), sp => sp.GetRequiredService<THandler>(), lifetime));

        // Register static dispatch delegate into HandlerRegistry without reflection in hot path
        var descriptor = new HandlerDescriptor(
            typeof(THandler),
            typeof(IEventHandler<TEvent>),
            static (handlerInstance, eventInstance, ct) =>
                ((IEventHandler<TEvent>)handlerInstance).HandleAsync((TEvent)eventInstance, ct));

        // Use post-configuration to populate singleton registry
        services.AddSingleton(new HandlerRegistrationToken(typeof(TEvent), descriptor));

        return services;
    }

    /// <summary>
    /// Registers an event middleware into the pipeline with the specified lifetime.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the event middleware implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="lifetime">The service lifetime for the middleware. The default is <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddEventMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMiddleware>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : class, IEventMiddleware
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Add(new ServiceDescriptor(typeof(IEventMiddleware), typeof(TMiddleware), lifetime));
        return services;
    }
}



