# Level 06 — Source Generators & NativeAOT Registries

In Level 06, we eliminate runtime reflection using compile-time Roslyn code generation with `EricksonLopez.Events.Generators`.

---

## 1. Declarative Event Registry Generation

```csharp
using EricksonLopez.Events.Generators;

[EventRegistry]
[RegisterEvent(typeof(OrderPlacedDomainEvent), "orders.placed.v1")]
[RegisterEvent(typeof(OrderShippedIntegrationEvent), "orders.shipped.v1")]
public partial class AppEventRegistry;
```

---

## 2. Compile-Time Type Mapping

The Roslyn generator outputs a static, zero-reflection dispatcher dictionary:

```csharp
// Source-generated output
public partial class AppEventRegistry : IEventRegistry
{
    public Type? ResolveType(string eventType) => eventType switch
    {
        "orders.placed.v1" => typeof(OrderPlacedDomainEvent),
        "orders.shipped.v1" => typeof(OrderShippedIntegrationEvent),
        _ => null
    };
}
```

This guarantees 100% NativeAOT trimming safety (`EnableTrimAnalyzer=true`) and zero dynamic assembly scanning overhead.
