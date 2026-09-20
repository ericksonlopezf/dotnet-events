# Diagnostics and Troubleshooting Guide — EricksonLopez.Events

Guide to identify and resolve common errors when working with **EricksonLopez.Events**.

---

## 1. Roslyn Analyzer Diagnostic Codes

| Code | Error Message | Cause | Resolution |
|---|---|---|---|
| **ELE001** | `Event types must be immutable.` | A property of an `IEvent` type has a mutable public setter (`set;`) or the type is not a `readonly record struct` or `sealed record`. | Change the property to `{ get; init; }` or convert the type to a `readonly record struct` or `sealed record`. |
| **ELE002** | `[EventVersion] must be greater than or equal to 1.` | A non-positive version ($< 1$) was specified in `[EventVersion]`. | Event versions must be positive integers greater than or equal to 1. |
| **ELE003** | `[EventName] attribute argument cannot be null or whitespace.` | An empty, null, or whitespace-only string was passed to `[EventName("")]`. | Specify a valid semantic name (e.g. `"orders.order-placed"`). |
| **ELE004** | `[EventSource] attribute argument cannot be null or whitespace.` | An empty or null string was passed to `[EventSource("")]`. | Specify a valid source identifier or URI. |
| **ELE005** | `Integration event cannot leak domain event.` | An `IIntegrationEvent` has a property of type `IDomainEvent`. | Map the data to primitive types or DTOs instead of nesting domain events. |


---

## 2. Common Runtime Exceptions

### `EventDispatchException`
- **Cause**: Occurs when one or more handlers throw an exception during dispatch with the `ErrorHandlingPolicy.AggregateAndContinue` policy.
- **Diagnosis**: Inspect the `.InnerExceptions` property to get the full list of failures that occurred in each handler.

### `InvalidOperationException: Maximum reentrancy depth exceeded (10)`
- **Cause**: A handler publishes an event that directly or indirectly re-fires the same handler in an infinite call cycle.
- **Diagnosis**: Verify that publishing flows do not create circular calls, or adjust `options.MaxReentrancyDepth` in `AddEventBus(...)`.

### `NotSupportedException: Event type ... is not registered in AOT serializer context`
- **Cause**: When serializing in Native AOT, the event type or its generic envelope `EventEnvelope<T>` was not decorated with `[JsonSerializable]` in the `JsonSerializerContext`.
- **Diagnosis**: Add `[JsonSerializable(typeof(EventEnvelope<MyEvent>))]` to your class derived from `JsonSerializerContext`.

---

## 3. .NET Code Analysis Warnings (CA1305)

- **Cause**: Invocation of `TryFormat` or `ToString()` without specifying an `IFormatProvider`.
- **Resolution**: Explicitly pass `System.Globalization.CultureInfo.InvariantCulture` as the format argument.
