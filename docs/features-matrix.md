# Features & Target Framework Compatibility Matrix

---

## 1. Target Framework Support

| Package | .NET 8.0 (LTS) | .NET 9.0 (STS) | .NET 10.0 | NativeAOT | Trimming Safe |
|---|:---:|:---:|:---:|:---:|:---:|
| `EricksonLopez.Events` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Contracts` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.CloudEvents` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Generators` | ✅ (.NET Standard 2.0) | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.OpenTelemetry` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Serialization.SystemTextJson` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `EricksonLopez.Events.Testing` | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 2. Roslyn Diagnostic Rules

| Diagnostic ID | Severity | Category | Description |
|---|---|---|---|
| `ELE001` | Error | DDD.Design | Event types must be immutable (`readonly record struct` or `sealed record` with init-only properties) |
| `ELE002` | Error | DDD.Design | Invalid event version in `[EventVersion]` (must be integer $\ge 1$) |
| `ELE003` | Warning | DDD.Design | Empty or whitespace-only event name in `[EventName]` |
| `ELE004` | Warning | DDD.Design | Empty or whitespace-only event source in `[EventSource]` |
| `ELE005` | Warning | DDD.Architecture | Domain event (`IDomainEvent`) leaked in integration event contract |

