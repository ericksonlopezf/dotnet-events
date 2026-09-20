# Progressive Showcase Guide (Levels 0 to 10)

This guide documents the official reference implementation of the repository in `samples/ECommerce.Sample` and `samples/NativeAotSample`, demonstrating sequentially and progressively how to adopt **EricksonLopez.Events** from elementary concepts to advanced Native AOT enterprise patterns.

---

## Showcase Project Structure

```
samples/
├── ECommerce.Sample/
│   ├── ECommerce.Domain/         # Level 1: Aggregate Roots, Domain Events with Guid v7
│   ├── ECommerce.Application/    # Levels 2, 3: Integration Events, Application Services, Handlers
│   ├── ECommerce.Infrastructure/ # Levels 4, 8, 10: Middlewares, Outbox, AOT System.Text.Json Context
│   └── ECommerce.App/            # Levels 0-10: Interactive reference executable
└── NativeAotSample/              # Level 10: Native AOT & Trimming smoke test with Source Generators
```

---

## Progressive Levels Matrix

| Level | Name | Key Concepts | API Involved | Document | Project |
|---|---|---|---|---|---|
| **0** | **Conceptual Architecture** | DDD, capability boundaries, zero mediator | `IEvent`, `IDomainEvent`, `IIntegrationEvent` | [Level 00](showcase/level-00-conceptual-architecture.md) | Architecture |
| **1** | **Quick Start** | Event emission in Aggregate, temporal ordering | `EventId.New()`, `Guid.CreateVersion7()` | [Level 01](showcase/level-01-quick-start.md) | `ECommerce.Domain` |
| **2** | **Full Configuration** | Contextual metadata, Frozen headers | `EventMetadataBuilder`, `EventEnvelope.Create` | [Level 02](showcase/level-02-full-configuration.md) | `ECommerce.Application` |
| **3** | **Real Use Cases** | Application flow, in-memory publisher/subscriber | `InMemoryEventPublisher`, `IEventSubscriber` | [Level 03](showcase/level-03-real-use-cases.md) | `ECommerce.App` |
| **4** | **Advanced Integration** | Dependency injection, async dispatch | `AddEventBus`, `AddEventHandler`, `IEventBus` | [Level 04](showcase/level-04-advanced-integration.md) | `ECommerce.App` |
| **5** | **Processing** | Concurrency strategies, all registry overloads, cancellation, Reset | `EventExecutionMode`, `StaticEventTypeRegistry`, `TryGetDescriptor` (all overloads) | [Level 05](showcase/level-05-processing.md) | `ECommerce.App` |
| **6** | **Error Handling** | Resilience and exception aggregation | `ErrorHandlingPolicy`, `EventDispatchException` | [Level 06](showcase/level-06-error-handling.md) | `ECommerce.App` |
| **7** | **Scalability & Performance** | Zero-allocation Spans, monotonic ordering | `TryFormat(Span<char>)`, `TryFormat(Span<byte>)` | [Level 07](showcase/level-07-scalability.md) | `ECommerce.App` |
| **8** | **Customization** | Middleware pipeline, Parallel mode, ScopePolicy, custom strategies | `IEventMiddleware`, `EventExecutionMode.Parallel`, `MaxDegreeOfParallelism`, `HandlerScopePolicy.ReuseAmbientScope`, `ParallelExecutionStrategy` | [Level 08](showcase/level-08-customization.md) | `ECommerce.Infrastructure` |
| **9** | **Ecosystem Extensions** | CloudEvents v1.0, Testing DSL, OpenTelemetry | `ToCloudEvent()`, `FakeEventPublisher`, `TestEventHandler` | [Level 09](showcase/level-09-ecosystem-extensions.md) | `ECommerce.App` |
| **10** | **Enterprise Architecture** | 100% Native AOT, reflection-free serialization | `JsonSerializerContext`, `EventsJsonSerializerOptionsExtensions` | [Level 10](showcase/level-10-enterprise-architecture.md) | `ECommerce.Infrastructure` |
| **11** | **Comprehensive Coverage** | All remaining API surface: attributes, diagnostics, transactional, context | `EventSourceAttribute`, `EventBusDiagnostics`, `TransactionalEventPublisher`, `EventContext`, `SequentialExecutionStrategy` | — | `ECommerce.App` |

---

## Level Details

### Level 0: Conceptual Architecture
Establishes the canonical boundaries of the library:
- **`EricksonLopez.Events`**: Dispatches domain and integration events **within the process**, with zero reflection and 100% Native AOT.
- **`EricksonLopez.Messaging`**: Distributed message transport to external brokers (Kafka, RabbitMQ, Azure Service Bus).

