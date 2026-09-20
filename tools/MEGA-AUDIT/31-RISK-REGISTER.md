# 31. FORENSIC RISK REGISTER

| Risk ID | Risk Description | Probability | Impact | Severity | Mitigation / Control Plan |
| :--- | :--- | :---: | :---: | :---: | :--- |
| **RSK-01** | Volatile event loss on container crash before publish. | HIGH | HIGH | **HIGH** | Forbid `TransactionalEventPublisher` in production; mandate `EricksonLopez.Outbox`. |
| **RSK-02** | State corruption via in-memory mutation of event collections. | MEDIUM | HIGH | **HIGH** | Architectural design rule: Enforce immutable collections (`ImmutableArray<T>`). |
| **RSK-03** | `DbContext` concurrency collision with `ParallelExecutionStrategy`. | MEDIUM | MEDIUM | **MEDIUM** | Enforce option validation in `EventBusOptions`; mandate per-handler scope. |
| **RSK-04** | Performance degradation via $O(N^2)$ transient instantiation storm. | LOW | HIGH | **MEDIUM** | Mandatory adoption of fluent `AddHandler<THandler>()` API. |
| **RSK-05** | Tenant context forgery or whitespace bypass (`TenantId`). | LOW | HIGH | **LOW** | Strict whitespace normalization remediated and tested via FsCheck. |