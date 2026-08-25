# ADR-025: CloudEvents 1.0 Adapter Strategy and Boundary

* **Status:** Implemented (v1.0.0)
* **Date:** 2026-08-15
* **Deciders:** Architecture Team, Erickson Lopez

## Context

CloudEvents is a vendor-neutral CNCF specification for describing event data in common formats. Many enterprise message brokers, event gateways (e.g. Azure Event Grid, Knative, AWS EventBridge), and serverless platforms support CloudEvents v1.0 natively.

`EricksonLopez.Events` defines `EventEnvelope<TEvent>` and `EventMetadata` as its core envelope model. 

We need to decide:
1. Whether CloudEvents support belongs in the core `EricksonLopez.Events` package or in a dedicated package `EricksonLopez.Events.CloudEvents`.
2. How the core `EventEnvelope<TEvent>` and `EventMetadata` map to CloudEvents 1.0 specification attributes.

## Decision

1. **Dedicated Package Boundary:** CloudEvents support will be provided as an optional, decoupled adapter package: `EricksonLopez.Events.CloudEvents`. The core `EricksonLopez.Events` library will remain 100% free of CloudEvents dependencies or external SDKs.
2. **Canonical Mapping Specification:**
   - `id` (required, string) <-> `EventEnvelope.Id.ToString()`
   - `source` (required, URI-reference / string) <-> `EventMetadata.Source` (or default URI identifier)
   - `specversion` (required, string) <-> `"1.0"`
   - `type` (required, string) <-> `EventEnvelope.Type.Value`
   - `time` (optional, RFC3339 timestamp) <-> `EventEnvelope.OccurredAt.ToUniversalTime().ToString("O")`
   - `datacontenttype` (optional, string) <-> `EventMetadata.ContentType` (e.g. `"application/json"`)
   - `dataschema` (optional, URI-reference) <-> Schema URL constructed from `EventEnvelope.Type` and `EventEnvelope.Version`
   - `data` (optional, payload) <-> `EventEnvelope.Payload`
   - **Extension Attributes (all lowercase alphanumeric per spec):**
     - `correlationid` <-> `EventMetadata.CorrelationId.Value`
     - `causationid` <-> `EventMetadata.CausationId.Value`
     - `tenantid` <-> `EventMetadata.TenantId.Value`
     - `customheaders` <-> Mapped directly to lowercase CloudEvents extension attributes.

## Why

- **Core Purity:** Keeping CloudEvents out of core adheres to ADR-001 (Core Responsibility) and ADR-014 (Package Decomposition Strategy). Core has 0 external dependencies and focuses exclusively on domain/integration event modeling.
- **Interoperability:** Providing a standardized mapping ensures that downstream transports and gateways can seamlessly project `EventEnvelope<TEvent>` to CloudEvents JSON/Structured or Binary mode without custom boilerplate.

## Alternatives Considered

1. **Include CloudEvents directly in `EricksonLopez.Events`:** Rejected because not all systems use CloudEvents (many use custom Kafka/RabbitMQ envelopes), and it would bloat the core API surface.
2. **Use CloudEvents SDK as the internal envelope:** Rejected because CloudEvents SDK uses dynamic dictionaries, object allocations, and lacks native Guid v7 monotonic structs and compile-time AOT source generators.

## Consequences

### Positive
- Strict adherence to Single Responsibility Principle and ADR-001.
- Clear, standardized translation path between `EricksonLopez.Events` and enterprise event meshes.
- Zero allocation/performance overhead for applications not using CloudEvents.

### Negative
- Applications requiring CloudEvents will reference an additional NuGet package (`EricksonLopez.Events.CloudEvents`) when released.
