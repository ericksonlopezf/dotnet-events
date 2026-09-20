# Level 01 — Quick Start: Domain Events & Strongly Typed Identity

> **Showcase Level 1** | Reference: `ECommerce.Domain/Model.cs` & `ECommerce.App/Program.cs` (`RunLevel1QuickStartAsync`)

---

## 1. Overview

In Level 01, we define pure immutable domain events using the zero-dependency `EricksonLopez.Events.Contracts` assembly. We leverage RFC 9562 GUID Version 7 (`EventId`) for monotonic, chronologically sortable identifiers optimized for high-throughput database index insertions.

---

## 2. Installation

```bash
dotnet add package EricksonLopez.Events.Contracts
dotnet add package EricksonLopez.Events
```

---

## 3. Defining Domain Events

Domain events represent immutable facts that occurred within an Aggregate Root:

```csharp
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

namespace ECommerce.Domain;

public sealed record OrderPlacedDomainEvent(
    EventId Id,
    OrderId OrderId,
    CustomerId CustomerId,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent;
```

---

## 4. Emitting from an Aggregate Root

```csharp
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

namespace ECommerce.Domain;

public sealed class Order
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    public Money TotalAmount { get; }
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Order(OrderId id, CustomerId customerId, Money totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        TotalAmount = totalAmount;

        // Monotonically ordered Guid v7 identifier generated natively
        var domainEvent = new OrderPlacedDomainEvent(
            EventId.New(),
            id,
            customerId,
            totalAmount,
            DateTimeOffset.UtcNow);

        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

---

## 5. Verifying Identity Primitives

```csharp
using System;
using EricksonLopez.Events.Identifiers;

// Generate new monotonic Guid v7
EventId id1 = EventId.New();
EventId id2 = EventId.New();

// Time-based natural ordering (id1 was generated before id2)
bool isMonotonic = id1.CompareTo(id2) < 0; // True

// Empty identity guard
EventId empty = EventId.Empty;
bool isEmpty = empty.IsEmpty; // True

// Conversions
Guid rawGuid = id1.Value;
EventId fromGuid = EventId.From(rawGuid);
bool areEqual = id1 == fromGuid; // True
```
