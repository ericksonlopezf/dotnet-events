# Governance Model

## 1. Project Overview & Mission

`EricksonLopez.Events` is an enterprise-grade, high-performance event-driven architectural foundation for modern .NET (`.NET 8`, `.NET 9`, `.NET 10`). Its mission is to deliver zero-allocation Domain Events, Integration Events, CloudEvents v1.0 specifications, transactional Outbox/Inbox patterns, OpenTelemetry activity propagation, and NativeAOT source-generated metadata serializers.

---

## 2. Roles & Responsibilities

- **Project Lead / Architect (@ericksonlopezf)**: Directs strategic architectural roadmap, API invariants, breaking change governance, and final releases.
- **Maintainers**: Review pull requests, triage issues, oversee Stryker mutation testing and NativeAOT trimming benchmarks.
- **Contributors**: Propose bug fixes, documentation improvements, unit tests, and feature enhancements.

---

## 3. Decision-Making & Architectural Invariants

All architectural changes must strictly uphold the following invariants:
1. **Clean Architecture & Boundary Decoupling**: Domain Events have zero runtime dependencies on persistence or external brokers.
2. **NativeAOT & Trimming Safety**: Zero runtime reflection in dispatch pipelines; source generation and strongly-typed metadata must be used.
3. **Zero Heap Allocations on Hot Paths**: Dispatching and activity enrichment must avoid unnecessary closures or boxing.
4. **DevSecOps Quality Gates**: 100% build pass, $\ge 99\%$ test coverage, $\ge 95\%$ Stryker mutation score, and 0 compiler warnings (`TreatWarningsAsErrors=true`).

---

## 4. Release Cadence & Versioning

- Semantic Versioning (SemVer 2.0.0) is enforced across all NuGet packages.
- Releases are automated via Conventional Commits and Google Release Please.
- Provenance is cryptographically signed using Sigstore (SLSA v2).
