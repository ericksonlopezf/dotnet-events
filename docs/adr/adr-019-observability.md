# ADR-019: Zero-Cost Observability and OpenTelemetry Integration

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Observability (distributed tracing via `System.Diagnostics.ActivitySource`, metrics via `System.Diagnostics.Metrics.Meter`) is critical in event-driven distributed architectures. However, introducing mandatory OpenTelemetry SDK packages into core contracts introduces runtime overhead and unwanted transitive dependencies.

## Problem
How can `EricksonLopez.Events` support distributed tracing (W3C trace context, OpenTelemetry) without requiring external NuGet dependencies or paying performance penalties when tracing is disabled?

## Options
1. **Take dependency on `OpenTelemetry.Api`:** Introduces external dependencies to the core package.
2. **Use Built-in .NET `ActivitySource` and `Meter` with Zero-Cost Checks:**
   - Use BCL's `System.Diagnostics.ActivitySource` ("EricksonLopez.Events") and `System.Diagnostics.Metrics.Meter`.
   - Check `ActivitySource.HasListeners()` before allocating activities or tags.

## Decision
Adopt **Option 2**. We use native `ActivitySource` and `Meter` from `System.Diagnostics` (built into the BCL). When no telemetry listeners are attached, all instrumentation calls have zero allocation and sub-nanosecond branch checks.

## Rationale
- Zero external package dependencies.
- Automatically recognized by OpenTelemetry .NET SDK collectors without custom adapters.
- Traces propagate W3C `traceparent` through `EventMetadata`.

## Consequences
- **Positive:** Standard OpenTelemetry compliance, zero overhead when inactive, zero external dependencies.
- **Negative:** None.

## Rejected Alternatives
- Requiring `OpenTelemetry` NuGet packages was rejected.
