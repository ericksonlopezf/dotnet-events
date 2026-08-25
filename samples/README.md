# Samples and Showcase Directory — EricksonLopez.Events

This directory contains the official reference implementations for the repository.

---

## Available Projects

### 1. `ECommerce.Sample`
Multi-layer reference implementation (Clean Architecture + DDD) demonstrating the 10 progressive levels of the official Showcase:
- **`ECommerce.Domain`**: DDD entities and aggregates with pure domain events and `EventId` identifiers (Guid v7).
- **`ECommerce.Application`**: Use cases, integration events with `[EventName]`, `[EventVersion]`, `[EventSource]` attributes, and typed handlers.
- **`ECommerce.Infrastructure`**: Pipeline middlewares, Outbox support, and Native AOT serializer with `System.Text.Json.JsonSerializerContext`.
- **`ECommerce.App`**: Executable console application stepping through levels 0 to 10 with assertion validation.

```bash
# Run the ECommerce Showcase
dotnet run --project samples/ECommerce.Sample/ECommerce.App
```

---

### 2. `NativeAotSample`
Smoke test for Native AOT & Trimming compilation and execution with zero runtime reflection.

```bash
# Run the Native AOT Smoke Test
dotnet run --project samples/NativeAotSample
```

