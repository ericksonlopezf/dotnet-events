# ADR-004: Explicit Event Type Identity vs. CLR Type Name

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Distributed event-driven systems need to identify the semantic type of an event payload across programming languages, runtimes, and refactorings. Relying on CLR `Type.AssemblyQualifiedName` or `Type.FullName` couples consumers to .NET internal namespaces and breaks when refactoring or interacting with non-.NET services. Furthermore, Native AOT trimming removes unreferenced CLR type names.

## Problem
How should event types be identified and represented in contracts and on the wire?

## Options
1. **CLR AssemblyQualifiedName:** Fragile, verbose, leaks internal implementation details, incompatible with safe trimming and polyglot systems.
2. **`readonly record struct EventType(string Value)` with explicit naming (e.g. `billing.invoice.issued`):** Immutable, normalized, human-readable URN-like naming convention decoupled from code structure.

## Decision
Adopt **Option 2**. We provide `readonly record struct EventType` and an attribute `[EventName("domain.entity.action")]`. Event types are defined using a structured format (e.g., `orders.order-created`, `billing.payment-processed`).

## Rationale
- Decouples wire contracts from C# namespaces and class renames.
- Facilitates polyglot consumer routing (e.g., in Kafka topics, RabbitMQ exchange routing keys, CloudEvents `type`).
- 100% Native AOT-safe; type mapping is resolved at compile time or via static registry.

## Consequences
- **Positive:** Refactoring C# classes never breaks event serialization or routing contracts.
- **Negative:** Requires declaring event type names explicitly for integration events.

## Rejected Alternatives
- CLR-based dynamic reflection resolution (`Type.GetType`) was completely rejected as anti-AOT and fragile.
