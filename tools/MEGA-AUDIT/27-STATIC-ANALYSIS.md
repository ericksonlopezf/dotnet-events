# 27. STATIC ANALYSIS & COMPILER WARNINGS AUDIT

## 1. CODE QUALITY CONFIGURATION
- **`TreatWarningsAsErrors`**: Enabled (`true`) across all production projects.
- **`Nullable` Reference Types**: Strictly enabled (`<Nullable>enable</Nullable>`).
- **Active Analyzers**:
  - `Microsoft.CodeAnalysis.NetAnalyzers`
  - `SonarAnalyzer.CSharp`
  - Custom internal analyzer `ELE001` (`EventImmutabilityAnalyzer`)

---

## 2. GOVERNANCE RULE EVALUATION
- `verify-compliance.ps1` validates the absence of unapproved `[Obsolete]` attributes in production code. Finding `EVT-REL-002` in `TransactionalEventPublisher` indicates this type should be documented and migrated cleanly to `EricksonLopez.Outbox`.