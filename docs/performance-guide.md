# Performance and Zero-Allocation Guide — EricksonLopez.Events

Design strategies and characteristics that enable **EricksonLopez.Events** to achieve sub-microsecond latencies and zero Garbage Collector pressure.

---

## 1. `readonly record struct` Identifiers

- **`EventId`**: 16-byte `readonly record struct` wrapping a `Guid` — fully stack-allocated, zero heap pressure.
- **`EventType`, `EventVersion`**: Lightweight struct wrappers; `EventType` wraps a `string` (heap reference pointer); `EventVersion` wraps a `uint` (stack-allocated).
- **`CorrelationId`, `CausationId`, `TenantId`**: `readonly record struct` wrappers around a `string` reference (8-byte pointer on 64-bit). The struct itself is stack-allocated; the underlying string value lives on the heap.
- Zero heap allocations when copying or passing the struct itself as an argument.

---

## 2. Direct Formatting with `Span<T>`

`EventId` implements `ISpanFormattable` and `IUtf8SpanFormattable`. This allows converting identifiers to strings or UTF-8 streams directly into stack memory (`stackalloc`):

```csharp
// 1. Format to Span<char> without string allocation
Span<char> charBuffer = stackalloc char[36];
eventId.TryFormat(charBuffer, out int charsWritten, default, CultureInfo.InvariantCulture);

// 2. Format directly to UTF-8 bytes for sockets or HTTP buffers
Span<byte> utf8Buffer = stackalloc byte[36];
eventId.TryFormat(utf8Buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
```

---

## 3. Frozen Immutable Collections (`FrozenDictionary`)

`EventMetadata.CustomHeaders` internally uses `FrozenDictionary<string, string>`, introduced in .NET 8.
- Compiler-optimized O(1) lookups with perfect hash tables.
- Strict immutability guaranteed by the runtime.

---

## 4. Reflection-Free Dispatch (`HandlerDescriptor`)

The `EventBus` stores pre-compiled static invocation delegates inside each `HandlerDescriptor`:
```csharp
static (handlerInstance, eventInstance, ct) =>
    ((IEventHandler<TEvent>)handlerInstance).HandleAsync((TEvent)eventInstance, ct)
```
- No `MethodInfo.Invoke` or `DynamicInvoke` is performed.
- Direct invocations as fast as native virtual method calls.

---

## 5. `ValueTask` Performance

All `HandleAsync` and `PublishAsync` methods return `ValueTask` instead of `Task`.
- In synchronous flows or handlers that complete immediately (e.g. `ValueTask.CompletedTask`), no `Task` object is allocated on the heap.
