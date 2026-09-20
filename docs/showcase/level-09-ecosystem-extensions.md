# Level 09 — Ecosystem Extensions: CloudEvents, Testing & OpenTelemetry

> **Showcase Level 9** | Reference: `ECommerce.App/Program.cs` (`RunLevel9EcosystemExtensionsAsync`)

---

## 1. Overview

In Level 09, we demonstrate native integrations with standard distributed ecosystems:
1. **CNCF CloudEvents v1.0**: Interoperable JSON event envelopes for event-driven meshes.
2. **Testing DSL**: Comprehensive in-memory fakes and test doubles (`FakeEventPublisher`, `TestEventHandler<T>`) without external mocking libraries.
3. **OpenTelemetry**: Standard W3C distributed tracing and BCL runtime metrics.

---

## 2. CNCF CloudEvents v1.0 Transformation

Transform strongly typed `EventEnvelope<T>` instances directly into CNCF CloudEvents v1.0:

```csharp
using System;
using System.Text.Json;
using EricksonLopez.Events.CloudEvents;
using EricksonLopez.Events.CloudEvents.Serialization;
using EricksonLopez.Events.Envelopes;
using ECommerce.Application;

var envelope = EventEnvelope.Create(integrationEvent, metadata);

// 1. Convert to CloudEvent<T> specification
Uri sourceUri = new("https://orders.ecommerce.corp/v1");
CloudEvent<OrderPlacedIntegrationEvent> cloudEvent = envelope.ToCloudEvent(sourceUri);

// Verify CloudEvent attributes
string id          = cloudEvent.Id;
string type        = cloudEvent.Type;        // e.g. "ecommerce.orders.order-placed"
string specVersion = cloudEvent.SpecVersion; // "1.0"
DateTimeOffset time = cloudEvent.Time;

// 2. Serialize using CloudEvents JSON options
var jsonOptions = new JsonSerializerOptions().ConfigureForCloudEvents();
string cloudEventJson = JsonSerializer.Serialize(cloudEvent, jsonOptions);

// 3. Roundtrip back to EventEnvelope<T>
EventEnvelope<OrderPlacedIntegrationEvent> restored = cloudEvent.ToEventEnvelope();
```

---

## 3. Testing DSL: FakeEventPublisher & TestEventHandler<T>

Perform zero-mock assertions in unit and integration test suites:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Testing;
using ECommerce.Application;

// 1. FakeEventPublisher captures all publications
var fakeBus = new FakeEventPublisher();

await fakeBus.PublishAsync(integrationEvent, CancellationToken.None);

// Fluent assertions
fakeBus.AssertPublished<OrderPlacedIntegrationEvent>();
fakeBus.AssertPublishedTimes<OrderPlacedIntegrationEvent>(1);
fakeBus.AssertPublished<OrderPlacedIntegrationEvent>(e => e.TotalAmount > 100m);
fakeBus.AssertNotPublished<OrderShippedIntegrationEvent>();

// 2. TestEventHandler acts as a test spy
var spyHandler = new TestEventHandler<OrderPlacedIntegrationEvent>()
    .WithCallback(e => Console.WriteLine($"Processed order {e.OrderId}"));

await spyHandler.HandleAsync(integrationEvent, CancellationToken.None);

bool wasCalled = spyHandler.WasInvoked;          // True
int count      = spyHandler.InvocationCount;     // 1
var lastEvent  = spyHandler.LastEvent;           // integrationEvent
```

---

## 4. Synthetic Test Builders: EventEnvelopeTestBuilder<T>

Build synthetically populated envelopes for testing edge cases:

```csharp
using System;
using EricksonLopez.Events.Testing;
using ECommerce.Application;

var testEnvelope = EventTestBuilder.For(integrationEvent)
    .WithCorrelationId("corr-test-123")
    .WithTenantId("tenant-qa-01")
    .WithSource("test-runner")
    .WithHeader("X-Synthetic-Test", "true")
    .Build();
```

---

## 5. OpenTelemetry Distributed Tracing & Metrics

Wire up the OpenTelemetry SDK with `AddEventsInstrumentation`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using EricksonLopez.Events.OpenTelemetry;

var services = new ServiceCollection();

services.AddOpenTelemetry()
    .WithTracing(tracerBuilder => tracerBuilder
        .AddEventsInstrumentation() // Automatically subscribes to EricksonLopez.Events ActivitySource
        .AddConsoleExporter())
    .WithMetrics(meterBuilder => meterBuilder
        .AddEventsInstrumentation() // Automatically subscribes to EricksonLopez.Events Meter
        .AddConsoleExporter());
```
