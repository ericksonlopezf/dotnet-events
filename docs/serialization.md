# Serialization & NativeAOT Trimming Specifications

---

## 1. Zero-Reflection Serialization Architecture

`EricksonLopez.Events.Serialization.SystemTextJson` eliminates runtime reflection through `System.Text.Json` source generation.

```csharp
using System.Text.Json.Serialization;
using EricksonLopez.Events.Contracts;

[JsonSerializable(typeof(EventEnvelope<IDomainEvent>))]
[JsonSerializable(typeof(EventEnvelope<IIntegrationEvent>))]
public partial class EventsJsonSerializerContext : JsonSerializerContext;
```

---

## 2. Trimming & AOT Compatibility Guarantees

- **No Polymorphic Reflection**: All event payloads are registered at compile-time via source generators or explicit JSON converter contexts.
- **Trimmer Root Descriptors**: Provided where required to prevent IL linker stripping of essential properties.
- **Zero Allocations**: Buffer pooling via `Utf8JsonWriter` ensures predictable memory consumption under heavy event ingestion.
