// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using ECommerce.Application;
using ECommerce.Domain;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Diagnostics;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Exceptions;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.OpenTelemetry;
using EricksonLopez.Events.Registry;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;
using EricksonLopez.Events.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ECommerce.App;

#pragma warning disable CA1711 // Suffix

public sealed class SampleEnvelopeEventConsumer : IEnvelopeEventHandler<OrderPlacedDomainEvent>
{
    public ValueTask HandleAsync(IEventEnvelope<OrderPlacedDomainEvent> envelope, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}

public static class Level11_ComprehensiveCoverage
{
    public static async Task RunAsync()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine(" LEVEL 11: COMPREHENSIVE PUBLIC API COVERAGE & LIVING VERIFICATION");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.ResetColor();

        // 1. AsEventType, AsEventVersion, and EventSourceAttribute (GAP-001)
        var nameAttr = new EventNameAttribute("order.placed.v1");
        var eventType = nameAttr.AsEventType();
        var versionAttr = new EventVersionAttribute(2);
        var eventVersion = versionAttr.AsEventVersion();
        Console.WriteLine($"    -> Attribute mapping: EventType={eventType.Value}, Version={eventVersion.Value}");

        // EventSourceAttribute — reads the source declared on an event type
        var sourceAttr = typeof(OrderPlacedIntegrationEvent)
            .GetCustomAttributes(typeof(EricksonLopez.Events.Attributes.EventSourceAttribute), inherit: false)
            .OfType<EricksonLopez.Events.Attributes.EventSourceAttribute>()
            .FirstOrDefault();
        Console.WriteLine($"    -> EventSourceAttribute on OrderPlacedIntegrationEvent: Source='{sourceAttr?.Source ?? "<none>"}'");
        // Direct instantiation
        var directSourceAttr = new EricksonLopez.Events.Attributes.EventSourceAttribute("showcase-service");
        Console.WriteLine($"    -> EventSourceAttribute(\"showcase-service\"): Source='{directSourceAttr.Source}'");


        // 2. TenantId.TryToGuid
        var tenantId = new TenantId(Guid.NewGuid().ToString());
        var isGuid = tenantId.TryToGuid(out var parsedTenantGuid);
        Console.WriteLine($"    -> TenantId.TryToGuid: Success={isGuid}, Guid={parsedTenantGuid}");

        // 3. EventBusOptions.Validate
        var options = new EventBusOptions();
        options.Validate();
        Console.WriteLine("    -> EventBusOptions.Validate() executed successfully.");

        // 4. EventBusDiagnostics.RecordPublish
        EventBusDiagnostics.RecordPublish("OrderPlacedDomainEvent", 15.4, 2, true);
        Console.WriteLine("    -> EventBusDiagnostics.RecordPublish executed.");

        // 5. OpenTelemetry AddEventsInstrumentation (Tracer and Meter)
        var tracer = Sdk.CreateTracerProviderBuilder()
            .AddEventsInstrumentation()
            .Build();
        tracer?.Dispose();

        var meter = Sdk.CreateMeterProviderBuilder()
            .AddEventsInstrumentation()
            .Build();
        meter?.Dispose();
        Console.WriteLine("    -> OpenTelemetry Tracer & Meter AddEventsInstrumentation executed.");

        // 6. HandlerRegistry (HasHandlers & GetHandlers)
        var registry = new HandlerRegistry();
        var hasHandlers = registry.HasHandlers(typeof(OrderPlacedDomainEvent));
        var handlers = registry.GetHandlers(typeof(OrderPlacedDomainEvent));
        Console.WriteLine($"    -> HandlerRegistry: HasHandlers={hasHandlers}, Count={handlers.Count}");

        // 7. DI extensions (AddEnvelopeEventHandler)
        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEnvelopeEventHandler<OrderPlacedDomainEvent, SampleEnvelopeEventConsumer>();
        using var serviceProvider = services.BuildServiceProvider();
        Console.WriteLine("    -> DI registered: AddEnvelopeEventHandler.");

        // 8. CausationDepthLimitMiddleware.InvokeAsync
        var testEvent = new OrderPlacedDomainEvent(
            EventId.New(),
            OrderId.New(),
            CustomerId.New(),
            Money.USD(99.50m),
            DateTimeOffset.UtcNow);

        var middleware = new CausationDepthLimitMiddleware(maxDepth: 10);
        await middleware.InvokeAsync(testEvent, (evt, ct) => ValueTask.CompletedTask, CancellationToken.None);
        Console.WriteLine("    -> CausationDepthLimitMiddleware.InvokeAsync executed.");

        // 9. SequentialExecutionStrategy.ExecuteAsync
        var strategy = new SequentialExecutionStrategy();
        await strategy.ExecuteAsync(handlers, testEvent, serviceProvider, options, CancellationToken.None);
        Console.WriteLine("    -> SequentialExecutionStrategy.ExecuteAsync executed.");

        // 10. FakeEventPublisher.PublishEnvelopeAsync
        var fakePublisher = new FakeEventPublisher();
        var envelope = EventEnvelope.Create(testEvent);
        await fakePublisher.PublishEnvelopeAsync(envelope, CancellationToken.None);
        Console.WriteLine($"    -> FakeEventPublisher.PublishEnvelopeAsync executed. Envelopes count: {fakePublisher.PublishedEnvelopes.Count}");

