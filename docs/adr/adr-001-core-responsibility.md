# ADR-001: Core Responsibility and Ecosystem Boundaries

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
In modern .NET distributed systems and Clean Architecture solutions, event modeling is frequently conflated with messaging frameworks (e.g. MassTransit, NServiceBus, Wolverine) or in-process mediator pipelines (e.g. MediatR). This leads to accidental coupling between pure domain logic, transport wire formats, serialization engines, and persistence layers.

## Problem
How can the `EricksonLopez.*` ecosystem define contracts and primitives for events that remain lightweight, 100% Native AOT-compatible, zero-allocation oriented, and completely independent of brokers, ORMs, and serialization frameworks?

## Options
1. **Monolithic Messaging Framework:** Combine event contracts, RabbitMQ/Kafka transport, outbox storage, and mediator pipeline in one package.
2. **Minimal Contracts & Primitives Package (`EricksonLopez.Events`):** Exclusively provide domain event contracts, integration event contracts, typed identifiers (`EventId`, `EventType`, `EventVersion`), typed metadata (`EventMetadata`), and envelopes (`EventEnvelope<TEvent>`).

## Decision
Adopt **Option 2**. `EricksonLopez.Events` is strictly a foundational contracts and primitives library. It does NOT implement message broker transports, distributed locks, retry engines, or outbox persistence.

## Rationale
- Keeps domain layers clean and independent of infrastructure.
- Enables Native AOT and trimming without runtime reflection.
- Guarantees long-term API stability.

## Consequences
- **Positive:** Zero third-party external dependencies; sole dependencies are Microsoft.Extensions Abstractions packages (`Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`) that are BCL-adjacent and universally present in .NET applications; predictable binary size; zero trimming warnings.
- **Negative:** Transport adapters and persistence must be provided by sibling packages or consumer infrastructure.

## Rejected Alternatives
- Embedding RabbitMQ or MassTransit abstractions into domain event contracts was rejected due to architectural pollution and runtime reflection overhead.
