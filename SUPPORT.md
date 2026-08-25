# Support Policy

Thank you for using **`EricksonLopez.Events`**!

This document describes the support options, community communication channels, and troubleshooting resources available for the library.

---

## 💬 Community Support Channels

| Channel | Purpose | Response Expectations |
| :--- | :--- | :--- |
| **[GitHub Issues](https://github.com/ericksonlopezf/dotnet-events/issues)** | Bug reports, verified defects, and breaking change regressions | Best-effort triage by maintainers |
| **[GitHub Discussions](https://github.com/ericksonlopezf/dotnet-events/discussions)** | Architecture Q&A, integration guidance, and design proposals | Community and maintainer collaboration |
| **[Security Advisory](SECURITY.md)** | Private vulnerability and security reports | Acknowledged within 48 hours |

---

## 📚 Official Documentation & Guides

Before filing an issue, please check our exhaustive documentation resources:

- **[Root README](README.md)**: Quickstart, installation, core types, and architecture overview.
- **[Architecture Decision Records (ADRs)](docs/adr/)**: 31 formal ADRs detailing why decisions were made.
- **[Source Generator Guide](docs/guides/using-generator-for-aot.md)**: How to configure zero-reflection Native AOT registries.
- **[Clean Architecture Integration](docs/guides/clean-architecture-integration.md)**: DDD and Clean Architecture layer boundaries.
- **[Transactional Outbox Integration](docs/guides/integrating-outbox.md)**: How to bridge `EventEnvelope<T>` with outbox storage.
- **[Mediator Integration](docs/guides/integrating-with-mediator.md)**: Integrating in-process handlers with mediator pipelines.
- **[Testing Strategy](tests/README.md)**: Unit testing, test doubles (`EricksonLopez.Events.Testing`), and mutation testing.

---

## 🐛 Reporting a Bug

When filing a bug report in [GitHub Issues](https://github.com/ericksonlopezf/dotnet-events/issues), please provide:
1. The exact **.NET SDK version** (`dotnet --version`).
2. The specific package name(s) and version(s) involved.
3. Target platform / OS (Windows, Linux, macOS) and architecture (`x64`, `arm64`).
4. Native AOT status (`PublishAot=true` or standard JIT).
5. A minimal, reproducible code example or test case demonstrating the unexpected behavior.

---

## 🏢 Commercial & Enterprise Inquiries

For custom integration consulting, architectural review, or enterprise licensing inquiries, contact **[ericksonlopezf@gmail.com](mailto:ericksonlopezf@gmail.com)**.
