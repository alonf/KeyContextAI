# Review Diagrams: Iteration 001

**Schema**: v1
**Diagram Format**: mermaid

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

## Structure Diagram

```mermaid
graph TD
  omitted["_omitted_"]
```

## Flow Diagram

```mermaid
flowchart TD
  scripts_internal_continuous_co_review_review_run_index_writer["scripts/internal/continuous-co-review/review-run-index-writer"]
  src_KeyContextAI_App_App_xaml["src/KeyContextAI.App/App.xaml"]
  src_KeyContextAI_App_MainWindow_xaml["src/KeyContextAI.App/MainWindow.xaml"]
  src_KeyContextAI_Core_Contracts_IMappingEngine["src/KeyContextAI.Core/Contracts/IMappingEngine"]
  src_KeyContextAI_Core_Engines_MappingEngine["src/KeyContextAI.Core/Engines/MappingEngine"]
```

## Omissions

- Structure diagram omitted: inter-module edges (0) below threshold (2).

## Local View Hints

- specs\001-layout-autocorrect\iterations\001\review-diagrams.md
