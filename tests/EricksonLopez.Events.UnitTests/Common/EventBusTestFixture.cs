// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.Common;

using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fluent fixture to facilitate building ServiceProvider instances with EventBus, handlers, and middlewares for testing.
/// </summary>
public sealed class EventBusTestFixture : IDisposable
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private ServiceProvider? _serviceProvider;
    private Action<EventBusOptions>? _configureOptions;

    public IServiceCollection Services => _services;

    public EventBusTestFixture WithOptions(Action<EventBusOptions> configure)
    {
        _configureOptions = configure;
        return this;
    }

    public EventBusTestFixture WithSingletonService<TService>(TService implementation) where TService : class
    {
        _services.AddSingleton(implementation);
        return this;
    }

    public EventBusTestFixture WithEventHandler<TEvent, THandler>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        _services.AddEventHandler<TEvent, THandler>(lifetime);
        return this;
    }

    public EventBusTestFixture WithEventMiddleware<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : class, IEventMiddleware
    {
        _services.AddEventMiddleware<TMiddleware>(lifetime);
        return this;
    }

    public IServiceProvider Build()
    {
        if (_configureOptions != null)
        {
            _services.AddEventBus(_configureOptions);
        }
        else
        {
            _services.AddEventBus();
        }

        _serviceProvider = _services.BuildServiceProvider();
        return _serviceProvider;
    }

    public IEventBus GetBus()
    {
        var sp = _serviceProvider ?? Build();
        return sp.GetRequiredService<IEventBus>();
    }

    public T GetService<T>() where T : notnull
    {
        var sp = _serviceProvider ?? Build();
        return sp.GetRequiredService<T>();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}


