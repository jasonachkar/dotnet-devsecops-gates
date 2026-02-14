# Portfolio Assets Guide

## 60-Second Video Script Outline

### Opening (0–10s)
"This is a .NET 8 Web API with full DevSecOps security gates configured for both GitHub Actions and Azure DevOps."

### GitHub Actions Demo (10–30s)
- Show the CI workflow: build, test, format check, vulnerability scan, SBOM generation
- Show the Security workflow: CodeQL SAST analysis, Gitleaks secrets scanning
- Open a PR with a demo secret — show Gitleaks blocking the merge
- Show the CodeQL alert for an open redirect vulnerability on a demo branch

### Azure DevOps Demo (30–45s)
- Show the pipeline run with Microsoft Security DevOps scan
- Show the CodeAnalysisLogs artifact with SARIF output
- Show the test results tab with all tests passing

### Closing (45–60s)
"Every PR is automatically scanned for vulnerabilities, secrets, and insecure code patterns before it can be merged. This is what shift-left security looks like in practice."

---

## Gig Headlines / Captions

1. **"Automated Security Gates for Your .NET CI/CD Pipeline"**
   I will set up CodeQL, secrets scanning, dependency review, and SBOM generation in your GitHub Actions or Azure DevOps pipelines.

2. **"DevSecOps Pipeline Setup for .NET Applications"**
   I will configure SAST, SCA, and secrets scanning gates that block vulnerable code from reaching production.

3. **"Shift-Left Security for Your .NET API"**
   I will implement security-as-code with automated scanning, build gates, and compliance reporting for your .NET projects.

---

## Assets to Capture Checklist

### Screenshots (see screenshot-checklist.md for details)
- [ ] GitHub Actions CI workflow success
- [ ] GitHub Security tab — Code scanning alerts
- [ ] Gitleaks failure on demo PR
- [ ] SBOM artifact in CI run
- [ ] Azure DevOps pipeline success
- [ ] Azure DevOps CodeAnalysisLogs artifact
- [ ] Azure DevOps test results tab

### PDF Exports
- [ ] Security Gates Report (from `security-gates-report-sample.md`)
- [ ] Screenshot compilation document

### Video Recording
- [ ] 60-second walkthrough following the script above
- [ ] Record at 1080p minimum, use browser zoom for readability
- [ ] No personal information visible in browser tabs or bookmarks

### Repository
- [ ] Ensure README is complete and professional
- [ ] Verify all workflows run green on main branch
- [ ] Confirm demo branches exist with clear documentation
- [ ] Pin the repo on your GitHub profile
