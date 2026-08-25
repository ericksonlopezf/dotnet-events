# ADR-026: Testing Naming Convention (Osherove Pattern) and Local IDE1006 Suppression

* **Status:** Accepted
* **Date:** 2026-08-18
* **Deciders:** Architecture Team, Erickson Lopez

## Context

In modern Clean Architecture and Domain-Driven Design (DDD) codebases, automated tests serve a dual purpose:
1. **Verification Mechanism:** Ensuring functional correctness, regression avoidance, and resilience against mutation testing.
2. **Living Executable Specifications:** Documenting domain invariants, architectural constraints, serialization rules, and edge cases in a human-readable format.

In CI/CD environments (GitHub Actions, Azure DevOps, TRX/JUnit reports, test runner explorers), the test method identifier is frequently the primary diagnostic information displayed upon failure. When a test name is ambiguous (e.g., `Test1`, `ShouldSerialize`, `HandleAsyncTest`), developers must open the codebase and inspect line-by-line implementation to understand what broke.

Roy Osherove's testing naming convention (`[UnitOfWork]_[StateUnderTest]_[ExpectedBehavior]` or `[Method]_[Scenario]_[Outcome]`) provides a standardized, three-part structure that maximizes clarity and diagnostic speed.

However, standard .NET Roslyn code analysis includes rules that enforce strict PascalCase naming without special characters:
- **`IDE1006` (Naming Styles):** Enforces PascalCase for method identifiers and flags underscores (`_`) as style violations.
- **`CA1707` (Identifiers should not contain underscores):** Flags underscores in member identifiers.

Applying `IDE1006` and `CA1707` universally to test assemblies creates a direct conflict between production API naming rules and test readability requirements.

## Decision

1. **Institutionalize Osherove's Naming Convention:** All test methods across the `EricksonLopez.Events` solution must strictly adhere to the tri-part Osherove pattern:
   ```text
   [UnitOfWork]_[StateUnderTest]_[ExpectedBehavior]
   ```
   *Examples:*
   - `EventType_From_ValidString_ShouldInitializeCorrectly`
   - `EventEnvelope_DirectConverterRead_MissingPayload_ThrowsJsonException`
   - `InMemoryEventPublisher_WhenHandlerThrows_ShouldPropagateExceptionAndRecordFailureMetric`

2. **Locally Suppress `IDE1006` and `CA1707` in Test Projects:** Suppress `IDE1006` and `CA1707` exclusively for test projects and test files via `.editorconfig` and `Directory.Build.props`.

## Why

1. **Tests as Living Documentation in CI/CD:** Test methods are never invoked as public APIs by consuming libraries; they are executable specifications. In failure summaries, logs, and dashboard widgets, the underscore acts as a natural visual delimiter that reduces cognitive load and allows immediate triage:
   - **Part 1 (What):** Target unit under test (`EventVersionJsonConverter`).
   - **Part 2 (Condition):** Input scenario or state (`WithInvalidNumericToken`).
   - **Part 3 (Outcome):** Expected behavior or side effect (`ThrowsJsonException`).

2. **Cognitive Load & Readability:** A test named `EventVersionJsonConverter_WithInvalidNumericToken_ThrowsJsonException` is parsed instantly by engineers during incident response. The PascalCase equivalent without underscores (`EventVersionJsonConverterWithInvalidNumericTokenThrowsJsonException`) degrades readability significantly as the complexity of the scenario increases.

3. **Strict Scope Isolation:** The suppression is scoped strictly to test projects (`tests/**` / projects ending in `Tests`). Production code under `src/` continues to enforce `TreatWarningsAsErrors=true`, `IDE1006`, and `CA1707` at the highest analyzer level (`latest-recommended`).

## Alternatives Considered

1. **Strict PascalCase without Underscores (`MethodScenarioOutcome`):**
   - *Rejected:* Hard to read in CI output; words blend together when scenario descriptions are detailed.
2. **`[Fact(DisplayName = "...")]` / `[Theory(DisplayName = "...")]` on Every Test:**
   - *Rejected:* High maintenance overhead, introduces duplication between method name and display string, prone to drift, and not uniformly indexed by all command-line and IDE test discovery tools.
3. **Given_When_Then BDD Naming in Method Names:**
   - *Rejected:* More verbose than Osherove; often forces redundant "Given" clauses when the test fixture already establishes the context.

## Consequences

### Positive
- Unified, consistent naming style across all 5 test projects in the solution.
- Enhanced developer productivity and immediate failure triage in CI/CD pipelines.
- Zero Roslyn analyzer noise or false-positive build breaks for test projects while maintaining `TreatWarningsAsErrors=true`.
- Clear architectural separation between production API conventions and testing specification conventions.

### Negative
- None. Suppressions are strictly isolated from production assemblies.

### Neutral
- New test additions must follow the tri-part Osherove structure as part of the standard code review and QA checklist.

## Implementation & Configuration

### 1. `Directory.Build.props`
```xml
<!-- Rules for Test Projects -->
<PropertyGroup Condition="$(MSBuildProjectName.EndsWith('Tests'))">
  <NoWarn>$(NoWarn);CA1707;IDE1006</NoWarn>
</PropertyGroup>
```

### 2. `.editorconfig`
```ini
# Testing Projects: Osherove Pattern (Method_Scenario_Result) & IDE1006/CA1707 suppression
[tests/**.cs]
dotnet_diagnostic.IDE1006.severity = none
dotnet_diagnostic.CA1707.severity = none
```

## Related ADRs

- **ADR-014:** Package Decomposition Strategy
- **ADR-020:** Performance and Zero-Allocation Strategy
