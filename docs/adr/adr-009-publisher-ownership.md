# ADR-009: Event Publication and Dispatching Contracts

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Applications produce events and need an abstraction to publish them either in-memory to local subscribers, to an outbox buffer, or onto an external message bus.

## Problem
What contracts should be defined for event publication, and how do we prevent the publisher interface from hiding multiple hidden responsibilities (serialization, database writes, network calls, retries)?

## Options
1. **Fat `IPublisher` interface:** Includes `PublishToTopicAsync`, `PublishWithRetryAsync`, `PublishToOutboxAsync`.
2. **Minimal In-Memory Publisher Contract (`IEventPublisher`):**
   ```csharp
   public interface IEventPublisher
   {
       ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
           where TEvent : IEvent;
   }
   ```

## Decision
Adopt **Option 2**. We provide a lean, single-purpose `IEventPublisher` contract. It represents only the in-process notification dispatching abstraction. Persistence, outbox buffering, and network transportation are separate concerns handled by dedicated implementations (e.g. `EricksonLopez.Outbox` or transport adapters).

## Rationale
- Adheres strictly to the Single Responsibility Principle (SRP).
- Avoids coupling the core interface to external infrastructure concerns.
- Consumers can easily mock, decorate, or implement `IEventPublisher` for domain events.

## Consequences
- **Positive:** Unambiguous semantics, easy testing, zero infrastructure coupling.
- **Negative:** Transport-specific publish operations (e.g., Kafka keying, partition headers) are configured in transport adapters.

## Rejected Alternatives
- Fat multi-method publisher interfaces were rejected.
