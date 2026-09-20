# ADR-018: Outbox Integration Contract and Separation of Persistence

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
The Transactional Outbox pattern guarantees at-least-once delivery by writing events into an outbox database table in the same database transaction as business state changes. Background workers subsequently read and publish these events to external message brokers.

## Problem
What is the boundary between `EricksonLopez.Events` and `EricksonLopez.Outbox`?

## Options
1. **Include SQL/EF Core Outbox tables in Events:** Couples domain contracts to persistence technology.
2. **Strict Separation of Contracts and Persistence:**
   - `EricksonLopez.Events` provides `EventEnvelope<TEvent>`, `EventId`, `EventType`, `EventMetadata`, and serialization adapters.
   - `EricksonLopez.Outbox` defines the database schema, EF Core/Dapper persistence, transaction hooks, polling/CDC workers, and background publishing pipelines.

## Decision
Adopt **Option 2**. `EricksonLopez.Events` contains zero persistence logic. Outbox libraries store the serialized envelope (`EventEnvelope`) generated from `EricksonLopez.Events`.

## Rationale
- Keeps `EricksonLopez.Events` free from ORM or database driver dependencies.
- The same event envelope format works across relational databases (PostgreSQL, SQL Server), document databases (MongoDB), and key-value stores.

## Consequences
- **Positive:** Ultimate persistence independence.
- **Negative:** None.

## Rejected Alternatives
- Embedding EF Core or SQL schema migrations in `EricksonLopez.Events` was rejected.
