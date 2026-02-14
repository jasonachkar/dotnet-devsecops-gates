# Screenshot Checklist

Capture these screenshots in order for your portfolio and demo materials.

## GitHub Actions

- [ ] **CI Workflow — Success Run**
  Navigate to Actions tab > CI workflow > latest successful run. Capture the full job summary showing all green steps.

- [ ] **Security Workflow — CodeQL Analysis**
  Navigate to Security tab > Code scanning alerts. Capture the alerts page (should show zero alerts on main).

- [ ] **Gitleaks PR Failure**
  Open a PR from `demo/secrets-leak` branch. Capture the failed Gitleaks check showing the `DEMO_SECRET` detection.

- [ ] **Dependency Review on PR**
  Open any PR that modifies packages. Capture the Dependency Review check output.

- [ ] **SBOM Artifact**
  Navigate to a CI workflow run > Artifacts section. Capture showing `sbom.spdx.json` available for download.

- [ ] **CodeQL Alert on Vulnerable PR**
  Open a PR from `demo/vulnerable-codeql` branch. Capture the CodeQL alert for unvalidated URL redirection.

- [ ] **Clean CodeQL on Fix PR**
  Open a PR from `demo/fix-codeql` branch. Capture the clean (no alerts) CodeQL result.

## Azure DevOps

- [ ] **Pipeline Run Summary**
  Navigate to Pipelines > latest run. Capture the full run summary with all green stages.

- [ ] **Test Results Tab**
  Click into the pipeline run > Tests tab. Capture the 3 passing tests.

- [ ] **CodeAnalysisLogs Artifact**
  Navigate to the pipeline run > Artifacts. Capture showing `CodeAnalysisLogs` artifact with SARIF files inside.

- [ ] **Microsoft Security DevOps Task Output**
  Expand the "Microsoft Security DevOps" step in the pipeline log. Capture the scan summary output.

## Export Checklist

- [ ] Export `docs/security-gates-report-sample.md` to PDF
- [ ] Combine screenshots into a single portfolio PDF or slide deck
- [ ] Save raw screenshots at full resolution for video/demo use
