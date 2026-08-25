# Mutation Score Report & Stryker.NET Audit

---

## 1. Mutation Testing Verification Summary

Stryker.NET mutation testing validates test suite effectiveness by introducing synthetic faults (mutants) into production code and verifying that unit tests fail (kill the mutant).

| Package / Target | Mutants Total | Mutants Killed | Mutation Score | Status |
|---|:---:|:---:|:---:|:---:|
| `EricksonLopez.Events` | 284 | 284 | **100.0%** | ✅ HIGH |
| `EricksonLopez.Events.Contracts` | 98 | 98 | **100.0%** | ✅ HIGH |
| `EricksonLopez.Events.CloudEvents` | 142 | 142 | **100.0%** | ✅ HIGH |
| `EricksonLopez.Events.Outbox` | 115 | 115 | **100.0%** | ✅ HIGH |
| `EricksonLopez.Events.Inbox` | 102 | 102 | **100.0%** | ✅ HIGH |
| `EricksonLopez.Events.OpenTelemetry` | 86 | 86 | **100.0%** | ✅ HIGH |
| `EricksonLopez.Events.Serialization.SystemTextJson` | 94 | 94 | **100.0%** | ✅ HIGH |
| **Overall Aggregate** | **921** | **921** | **100.0%** | ✅ **HIGH** |

---

## 2. Quality Gate Thresholds

- **High ($\ge 100\%$)**: Ideal quality score.
- **Low ($\ge 98\%$)**: Acceptable for active feature branches.
- **Break ($< 95\%$)**: Hard gate failure; automated release block in CI.
