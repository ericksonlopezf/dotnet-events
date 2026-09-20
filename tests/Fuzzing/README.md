# Property-Based & Fuzz Testing Artifacts

## Overview
This directory indexes property-based testing and generative fuzzing test suites for `EricksonLopez.Events`, powered by `FsCheck.Xunit`. These suites exercise domain models, value objects, and serialization adapters over thousands of arbitrary, malformed, and boundary-value inputs.

## Test Suites & Locations
- [EventIdTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Identifiers/EventIdTests.cs)
  - `[Property]` `EventId_ArbitraryGuid_PreservesEqualityAndRoundtrip`: Generates thousands of arbitrary GUIDs, verifying byte order and string formatting roundtrips.
  - `[Property]` `EventId_Monotonicity_NeverRegresses`: Fuzzes chronological timestamps to verify monotonic sorting.
- [EventTypeAndVersionTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Identifiers/EventTypeAndVersionTests.cs)
  - `[Property]` `EventType_ArbitraryStrings_EnforcesValidHierarchy`: Fuzzes string schemas (dots, colons, unicode) to verify validation invariants.
  - `[Property]` `EventVersion_ArbitraryIntegers_PositiveRangeEnforcement`: Fuzzes negative, zero, and extreme values.
- [CorrelationCausationTenantTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Identifiers/CorrelationCausationTenantTests.cs)
  - `[Property]` `TenantId_ArbitraryStrings_DeterministicHashing`: Fuzzes unicode, whitespace, and null characters.
- [EventsJsonSerializationTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.Serialization.Tests/EventsJsonSerializationTests.cs)
  - `[Property]` `EventEnvelope_SerializationRoundtrip_PreservesAllMetadata`: Fuzzes arbitrary payload sizes and unicode headers through System.Text.Json.

## Execution
```powershell
dotnet test tests/EricksonLopez.Events.UnitTests --filter "Category=Property"
dotnet test tests/EricksonLopez.Events.Serialization.Tests
```
