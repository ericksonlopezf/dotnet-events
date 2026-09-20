# Integration Test Artifacts & Regression Suite

## Overview
This directory indexes integration tests across Microsoft.Extensions.DependencyInjection, OpenTelemetry activity listeners, CloudEvents 1.0 specifications, and System.Text.Json serialization pipelines for `EricksonLopez.Events`.

## Test Projects & Suites
- [EricksonLopez.Events.OpenTelemetry.Tests](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.OpenTelemetry.Tests)
  - `EventsOpenTelemetryTests.cs`: Validates W3C `traceparent` propagation, Activity tags (`events.event_id`, `events.event_type`), and metrics counters.
- [EricksonLopez.Events.CloudEvents.Tests](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.CloudEvents.Tests)
  - `CloudEventTests.cs`: Validates CloudEvents 1.0 JSON specification conformance, structured envelopes, and custom extension attributes.
- [EricksonLopez.Events.Serialization.Tests](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.Serialization.Tests)
  - `EventsJsonSerializationTests.cs`: Validates System.Text.Json source generation, polymorphic envelopes, and roundtrips.
- [EricksonLopez.Events.Testing.Tests](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.Testing.Tests)
  - `TestingUtilitiesTests.cs`: Validates `FakeEventPublisher`, `TestEventHandler<T>`, and `EventTestBuilder`.
- [EricksonLopez.Events.ArchitectureTests](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.ArchitectureTests)
  - `ArchitectureRulesTests.cs`: Validates NetArchTest architectural layer separation and zero forbidden references.

## Execution
```powershell
dotnet test --filter "Category=Integration"
```
