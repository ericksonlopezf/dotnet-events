# DevSecOps Quality Gates Specification

---

## 1. Enterprise Quality Gates

1. **Gate 1: Strict Compilation & Diagnostics**
   - Solution must compile cleanly under Release configuration with `TreatWarningsAsErrors=true`.
   - CS1591 XML documentation comments required on 100% of public types and methods.
2. **Gate 2: Fast-Path & Integration Tests**
   - 100% test pass rate across xUnit test suites on .NET 8, .NET 9, and .NET 10.
3. **Gate 3: Code Coverage ($\ge 99\%$)**
   - Line coverage monitored via Coverlet and reported to Codecov.
4. **Gate 4: SonarCloud Static Code Analysis**
   - Maintain Quality Gate 'A' with 0 Security Vulnerabilities and 0 High-severity code smells.
5. **Gate 5: NativeAOT Smoke Test**
   - Full publish compilation using `PublishAot=true` and `-p:TreatWarningsAsErrors=true` executed on Windows and Linux runners with binary execution test (`tests/EricksonLopez.Events.AotSmokeTest`).
6. **Gate 6: Stryker.NET Mutation Testing ($\ge 95\%$)**
   - Automated mutation testing across all 7 packages verified prior to NuGet release.
7. **Gate 7: Benchmark Regression Gate**
   - Zero-allocation (0 B allocated) invariant on hot path combinators and dispatch operations.
   - Mean latency regression threshold $\le 5\%$ enforced on pull requests touching `src/**` or `benchmarks/**`.
8. **Gate 8: Supply Chain Security & Attestation**
   - Strong Name key signing with `EricksonLopez.snk`.
   - Cryptographic Sigstore provenance attestation via GitHub Actions (`actions/attest-build-provenance@v2`).
   - OIDC Trusted Publishing to NuGet.org via `NuGet/login@v1` (zero long-lived API keys).

