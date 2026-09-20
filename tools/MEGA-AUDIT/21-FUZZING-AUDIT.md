# 21. FUZZING & PROPERTY-BASED TESTING AUDIT

## 1. FSCHECK PROPERTY-BASED TEST SUITE
The FsCheck test suite in `EricksonLopez.Events.UnitTests` validates critical mathematical properties:
- **Property 1**: For any arbitrary pair of strings (including control characters, emojis, null sequences, and extreme Unicode), `TenantId.From(s)` maintains reflexive, transitive, and hash code symmetry.
- **Property 2**: Serializing and subsequently deserializing an `EventEnvelope<T>` produces an object identical in all metadata headers and payload fields.
- **Property 3**: Header insertion order within `EventMetadata` does not alter value equality or hash code computation.

---

## 2. REMEDIATED FINDING: EVT-DAT-002 (`EventMetadata` Case-Insensitivity)
During metadata dictionary fuzzing, it was discovered that two `EventMetadata` instances with header keys differing only in casing (e.g., `"x-trace"` vs `"X-Trace"`) evaluated as equal under `Equals()`, but produced differing hash codes because `GetHashCode()` invoked `StringComparer.Ordinal` rather than `StringComparer.OrdinalIgnoreCase`.
The defect was resolved in `EventMetadata.cs` and verified across 100 randomized FsCheck iterations.