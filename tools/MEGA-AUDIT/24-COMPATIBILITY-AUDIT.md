# 24. BINARY, SOURCE & SCHEMA COMPATIBILITY AUDIT

## 1. SOURCE AND BINARY COMPATIBILITY
- The solution strictly follows Semantic Versioning (SemVer 2.0.0).
- All public asynchronous methods provide optional `CancellationToken` parameters with default values (`= default`).
- Event interfaces avoid default interface implementations that break legacy consumers.

---

## 2. EVENT SCHEMA EVOLUTION
- The `EventVersion` property in `EventEnvelope<T>` supports additive evolution strategies:
  - V1: Baseline fields.
  - V2: Addition of new optional properties without breaking V1 consumers, supported by tolerant deserialization and `JsonSerializerOptions.PropertyNameCaseInsensitive`.