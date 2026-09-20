# 35. RED TEAM SECURITY ATTACK MATRIX

| Attack Scenario | Vector / Payload | Expected Behavior | Observed Behavior | Status |
| :--- | :--- | :--- | :--- | :---: |
| **Payload Tampering** | Mutating `List<T>` in handler 1. | Deep immutability. | Mutable references are altered in memory. | **MITIGATED BY GUIDANCE** |
| **Tenant Bypass** | `TenantId.From("   ")` vs `Empty`. | Identical equivalence. | Normalized symmetrically after fix EVT-SEC-004. | **REMEDIATED** |
| **Header Casing** | HashCode with keys `"trace"` vs `"TRACE"`. | Equal hash code for equal keys. | Resolved with `OrdinalIgnoreCase` in EVT-DAT-002. | **REMEDIATED** |
| **JSON Bombs** | Nesting depth > 64 levels. | Safe rejection by deserializer. | Throws controlled `JsonException` without StackOverflow. | **PROTECTED** |
| **Polymorphic RCE** | Payload with malicious TypeNameHandling. | Immediate rejection. | Strict System.Text.Json ignores unknown types. | **SECURE** |