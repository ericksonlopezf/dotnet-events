# 30. MASTER FINDINGS REGISTER

```text
EVT-SEC-001
Severity: HIGH
Category: Security / Immutability
Location: In-Memory Bus / Event Payload References
Evidence: Events are passed by reference to multiple in-memory subscribers. If an event contains mutable collection members (List<T>, Dictionary<TKey, TValue>), a handler can mutate data before other subscribers receive the reference.
Reproduction: ForensicAdversarialEvidenceTests.Verify_Mutable_Payload_Tampering_Risk()
Impact: Data corruption and non-deterministic behavior in subsequent handlers.
Root Cause: Shallow immutability guaranteed by C# compiler without deep immutability enforcement on composite collections.
Recommendation: Mandate IReadOnlyList<T> and ImmutableArray<T>; enhance Roslyn analyzer ELE001 to disallow mutable collection properties.
Status: IDENTIFIED / MITIGATED BY GUIDANCE

EVT-LFC-001
Severity: HIGH
Category: Lifecycle / Performance
Location: HandlerResolutionHelper.cs:42
Evidence: When handlers are registered as transient purely via the IEventHandler<T> interface without a concrete descriptor, HandlerResolutionHelper resolves IEnumerable<IEventHandler<T>> N times, producing N^2 instantiations.
Reproduction: ForensicAdversarialEvidenceTests.Verify_Transient_Handler_Instantiation_Storm()
Impact: Severe memory waste and Garbage Collector pressure under high subscriber counts.
Root Cause: Cascading collection resolution instead of targeted resolution by implementation type.
Recommendation: Standardize registration using AddHandler<THandler>(), which registers the concrete type in the HandlerDescriptor.
Status: IDENTIFIED / DOCUMENTED

EVT-REL-002
Severity: MEDIUM
Category: Reliability / Distributed Consistency
Location: TransactionalEventPublisher.cs:15
Evidence: TransactionalEventPublisher retains events in a volatile in-memory list (_pendingEvents). If the process terminates after database commit but prior to CommitAndPublishAsync, events are lost.
Reproduction: ForensicAdversarialEvidenceTests.Verify_Transactional_Publisher_Volatile_Memory_Loss()
Impact: Permanent loss of integration events with no replay mechanism.
Root Cause: In-memory simulation of transactional outbox instead of durable transactional storage.
Recommendation: Mark TransactionalEventPublisher as obsolete and delegate durability to EricksonLopez.Outbox.
Status: MITIGATED / OBSOLETE

EVT-CNC-001
Severity: MEDIUM
Category: Concurrency / Thread-Safety
Location: ParallelExecutionStrategy.cs:28
Evidence: When ParallelExecutionStrategy is combined with HandlerScopePolicy.ReuseAmbientScope, multiple parallel tasks share the same IServiceProvider instance and scoped dependencies (e.g., EF Core DbContext).
Reproduction: Concurrent tests in ForensicAdversarialEvidenceTests.cs
Impact: System.InvalidOperationException due to multi-threaded access on non-thread-safe DbContext instances.
Root Cause: Reusing a single ambient scope across concurrent tasks.
Recommendation: Validate in EventBusOptions and reject the Parallel + ReuseAmbientScope combination or force CreateScopePerHandler.
Status: IDENTIFIED / RECOMMENDATION GENERATED

EVT-DAT-002
Severity: HIGH
Category: Correctness / Data Integrity
Location: EventMetadata.cs:176
Evidence: EventMetadata.GetHashCode() computed header key hashes using StringComparer.Ordinal, while Equals() evaluated keys using StringComparer.OrdinalIgnoreCase.
Reproduction: ForensicAdversarialEvidenceTests.Verify_EventMetadata_HashCode_Case_Insensitive_Fix()
Impact: Two metadata dictionaries with identical keys differing only in casing ("x-trace" vs "X-Trace") equated under Equals() but produced differing hash codes, breaking HashSet and Dictionary lookups.
Root Cause: Mismatch between equality comparator and hash code generator.
Recommendation: Use StringComparer.OrdinalIgnoreCase across both Equals and GetHashCode.
Status: REMEDIATED AND VERIFIED (406 passing tests)

EVT-SEC-004
Severity: HIGH
Category: Security / Multi-Tenancy
Location: TenantId.cs:38
Evidence: A TenantId containing whitespace-only strings ("   ") evaluated inconsistently against TenantId.Empty across CompareTo() and GetHashCode().
Reproduction: ForensicAdversarialEvidenceTests.Verify_TenantId_Whitespace_Normalization_Fix()
Impact: Potential multi-tenant filter bypass or non-deterministic ordering in indexed database columns.
Root Cause: Missing symmetric normalization in comparison and hashing branches for non-empty whitespace strings.
Recommendation: Normalize whitespace strings to TenantId.Empty in Equals, GetHashCode, and CompareTo.
Status: REMEDIATED AND VERIFIED (406 passing tests)

EVT-GOV-001
Severity: LOW
Category: Governance / Compliance
Location: StaticEventTypeRegistry.cs / EventBusOptions.cs / IEventEnvelope.cs
Evidence: StaticEventTypeRegistry omitted the first-line MIT license header; EventBusOptions declared an enum in the same file; IEventEnvelope defined two interfaces in a single file.
Reproduction: verify-compliance.ps1
Impact: Violation of repository governance guidelines.
Root Cause: Historical omission of file splitting for secondary types.
Recommendation: Split secondary types into dedicated files and prepend canonical MIT license headers.
Status: MITIGATED / DOCUMENTED
```