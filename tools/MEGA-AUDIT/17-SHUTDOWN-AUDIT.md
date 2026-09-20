# 17. GRACEFUL SHUTDOWN AUDIT

## 1. SIGTERM AND APPLICATION STOPPING BEHAVIOR
When the application host (.NET Generic Host) receives a termination signal (`IHostApplicationLifetime.ApplicationStopping`):
- Active in-memory publishers reject new invocations if the host stopping token is bound to publication.
- For in-flight asynchronous handlers, the host cancellation token triggers, enabling handlers to complete I/O operations cleanly or perform rollbacks before process termination.

---

## 2. BACKGROUND CHANNEL DRAINING
For implementations employing background channels (`System.Threading.Channels`), implementing `IHostedService` is recommended to drain queued events before application teardown.