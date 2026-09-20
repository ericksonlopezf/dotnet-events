# 20. SERIALIZATION & TRANSPORT FORMATS AUDIT

## 1. SYSTEM.TEXT.JSON AND NATIVE AOT SERIALIZATION
The `EricksonLopez.Events.Serialization.SystemTextJson` package implements optimized converters for all core value objects:
- `EventIdJsonConverter`: Serializes and deserializes GUID v7 strings without allocations using UTF-8 character spans.
- `TenantIdJsonConverter`, `CorrelationIdJsonConverter`, `CausationIdJsonConverter`: Direct zero-copy conversion between JSON strings and structs.
- `EventMetadataJsonConverter`: Serializes header key-value pairs while guaranteeing case-insensitive lookup.

---

## 2. MEASURED SERIALIZATION PERFORMANCE
BenchmarkDotNet performance measurements:
- **Envelope Serialization**: **465.48 ns** (1,360 bytes allocated).
- **Envelope Deserialization**: **1,073.14 ns** (2,088 bytes allocated).
- **AOT Source Generation**: `EventJsonSerializerContext` eliminates runtime reflection.

---

## 3. CLOUDEVENTS 1.0 INTEGRATION (`EricksonLopez.Events.CloudEvents`)
The CloudEvents 1.0 JSON specification is natively supported:
- `id` -> `EventId`
- `source` -> URI or publishing service identifier
- `type` -> `EventType`
- `time` -> `Timestamp` in ISO 8601 / RFC 3339 format
- `datacontenttype` -> `"application/json"`
- `data` -> Strongly typed event payload