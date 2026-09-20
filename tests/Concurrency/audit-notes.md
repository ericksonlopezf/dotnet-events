# Concurrency Tests — Audit-Generated

This directory contains concurrency regression tests identified during the MEGA-AUDIT.

## Status
Tests are designed (stubs) — implementation required per remediation plan EVT-HIGH-001 and EVT-HIGH-003.

## Required Tests

### EventBus_GetOrDiscoverHandlers_ConcurrentPublish_NoHandlerDuplication
Reproduces HIGH-001: 100 concurrent publishers for same event type (first publish).
Assert: handler called exactly N times (not N*concurrency).

### StaticEventTypeRegistry_Reset_ConcurrentAccess_NoInconsistentState
Reproduces HIGH-003: concurrent Reset() + SetCurrent() + GetDescriptor().
Assert: no InvalidOperationException from SetCurrent after Reset.

### EventBus_UnderHighConcurrency_NoDeadlock
10,000 concurrent publish operations.
Assert: completes within timeout without deadlock.
