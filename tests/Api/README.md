# API & Developer Experience (DX) Test Artifacts

## Overview
This directory indexes adversarial API misuse tests, type-constraint validations, nullability defenses, and configuration ergonomics tests for `EricksonLopez.Events`.

## Test Suites & Locations
- [AdversarialApiMisuseTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Api/AdversarialApiMisuseTests.cs)
  - `ApiMisuse_NullEventInstance_ThrowsArgumentNullExceptionImmediately`
  - `ApiMisuse_NullServiceProvider_ThrowsArgumentNullException`
  - `ApiMisuse_UnregisteredEvent_CompletesWithoutExceptions`
  - `ApiMisuse_NegativeParallelism_ThrowsArgumentOutOfRangeException`
- [ApiConstraintsAndStructEventsTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Api/ApiConstraintsAndStructEventsTests.cs)
  - `Api_StructEventRegistration_CompilesAndDispatchesCorrectly`
  - `Api_InvalidHandlerGenericSignature_ThrowsInformativeException`
- [StructEventHandlerRegistrationTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Api/StructEventHandlerRegistrationTests.cs)
  - `Api_DependencyInjection_RegistersStructHandlersWithoutReflection`
- [ForensicAdversarialEvidenceTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/ForensicAdversarialEvidenceTests.cs)
  - `EVT_REG_001_EventTypeRegistry_UnversionedLookup_DeterministicallyReturnsHighestVersion`

## Execution
```powershell
dotnet test tests/EricksonLopez.Events.UnitTests --filter "FullyQualifiedName~Api"
```
