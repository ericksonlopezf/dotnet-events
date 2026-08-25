# ADR-012: Roslyn Incremental Source Generator for Static Descriptors

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
When building event-driven applications, systems need to discover available event types, their string names, versions, and JSON serialization contexts without scanning assemblies at runtime via reflection.

## Problem
How can event discovery and type registration be achieved without runtime reflection while keeping developer experience ergonomic?

## Options
1. **Manual Registration Only:** Developers write boilerplate static registries by hand.
2. **Roslyn Incremental Source Generator (`EricksonLopez.Events.Generators`):** An incremental generator analyzes event types (classes/records implementing `IEvent`, `IDomainEvent`, `IIntegrationEvent` or annotated with `[EventName]`, `[EventVersion]`) and generates static descriptors (`EventTypeDescriptor`), a compile-time `EventTypeRegistry`, and serialization helpers automatically.

## Decision
Adopt **Option 2** (with manual registration APIs as a fallback in Core). We provide `EricksonLopez.Events.Generators` as an analyzer/generator package that automatically produces static, zero-reflection type descriptors at build time.

## Rationale
- Zero runtime overhead: all type discovery happens during `dotnet build`.
- 100% incremental generator architecture prevents slowing down Visual Studio / IDE typing.
- Generates Native AOT safe registries and metadata builders.

## Consequences
- **Positive:** Ultimate developer experience with zero reflection runtime penalties.
- **Negative:** Generator package requires maintenance against Roslyn APIs.

## Rejected Alternatives
- Runtime assembly scanning (`Assembly.GetExecutingAssembly().GetTypes()`) was rejected.
