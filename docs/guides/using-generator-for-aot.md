# Guide: Using the Roslyn Incremental Generator for Native AOT

`EricksonLopez.Events.Generators` is a compile-time Roslyn Source Generator that discovers your event types and creates an immutable, zero-reflection event registry (`GeneratedEventRegistry`).

## 1. Why Use the Generator?

Under Native AOT and Trimming, runtime reflection (such as calling `typeof(T).GetCustomAttributes()`) can fail or cause trimming warnings (`IL2026`, `IL3050`). 

By referencing the source generator:
1. All event metadata (`[EventName]`, `[EventVersion]`, `[EventSource]`) is resolved at **compile time**.
2. A static class `GeneratedEventRegistry` is generated directly into your assembly.
3. Your application starts with **zero reflection overhead** and **100% Native AOT compliance**.

---

## 2. Installation & Configuration

Add the generator package to your project with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\EricksonLopez.Events.Generators\EricksonLopez.Events.Generators.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

Or when consuming via NuGet:

```xml
<PackageReference Include="EricksonLopez.Events.Generators" Version="1.0.0" PrivateAssets="all" />
```

---

## 3. Defining Events

The generator automatically discovers any `public` type that implements `IEvent`, `IDomainEvent`, or `IIntegrationEvent` (from the `EricksonLopez.Events.Contracts` namespace) or is decorated with `[EventName]`:

```csharp
namespace MyApp.Domain.Events;

using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

[EventName("orders.order-placed")]
[EventVersion(1)]
[EventSource("ordering-service")]
public sealed record OrderPlacedEvent(
    EventId Id,
    Guid OrderId,
    decimal Amount,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
```

---

## 4. Application Startup Registration

At application startup (e.g., in `Program.cs`), initialize `StaticEventTypeRegistry.Current`:

```csharp
using EricksonLopez.Events.Generated;
using EricksonLopez.Events.Registry;

// One-line registration of all compile-time discovered event descriptors
StaticEventTypeRegistry.Current = GeneratedEventRegistry.CreateRegistry();
```

---

## 5. Verification Under Native AOT

You can build and publish your application with strict Native AOT verification:

```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

With `GeneratedEventRegistry` registered, all lookups via `StaticEventTypeRegistry.GetDescriptor<TEvent>()` and `StaticEventTypeRegistry.GetEventType<TEvent>()` execute in O(1) time without reflection warnings.
