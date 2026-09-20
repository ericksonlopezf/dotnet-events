# 10. FORENSIC SECURITY AUDIT (RED TEAM / THREAT MODEL)

## 1. ATTACK VECTOR 1: IN-MEMORY PAYLOAD MUTABILITY (EVT-SEC-001)
- **Attack Surface**: In an in-memory event bus, the same event object reference is passed to multiple registered subscribers (`IEventHandler<T>`).
- **Adversarial Exploitation**:
  If an event model contains properties with mutable reference types (e.g., `List<Item>`, `Dictionary<string, string>`), a defective or compromised handler can mutate event state mid-flight:
  ```csharp
  public ValueTask HandleAsync(OrderCreated eventInstance, CancellationToken ct)
  {
      eventInstance.Items.Clear(); // Corrupts event data for subsequent handlers
      return ValueTask.CompletedTask;
  }
  ```
- **Mitigation**:
  1. The Roslyn analyzer `ELE001` mandates `record` declarations with `init-only` properties.
  2. Developer guidance: Use immutable collections (`IReadOnlyList<T>`, `ImmutableArray<T>`) rather than mutable collections.

---

## 2. ATTACK VECTOR 2: TENANTID FORGERY AND BYPASS (EVT-SEC-004)
- **Discovered Vulnerability**: In prior iterations, `TenantId` did not normalize whitespace symmetrically across `Equals`, `GetHashCode`, and `CompareTo`. An attacker could pass `"   "` or tab characters, resulting in equality discrepancies relative to `TenantId.Empty`.
- **Applied Remediation**:
  `TenantId` was updated in `TenantId.cs` so that any whitespace-only or empty string evaluates strictly to `TenantId.Empty`, yielding consistent hash code `0` and deterministic ordering, verified via FsCheck property tests in `ForensicAdversarialEvidenceTests.cs`.

---

## 3. ATTACK VECTOR 3: MALICIOUS POLYMORPHIC DESERIALIZATION
- **Risk in Event Libraries**: Usage of `TypeNameHandling.All` or `Type.GetType(typeName)` allowing Remote Code Execution (RCE) when deserializing events from untrusted network boundaries.
- **Code Audit**:
  `EricksonLopez.Events.Serialization.SystemTextJson` uses **System.Text.Json with Source Generation** (`JsonSerializerContext`). No usage of `Newtonsoft.Json` or `TypeNameHandling` exists. Type resolution is strictly restricted to types declared in the application's `EventTypeRegistry`. Attack mitigated by design.

---

## 4. ATTACK VECTOR 4: DENIAL OF SERVICE (DOS) VIA JSON BOMBS
- **Risk**: Payloads with deep recursive nesting or massive string payloads causing stack overflow or out-of-memory crashes.
- **Control**: `JsonSerializerOptions` enforces a maximum depth limit (`MaxDepth = 64`), and stream readers operate asynchronously and with bounded buffers.