# Native AOT & Trimming Verification Artifacts

## Overview
This directory indexes Native AOT smoke tests, trimming analysis, and static reflection verification for `EricksonLopez.Events`.

## Test Projects & Suites
- [EricksonLopez.Events.AotSmokeTest](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.AotSmokeTest)
  - Full end-to-end Native AOT compiled smoke test console application.
  - Configured with `<PublishAot>true</PublishAot>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, and `<TrimMode>full</TrimMode>`.
  - Verifies zero IL trimming warnings (IL2026, IL2091, IL3050).
- [AotStaticReflectionSafetyTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/AotAndTrimming/AotStaticReflectionSafetyTests.cs)
  - `Aot_ZeroDynamicCodeEmission_InCoreBus`
  - `Aot_SourceGenerator_EmitsDirectInvokersWithoutReflection`
  - `Aot_TypeMetadataRegistration_WorksWithoutAssemblyScanning`
- [EricksonLopez.Events.Generators.Tests](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.Generators.Tests)
  - `EventIncrementalGeneratorTests.cs`: Compiles source generator outputs and asserts static registration generation.

## Execution
```powershell
# Run smoke tests
dotnet run --project tests/EricksonLopez.Events.AotSmokeTest

# Publish under Native AOT
dotnet publish tests/EricksonLopez.Events.AotSmokeTest -c Release -r win-x64
```
