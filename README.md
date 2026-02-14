# .NET DevSecOps Gates Demo

A portfolio-ready **.NET 8 Web API** repo that showcases **DevSecOps security gates** in both **GitHub Actions** and **Azure DevOps**:
- ✅ Build + test gates
- ✅ Secrets scanning (Gitleaks demo rule)
- ✅ SAST (CodeQL on GitHub)
- ✅ Dependency risk checks
- ✅ SBOM generation (SPDX JSON)
- ✅ Azure DevOps security scanning with SARIF output (Microsoft Security DevOps)

> No real secrets are required or included. This repository is designed for screenshots, videos, and PDF handoff docs for Fiverr/Upwork.

---

## What’s inside

### Pipelines
**GitHub Actions**
- `.github/workflows/ci.yml`  
  Builds/tests and publishes an SBOM artifact.
- `.github/workflows/security.yml`  
  Runs CodeQL (SAST), Gitleaks (secrets scan), and dependency review on PRs.

**Azure DevOps**
- `azure-pipelines.yml` (or `eng/pipelines/azure-pipelines.yml`)  
  Builds/tests and runs Microsoft Security DevOps scan (SARIF + artifacts).

### App
- `src/GatesDemo.Api` — secure .NET 8 API demo
- `tests/GatesDemo.Api.Tests` — unit tests (xUnit)

### Docs for your portfolio assets
- `docs/screenshot-checklist.md`
- `docs/security-gates-report-sample.md`
- `docs/portfolio-assets.md`

---

## Quick start (local)

### Prereqs
- .NET SDK 8.x

### Build and test
```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
