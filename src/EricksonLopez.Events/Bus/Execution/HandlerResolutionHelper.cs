// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Events.Bus.Execution;

internal static class HandlerResolutionHelper
{
    public static object? ResolveHandler<TEvent>(
        IServiceProvider serviceProvider,
        HandlerDescriptor descriptor,
        Dictionary<Type, object?>? fallbackCache = null)
        where TEvent : IEvent
    {
        var instance = serviceProvider.GetService(descriptor.HandlerType);
        if (instance != null)
        {
            return instance;
        }

        if (fallbackCache != null && fallbackCache.TryGetValue(descriptor.HandlerType, out var cached))
        {
            return cached;
        }

        if (descriptor.ServiceType == typeof(IEventHandler<TEvent>))
        {
            var directServices = serviceProvider.GetService<IEnumerable<IEventHandler<TEvent>>>();
            if (directServices != null)
            {
                object? matched = null;
                foreach (var svc in directServices)
                {
                    if (svc != null)
                    {
                        var svcType = svc.GetType();
                        fallbackCache?.TryAdd(svcType, svc);

                        if (matched == null && (svcType == descriptor.HandlerType || descriptor.HandlerType.IsAssignableFrom(svcType)))
                        {
                            matched = svc;
                        }
                    }
                }

                if (matched != null)
                {
                    return matched;
                }
            }
        }
        else if (descriptor.ServiceType == typeof(IEnvelopeEventHandler<TEvent>))
        {
            var directServices = serviceProvider.GetService<IEnumerable<IEnvelopeEventHandler<TEvent>>>();
            if (directServices != null)
            {
                object? matched = null;
                foreach (var svc in directServices)
                {
                    if (svc != null)
                    {
                        var svcType = svc.GetType();
                        fallbackCache?.TryAdd(svcType, svc);

                        if (matched == null && (svcType == descriptor.HandlerType || descriptor.HandlerType.IsAssignableFrom(svcType)))
                        {
                            matched = svc;
                        }
                    }
                }

                if (matched != null)
                {
                    return matched;
                }
            }
        }

        return serviceProvider.GetService(descriptor.ServiceType);
    }
}
