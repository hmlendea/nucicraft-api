# Security Policy

This policy defines the process for responsible vulnerability reporting for NuciCraft API and clarifies which released artefacts receive security maintenance.

## 📑 Table of Contents

- [Supported Versions](#-supported-versions)
- [Reporting a Vulnerability](#-reporting-a-vulnerability)
- [Scope](#-scope)
- [Disclosure Policy](#-disclosure-policy)
- [Safe Harbour](#-safe-harbour)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Latest version | Source build from the default branch | ✅ |
| Latest version | Unofficial container registries | ❌ |
| Latest version | Unofficial package mirrors | ❌ |
| Latest version | Unofficial third-party distribution channels | ❌ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/nucicraft-api/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The subsequent report categories are in scope for this repository:
- Authentication and authorisation bypass in API endpoints
- Data exposure, tampering, or injection affecting player, RTP location, zone, or event data

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Vulnerabilities in third-party services or dependencies outside this repository's codebase
- Denial-of-service conditions requiring unrealistic traffic levels or non-supported deployment configurations

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🧾 Safe Harbour

If your research is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.
