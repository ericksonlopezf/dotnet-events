# Security Tests — Audit-Generated

This directory contains security regression tests identified during the MEGA-AUDIT.

## Status
Tests designed — implementation required per remediation plan.

## Required Tests

### CausationDepthLimitMiddleware_WithZeroDepthHeader_ShouldNotBypassLimit
Reproduces ATK-001: X-Causation-Depth: 0 in headers.
Assert: middleware still enforces MaxDepth limit.

### TransactionalEventPublisher_PendingEvents_ShouldHaveMaxLimit
Reproduces ATK-003: unbounded PendingEvents list.
Assert: PublishAsync throws when MaxPendingEvents exceeded.
