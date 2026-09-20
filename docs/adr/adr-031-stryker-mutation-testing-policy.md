# ADR-031: Stryker.NET Mutation Testing Strategy, Module Decomposition, and 95% Break Threshold

## Status
Accepted

## Date
2026-08-20

* **Status:** Accepted
* **Date:** 2026-08-20
* **Deciders:** Architecture Team, Erickson Lopez

## Context

Line and branch code coverage (via Coverlet / Cobertura) measure which code paths were executed during test runs, but do not assess whether test assertions are capable of detecting behavioral regressions (i.e. weak assertions, assertion-free tests).

Mutation testing via Stryker.NET introduces deliberate syntactic and semantic mutations into the codebase (e.g., negating conditions, altering operators, removing statement blocks) and verifies whether at least one test fails ("kills the mutant").

Running Stryker across a multi-project monolithic solution:
1. Causes excessive execution times (> 30-60 minutes) due to combinatorial mutation space.
2. Masks weak coverage in critical packages if a large auxiliary project inflates the aggregate score.
3. Obscures module-specific regression gates in CI pipelines.

## Decision

1. **Modular Stryker Configuration per Package:** Maintain independent `stryker-config.{module}.json` configuration files for each package (`contracts`, `core`, `generators`, `serialization`, `cloudevents`, `opentelemetry`, `testing`).
2. **Standardize Strict Mutation Score Thresholds:**
   ```json
   "thresholds": {
     "high": 100,
     "low": 98,
     "break": 95
   }
   ```
   A mutation score below 95% triggers an immediate non-zero exit code (`break: 95`), failing the CI pipeline.
3. **Zero `// Stryker disable` Policy on Business Logic:** Disabling mutation testing via source comments is strictly prohibited on core domain logic, guard clauses, serializers, and analyzers. Exclusions are limited strictly to build output artifacts (`!bin/**/*`, `!obj/**/*`).

## Consequences

### Positive
- **High-Fidelity Regression Gatekeeper:** The 95% break threshold guarantees that assertions are strong, specific, and fail reliably when code behavior changes.
- **Granular CI Feedback:** Mutation testing can run incrementally or targeted per modified module in pull request pipelines.
- **Assertion Quality Culture:** Forces developers to write meaningful state/behavior assertions rather than superficial execution-coverage tests.

### Negative / Trade-offs
- Demands disciplined test authoring; trivial changes to analyzers or edge cases must be accompanied by precise tests to maintain the >=95% threshold.
