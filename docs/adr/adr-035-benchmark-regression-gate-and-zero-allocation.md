# ADR-035: Automated CI Benchmark Regression Gate and Zero-Allocation Invariant

## Status
Accepted

## Date
2026-09-03

* **Status:** Accepted
* **Date:** 2026-09-03
* **Deciders:** Architecture Team, Erickson Lopez

---

## Context
A primary architectural tenet of `EricksonLopez.Events` is zero heap allocation (0 B) on hot execution paths (identifier generation, span formatting, in-process sequential dispatch, and activity enrichment). Over time, well-intentioned changes (such as adding closures, boxing value types, or introducing LINQ expressions) can accidentally introduce heap allocations or latency regressions.

## Problem
How should the repository automatically prevent memory allocation leaks and throughput regressions from merging into `main` and `develop`?

## Options Considered
1. **Manual Ad-hoc Developer Profiling:** Rely on developers running `dotnet run -c Release` with BenchmarkDotNet locally before submitting PRs. (Rejected: Inconsistent, error-prone, untracked).
2. **Nightly Benchmarks Only:** Run benchmarks once a week. (Rejected: Regressions are detected only after merging, requiring complex forensic rollbacks).
3. **Automated Pull Request Benchmark Regression Gate:** Integrate an automated quality gate in GitHub Actions (`benchmark-regression-gate.yml`) that runs on PRs touching `src/**` or `benchmarks/**`.

## Decision
Adopt **Option 3**. Implement `.github/workflows/benchmark-regression-gate.yml` orchestrated by `./scripts/verify-benchmark-gate.ps1`. The gate enforces two non-negotiable invariants:
1. **Zero Heap Allocation Invariant:** Hot path benchmarks must report strictly `0 B` allocated heap memory.
2. **Latency Regression Threshold:** Mean execution time must not degrade by more than 5% compared to the committed baseline in `benchmarks/results/baseline.json`.

## Rationale
- Immediate feedback in PR reviews before code merges.
- Mathematical certainty that zero-allocation guarantees are maintained.
- Prevents silent performance drift in high-throughput enterprise deployments.

## Consequences
- **Positive:** Pristine allocation profiles and nanosecond latency guarantees remain continuously verified.
- **Negative:** PR execution time increases by ~10-15 minutes when core or benchmark files are modified.

## Related ADRs
- [ADR-003: Event Identity & UUID Guarantees](adr-003-event-identity.md)
- [ADR-020: Zero-Allocation Performance Strategy](adr-020-performance-strategy.md)
- [ADR-031: Stryker Mutation Testing Quality Gate](adr-031-stryker-mutation-testing-policy.md)
