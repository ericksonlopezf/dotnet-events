# Level 02 — Full Configuration: Metadata & Event Envelopes

> **Showcase Level 2** | Reference: `ECommerce.Application/Application.cs` & `ECommerce.App/Program.cs` (`RunLevel2FullConfigurationAsync`)

---

## 1. Overview

In Level 02, we enrich domain events with ambient context without polluting the core domain model. We package payloads into strongly typed `EventEnvelope<TEvent>` instances carrying distributed tracing IDs (`CorrelationId`, `CausationId`), multi-tenant boundaries (`TenantId`), and contextual headers.

---

## 2. Composing Metadata with EventMetadataBuilder

`EventMetadataBuilder` provides a fluent, zero-allocation builder pattern supporting strongly typed value objects, primitive strings, and `Guid` values:

```csharp
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

var metadata = new EventMetadataBuilder()
    .WithCorrelationId(CorrelationId.New())
    .WithCausationId("CMD-ORDER-CREATE-9871")
    .WithTenantId(TenantId.From("tenant-enterprise-alpha"))
    .WithSource("https://ordering.ecommerce.internal")
    .WithHeader("X-Client-Version", "3.2.1")
    .WithHeader("X-Environment", "Production")
    .Build();
```

---

## 3. Packaging into EventEnvelope<TEvent>

`EventEnvelope<TEvent>` bundles the business payload with complete technical metadata:

```csharp
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Metadata;
using ECommerce.Domain;

var domainEvent = new OrderPlacedDomainEvent(
    EventId.New(),
    OrderId.New(),
    CustomerId.New(),
    Money.USD(450.00m),
    DateTimeOffset.UtcNow);

// Factory method with metadata
var envelope = EventEnvelope.Create(domainEvent, metadata);

// Direct access to properties
EventId id = envelope.Id;
EventType type = envelope.Type;           // Derived automatically from CLR type or attribute
EventVersion version = envelope.Version; // Default v1 or from [EventVersion]
DateTimeOffset occurredAt = envelope.OccurredAt;
OrderPlacedDomainEvent payload = envelope.Payload;
EventMetadata meta = envelope.Metadata;

// Polymorphic access via non-generic IEventEnvelope
IEventEnvelope polymorphic = envelope;
object rawPayload = polymorphic.GetPayload();
```

---

## 4. Alternative Envelope Factories

```csharp
// 1. Factory with default empty metadata
var defaultEnvelope = EventEnvelope.Create(domainEvent);

// 2. Strict wrapping with existing metadata
var wrappedEnvelope = EventEnvelope.Wrap(domainEvent, metadata);

// 3. Direct constructor instantiation with explicit overrides
var explicitEnvelope = new EventEnvelope<OrderPlacedDomainEvent>(
    EventId.New(),
    EventType.From("custom.order.placed"),
    EventVersion.From(2),
    DateTimeOffset.UtcNow,
    domainEvent,
    metadata);
```
