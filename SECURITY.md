# Security Policy

## Supported Versions

Security fixes and patches are released for the following versions of `EricksonLopez.Events`:

| Version | Supported | .NET Target | Status |
| :--- | :---: | :---: | :--- |
| **2.0.x** | ✅ Yes | .NET 8.0, 9.0, 10.0 | **Active Support (Current)** |
| **1.0.x** | ⚠️ Maintenance | .NET 8.0, 9.0, 10.0 | Maintenance (Critical Security Only) |
| < 1.0.0 | ❌ No | — | Unsupported |

---

## 🔒 Reporting a Vulnerability

We take the security of `EricksonLopez.Events` seriously. If you believe you have discovered a vulnerability, security flaw, or boundary isolation defect, please follow these reporting guidelines:

1. **Do NOT report security vulnerabilities via public GitHub issues, discussions, or pull requests.**
2. Report vulnerabilities privately via **GitHub Private Vulnerability Reporting** directly in this repository:
   - Navigate to the **Security** tab of this repository.
   - Click **Report a vulnerability**.
   - Provide a detailed description, reproduction steps, and impact assessment.
3. Alternatively, you can email our security team directly at **[ericksonlopezf@gmail.com](mailto:ericksonlopezf@gmail.com)**.

### Our Commitment
- **Initial Response**: We will acknowledge receipt of your vulnerability report within **48 hours**.
- **Assessment**: We will provide a status update and remediation timeline within **5 business days**.
- **Coordinated Disclosure**: Once patched, a GitHub Security Advisory and CVE (if applicable) will be published alongside the patched release.

---

## 🛡️ Supply Chain Security

To protect consumers against supply chain tampering and unauthorized package injection, `EricksonLopez.Events` implements the following safeguards:

1. **Strong Name Cryptographic Signing**: All production assemblies are signed with a strong name key (`EricksonLopez.snk`), ensuring assembly binary identity and preventing assembly spoofing.
2. **Sigstore Keyless Provenance Attestation**: Release artifacts generate verifiable cryptographic build provenance attestations using Sigstore via GitHub Actions (`actions/attest-build-provenance@v2`), achieving SLSA Build Level 2/3 supply chain integrity.
3. **NuGet Trusted Publishing (OIDC)**: Packages are published to NuGet.org using passwordless OpenID Connect (OIDC) through GitHub Actions (`NuGet/login@v1`), eliminating long-lived API tokens and credential theft risks.
4. **Central Package Management (CPM)**: All external and transitively referenced package versions are pinned in [`Directory.Packages.props`](Directory.Packages.props) to prevent dependency confusion attacks and untrusted package floating.
5. **Deterministic & Reproducible Builds**: All builds configure `EmbedUntrackedSources=true`, `PublishRepositoryUrl=true`, and produce deterministic symbol packages (`.snupkg`).
6. **Strict Warning & Analyzer Policies**: The entire solution enforces `TreatWarningsAsErrors=true` and `AnalysisLevel=latest-recommended`.
7. **Roslyn Security Analyzers**: Built-in analyzers (`ELE001`–`ELE006`) prevent domain event leaks and ensure event immutability at compile time.

---

## 🧱 Known Security Boundaries & Design Guarantees

1. **Tenant Isolation Boundary**: `TenantId` in `EricksonLopez.Events.Contracts` is strictly an event envelope partition routing and diagnostic correlation identifier. It does not replace or perform authorization/database isolation checks, which are the sovereign responsibility of `EricksonLopez.MultiTenancy`.
2. **In-Process Boundary Isolation**: `InMemoryEventPublisher` and `EventBus` are strictly in-process dispatchers. They do not cross process or machine boundaries and do not parse untrusted network packets directly.
3. **Serialization Hardening**: `EricksonLopez.Events.Serialization.SystemTextJson` uses Native AOT-safe, strongly typed converters and avoids polymorphic runtime `Type.GetType()` reflection deserialization, preventing untrusted type instantiation exploits.
