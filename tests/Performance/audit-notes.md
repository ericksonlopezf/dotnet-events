# Performance Tests — Audit-Generated

## Status
Directory for performance regression tests.

## Required Performance Gates

Ensure the following benchmarks remain within acceptable limits:
- Single handler publish (warm): < 3 allocations, < 1µs
- 10 handlers publish (warm): < 12 allocations
- Source Generator registration startup (100 types): < 10ms

Run via:
```
dotnet run -c Release --project benchmarks/EricksonLopez.Events.Benchmarks -- --memory --filter *
```
