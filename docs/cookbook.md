# EricksonLopez.Events Cookbook

Official collection of architectural recipes and practical patterns for building event-driven systems, Domain-Driven Design (DDD), Clean Architecture, and 100% Native AOT-compatible applications on .NET 8, .NET 9, and .NET 10.

---

## Table of Contents

1. [Recipe 1: Pure Domain Events with EventId (Guid v7)](#recipe-1-pure-domain-events-with-eventid-guid-v7)
2. [Recipe 2: Metadata Enrichment with EventMetadataBuilder](#recipe-2-metadata-enrichment-with-eventmetadatabuilder)
3. [Recipe 3: In-Process Dispatch with IEventBus and Dependency Injection](#recipe-3-in-process-dispatch-with-ieventbus-and-dependency-injection)
4. [Recipe 4: Concurrent Execution Strategies (Sequential vs Parallel)](#recipe-4-concurrent-execution-strategies-sequential-vs-parallel)
5. [Recipe 5: Resilience and Error Handling with ErrorHandlingPolicy](#recipe-5-resilience-and-error-handling-with-errorhandlingpolicy)
6. [Recipe 6: Pipeline Interception with Custom Middlewares](#recipe-6-pipeline-interception-with-custom-middlewares)
7. [Recipe 7: Native AOT Serialization and Deserialization without Reflection](#recipe-7-native-aot-serialization-and-deserialization-without-reflection)
8. [Recipe 8: Interoperability with CloudEvents v1.0](#recipe-8-interoperability-with-cloudevents-v10)
9. [Recipe 9: Isolated Unit Tests with the Testing DSL](#recipe-9-isolated-unit-tests-with-the-testing-dsl)
10. [Recipe 10: Transactional Integration with Outbox and Idempotent Inbox](#recipe-10-transactional-integration-with-outbox-and-idempotent-inbox)

---

## Recipe 1: Pure Domain Events with EventId (Guid v7)

### Problem
In DDD and Clean Architecture architectures, aggregates emit domain events that must be immutable, temporally sortable, and completely decoupled from transport infrastructure or mediation libraries.

### Solution
Implement `IDomainEvent` in immutable records using `EventId` (based on GUID Version 7) to guarantee monotonic chronological ordering and zero heap allocations.

### Complete Code
```csharp
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

namespace MyProject.Domain;

public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.CreateVersion7());
}

public sealed record OrderPlacedDomainEvent(
    EventId Id,
    OrderId OrderId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed class Order
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public OrderId Id { get; }
    public decimal Total { get; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Order(OrderId id, decimal total)
    {
        Id = id;
        Total = total;

        _domainEvents.Add(new OrderPlacedDomainEvent(
            EventId.New(),
            id,
            total,
            "USD",
            DateTimeOffset.UtcNow));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Step-by-Step
1. `OrderPlacedDomainEvent` implements `IDomainEvent` (`IEvent`), inheriting the essential `Id` and `OccurredAt` fields.
2. `EventId.New()` is generated, which internally uses `Guid.CreateVersion7()` on .NET 9 and later (or `Guid.NewGuid()` on .NET 8).
3. The aggregate root `Order` stores events internally and exposes them via `IReadOnlyCollection<IDomainEvent>`.
4. The `ClearDomainEvents()` method allows the dispatcher to clear events once they have been persisted or published.

### Best Practices
- Use `sealed record` for events to enforce semantic value immutability.
- Keep domain events within the `Domain` layer without transport dependencies.

### Common Mistakes
- ❌ Using `Guid.NewGuid()` (Guid v4) directly in your event constructors instead of `EventId.New()`. On .NET 9+, `EventId.New()` uses `Guid.CreateVersion7()` for chronological ordering; on .NET 8, it falls back to `Guid.NewGuid()` but keeps all the type-safety guarantees of `EventId`.
- ❌ Modifying event properties after creation.

---

## Recipe 2: Metadata Enrichment with EventMetadataBuilder

### Problem
Ambient context (distributed correlation, causation, multitenant identifier, and custom headers) must be attached to events without contaminating the business payload contract.

### Solution
Use `EventMetadataBuilder` to compose an immutable `EventMetadata` instance optimized with `FrozenDictionary` and wrap the event inside `EventEnvelope<TEvent>`.

### Complete Code
```csharp
using System;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

namespace MyProject.Application;

[EventName("billing.invoices.invoice-issued")]
[EventVersion(1)]
[EventSource("billing-service")]
public sealed record InvoiceIssuedIntegrationEvent(
    EventId Id,
    Guid InvoiceId,
    decimal AmountDue,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

public static class MetadataRecipe
{
    public static EventEnvelope<InvoiceIssuedIntegrationEvent> WrapInvoiceEvent(
        InvoiceIssuedIntegrationEvent invoiceEvent,
        CorrelationId correlationId,
        CausationId causationId,
        TenantId tenantId)
    {
        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(correlationId)
            .WithCausationId(causationId)
            .WithTenantId(tenantId)
            .WithSource("billing-service")
            .WithContentType("application/json")
            .WithHeader("X-Environment", "Production")
            .WithHeader("X-Correlation-Chain", "Gateway->Billing")
            .Build();

        return EventEnvelope.Create(invoiceEvent, metadata);
    }
}
```

### Step-by-Step
1. The integration event is declared decorated with `[EventName]`, `[EventVersion]`, and `[EventSource]`.
2. `EventMetadataBuilder` is instantiated and fluent methods are chained (`WithCorrelationId`, `WithTenantId`, etc.).
3. `.Build()` is invoked, which freezes custom headers in a `FrozenDictionary<string, string>`.
4. `EventEnvelope.Create(invoiceEvent, metadata)` is invoked to create the strongly typed envelope.

### Best Practices
- Use `CorrelationId` throughout the entire async flow for end-to-end traceability.
- Use `CausationId` pointing to the immediate preceding command or event that caused the emission.

---

## Recipe 3: In-Process Dispatch with IEventBus and Dependency Injection

### Problem
Dispatch events within the process asynchronously to multiple subscribers registered in the `Microsoft.Extensions.DependencyInjection` container without using runtime reflection.

### Solution
Configure `AddEventBus` and register handlers via `AddEventHandler<TEvent, THandler>`, injecting `IEventBus` in application services.

### Complete Code
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Application;

public sealed record UserRegisteredEvent(
    EventId Id,
    string UserId,
    string Email,
    DateTimeOffset OccurredAt) : IEvent;

public sealed class SendWelcomeEmailHandler : IEventHandler<UserRegisteredEvent>
{
    public ValueTask HandleAsync(UserRegisteredEvent eventInstance, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Sending welcome email to {eventInstance.Email} (User: {eventInstance.UserId}).");
        return ValueTask.CompletedTask;
    }
}

public sealed class AuditRegistrationHandler : IEventHandler<UserRegisteredEvent>
{
    public ValueTask HandleAsync(UserRegisteredEvent eventInstance, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Recording audit for user {eventInstance.UserId}.");
        return ValueTask.CompletedTask;
    }
}

public static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();

        services.AddEventBus(options =>
        {
            options.ExecutionMode = EventExecutionMode.Sequential;
            options.ErrorPolicy = ErrorHandlingPolicy.FailFast;
            options.MaxReentrancyDepth = 10;
        });

        services.AddEventHandler<UserRegisteredEvent, SendWelcomeEmailHandler>();
        services.AddEventHandler<UserRegisteredEvent, AuditRegistrationHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var evt = new UserRegisteredEvent(EventId.New(), "usr_123", "alice@example.com", DateTimeOffset.UtcNow);
        await eventBus.PublishAsync(evt);
    }
}
```

### Step-by-Step
1. Register the bus with `services.AddEventBus(...)`.
2. Register each handler with `services.AddEventHandler<TEvent, THandler>()`.
3. Inject `IEventBus` into the service that needs to publish.
4. Invoke `await eventBus.PublishAsync(evt)`.

---

## Recipe 4: Concurrent Execution Strategies (Sequential vs Parallel)

### Problem
Different scenarios require different execution behaviors: operations with strict ordering dependencies require sequential execution, while independent notifications require concurrent parallel execution to maximize throughput.

### Solution
Configure the `ExecutionMode` option in `EventBusOptions` with `EventExecutionMode.Sequential` or `EventExecutionMode.Parallel`.

### Complete Code
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Execution;

public sealed record PaymentReceivedEvent(EventId Id, decimal Amount, DateTimeOffset OccurredAt) : IEvent;

public static class ExecutionModeRecipe
{
    public static IServiceProvider ConfigureSequentialBus()
    {
        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential; // Deterministic
        });
        return services.BuildServiceProvider();
    }

    public static IServiceProvider ConfigureParallelBus()
    {
        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Parallel; // Concurrent with Task.WhenAll
        });
        return services.BuildServiceProvider();
    }
}
```

### Best Practices
- Use `EventExecutionMode.Sequential` when handlers access non-thread-safe resources within the same scope (e.g. `DbContext`).
- Use `EventExecutionMode.Parallel` when handlers are completely independent or perform pure async I/O.

---

## Recipe 5: Resilience and Error Handling with ErrorHandlingPolicy

### Problem
When publishing an event to multiple handlers, if one of them fails, a decision must be made: abort immediately or allow the other handlers to finish processing, accumulating all errors for later diagnosis.

### Solution
Configure `ErrorHandlingPolicy.AggregateAndContinue` in `EventBusOptions` and catch `EventDispatchException`.

### Complete Code
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Resilience;

public sealed record CustomerDeactivatedEvent(EventId Id, string CustomerId, DateTimeOffset OccurredAt) : IEvent;

public static class ResilienceRecipe
{
    public static async Task DispatchWithResilienceAsync(IEventBus bus, CustomerDeactivatedEvent evt)
    {
        try
        {
            await bus.PublishAsync(evt);
        }
        catch (EventDispatchException ex)
        {
            Console.WriteLine($"Dispatch error for event '{ex.EventType.Name}': {ex.Message}");
            Console.WriteLine($"Total failed handlers: {ex.InnerExceptions.Count}");

            foreach (var inner in ex.InnerExceptions)
            {
                Console.WriteLine($"  -> Exception: {inner.GetType().Name} - {inner.Message}");
            }
        }
    }
}
```

---

## Recipe 6: Pipeline Interception with Custom Middlewares

### Problem
Cross-cutting concerns must be implemented (structured logging, latency measurement, ambient context enrichment) around the execution of all events in the bus.

### Solution
Implement `IEventMiddleware` and register it in the container via `AddEventMiddleware<TMiddleware>()`.

### Complete Code
```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Middlewares;

public sealed class StopwatchLoggingMiddleware : IEventMiddleware
{
    public async ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(nextHandler);

        Console.WriteLine($"[Pipeline] Starting dispatch of {typeof(TEvent).Name} (Id: {eventInstance.Id})...");
        var sw = Stopwatch.StartNew();

        try
        {
            await nextHandler(eventInstance, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"[Pipeline] Finished {typeof(TEvent).Name} in {sw.Elapsed.TotalMilliseconds:F2} ms.");
        }
    }
}

public static class MiddlewareRegistration
{
    public static void ConfigurePipeline(IServiceCollection services)
    {
        services.AddEventBus();
        services.AddEventMiddleware<StopwatchLoggingMiddleware>();
    }
}
```

---

## Recipe 7: Native AOT Serialization and Deserialization without Reflection

### Problem
In applications compiled with Native AOT and Trimming enabled, reflection-based serialization fails at runtime.

### Solution
Use `System.Text.Json` Source Generators together with the dedicated converters provided in `EricksonLopez.Events.Serialization.SystemTextJson`.

### Complete Code
```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;

namespace MyProject.Serialization;

[EventName("finance.payments.received")]
[EventVersion(1)]
public sealed record PaymentReceivedIntegrationEvent(
    EventId Id,
    string PaymentRef,
    decimal Amount,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Converters = [
        typeof(EventIdJsonConverter),
        typeof(EventTypeJsonConverter),
        typeof(EventVersionJsonConverter),
        typeof(CorrelationIdJsonConverter),
        typeof(CausationIdJsonConverter),
        typeof(TenantIdJsonConverter),
        typeof(EventMetadataJsonConverter)
    ])]
[JsonSerializable(typeof(EventId))]
[JsonSerializable(typeof(EventType))]
[JsonSerializable(typeof(EventVersion))]
[JsonSerializable(typeof(CorrelationId))]
[JsonSerializable(typeof(CausationId))]
[JsonSerializable(typeof(TenantId))]
[JsonSerializable(typeof(EventMetadata))]
[JsonSerializable(typeof(PaymentReceivedIntegrationEvent))]
[JsonSerializable(typeof(EventEnvelope<PaymentReceivedIntegrationEvent>))]
public sealed partial class FinanceJsonContext : JsonSerializerContext
{
}

public static class AotSerializationHelper
{
    public static string SerializeEnvelope(EventEnvelope<PaymentReceivedIntegrationEvent> envelope)
    {
        return JsonSerializer.Serialize(envelope, FinanceJsonContext.Default.EventEnvelopePaymentReceivedIntegrationEvent);
    }

    public static EventEnvelope<PaymentReceivedIntegrationEvent>? DeserializeEnvelope(string json)
    {
        return JsonSerializer.Deserialize(json, FinanceJsonContext.Default.EventEnvelopePaymentReceivedIntegrationEvent);
    }
}
```

---

## Recipe 8: Interoperability with CloudEvents v1.0

### Problem
Events must be sent through industry-standard brokers (Knative, Azure Event Grid, Apache Kafka, AWS EventBridge) following the CloudEvents v1.0 specification.

### Solution
Use the `ToCloudEvent()` and `ToEventEnvelope()` extension methods from `EricksonLopez.Events.CloudEvents`.

### Complete Code
```csharp
using System;
using EricksonLopez.Events.CloudEvents;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

namespace MyProject.CloudEventsIntegration;

public sealed record SensorReadingEvent(EventId Id, string DeviceId, double Temperature, DateTimeOffset OccurredAt) : IEvent;

public static class CloudEventsRecipe
{
    public static CloudEvent<SensorReadingEvent> ExportToCloudEvent(SensorReadingEvent payload)
    {
        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithSource("https://iot.mycompany.com/sensors/sensor-01")
            .WithTenantId(TenantId.From("tenant-iot-primary"))
            .Build();

        var envelope = EventEnvelope.Create(payload, metadata);

        // Conversion to CloudEvents v1.0
        CloudEvent<SensorReadingEvent> cloudEvent = envelope.ToCloudEvent(
            schemaBaseUri: new Uri("https://schemas.mycompany.com/"));

        return cloudEvent;
    }

    public static EventEnvelope<SensorReadingEvent> ImportFromCloudEvent(CloudEvent<SensorReadingEvent> cloudEvent)
    {
        // Reverse conversion
        return cloudEvent.ToEventEnvelope();
    }
}
```

---

## Recipe 9: Isolated Unit Tests with the Testing DSL

### Problem
Testing application services and aggregates that emit events without needing to spin up dependency injection containers or complex mocks.

### Solution
Use `FakeEventPublisher`, `TestEventHandler<T>`, and `EventTestBuilder` provided in `EricksonLopez.Events.Testing`.

### Complete Code
```csharp
using System;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Testing;

namespace MyProject.Tests;

public sealed record UserBannedEvent(EventId Id, string UserId, string Reason, DateTimeOffset OccurredAt) : IEvent;

public sealed class UserManagementService
{
    private readonly IEventPublisher _publisher;

    public UserManagementService(IEventPublisher publisher) => _publisher = publisher;

    public async Task BanUserAsync(string userId, string reason)
    {
        var evt = new UserBannedEvent(EventId.New(), userId, reason, DateTimeOffset.UtcNow);
        await _publisher.PublishAsync(evt);
    }
}

public static class TestingRecipe
{
    public static async Task TestUserBanPublishing()
    {
        // 1. Arrange
        var fakePublisher = new FakeEventPublisher();
        var service = new UserManagementService(fakePublisher);

        // 2. Act
        await service.BanUserAsync("user_456", "Violation of terms");

        // 3. Fluent Assert
        fakePublisher
            .ShouldHavePublished<UserBannedEvent>()
            .ShouldHavePublished<UserBannedEvent>(e => e.UserId == "user_456" && e.Reason == "Violation of terms")
            .ShouldHavePublishedCount<UserBannedEvent>(1);
    }
}
```

---

## Recipe 10: Transactional Integration with Outbox and Idempotent Inbox

### Problem
Atomically guarantee event publishing when saving changes to the database (Transactional Outbox) and ensure the receiver processes each event exactly once (Idempotent Inbox).

### Solution
Use `OutboxEventPublisher` from `EricksonLopez.Events.Outbox` and `IdempotentEventHandler<T>` from `EricksonLopez.Events.Inbox`.

### Complete Code
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Inbox;
using EricksonLopez.Events.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Reliability;

public sealed record OrderCompletedEvent(EventId Id, Guid OrderId, DateTimeOffset OccurredAt) : IEvent;

public sealed class OrderCompletedHandler : IEventHandler<OrderCompletedEvent>
{
    public ValueTask HandleAsync(OrderCompletedEvent eventInstance, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Processing order idempotently: {eventInstance.OrderId}");
        return ValueTask.CompletedTask;
    }
}

public static class OutboxInboxConfiguration
{
    public static void ConfigureReliableMessaging(IServiceCollection services)
    {
        // Outbox Publisher
        services.AddOutboxEventPublisher();

        // Idempotent Inbox Handler
        services.AddIdempotentEventHandler<OrderCompletedEvent, OrderCompletedHandler>();
    }
}
```

---

## Cross-Cutting Best Practices Summary

1. **Absolute Immutability**: Always define event contracts as `sealed record` or `readonly record struct`.
2. **Native Guid v7**: Use `EventId.New()` to ensure temporal ordering and efficient database indexes.
3. **Zero Reflection**: Use the AOT System.Text.Json converters and the Source Generator for native compilation without Trimming warnings.
4. **Layer Separation**: Keep `IDomainEvent` in Domain, `IIntegrationEvent` in Application, and middlewares/converters in Infrastructure.
