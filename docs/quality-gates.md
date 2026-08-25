# DevSecOps Quality Gates Specification

---

## 1. Six Mandatory Enterprise Gates

1. **Gate 1: Strict Compilation & Diagnostics**
   - Solution must compile cleanly under Release configuration with `TreatWarningsAsErrors=true`.
   - CS1591 XML documentation comments required on 100% of public types and methods.
2. **Gate 2: Fast-Path & Integration Tests**
   - 100% test pass rate across xUnit test suites on .NET 10.
3. **Gate 3: Code Coverage ($\ge 99\%$)**
   - Line coverage monitored via Coverlet and reported to Codecov.
4. **Gate 4: SonarCloud Static Code Analysis**
   - Maintain Quality Gate 'A' with 0 Security Vulnerabilities and 0 High-severity code smells.
5. **Gate 5: NativeAOT Smoke Test**
   - Full publish compilation using `PublishAot=true` and `-p:TreatWarningsAsErrors=true` executed on native Linux runner.
6. **Gate 6: Stryker.NET Mutation Testing ($\ge 95\%$)**
   - Mutation testing quality gate verified prior to NuGet release.
