# Internationalization & Culture Invariance (i18n)

---

## 1. Principles of Culture-Invariant Event Data

Events are immutable data records exchanged across distributed systems, message queues, and temporal audit logs. To guarantee cross-boundary consistency:

1. **Standardized Timestamps**: All event timestamps (`OccurredAt`, `PublishedAt`) MUST be serialized as UTC ISO 8601 strings (`yyyy-MM-ddTHH:mm:ss.fffffffZ`) via `DateTimeOffset`.

2. **Culture-Invariant Numeric & Currency Serialization**: All decimal and numeric values within event payloads MUST format using `CultureInfo.InvariantCulture`.
3. **Canonical Event Names**: Event names must use ASCII alphanumeric characters with dot separators (`orders.placed.v1`).
