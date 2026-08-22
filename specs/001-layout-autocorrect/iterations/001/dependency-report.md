# Dependency Report: Iteration 001

**Schema**: v1
**Reviewed**: 2026-08-19 (independence evidence updated 2026-08-20; restated as history 2026-08-22)
**Baseline Ref**: 218be8bc61d26a2b2449332c15afa21a3b59e6af

> **⚠️ Review Evidence Warning** _(Form-vs-Meaning Gap Detected)_
>
> This iteration's task tracking declares **18 completed task(s)**, but the git diff against baseline `218be8bc61d26a2b2449332c15afa21a3b59e6af` contains **251 file(s)**.
>
> **Severity**: WARNING
> **Implication**: Review evidence may be incomplete or misleading.
>
> **Possible causes**:
>
> - Implementation work was not committed before scaffolding review artifacts
> - Task status markers in plan.md or review.md do not match actual progress
> - Baseline reference in state.md is stale or incorrect
>
> **Remediation**:
>
> 1. Verify implementation is committed: `git diff 218be8bc61d26a2b2449332c15afa21a3b59e6af...HEAD --stat`
> 2. If uncommitted work exists: `git add . && git commit -m "Implementation complete"`
> 3. Re-run scaffolder with `-Force` flag to regenerate review artifacts after commit
> 4. Re-run `validate-governance.ps1` to clear pre-review commit gate error
>
> _See Proposal 073 (Review Evidence Integrity) for background on this validation._

---

## Dependency Delta

| Ecosystem | Package | Prior Version | New Version | Change Type | License | Owning Task |
| --------- | ------- | ------------- | ----------- | ----------- | ------- | ----------- |
| (none) | (none) | none | none | none | unknown | (none) |

## New-to-Project

- none

## Vulnerability Scan

- status: scanned
- tool: npm
- version: 11.12.1
- exit_code: 1
- high_critical_findings: 0

```text
npm error code ENOLOCK
npm error audit This command requires an existing lockfile.
npm error audit Try creating one first with: npm i --package-lock-only
npm error audit Original error: loadVirtual requires existing shrinkwrap file
{
  "error": {
    "code": "ENOLOCK",
    "summary": "This command requires an existing lockfile.",
    "detail": "Try creating one first with: npm i --package-lock-only\nOriginal error: loadVirtual requires existing shrinkwrap file"
  }
}
npm error A complete log of this run can be found in: C:\Users\alon\AppData\Local\npm-cache\_logs\2026-08-22T10_33_21_501Z-debug-0.log
```

## Transitive Surface

- unresolved | No lockfile- or tool-backed transitive resolution signal was captured in v1.
