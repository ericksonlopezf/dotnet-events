# ADR-015: Target Framework Strategy (.NET 10.0 Standard)

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
.NET 10 introduces the latest advancements in runtime performance, hardware intrinsics, `Guid.CreateVersion7()`, `FrozenDictionary`, `IUtf8SpanFormattable`, and refined Native AOT toolchains.

## Problem
Which Target Framework Monikers (TFMs) should `EricksonLopez.Events` target?

## Options
1. **Multi-target `netstandard2.0`, `net8.0`, `net9.0`, `net10.0`:** High maintenance cost, conditional compilation (`#if NET10_0_OR_GREATER`), inability to rely on native `Guid.CreateVersion7()` and span-formatting without polyfills.
2. **Target Modern .NET Baseline (`net10.0` for runtime packages, `netstandard2.0` for Roslyn generator):** Focus purely on state-of-the-art .NET 10 performance, Native AOT, and BCL features.

## Decision
Adopt **Option 2**. All runtime libraries and sample projects target `net10.0`. The Roslyn Source Generator project targets `netstandard2.0` (as required by the Roslyn compiler infrastructure).

## Rationale
- Maximizes performance and Native AOT guarantees.
- Eliminates legacy boilerplate and polyfills.
- Consistent with modern greenfield .NET architecture standards.

## Consequences
- **Positive:** Maximum performance, clean codebase, zero legacy baggage.
- **Negative:** Consuming projects must use .NET 10 or newer.

## Rejected Alternatives
- Legacy .NET Standard 2.0 runtime multi-targeting was rejected.
