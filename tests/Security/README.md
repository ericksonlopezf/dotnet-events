# Security Test Artifacts & Regression Suite

## Overview
This directory indexes the adversarial security and red-team tests for `EricksonLopez.Events`. All tests validate defensive boundaries, tenant isolation, immutable context propagation, and protection against state-poisoning and injection attacks.

## Test Suites & Locations
- [MultiTenancyContextIsolationTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Security/MultiTenancyContextIsolationTests.cs)
  - `MultiTenancyContext_SimultaneousTenants_MaintainStrictIsolation`
  - `MultiTenancyContext_NestedPublish_PreservesTenantIdentity`
- [RedTeamSecurityTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Security/RedTeamSecurityTests.cs)
  - `RedTeam_OversizedPayload_EnforcesMaxHeaderLimits`
  - `RedTeam_ForgedTenantId_CannotBypassValidation`
  - `RedTeam_MaliciousHeaderInjection_RejectsCrlfAndNullChars`
- [AdversarialStatePoisoningTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Security/AdversarialStatePoisoningTests.cs)
  - `EventMetadata_MutationAttemptAfterPublication_ThrowsOrIsImmutable`
  - `EventContext_AsyncLocalLeakageAcrossTasks_IsGuarded`
- [StaticRegistryPoisoningSecurityTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Security/StaticRegistryPoisoningSecurityTests.cs)
  - `StaticRegistry_ConcurrentTypeRegistration_IsLockedAndDeduplicated`
  - `StaticRegistry_TamperedDescriptors_FailOpenRejection`
- [ForensicAdversarialEvidenceTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/ForensicAdversarialEvidenceTests.cs)
  - `EVT_SEC_003_TenantId_ConsidersWhitespaceStrings_AsEmpty`
  - `EVT_SEC_004_TenantId_EmptyAndWhitespace_AreEqualAndProduceIdenticalHashCode`
  - `EVT_DAT_001_EventMetadata_DifferentHeaders_ProduceDistinctHashCodes`
  - `EVT_DAT_002_EventMetadata_CaseInsensitiveHeaders_ProduceIdenticalHashCodes`

## Execution
```powershell
dotnet test tests/EricksonLopez.Events.UnitTests --filter "FullyQualifiedName~Security"
```
