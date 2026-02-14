# DevSecOps Security Gates Report

**Project:** GatesDemo — .NET 8 Web API
**Date:** February 2026
**Platforms:** GitHub Actions, Azure DevOps

---

## 1. Security Gates Overview

| Gate | Tool | Trigger | Platform | Blocks Merge? |
|------|------|---------|----------|---------------|
| SAST (Static Analysis) | CodeQL | Push & PR to main | GitHub Actions | Yes — on High/Critical findings |
| Secrets Scanning | Gitleaks | Push & PR to main | GitHub Actions | Yes — on any detected secret |
| Dependency Review | dependency-review-action | PR to main | GitHub Actions | Yes — on High/Critical vulnerabilities |
| Vulnerability Scan | `dotnet list package --vulnerable` | Push & PR to main | GitHub Actions | Yes — on any vulnerable package |
| SBOM Generation | Anchore SBOM Action | Push & PR to main | GitHub Actions | No — artifact only |
| Security DevOps Scan | MicrosoftSecurityDevOps@1 | Push & PR to main | Azure DevOps | Configurable |
| Build + Test | dotnet CLI | Push & PR to main | Both | Yes — on failure |
| Format Check | dotnet format | Push & PR to main | GitHub Actions | Yes — on formatting violations |

## 2. Gate Details

### 2.1 SAST — CodeQL

**What it detects:** Injection flaws, open redirects, insecure deserialization, hardcoded credentials, and other OWASP Top 10 patterns in C# code.

**Query suite:** `security-extended` — includes standard security queries plus additional coverage for common vulnerability patterns.

**How to triage findings:**
- Review the alert in GitHub Security > Code scanning
- Identify the data flow from source (user input) to sink (dangerous operation)
- Fix by adding validation, sanitization, or using safe APIs
- Mark as "won't fix" only with documented justification

### 2.2 Secrets Scanning — Gitleaks

**What it detects:** API keys, tokens, passwords, and custom patterns defined in `.gitleaks.toml`.

**Custom rules:** This repo includes a demo rule that detects lines beginning with the
token `DEMO_SECRET` followed by an equals sign and a value (see `.gitleaks.toml`).

**How to triage findings:**
1. **If real secret:** Immediately rotate the credential, then remove from history using `git filter-repo` or BFG
2. **If false positive:** Add to `.gitleaks.toml` allowlist with justification comment
3. **If demo/test value:** Ensure it is clearly marked and non-functional

### 2.3 Dependency Review

**What it detects:** Known vulnerabilities (CVEs) in NuGet package dependencies introduced by a pull request.

**Severity threshold:** Fails on High and Critical vulnerabilities by default.

**How to triage findings:**
1. Update the vulnerable package to a patched version
2. If no patch exists, evaluate if the vulnerability applies to your usage
3. Document accepted risk with a tracking issue if suppression is needed

> **Note:** Dependency Review and Code Scanning UI features require the repository to be **public**, or require **GitHub Advanced Security** licensing for private repositories.

### 2.4 Microsoft Security DevOps (Azure DevOps)

**What it runs:** A suite of security analyzers including credential scanning, SARIF-based reporting, and optional tools (Trivy, ESLint, etc.).

**Output:** SARIF files published as pipeline artifacts under `CodeAnalysisLogs`.

**How to view results:**
- Pipeline run > Artifacts > CodeAnalysisLogs
- Download SARIF files and view in VS Code with the SARIF Viewer extension
- Or use the Azure DevOps "Scans" tab if Advanced Security is enabled

> **Prerequisite:** The [Microsoft Security DevOps](https://marketplace.visualstudio.com/items?itemName=ms-securitydevops.microsoft-security-devops-azdevops) extension must be installed in your Azure DevOps organization.

## 3. Findings Summary (Sample)

| Category | Total | Critical | High | Medium | Low |
|----------|-------|----------|------|--------|-----|
| SAST (CodeQL) | 0 | 0 | 0 | 0 | 0 |
| Secrets | 0 | — | — | — | — |
| Dependencies | 0 | 0 | 0 | 0 | 0 |

*Main branch is clean. Demo branches contain intentional findings for demonstration.*

## 4. Recommendations

1. **Run all gates on every PR** — Never merge without green security checks.
2. **Review CodeQL alerts weekly** — Even dismissed alerts should be periodically re-evaluated.
3. **Keep dependencies updated** — Dependabot is configured to open weekly PRs for NuGet and GitHub Actions updates.
4. **Rotate any detected secrets immediately** — Treat every Gitleaks finding as a real incident until confirmed otherwise.
5. **Export SBOM regularly** — The SPDX JSON artifact provides a software bill of materials for compliance and audit purposes.

---

*This report was generated from the GatesDemo portfolio repository. All findings are from demonstration scenarios. No real vulnerabilities or secrets are present in the main branch.*
