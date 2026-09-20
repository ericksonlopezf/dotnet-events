# 11. MULTI-TENANCY & DATA ISOLATION AUDIT

## 1. TENANT IDENTIFICATION AND CONTRACT (`TenantId`)
The `TenantId` primitive is encapsulated as a `readonly record struct` in `EricksonLopez.Events.Contracts.Identifiers`:
- Prevents heap allocations (0 allocations).
- Supports direct character span formatting.
- Explicitly propagated in the `EventMetadata.TenantId` header.

---

## 2. MULTI-TENANT CONTEXT PROPAGATION
In multi-tenant architectures, events must preserve the originating tenant identifier to ensure handlers execute within the proper security boundary:
- When dispatching an event, audit middleware or handler resolvers extract `TenantId` from `EventEnvelope<T>` and establish execution context (`AsyncLocal<TenantContext>`).
- Concurrent stress testing verified that simultaneous processing of events from `Tenant A` and `Tenant B` across ThreadPool worker tasks produces zero cross-tenant state leakage when utilizing `HandlerScopePolicy.CreateScopePerHandler`.

---

## 3. INTEGRATION WITH `EricksonLopez.MultiTenancy`
For complex architectures featuring Row-Level Security (PostgreSQL RLS) and dynamic tenant resolution, `EricksonLopez.Events` integrates directly with `EricksonLopez.MultiTenancy` to resolve the active tenant from `ITenantContextAccessor` when constructing outgoing envelope metadata.