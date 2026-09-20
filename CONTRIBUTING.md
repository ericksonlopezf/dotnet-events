# Contributing to EricksonLopez.Events

Thank you for your interest in contributing to **`EricksonLopez.Events`**!

This document outlines our development workflow, engineering standards, testing policies, and pull request requirements.

---

## 📋 Prerequisites

- **.NET SDK**: `10.0.302` or higher (configured in [`global.json`](global.json)).
- **C# / IDE**: Visual Studio 2022 / 2025, JetBrains Rider 2024+, or VS Code with the C# Dev Kit extension.
- **Git**: Git 2.40+.
- **Stryker.NET** (for mutation testing): `dotnet tool install -g dotnet-stryker`.

---

## 🛠️ Build & Development Commands

### 1. Clone & Restore
```bash
git clone https://github.com/ericksonlopezf/dotnet-events.git
cd dotnet-events
dotnet restore EricksonLopez.Events.slnx
```

### 2. Build Solution
All projects enforce strict `TreatWarningsAsErrors=true` and `AnalysisLevel=latest-recommended`:
```bash
dotnet build EricksonLopez.Events.slnx -c Release
```

### 3. Run Automated Tests
Execute the full solution test suite (455+ tests across 8 test projects):
```bash
dotnet test EricksonLopez.Events.slnx -c Release --verbosity normal
```

Filter tests by category:
```bash
# Unit tests
dotnet test --filter "Category=Unit"

# Integration tests
dotnet test --filter "Category=Integration"

# Architecture and boundary tests
dotnet test --filter "Category=Architecture"
```

### 4. Verify Native AOT Compilation
Verify that smoke test suites and sample applications compile with `PublishAot=true` without trimming warnings:
```bash
# Windows
dotnet publish tests/EricksonLopez.Events.AotSmokeTest/EricksonLopez.Events.AotSmokeTest.csproj -c Release -r win-x64 -p:PublishAot=true
.\tests\EricksonLopez.Events.AotSmokeTest\bin\Release\net10.0\win-x64\publish\EricksonLopez.Events.AotSmokeTest.exe

# Linux
dotnet publish tests/EricksonLopez.Events.AotSmokeTest/EricksonLopez.Events.AotSmokeTest.csproj -c Release -r linux-x64 -p:PublishAot=true
./tests/EricksonLopez.Events.AotSmokeTest/bin/Release/net10.0/linux-x64/publish/EricksonLopez.Events.AotSmokeTest
```

### 5. Run Mutation Testing (Stryker.NET)
Per [ADR-031](docs/adr/adr-031-stryker-mutation-testing-policy.md), we enforce a **95% break threshold** across all modules:
```bash
# Core Contracts (Identifiers, Metadata, Envelopes)
dotnet-stryker -f stryker-contracts-config.json

# Core Bus & Dispatch
dotnet-stryker -f stryker-config.json

# Roslyn Source Generators & Analyzers
dotnet-stryker -f stryker-generators-config.json

# System.Text.Json Serialization
dotnet-stryker -f stryker-serialization-config.json

# CloudEvents 1.0 Adapter
dotnet-stryker -f stryker-cloudevents-config.json

# OpenTelemetry Instrumentation
dotnet-stryker -f stryker-opentelemetry-config.json

# Testing Utilities
dotnet-stryker -f stryker-testing-config.json
```

### 6. Run Performance Benchmarks & Quality Gate
Run the BenchmarkDotNet suite and verify that hot paths preserve zero heap allocations (0 B) with no more than 5% latency regression:
```bash
# Run benchmarks and export JSON metrics
dotnet run -c Release --project benchmarks/EricksonLopez.Events.Benchmarks --framework net10.0 -- --filter "*" --job short --exporters json --memory --artifacts ./benchmarks/pr-results

# Evaluate Benchmark Regression Gate
pwsh ./scripts/verify-benchmark-gate.ps1 -ReportDir ./benchmarks/pr-results -BaselinePath ./benchmarks/results/baseline.json -MaxLatencyRegressionPercent 5
```

---

## 📐 Architecture & Coding Invariants

When writing code for this repository, you must adhere to these foundational tenets:

1. **Zero Runtime Reflection in Core**: Never use `Assembly.GetTypes()`, `Type.MakeGenericType`, or runtime code generation in production code. Static discovery is performed via `EricksonLopez.Events.Generators`.
2. **Native AOT & Trimming First**: All production assemblies must compile with `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>`. Never suppress trim warnings unless accompanied by explicit fallback annotations (`ADR-021`).
3. **Immutability & Zero Allocation**:
   - Identifiers (`EventId`, `EventType`, `EventVersion`, `CorrelationId`, `CausationId`, `TenantId`) are `readonly record struct` value types.
   - `EventMetadata` is an immutable record backed by `FrozenDictionary<string, string>`.
   - `EventEnvelope<TEvent>` is a reference-type `sealed record` (`ADR-023`).
4. **Boundary Isolation**:
   - `EricksonLopez.Events.Contracts` depends strictly on the .NET BCL (0 external dependencies).
   - Core packages must never reference message broker SDKs (RabbitMQ, Kafka, Azure Service Bus) or relational ORMs.

---

## 🧪 Testing Guidelines & Conventions

### Test Method Naming (Osherove Tri-Part Pattern)
Per [ADR-026](docs/adr/adr-026-testing-naming-convention-and-ide1006.md), all test methods must use the standardized 3-part naming structure:
```text
[UnitOfWork]_[StateUnderTest]_[ExpectedBehavior]
```
*Examples:*
- `EventId_New_ShouldGenerateMonotonicallySortableGuidVersion7`
- `EventBus_PublishAsync_WithCancelledToken_ShouldThrowOperationCanceledException`
- `DomainEventLeakAnalyzer_WhenDomainEventUsedOutsideAggregate_ShouldReportELE005`

### Diagnostic Telemetry Tests Isolation
Per [ADR-029](docs/adr/adr-029-diagnostic-testing-isolation.md), any test classes inspecting `ActivitySource` or `Meter` singletons must be decorated with `[Collection("Diagnostics")]` to prevent cross-test telemetry race conditions in multi-threaded test runners.

---

## 🌿 Git & Pull Request Workflow

### Branch Strategy
- Main development branch: `main`.
- Create feature branches with descriptive names:
  - `feat/feature-name`
  - `fix/bug-description`
  - `docs/doc-update`
  - `refactor/scope`

### Commit Message Convention
We adhere to Conventional Commits:
- `feat(contracts): add new immutable identifier format`
- `fix(bus): resolve reentrancy race condition in parallel strategy`
- `docs(adr): add ADR-032 on serialization tolerance`
- `test(generators): add mutant killer tests for ELE004 analyzer`

### Pull Request Checklist
Before submitting a PR, verify:
- [ ] All projects build with `0` warnings and `0` errors (`TreatWarningsAsErrors=true`).
- [ ] All automated tests pass locally (`dotnet test -c Release`).
- [ ] Native AOT smoke tests pass (`PublishAot=true`).
- [ ] Stryker mutation testing score meets or exceeds the **95% threshold**.
- [ ] Benchmark regression gate passes with 0 B hot path allocations and $\le 5\%$ latency deviation.
- [ ] Public types and methods include comprehensive XML documentation comments (`CS1591` is enabled).
- [ ] New architectural decisions are documented as an ADR under `docs/adr/`.

---

## 🤝 Code of Conduct

All contributors are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md). Please report any unacceptable behavior according to the guidelines in [SECURITY.md](SECURITY.md).
