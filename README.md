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
````

### Run the API

```bash
dotnet run --project src/GatesDemo.Api
```

Default endpoints:

* `GET /api/ping` → `{ "status": "ok" }`
* `POST /api/echo` → validates input and returns a sanitized echo
* `GET /api/redirect?target=...` → allowlist-only redirect (blocked unless host is allowed)
* `GET /health` → health check

---

## GitHub Actions: what to screenshot

1. **CI success**

* Actions → CI → successful run

2. **Security scans**

* Actions → Security → successful run

3. **CodeQL alerts**

* Security → Code scanning → alerts view

4. **SBOM artifact**

* CI run → Artifacts → `sbom.spdx.json`

> Note: CodeQL alert UI is available on public repos. Private repos may require GitHub Advanced Security for full code scanning features.

---

## Azure DevOps: setup + what to screenshot

### One-time setup

1. Create a new pipeline in Azure DevOps:

   * Pipelines → New pipeline → select this repo → “Existing Azure Pipelines YAML file”
2. Select `azure-pipelines.yml`
3. Install the **Microsoft Security DevOps** extension in your Azure DevOps organization (required).

### Screenshots to capture

* Pipeline run summary (green)
* Artifacts showing `CodeAnalysisLogs` / SARIF output

---

## Demo scenarios (for proof screenshots)

### 1) Secrets scanning failure (safe demo)

This repo uses a demo-only Gitleaks rule in `.gitleaks.toml`.

To demonstrate a failing PR:

1. Create a branch and add a file like `docs/demo-leak.txt`:

   ```
   DEMO_SECRET=leak_me
   ```
2. Open a PR to `main`
3. Capture the failed workflow screenshot
4. Close the PR (do not merge)

### 2) CodeQL “before/after” (optional)

Use the demo branches (if present):

* `demo/vulnerable-codeql` → intentionally introduces a CodeQL-detectable issue (demo-only)
* `demo/fix-codeql` → fixes it properly

Create PRs from each branch into `main` to capture:

* “finding detected” screenshot
* “fixed / resolved” screenshot

---

## Project standards used

* Secure-by-default API behavior (validation, safe redirects, sensible middleware)
* Central package management (`Directory.Packages.props`)
* Consistent build settings (`Directory.Build.props`)
* Minimal, portfolio-focused documentation

---

## What this repo is for

This repo exists to demonstrate that you can deliver a **PR-based DevSecOps gates install**:

* A buyer’s repo → you add gates → runs fail/pass → you ship handoff docs
* You capture screenshots/video/PDF assets without using client IP

---

## License

Choose a license that fits your publishing goals (MIT is common for portfolio templates).

---

## Contact / Security

See `SECURITY.md` for reporting guidance.