        // 11. EventContext & IEventExecutionTracker (SetExecutionTracker, MarkHandlerCompleted, IsHandlerCompleted)
        var sampleTracker = new SampleExecutionTracker();
        using (EventContext.SetExecutionTracker(sampleTracker))
        {
            EventContext.MarkHandlerCompleted(testEvent.Id, typeof(SampleEnvelopeEventConsumer));
            var completed = EventContext.IsHandlerCompleted(testEvent.Id, typeof(SampleEnvelopeEventConsumer));
            Console.WriteLine($"    -> EventContext execution tracker: IsHandlerCompleted={completed}");
        }

        // 12. HandlerDescriptor, HandlerInvoker, and HandlerRegistrationToken
        HandlerInvoker<OrderPlacedDomainEvent> invokerDelegate = (handler, evt, ct) => ((IEventHandler<OrderPlacedDomainEvent>)handler).HandleAsync(evt, ct);
        var descriptor = new HandlerDescriptor(
            typeof(SampleEnvelopeEventConsumer),
            typeof(IEnvelopeEventHandler<OrderPlacedDomainEvent>),
            (h, e, ct) => ((IEnvelopeEventHandler<OrderPlacedDomainEvent>)h).HandleAsync((IEventEnvelope<OrderPlacedDomainEvent>)e, ct),
            typedInvoker: invokerDelegate);
        var regToken = new HandlerRegistrationToken(typeof(OrderPlacedDomainEvent), descriptor);
        Console.WriteLine($"    -> HandlerRegistrationToken: EventType={regToken.EventType.Name}, Descriptor={regToken.Descriptor.HandlerType.Name}");

        // 13. HandlerScopePolicy enumeration
        options.ScopePolicy = HandlerScopePolicy.CreatePerHandler;
        options.ScopePolicy = HandlerScopePolicy.Auto;
        Console.WriteLine($"    -> HandlerScopePolicy configured: {options.ScopePolicy}");

        // 14. EventEnvelopeJsonConverterFactory and EventEnvelopeJsonConverter
        var converterFactory = new EventEnvelopeJsonConverterFactory();
        var canConvert = converterFactory.CanConvert(typeof(EventEnvelope<OrderPlacedIntegrationEvent>));
        var genericConverter = new EventEnvelopeJsonConverter<OrderPlacedIntegrationEvent>();
        Console.WriteLine($"    -> EventEnvelopeJsonConverterFactory: CanConvert={canConvert}, GenericConverter={genericConverter.GetType().Name}");

        // 15. EventsDiagnostics StartPublishActivity and RecordEventHandled
        using (var pubActivity = EventsDiagnostics.StartPublishActivity(testEvent))
        {
            Console.WriteLine($"    -> EventsDiagnostics.StartPublishActivity executed (Activity null without listener: {pubActivity == null})");
        }
        EventsDiagnostics.RecordEventHandled("OrderPlacedDomainEvent", 4.2, true);
        Console.WriteLine("    -> EventsDiagnostics.RecordEventHandled recorded successfully.");

        // 16. ReadOnlySpan<char> parsing and comparison operators
        ReadOnlySpan<char> spanId = testEvent.Id.ToString().AsSpan();
        var parseSpanSuccess = EventId.TryParse(spanId, null, out var parsedSpanEventId);
        var isLess = testEvent.Id < EventId.New();
        var isGreaterOrEqual = testEvent.Id >= parsedSpanEventId;
        Console.WriteLine($"    -> EventId.TryParse(ReadOnlySpan): Success={parseSpanSuccess}, Less={isLess}, GreaterOrEqual={isGreaterOrEqual}");

        // 17. Exception types (EventTypeNotFoundException and EventValidationException)
        var notFoundEx = new EventTypeNotFoundException(EricksonLopez.Events.Identifiers.EventType.From("unknown.event"));
        var valEx1 = new EventValidationException("Invalid event structure");
        var valEx2 = new EventValidationException("Validation failed", new InvalidOperationException("Inner failure"));
        Console.WriteLine($"    -> Exceptions: {notFoundEx.EventType}, {valEx1.Message}, Inner={valEx2.InnerException?.Message}");

        // 18. StaticEventTypeRegistry.Freeze & IsFrozen
        StaticEventTypeRegistry.Freeze();
        Console.WriteLine($"    -> StaticEventTypeRegistry.Freeze executed. IsFrozen={StaticEventTypeRegistry.IsFrozen}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✔ Level 11 Comprehensive API Coverage completed successfully.");
        Console.ResetColor();
    }
}

file sealed class SampleExecutionTracker : IEventExecutionTracker
{
    private readonly System.Collections.Generic.HashSet<(EventId, Type)> _completed = new();
    public bool IsCompleted(EventId eventId, Type handlerType) => _completed.Contains((eventId, handlerType));
    public void MarkCompleted(EventId eventId, Type handlerType) => _completed.Add((eventId, handlerType));
}

