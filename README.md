# .NET DevSecOps Gates Demo

A portfolio-ready **.NET 8 Web API** repository demonstrating **DevSecOps security gates** in both **GitHub Actions** and **Azure DevOps**.

| Gate | Tool | Platform |
|------|------|----------|
| SAST | CodeQL (security-extended) | GitHub Actions |
| Secrets Scanning | Gitleaks | GitHub Actions |
| Dependency Review | dependency-review-action | GitHub Actions (PR) |
| Vulnerability Check | `dotnet list package --vulnerable` | GitHub Actions |
| SBOM Generation | Anchore SBOM Action (SPDX JSON) | GitHub Actions |
| Security DevOps Scan | MicrosoftSecurityDevOps@1 (SARIF) | Azure DevOps |
| Build + Test | dotnet CLI | Both |
| Format Check | dotnet format | GitHub Actions |

> No real secrets are required or included. This repository is designed for screenshots, videos, and PDF portfolio docs.

---

## Project Structure

```
src/GatesDemo.Api/           Secure .NET 8 minimal API
tests/GatesDemo.Api.Tests/   Integration tests (xUnit)
.github/workflows/ci.yml     CI: build, test, format, vuln check, SBOM
.github/workflows/security.yml  CodeQL + Gitleaks + Dependency Review
azure-pipelines.yml           Azure DevOps: build, test, MSDO scan
eng/pipelines/                Alternative pipeline location
docs/                         Portfolio docs, screenshot checklist, report
```

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.x)

### Build and Test

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

### Run the API

```bash
dotnet run --project src/GatesDemo.Api
```

### Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Health check |
| GET | `/api/ping` | Returns `{ "status": "ok" }` |
| POST | `/api/echo` | Validates and echoes a sanitized message |
| GET | `/api/redirect?target=...` | Allowlist-only HTTPS redirect |

---

## GitHub Actions

### CI Workflow (`.github/workflows/ci.yml`)

Runs on push/PR to main:
1. Build with `TreatWarningsAsErrors` (CI mode)
2. Run tests
3. Format check (`dotnet format --verify-no-changes`)
4. Vulnerability check (`dotnet list package --vulnerable`)
5. SBOM generation (SPDX JSON artifact)

### Security Workflow (`.github/workflows/security.yml`)

Runs on push/PR to main:
1. **CodeQL** — C# SAST with `security-extended` queries
2. **Gitleaks** — Secrets scanning with custom demo rule
3. **Dependency Review** — Blocks PRs introducing vulnerable dependencies (PR only)

> **Note:** Code Scanning alerts and Dependency Review UI features require the repository to be **public**, or require [GitHub Advanced Security](https://docs.github.com/en/get-started/learning-about-github/about-github-advanced-security) licensing for private repositories.

### Dependabot

Configured in `.github/dependabot.yml` for weekly updates to GitHub Actions and NuGet packages.

---

## Azure DevOps

### One-Time Setup

1. Install the [Microsoft Security DevOps](https://marketplace.visualstudio.com/items?itemName=ms-securitydevops.microsoft-security-devops-azdevops) extension from the Azure DevOps Marketplace (org-level).
2. Create a new pipeline: **Pipelines > New pipeline > Existing Azure Pipelines YAML file** > select `azure-pipelines.yml`.

### What the Pipeline Does

1. Installs .NET 8 SDK
2. Restore, build, and test
3. Publishes test results (`.trx`)
4. Runs **MicrosoftSecurityDevOps@1** scan with SARIF output
5. Publishes `CodeAnalysisLogs` artifact (downloadable SARIF files)

---

## Demo Scenarios

### 1. Secrets Scanning Failure (Safe Demo)

The `.gitleaks.toml` includes a demo-only rule that detects lines starting with
`DEMO_SECRET` followed by an equals sign and a value.

To trigger a failure for screenshots:

```bash
git checkout -b demo/secrets-leak
# Create a file whose first line is the token DEMO_SECRET, an equals sign, then a value:
printf 'DEMO_SECRET=leak_me\n' > demo/demo-leak.txt
git add demo/demo-leak.txt
git commit -m "Add demo leak for Gitleaks testing"
git push -u origin demo/secrets-leak
```

Open a PR to `main`, capture the Gitleaks failure screenshot, then **close the PR without merging**.

### 2. CodeQL Before/After

**Vulnerable branch** (`demo/vulnerable-codeql`):
- Removes redirect allowlist validation (open redirect)
- Marked with `// DEMO VULNERABLE - do not merge` comments
- Open PR to main > capture CodeQL alert screenshot > close PR

**Fix branch** (`demo/fix-codeql`):
- Restores the allowlist validation (same as main)
- Open PR to main > capture clean CodeQL result > close PR

---

## Build Conventions

- **Central Package Management** — all versions in `Directory.Packages.props`
- **Shared Build Props** — `Directory.Build.props` sets `net8.0`, nullable, analyzers
- **CI Strictness** — `TreatWarningsAsErrors` enabled only when `ContinuousIntegrationBuild=true`
- **SDK Pin** — `global.json` pins to .NET 8.0.x SDK

---

## Portfolio Docs

| File | Purpose |
|------|---------|
| `docs/screenshot-checklist.md` | Exact screenshot targets for both platforms |
| `docs/security-gates-report-sample.md` | PDF-ready security gates report |
| `docs/portfolio-assets.md` | Video script, gig captions, asset checklist |

---

## License

[MIT](LICENSE)

## Security

See [SECURITY.md](SECURITY.md) for vulnerability reporting guidance.
