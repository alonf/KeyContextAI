# Code Map: Iteration 001

**Schema**: v1
**Reviewed**: 2026-08-19 (independence evidence updated 2026-08-20; restated as history 2026-08-22)
**Baseline Ref**: 218be8bc61d26a2b2449332c15afa21a3b59e6af
**Test-to-Code Ratio**: 15:40

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

## Files Touched

| Path | Lines Added | Lines Removed | Owning Task ID(s) | Owning Role |
| ---- | ----------- | ------------- | ----------------- | ----------- |
| .agents/hooks.json | 19 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .github/workflows/ci.yml | 44 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .gitignore | 5 | 32 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/.specrew-extension-runtime.json | 851 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/brownfield-merge.ps1 | 4 | 4 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/deploy-speckit-extension.ps1 | 20 | 1 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/intake/helpers/Detect-RepoStack.ps1 | 2 | 2 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/intake/helpers/Read-IntakeYaml.ps1 | 16 | 16 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/resolve-quality-profile.ps1 | 27 | 27 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/scaffold-retro-artifact.ps1 | 40 | 10 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/scaffold-review-artifact.ps1 | 36 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/scaffold-reviewer-artifacts.ps1 | 115 | 20 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/shared-governance.ps1 | 452 | 4 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/specrew-conformance-provider.ps1 | 73 | 7 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/scripts/validate-governance.ps1 | 346 | 2 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/extensions/specrew-speckit/squad-templates/agents/reviewer/charter.md | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specify/feature.json | 3 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/handoff-evidence.json | 49 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/handover/session-handover.md | 83 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/handover/session-handover.md.old | 83 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/last-start-prompt.md | 372 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/last-validator-summary.json | 12 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/budget-resets/reset-98c5f7bf866da569a94c.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000001-held.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000001-released.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000002-held.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000002-released.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000003-held.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000003-released.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000004-held.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000004-released.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000005-held.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/claims/lin-001-layout-autocorrect/00000005-released.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/dispositions/run-20260819-061549610-c414c854/disposition-0c9d59388470b782ae37.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/grants/grant-590976d8337a88526e4b.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/grants/grant-6112659277b2c8a5dafb.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/grants/grant-6139b9e7122a03462e3e.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/grants/grant-be66a34e1c15d7564f68.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/grants/grant-c8a8d815991e642ee980.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/releases/res-5692740df75198de3345.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/reservations/grant-590976d8337a88526e4b/slot-001/generation-001.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/reservations/grant-6112659277b2c8a5dafb/slot-001/generation-001.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/reservations/grant-6139b9e7122a03462e3e/slot-001/generation-001.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/reservations/grant-6139b9e7122a03462e3e/slot-001/generation-002.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/reservations/grant-be66a34e1c15d7564f68/slot-001/generation-001.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/reservations/grant-c8a8d815991e642ee980/slot-001/generation-001.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/claimed.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/invoked.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/pause-decision.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/pending-pause.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/preflighted.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/recovery.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/report.md | 21 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/requested.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/reserved.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-061549610-c414c854/result.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-202406609-404c823c/report.md | 23 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-202406609-404c823c/requested.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-202406609-404c823c/reserved.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-202406609-404c823c/result.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/claimed.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/invoked.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/pause-decision.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/pending-pause.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/preflighted.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/recovery.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/report.md | 38 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/requested.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/reserved.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-210747148-9bd5980b/result.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/claimed.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/invoked.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/pending-pause.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/preflighted.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/recovery.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/report.md | 21 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/requested.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/reserved.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260819-211204294-86de8c6e/result.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/claimed.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/invoked.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/pending-pause.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/preflighted.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/recovery.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/report.md | 21 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/requested.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/reserved.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-083412478-d85dda20/result.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/claimed.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/invoked.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/pending-pause.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/preflighted.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/recovery.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/report.md | 21 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/requested.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/reserved.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/runs/run-20260820-150735904-458c5888/result.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/spend/res-10e655502b436e96f75c.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/spend/res-96e3490934632223289d.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/spend/res-9e5a10eb87ec9dbc02ff.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/spend/res-af2072acda96cc62979f.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i001/spend/res-b3bb90efa1a0f2ad9ef4.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/history/20260819T061759Z-5575fb6d.json | 27 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/history/20260819T090913Z-bf9c7a7b.json | 36 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/history/20260819T211503Z-ae5633d0.json | 27 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/history/20260821T100235Z-ff6146e7.json | 36 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/history/20260821T152336Z-ccc07834.json | 33 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/latest.json | 33 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/override-authorizations/override-0ca3df35085f3ace757c1668.json | 17 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/review/signoff-gate/pending-override.json | 9 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/bootstrap-journal.jsonl | 6 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/co-review-navigator-journal.jsonl | 50 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-journal.jsonl | 66 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-material-owner.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/2aaf1b0c0ec4dda078ecdc52bcdc6007a43df39c7a41d8a8ae6114aa2653aa5c/last-fire.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/2aaf1b0c0ec4dda078ecdc52bcdc6007a43df39c7a41d8a8ae6114aa2653aa5c/stop-block.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/2aaf1b0c0ec4dda078ecdc52bcdc6007a43df39c7a41d8a8ae6114aa2653aa5c/turn-baseline.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/307f8f38411e5087c4c937227cb6965d00eb9a8fd4dd2b3a93474fb6f877ed6c/last-fire.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/307f8f38411e5087c4c937227cb6965d00eb9a8fd4dd2b3a93474fb6f877ed6c/material-nudged.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/307f8f38411e5087c4c937227cb6965d00eb9a8fd4dd2b3a93474fb6f877ed6c/material-satisfied.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/307f8f38411e5087c4c937227cb6965d00eb9a8fd4dd2b3a93474fb6f877ed6c/orientation-rendered.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/307f8f38411e5087c4c937227cb6965d00eb9a8fd4dd2b3a93474fb6f877ed6c/turn-baseline.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/50b513d6f12a6698dbd21ea73490cb8d4125cc43233379c9d9d92e3bec03ba6a/last-fire.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/50b513d6f12a6698dbd21ea73490cb8d4125cc43233379c9d9d92e3bec03ba6a/material-nudged.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/50b513d6f12a6698dbd21ea73490cb8d4125cc43233379c9d9d92e3bec03ba6a/material-satisfied.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/50b513d6f12a6698dbd21ea73490cb8d4125cc43233379c9d9d92e3bec03ba6a/orientation-rendered.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/50b513d6f12a6698dbd21ea73490cb8d4125cc43233379c9d9d92e3bec03ba6a/turn-baseline.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/a41e47aa2cfe326e4c1d1a8d0050ff7b8eb29c626d4bc8d4a1874ed14f9f0834/last-fire.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/a41e47aa2cfe326e4c1d1a8d0050ff7b8eb29c626d4bc8d4a1874ed14f9f0834/material-nudged.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/a41e47aa2cfe326e4c1d1a8d0050ff7b8eb29c626d4bc8d4a1874ed14f9f0834/material-satisfied.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/a41e47aa2cfe326e4c1d1a8d0050ff7b8eb29c626d4bc8d4a1874ed14f9f0834/orientation-rendered.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/a41e47aa2cfe326e4c1d1a8d0050ff7b8eb29c626d4bc8d4a1874ed14f9f0834/turn-baseline.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f25cd9242b79ff56164a214f75f48ab187663f640da3446df7a4db610a1ae317/last-fire.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f25cd9242b79ff56164a214f75f48ab187663f640da3446df7a4db610a1ae317/material-nudged.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f25cd9242b79ff56164a214f75f48ab187663f640da3446df7a4db610a1ae317/material-satisfied.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f25cd9242b79ff56164a214f75f48ab187663f640da3446df7a4db610a1ae317/orientation-rendered.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f25cd9242b79ff56164a214f75f48ab187663f640da3446df7a4db610a1ae317/turn-baseline.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f6adcb8d05cc38cd6789e6deae47002a55c739ced410ee205be43db76a03b360/material-nudged.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/conformance-sessions/f6adcb8d05cc38cd6789e6deae47002a55c739ced410ee205be43db76a03b360/turn-baseline.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-bootstrap-render-0603217a-ebbe-492a-bbaf-19a06d16a999-startup.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-bootstrap-render-142a4782-bd93-402e-9aef-56084372c613-startup.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-bootstrap-render-15d32bb7-f620-4bad-9da7-7c10731bbaff-new.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-bootstrap-render-4ed0a797-1a91-4980-81d4-6d864e5dfb9c-startup.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-bootstrap-render-d28bd5f9-00cd-4c1b-a062-5a2611484bc0-startup.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-bootstrap-render-e9c42e87-5f87-44b1-8b2c-9dfc70bfdd46-startup.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-health/claude-cli-sessionstart.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-health/claude-cli-stop.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-health/copilot-cli-agentstop.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-health/copilot-cli-sessionstart.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/hook-output-authority.jsonl | 67 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-channel1.json | 1 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-state-0603217a-ebbe-492a-bbaf-19a06d16a999.json | 16 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-state-142a4782-bd93-402e-9aef-56084372c613.json | 24 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-state-15d32bb7-f620-4bad-9da7-7c10731bbaff.json | 16 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-state-4ed0a797-1a91-4980-81d4-6d864e5dfb9c.json | 16 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-state-d28bd5f9-00cd-4c1b-a062-5a2611484bc0.json | 16 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/refocus-state-e9c42e87-5f87-44b1-8b2c-9dfc70bfdd46.json | 56 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/review-authorship.json | 22 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/session-marker.json | 8 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/runtime/workshop-authority.jsonl | 32 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/start-context.json | 120 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .specrew/version-check-cache.json | 7 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .squad/active-features.yml | 1 | 1 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .squad/casting/registry.json | 10 | 10 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .squad/decisions.md | 92 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .squad/events/lifecycle-events.jsonl | 7 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .squad/identity/now.md | 5 | 5 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| .squad/team.md | 5 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| Directory.Build.props | 29 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| KeyContextAI.slnx | 12 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| data/dictionaries/en-US/pack.json | 17 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| data/dictionaries/en-US/words.txt | 370079 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| data/dictionaries/he-IL/pack.json | 23 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| data/dictionaries/he-IL/words.txt | 22250 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| data/keymaps/en-US_he-IL.json | 52 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/.specrew-runtime.json | 12 | 12 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/continuous-co-review-navigator.ps1 | 24 | 5 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/review-authority-core.ps1 | 39 | 5 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/review-campaign-orchestrator.ps1 | 115 | 6 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/review-design-context.ps1 | 46 | 4 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/review-result-ingestor.ps1 | 86 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/review-run-index-writer.ps1 | 29 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/review-signoff-evidence-gate.ps1 | 1 | 1 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/reviewer-candidate-prompt.md | 9 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/verification-plan-contract.ps1 | 2 | 2 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/verification-plan-runner.ps1 | 11 | 2 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| scripts/internal/continuous-co-review/verification-plan-supplier.ps1 | 2 | 2 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/contracts/keycontext-ai.md | 153 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/contracts/mechanical-findings.schema.json | 77 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/current-architecture.md | 15 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/data-model.md | 203 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/code-map.md | 297 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/coverage-evidence.md | 57 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/dashboard.md | 38 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/dependency-report.md | 66 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/design-analysis.md | 436 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/drift-log.md | 333 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/lens-applicability.json | 36 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/plan.md | 164 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/quality/hardening-gate.md | 59 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/quality/mechanical-findings.json | 11 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/quality/quality-evidence.md | 129 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/retro.md | 205 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/review-diagrams.md | 53 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/review.md | 306 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/reviewer-index.md | 63 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/state.md | 71 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/iterations/001/tasks-progress.yml | 113 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/plan.md | 245 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/quickstart.md | 70 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/review-diagrams.md | 162 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| specs/001-layout-autocorrect/tasks.md | 270 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.App/App.xaml | 9 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.App/App.xaml.cs | 13 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.App/AssemblyInfo.cs | 10 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.App/Composition/ServiceRegistration.cs | 90 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.App/KeyContextAI.App.csproj | 21 | 0 | T001 | Implementer |
| src/KeyContextAI.App/MainWindow.xaml | 12 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.App/MainWindow.xaml.cs | 21 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Contracts/IDetectionEngine.cs | 29 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Contracts/IDictionaryAccessor.cs | 57 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Contracts/IMappingEngine.cs | 31 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Contracts/IWordAssemblyEngine.cs | 54 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Engines/DetectionEngine.cs | 163 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Engines/MappingEngine.cs | 89 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Engines/WordAssemblyEngine.cs | 95 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/KeyContextAI.Core.csproj | 9 | 0 | T001 | Implementer |
| src/KeyContextAI.Core/Model/Candidate.cs | 16 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Model/CautionLevel.cs | 17 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Model/CorrectionVerdict.cs | 52 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Model/DictionarySnapshot.cs | 59 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Model/KeyEvent.cs | 27 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Model/KeyEventKind.cs | 26 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Core/Model/LayoutId.cs | 34 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| src/KeyContextAI.Platform/KeyContextAI.Platform.csproj | 13 | 0 | T001 | Implementer |
| src/KeyContextAI.Platform/Storage/DictionaryAccessor.cs | 162 | 0 | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T020, T025 | Implementer |
| tests/Directory.Build.props | 22 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Architecture.Tests/CallRuleTests.cs | 177 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Architecture.Tests/KeyContextAI.Architecture.Tests.csproj | 25 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Architecture.Tests/UnitTest1.cs | 10 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/CautionLevelTests.cs | 127 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/DetectionEngineTests.cs | 255 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/KeyContextAI.Core.Tests.csproj | 25 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/LayoutMaps.cs | 78 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/MappingEngineTests.cs | 120 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/UnitTest1.cs | 10 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Core.Tests/WordAssemblyEngineTests.cs | 186 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Platform.Tests/CorpusAccuracyTests.cs | 192 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Platform.Tests/KeyContextAI.Platform.Tests.csproj | 26 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/KeyContextAI.Platform.Tests/UnitTest1.cs | 10 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |
| tests/corpus/en-he-corpus.json | 69 | 0 | T002, T007, T011, T012, T013, T025 | Implementer |

