# Level 07 — Scalability & Performance: Zero-Allocation Spans

> **Showcase Level 7** | Reference: `ECommerce.App/Program.cs` (`RunLevel7ScalabilityAndPerformanceAsync`)

---

## 1. Overview

In Level 07, we explore the low-level mechanical sympathy and zero-allocation characteristics of `EricksonLopez.Events`. We demonstrate how `EventId` implements `ISpanFormattable` and `IUtf8SpanFormattable` to format directly into stack-allocated character and byte buffers (`Span<char>`, `Span<byte>`), completely bypassing the heap and Garbage Collector.

---

## 2. Zero-Allocation Character Formatting: Span<char>

```csharp
using System;
using System.Globalization;
using EricksonLopez.Events.Identifiers;

var eventId = EventId.New();

// Allocate 36 characters directly on the execution stack (0 bytes heap)
Span<char> charBuffer = stackalloc char[36];

if (eventId.TryFormat(charBuffer, out int charsWritten, default, CultureInfo.InvariantCulture))
{
    Console.WriteLine($"Formatted to stack span: {charBuffer.ToString()} ({charsWritten} characters, 0B GC alloc)");
}
```

---

## 3. Zero-Allocation UTF-8 Byte Formatting: Span<byte>

For high-speed HTTP, gRPC, and socket serialization:

```csharp
using System;
using System.Globalization;
using EricksonLopez.Events.Identifiers;

var eventId = EventId.New();

// Allocate 36 UTF-8 bytes on the stack
Span<byte> utf8Buffer = stackalloc byte[36];

if (eventId.TryFormat(utf8Buffer, out int bytesWritten, default, CultureInfo.InvariantCulture))
{
    Console.WriteLine($"Formatted to UTF-8 stack span: {bytesWritten} bytes written (0B GC alloc)");
}
```

---

## 4. Monotonic Database Indexing: GUID v7 vs GUID v4

Standard `Guid.NewGuid()` generates random Version 4 UUIDs that cause massive B-Tree index fragmentation in relational databases (SQL Server, PostgreSQL). 

In contrast, `EventId.New()` uses RFC 9562 Version 7:
- **Timestamp Prefix**: Millisecond-precision Unix timestamp in the most significant bits.
- **Monotonic Sequence**: Monotonically ordered within the same millisecond.
- **Zero B-Tree Page Splits**: New records are appended cleanly to the right side of the clustered index.

```csharp
using System.Threading;
using EricksonLopez.Events.Identifiers;

var id1 = EventId.New();
Thread.Sleep(2);
var id2 = EventId.New();

// Natural chronological sorting
bool isSorted = id1 < id2; // Guaranteed true
```
