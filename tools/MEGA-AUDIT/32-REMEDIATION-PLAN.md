# 32. COMPREHENSIVE REMEDIATION PLAN (PRIORITY ROADMAP)

## P0 — BLOCKERS (CRITICAL / IMMEDIATE HIGH)
- [x] **EVT-DAT-002**: Fix casing discrepancy in `EventMetadata.GetHashCode()` using `StringComparer.OrdinalIgnoreCase`. **(COMPLETED)**
- [x] **EVT-SEC-004**: Fix equality and hash code symmetry in `TenantId` for whitespace strings. **(COMPLETED)**
- [x] **Regression Validation**: Add adversarial suite `ForensicAdversarialEvidenceTests.cs` to lock in remediated behaviors. **(COMPLETED)**

## P1 — MUST FIX
- [ ] **EVT-REL-002**: Extract `TransactionalEventPublisher` to an experimental package or mark with explicit XML doc warnings; direct consumers to `EricksonLopez.Outbox`.
- [ ] **EVT-CNC-001**: Add defensive validation in `EventBusOptions` rejecting `ParallelExecutionStrategy` combined with `ReuseAmbientScope`.

## P2 — SHOULD FIX
- [ ] **EVT-SEC-001**: Extend Roslyn analyzer `ELE001` to warn if an event property type is `List<T>` or `Dictionary<TKey, TValue>`.
- [ ] **EVT-LFC-001**: Refactor `HandlerResolutionHelper` to resolve via typed descriptor without enumerating full collections.

## P3 — IMPROVEMENTS & GOVERNANCE
- [ ] **EVT-GOV-001**: Separate `HandlerScopePolicy` from `EventBusOptions.cs` into its own file and ensure canonical MIT headers across the tree.