## Public-API Delta

### Added

- Initialize-SpecrewPreexistingArtifacts (.specify/extensions/specrew-speckit/scripts/scaffold-reviewer-artifacts.ps1)
- Test-SpecrewFileExistedBeforeThisRun (.specify/extensions/specrew-speckit/scripts/scaffold-reviewer-artifacts.ps1)
- Get-SpecrewDeployedExtensionMarkerPath (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewDeployedExtensionManifest (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Write-SpecrewDeployedExtensionMarker (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Test-SpecrewDeployedExtensionIntegrity (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewReviewAuthorshipPath (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Test-SpecrewReviewAuthorshipSourcePath (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewReviewRecordPathMatch (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Write-SpecrewReviewAuthorshipObservation (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewReviewAuthorship (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Test-SpecrewDerivedCoverageSourcePath (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewQualifyingIndependentRun (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewDerivedIndependenceBlock (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Get-SpecrewEmbeddedIndependenceBlock (.specify/extensions/specrew-speckit/scripts/shared-governance.ps1)
- Test-ReviewDerivedIndependenceBlock (.specify/extensions/specrew-speckit/scripts/validate-governance.ps1)
- Test-DeployedExtensionIntegrity (.specify/extensions/specrew-speckit/scripts/validate-governance.ps1)
- Test-ScaffoldPendingSiblings (.specify/extensions/specrew-speckit/scripts/validate-governance.ps1)
- Test-ReviewRecordAuthorship (.specify/extensions/specrew-speckit/scripts/validate-governance.ps1)
- Test-ReviewCitedRunEvidence (.specify/extensions/specrew-speckit/scripts/validate-governance.ps1)
- Test-ReviewAuthorityExaminedPathsField (scripts/internal/continuous-co-review/review-authority-core.ps1)
- Test-ReviewExaminedPathIsSource (scripts/internal/continuous-co-review/review-result-ingestor.ps1)
- Resolve-ReviewDeclaredCoverage (scripts/internal/continuous-co-review/review-result-ingestor.ps1)

### Removed

- none

## Module Hotspots

- Threshold: 250 changed lines per file
- .specify/extensions/specrew-speckit/.specrew-extension-runtime.json (851 changed lines)
- .specify/extensions/specrew-speckit/scripts/shared-governance.ps1 (456 changed lines)
- .specify/extensions/specrew-speckit/scripts/validate-governance.ps1 (348 changed lines)
- .specrew/last-start-prompt.md (372 changed lines)
- data/dictionaries/en-US/words.txt (370079 changed lines)
- data/dictionaries/he-IL/words.txt (22250 changed lines)
- specs/001-layout-autocorrect/iterations/001/code-map.md (297 changed lines)
- specs/001-layout-autocorrect/iterations/001/design-analysis.md (436 changed lines)
- specs/001-layout-autocorrect/iterations/001/drift-log.md (333 changed lines)
- specs/001-layout-autocorrect/iterations/001/review.md (306 changed lines)
- specs/001-layout-autocorrect/tasks.md (270 changed lines)
- tests/KeyContextAI.Core.Tests/DetectionEngineTests.cs (255 changed lines)
