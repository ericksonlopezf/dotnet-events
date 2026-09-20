---
name: Feature Request
about: Suggest an idea or architectural improvement for EricksonLopez.Events
title: "[FEATURE] "
labels: ["enhancement"]
assignees: "ericksonlopezf"
---

## 🎯 Feature Description

<!-- A clear and concise description of what feature you are proposing. -->

## ❓ Problem Statement / Motivation

<!-- Is your feature request related to a problem? Please describe. (e.g. "I'm always frustrated when [...]") -->

## 💡 Proposed Technical Solution

<!-- How do you envision this feature being implemented? Provide API sketches if applicable. -->

```csharp
// Example API usage
```

## ⚖️ Architectural Invariants & Compatibility

Please verify how the proposal aligns with repository tenets:
- [ ] **Zero Runtime Reflection**: Does not require `Assembly.GetTypes()` or dynamic runtime code generation.
- [ ] **Native AOT Compatible**: Compiles with `PublishAot=true` with zero trim warnings.
- [ ] **Boundary Isolation**: Does not introduce message broker SDKs, ORMs, or persistence drivers into core contracts.
- [ ] **Zero/Low Allocation**: Preserves value-type and span-oriented low-allocation semantics where appropriate.

## 🔄 Alternatives Considered

<!-- A clear and concise description of any alternative solutions or features you've considered. -->

## 📑 Additional Context

<!-- Add any other context or screenshots about the feature request here. -->
