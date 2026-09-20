# Level 10 — Enterprise Architecture: Native AOT & Zero-Reflection Serialization

> **Showcase Level 10** | Reference: `ECommerce.Infrastructure/Infrastructure.cs` & `ECommerce.App/Program.cs` (`RunLevel10EnterpriseAotAsync`)

---

## 1. Overview

In Level 10, we achieve the enterprise pinnacle: **100% Native AOT trimming safety with zero runtime reflection**. We demonstrate how compile-time Roslyn Source Generators and `System.Text.Json` source generation contexts eliminate reflection-based assembly scanning, dynamic invokers, and serializer caches.

---

## 2. Roslyn Incremental Source Generators (EricksonLopez.Events.Generators)

When `EricksonLopez.Events.Generators` is referenced, the Roslyn compiler automatically inspects types implementing `IEvent` (with `[EventName]`/`[EventVersion]`) and `IEventHandler<TEvent>`. It generates compile-time code:

- `GeneratedEventRegistry.CreateRegistry()`: Static `IEventTypeRegistry` initialization.
- `GeneratedEventServiceCollectionExtensions.AddGeneratedEventHandlers()`: Direct registration of all discovered handlers in DI.

```csharp
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Events.Bus.Extensions;

var services = new ServiceCollection();

// Discovers and registers all handlers at compile-time with zero runtime reflection
services.AddEventBus();
// services.AddGeneratedEventHandlers(); // Emitted by Roslyn Source Generator
```

---

## 3. Roslyn Diagnostic Analyzers

The generator package enforces architectural invariants during compilation:

| Rule ID | Name | Severity | Description |
|---|---|---|---|
| **`ELEVT001`** | Domain Event Leak | Error | Prevents `IDomainEvent` implementations from being decorated with `[EventName]` (only `IIntegrationEvent` can cross service boundaries). |
| **`ELEVT002`** | Attribute Validation | Error | Enforces valid, non-empty event names and positive versions on `[EventName]` and `[EventVersion]`. |
| **`ELEVT003`** | Immutability Enforcement | Error | Flags mutable fields or properties on events. Events must be immutable `sealed record` or `readonly struct`. |

---

## 4. Native AOT JSON Serialization Context

To serialize event envelopes under Native AOT without reflection, define a `JsonSerializerContext` registering all event types and the library's custom converters:

```csharp
using System.Text.Json.Serialization;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;
using ECommerce.Application;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    Converters = [
        typeof(EventIdJsonConverter),
        typeof(EventTypeJsonConverter),
        typeof(EventVersionJsonConverter),
        typeof(CorrelationIdJsonConverter),
        typeof(CausationIdJsonConverter),
        typeof(TenantIdJsonConverter),
        typeof(EventMetadataJsonConverter)
    ])]
[JsonSerializable(typeof(EventEnvelope<OrderPlacedIntegrationEvent>))]
[JsonSerializable(typeof(EventEnvelope<OrderShippedIntegrationEvent>))]
[JsonSerializable(typeof(EventEnvelope<CustomerRegisteredIntegrationEvent>))]
internal sealed partial class ECommerceJsonContext : JsonSerializerContext
{
}
```

---

## 5. Zero-Reflection Serialization & Deserialization

```csharp
using System;
using System.Text.Json;
using EricksonLopez.Events.Envelopes;
using ECommerce.Application;
using ECommerce.Infrastructure;

var envelope = EventEnvelope.Create(integrationEvent, metadata);

// 1. Serialize using AOT TypeInfo (Zero reflection)
string json = JsonSerializer.Serialize(envelope, ECommerceJsonContext.Default.EventEnvelopeOrderPlacedIntegrationEvent);

// 2. Deserialize using AOT TypeInfo (Zero reflection)
var deserialized = JsonSerializer.Deserialize(json, ECommerceJsonContext.Default.EventEnvelopeOrderPlacedIntegrationEvent);

Console.WriteLine($"Deserialized ID: {deserialized?.Id}");
Console.WriteLine($"Deserialized Type: {deserialized?.Type}");
Console.WriteLine($"Deserialized Tenant: {deserialized?.Metadata.TenantId}");
```

---

## 6. Global Converter Extensions

For standard or hybrid scenarios, `EventsJsonSerializerOptionsExtensions` provides pre-configured options:

```csharp
using System.Text.Json;
using EricksonLopez.Events.Serialization.SystemTextJson;

// Configures standard System.Text.Json options with all 8 event converters
var options = EventsJsonSerializerOptionsExtensions.CreateDefaultOptions();
// or: new JsonSerializerOptions().AddEventsConverters();
```
