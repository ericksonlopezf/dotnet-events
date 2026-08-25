# CI/CD & Build Pipeline

This document describes the continuous integration, continuous delivery, automated testing, and security scanning pipelines of `EricksonLopez.Events`.

---

## 1. Pipeline Architecture & Workflow Topology

The CI/CD subsystem is built on GitHub Actions and enforces 6 mandatory quality gates:

```mermaid
flowchart TD
    Push[Push / Pull Request] --> CI[ci.yml Orchestrator]
    CI --> Build[dotnet-build-test.yml]
    CI --> AOT[aot-smoke-test.yml]
    
    Build --> Sonar[SonarCloud Static Analysis]
    Build --> Tests[dotnet test + Coverlet Coverage]
    Build --> Codecov[Codecov Upload (99% Target)]
    
    AOT --> NativeCompile[PublishAot=true + Clang/LLD]
    AOT --> ExecCheck[Native Binary Execution Assertions]

    Merge[Merge to main] --> RP[release-please.yml]
    RP -->|Release PR Merged| Pub[publish.yml]
    Pub --> GateCheck[verify-mutation-gate.js (Stryker >= 95%)]
    Pub --> NugetPush[NuGet.org Push via OIDC]
    Pub --> Provenance[Sigstore SLSA v2 Attestation]
```

---

## 2. DevSecOps Quality Gates Matrix

| Gate | Tool / Check | Threshold | Action on Failure |
|---|---|---|---|
| **Build & Diagnostics** | `dotnet build` (`TreatWarningsAsErrors=true`) | 0 warnings / 0 errors | Blocks PR |
| **Code Coverage** | Coverlet + Codecov | $\ge 99\%$ project / $\ge 90\%$ patch | Blocks PR |
| **Static Analysis** | SonarCloud + Roslyn Analyzers | 0 Security Vulnerabilities / Quality Gate A | Blocks PR |
| **NativeAOT Smoke** | NativeAOT binary compiler + smoke harness | 0 IL2026/IL3050 trimmer warnings | Blocks PR |
| **Mutation Testing** | Stryker.NET | $\ge 95\%$ Mutation Score | Blocks Release |
| **Provenance** | GitHub Sigstore Keyless Signing | SLSA Build Level 2/3 Attestation | Blocks Release |
