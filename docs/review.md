# Architecture Review & Governance Checklist

---

## 1. Governance Review Items

- [x] Unidirectional Clean Architecture dependency flow respected.
- [x] Zero reflection in dispatch pathways.
- [x] All types sealed unless designed for inheritance.
- [x] Multi-targeting .NET 8 (LTS), .NET 9, and .NET 10.
- [x] NativeAOT `[RequiresUnreferencedCode]` attributes applied where dynamic fallback is unavoidable.
- [x] All documentation written strictly in English with kebab-case naming.
