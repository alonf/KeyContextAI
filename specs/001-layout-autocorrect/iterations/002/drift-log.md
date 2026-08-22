# Drift Log: Iteration 002

**Schema**: v2

<!--
  Markdown authoring note (Specrew lifecycle convention):

  When you add new drift events to this file, watch for MD032 (blanks-around-lists).
  A sentence ending with a colon, immediately followed by a bullet list, is the most
  common violation. Always put a BLANK LINE between the colon line and the list:

      BAD:                              GOOD:
      Resolution steps:                 Resolution steps:
      - Step one                        <— blank line here
      - Step two                        - Step one
                                        - Step two

  The F-033 pre-boundary markdownlint gate runs markdownlint-cli --fix on .md
  changes before every boundary-sync write, so most violations auto-fix — but the
  blank line you write in the first place avoids the cleanup churn.
-->

## Summary

**Total drift events**: 1 (0 in this project, 1 Specrew-side finding carried in at iteration open)
**Resolution rate**: no in-project drift detected yet (0/0); the 1 tooling finding is open upstream
**Specification drift**: None detected

## Events

### DRIFT-008: the navigator's currency check stales a review on a records-only delta that the validator reads as current

**Detected**: 2026-08-22, at the iteration-closeout → plan boundary stop, immediately after deploying
the Specrew b5c84f48 update.
**Class**: defect (incomplete fix propagation) in Specrew's continuous-co-review evidence gate,
surfaced by this project. Not spec, plan, or implementation drift in KeyContextAI.
**Requirement**: none. No FR or SC of this feature is affected.

**What fired**: the stop-hook advisory reported that run-20260820-150735904-458c5888 "targets a moved
or earlier snapshot and cannot authorize the current tree" (reason `latest-result-not-current`) — on a
tree where the freshly deployed validator renders that same run as pass, complete, current, valid.
Two checks over the same evidence, disagreeing.

**Why, read from the installed module**: the evidence gate does carry the FR-009 records-only
exemption, so recording a review is not supposed to invalidate it. But the exemption classifies the
delta path-by-path in `Test-ReviewCampaignDeltaIsRecordsOnly`, whose machinery list is the core
methodology directories plus every directory carrying a `.specrew-managed` marker, and it fails closed
on the first path it cannot classify. The delta since reviewed tree `273c69bb` contains host mirrors
deployed *without* that marker — `.github/agents/*.agent.md`, `.github/prompts/*.prompt.md`,
`.claude/skills/speckit-*/` (verified: no marker on disk), and the Squad skill directories under
`.github/skills/`. They are written by the Spec-Kit and Squad deployers rather than Specrew's
`Set-ManagedFile`, so the self-describing marker mechanism never covers them, and inert deployed
machinery reads as reviewable content.

Meanwhile the validator's W38 fix asks the source-aware question through
`Test-SpecrewReviewAuthorshipSourcePath` and finds no source drift on the same delta. The DRIFT-007
fix landed in one of the two places that asks whether a review still covers the tree.

**Consequence if unfixed**: every Specrew re-deploy permanently stales every review in a downstream
project, and the advisory re-fires at every stop — the pattern the gate's own comments name as
training people to stop reading blocks.

- **Class closure**: NONE — the fix (mark the Spec-Kit/Squad-deployed mirrors at deploy time, or give
  the evidence gate the same shared source classifier the validator now uses) belongs in the Specrew
  repository. Handed to the maintainer at this boundary.

**Status**: open upstream. Watch-and-report only by the maintainer's instruction; no review round was
run on account of it.

<!--
  Every new ### DRIFT-... event includes:

  - **Class closure**: the executable mechanism that makes the next instance impossible or loud

  If the change fixes only this instance, write NONE — <why class closure is not in scope>.
  A bare Resolution: FIXED is not class closure.
-->

### Resolution Strategies (Unused)

The following resolution strategies remain available if drift is detected later in execution:

- **spec-updated**: Update the spec to reflect implementation choice
- **implementation-reverted**: Revert implementation to match spec
- **deferred**: Mark drift as deferred to next iteration
- **human-decision**: Escalate to Alon for resolution

### Notes

- This artifact was scaffolded before review starts so drift can be logged immediately when detected.
- Replace the zero-drift summary with real counts when the first drift event is recorded.
