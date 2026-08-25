# Event Identity: Native Guid Version 7 (on .NET 9+)

## 1. The Power of UUIDv7 in .NET 9+

`EricksonLopez.Events` uses `Guid.CreateVersion7()` as the default engine for `EventId` **on .NET 9 and later**. On .NET 8, `EventId.New()` falls back to `Guid.NewGuid()` (v4), which is random and not time-sortable.

### Why Guid Version 7?
1. **Monotonic Time-Ordering:** Encodes a 48-bit millisecond Unix timestamp in the high-order bits, followed by 74 bits of cryptographically secure random entropy. *(Available on .NET 9+ only.)*
2. **Database Performance:** Eliminates B-Tree index fragmentation and random page splits common with random UUIDv4.
3. **Zero Allocation:** `EventId` is a 16-byte `readonly record struct` with no heap allocations.
4. **Universal Interoperability:** Compatible with standard 128-bit UUID columns in PostgreSQL, SQL Server (`uniqueidentifier`), SQLite, MySQL, and binary/string storage in MongoDB and Redis.

```csharp
// Generates a time-sortable Guid v7 EventId
EventId id = EventId.New();

// Span-based zero-allocation string formatting
Span<char> buffer = stackalloc char[36];
id.TryFormat(buffer, out int charsWritten);

// Fast zero-allocation parsing
if (EventId.TryParse("018e38d6-3e4b-7a32-8419-7e9dfc01bc52", out var parsedId))
{
    // ...
}
```

---

# Event Type & Versioning

## `EventType`
`readonly record struct EventType(string Value)`:
- Validated naming format: `domain.entity.action` or `kebab-case`/`snake_case` hierarchy.
- Decouples message routing from CLR class names and namespaces.

## `EventVersion`
`readonly record struct EventVersion(uint Value)`:
- Represents monotonic contract version: `EventVersion.V1`, `EventVersion.From(2)`.
- Explicit version evolution:
  - **Additive / Tolerant Reader Changes (Non-breaking):** Retain version `1`, mark optional properties.
  - **Breaking Structural Transformations:** Increment to version `2` (e.g. `OrderPlacedIntegrationEventV2`).
