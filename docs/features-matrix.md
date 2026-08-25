# Features & Target Framework Compatibility Matrix

---

## 1. Target Framework Support

| Package | .NET 8.0 (LTS) | .NET 9.0 (STS) | .NET 10.0 | NativeAOT | Trimming Safe |
|---|:---:|:---:|:---:|:---:|:---:|
| `EricksonLopez.Events` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Contracts` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.CloudEvents` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Generators` | ✅ (.NET Standard 2.0) | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Inbox` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.OpenTelemetry` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Outbox` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Serialization.SystemTextJson` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Testing` | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 2. Roslyn Diagnostic Rules

| Diagnostic ID | Severity | Category | Description |
|---|---|---|---|
| `ELE001` | Error | Immutability | Domain events must be immutable (`readonly struct` or `record`) |
| `ELE002` | Warning | Architecture | Do not invoke external brokers directly from domain event handlers |
| `ELE003` | Error | Serialization | Non-AOT serializable types detected in event payload |
| `ELE004` | Warning | Performance | Avoid heap closures inside event publishing loops |
| `ELE005` | Warning | Telemetry | Missing CorrelationId in distributed event envelope |