### Level 1: Quick Start — Pure Domain Events
Creates an `Order` entity that emits an `OrderPlacedDomainEvent`:
```csharp
var orderId = OrderId.New();
var customerId = CustomerId.New();
var order = new Order(orderId, customerId, Money.USD(299.99m));
// order.DomainEvents contains the event with EventId (monotonic Guid v7)
```

### Level 2: Full Configuration — Metadata & EventEnvelope
Composes enriched ambient metadata without altering the domain payload contract:
```csharp
var metadata = new EventMetadataBuilder()
    .WithCorrelationId(correlationId)
    .WithCausationId(causationId)
    .WithTenantId(tenantId)
    .WithSource("ecommerce-ordering")
    .WithHeader("X-Client-Version", "v2.5.0")
    .Build();

var envelope = EventEnvelope.Create(integrationEvent, metadata);
```

### Level 3: Real Use Cases — Application Flow
The `OrderApplicationService` application service processes the command, persists the aggregate, and routes the integration event to the Outbox:
```csharp
var placedOrderId = await orderService.PlaceOrderAsync(customerId, 599.00m, correlationId);
```

### Level 4: Advanced Integration — Microsoft DI & EventBus
Configures the Microsoft dependency injection container:
```csharp
services.AddEventBus(options => {
    options.ExecutionMode = EventExecutionMode.Sequential;
    options.ErrorPolicy = ErrorHandlingPolicy.FailFast;
});
services.AddEventHandler<OrderPlacedIntegrationEvent, SendOrderConfirmationEmailHandler>();
services.AddEventHandler<OrderPlacedIntegrationEvent, UpdateInventoryOnOrderPlacedHandler>();
```

### Level 5: Processing Strategies, Registries & Cancellation
Covers all registry overloads:
- `TryGetDescriptor(EventType, out)` — by semantic name
- `TryGetDescriptor<TEvent>(out)` — generic, AOT-preferred
- `TryGetDescriptor(Type, out)` — non-generic CLR type overload
- `TryGetDescriptor(EventType, EventVersion, out)` — versioned lookup
- `StaticEventTypeRegistry.Reset()` — test-isolation API

### Level 6: Resilience and Error Handling
Demonstrates how `ErrorHandlingPolicy.AggregateAndContinue` ensures that if one handler fails, the other subscribers continue executing, grouping the errors in `EventDispatchException`.

### Level 7: Zero-Allocation Performance
Demonstrates direct formatting of `EventId` over `Span<char>` and `Span<byte>` buffers allocated on the stack (`stackalloc`) without generating Garbage Collector pressure.

### Level 8: Customization — Middleware, Parallel Mode & ScopePolicy
Demonstrates:
- `IEventMiddleware` pipeline (Logging → Performance → Handler)
- `MiddlewarePipeline.Build<T>()` explicit construction
- `EventExecutionMode.Parallel` + `MaxDegreeOfParallelism` with thread-safe handlers
- `HandlerScopePolicy.ReuseAmbientScope` with Sequential mode (+ thread-safety warning)
- `ParallelExecutionStrategy` instantiated directly

### Level 9: Ecosystem Extensions
- **CloudEvents v1.0**: Converts `EventEnvelope<T>` to the CNCF standard specification via `.ToCloudEvent()` and its inverse `.ToEventEnvelope()`.
- **Testing DSL**: Verifies fluent assertions with `FakeEventPublisher` and `TestEventHandler<T>` without external mocking dependencies.

### Level 10: Native AOT & Trimming
Serializes and deserializes event envelopes using `System.Text.Json.JsonSerializerContext` with zero runtime reflection.

### Level 11: Comprehensive API Coverage
Executable coverage of every remaining public API surface member:
- `EventSourceAttribute` — construction and reflection-based introspection
- `EventBusDiagnostics.RecordPublish`
- `TransactionalEventPublisher` (full lifecycle: Publish, CommitAndPublishAsync, Rollback)
- `EventContext.SetExecutionTracker`, `MarkHandlerCompleted`, `IsHandlerCompleted`
- `SequentialExecutionStrategy` / `HandlerRegistry` / `HandlerDescriptor`
- All JSON converters: `EventEnvelopeJsonConverterFactory`, `EventEnvelopeJsonConverter<T>`

> **External integrations** (out of scope — separate packages):
> - `EricksonLopez.Outbox` → `OutboxEventPublisher`, transactional relay
> - `EricksonLopez.Inbox` → idempotency / inbox processor

---

## How to Run the Showcase

```bash
# Run the full interactive ECommerce Showcase
dotnet run --project samples/ECommerce.Sample/ECommerce.App

# Run the Native AOT Smoke Test
dotnet run --project samples/NativeAotSample
```
