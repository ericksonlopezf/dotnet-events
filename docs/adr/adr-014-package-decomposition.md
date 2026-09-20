# ADR-014: Package Decomposition Strategy

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
A well-designed library suite balances fine-grained modularity against package explosion. Creating too many micro-packages creates dependency hell and version sync issues.

## Problem
What is the optimal set of NuGet packages for the `EricksonLopez.Events` ecosystem?

## Options
1. **Single Monolithic Package:** Everything bundled together.
2. **Extreme Micro-Packages (`Abstractions`, `Core`, `Envelopes`, `Metadata`, `Identifiers`, `Generators`, `Json`):** Overly fragmented.
3. **Focused Triad:**
   - `EricksonLopez.Events` (Core contracts, identifiers, metadata, envelopes, in-memory publisher/subscriber, static registry)
   - `EricksonLopez.Events.Generators` (Roslyn analyzer and incremental generator for zero-reflection registration and descriptors)
   - `EricksonLopez.Events.Serialization.SystemTextJson` (Native AOT converters and JSON serialization extensions)

## Decision
Adopt **Option 3**. The triad provides clear separation of concerns, zero superfluous packages, and crystal-clear dependencies.

## Rationale
- Core has 0 external dependencies.
- Generator is a development dependency (`PrivateAssets="all"`).
- JSON adapter is optional for applications doing wire serialization.

## Consequences
- **Positive:** Minimal package management overhead, clear boundaries.
- **Negative:** None.

## Rejected Alternatives
- Fragmenting Core into 5+ micro-packages was rejected as unnecessary friction.
