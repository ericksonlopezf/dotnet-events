# ADR-011: Trimming Annotations and Analyzer Warnings Policy

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Trimming removes unused code from self-contained .NET applications. If code relies on reflection without proper trimmer annotations (`[DynamicallyAccessedMembers]`), the trimmer may strip constructors, properties, or types needed at runtime.

## Problem
What is the policy for handling trimmer warnings and annotations across `EricksonLopez.Events`?

## Options
1. **Permissive suppression:** Use `[UnconditionalSuppressMessage]` when warnings arise.
2. **Strict Zero-Warning Policy ("Fix the architecture before suppressing the analyzer"):** Avoid any construct that produces trim warnings. Enable `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in all builds.

## Decision
Adopt **Option 2**. All library projects must compile with zero trim/AOT warnings without using unwarranted suppressions. If an API design triggers trim warnings, the design must be refactored to use static type descriptors, generic constraints, or source generators.

## Rationale
- Ensures applications consuming `EricksonLopez.Events` do not encounter trimming regressions in production.
- Promotes clean, statically verifiable API design.

## Consequences
- **Positive:** Production-grade trimming safety.
- **Negative:** Requires disciplined compile-time design.

## Rejected Alternatives
- Hiding trimming warnings using suppression attributes was rejected.
