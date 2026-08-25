# ADR-010: Native AOT and Trimming Zero-Reflection Guarantee

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
.NET 10 provides first-class Native AOT (Ahead-of-Time) compilation and full assembly trimming. Traditional .NET messaging and event libraries rely extensively on runtime reflection (`Assembly.GetTypes()`, `Type.MakeGenericType()`, `Activator.CreateInstance()`, `Expression.Compile()`), which causes runtime crashes or missing metadata in AOT published binaries.

## Problem
How can `EricksonLopez.Events` guarantee 100% Native AOT compatibility and zero reflection warnings without resorting to analyzers suppressions?

## Options
1. **Reflection with `[RequiresUnreferencedCode]` / `[UnconditionalSuppressMessage]`:** Defer the problem, emit compiler warnings, suppress them in code.
2. **Strict Zero-Reflection Architecture:** Design all core abstractions using static generic dispatch, strongly-typed structs, compile-time descriptors, and Roslyn Source Generation.

## Decision
Adopt **Option 2**. The primary architecture relies on static generic dispatch, strongly-typed structs, compile-time descriptors, and Roslyn Source Generation (`EricksonLopez.Events.Generators`). When using the Source Generator or manual builder registration, all type mappings and metadata resolvers use static compile-time descriptors (`EventTypeDescriptor`) and `FrozenDictionary` with zero runtime reflection.

For rapid prototyping where the Source Generator is not referenced, the static descriptor cache provides a convenience fallback annotated with `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` to alert AOT compilers (formalized in ADR-021).

## Rationale
- Guaranteed compatibility with `PublishAot=true` and `PublishTrimmed=true` when using the Source Generator.
- Zero JIT startup overhead; immediate execution and lower memory footprint.
- Robust, reliable behavior across serverless (AWS Lambda, Azure Container Apps, Google Cloud Run) and microservices.

## Consequences
- **Positive:** Pristine Native AOT support, zero trim warnings on primary path, lightning-fast execution.
- **Negative:** Dynamic runtime discovery of unreferenced assemblies is not supported; events must be registered statically or generated at compile time.

## Rejected Alternatives
- Using runtime reflection scanning (`Assembly.GetTypes()`) without compiler safety attributes was rejected.

## Related ADRs
- [ADR-012: Roslyn Incremental Source Generator for Static Descriptors](ADR-012-source-generation.md)
- [ADR-021: StaticEventTypeRegistry — Reflection Fallback and AOT Honesty](ADR-021-static-registry-aot-fallback.md)
