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

**Total drift events**: 2 (0 in this project, 2 Specrew-side findings)
**Resolution rate**: no in-project drift detected yet (0/0); both tooling findings are open upstream
**Specification drift**: None detected

## Events

### DRIFT-009: the disclosure seal hid the diagnosis from the only party who could act on it

**Detected**: 2026-08-27, while attempting an approved independent review round of T034/T035.
**Class**: defect (diagnosability) in Specrew's continuous-co-review verification preflight, surfaced
by this project. Not spec, plan, or implementation drift in KeyContextAI.
**Requirement**: none. No FR or SC of this feature is affected.

**What fired**: two approved review rounds failed preflight with the reason token
`verification-command-failed:specrew-governance:diagnostics-require-command-scoped-disclosure`. The
reviewer was never invoked, `examined_paths` was empty, and `validation` was `not-produced`. The
persisted evidence at
file:///C:/Dev/KeyContextAI/.specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i002/runs/run-20260827-112631081-4bf1cf0b/result.json
and
file:///C:/Dev/KeyContextAI/.specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i002/runs/run-20260827-113002433-3690acb8/result.json
carried the token and nothing else.

**The actual cause**: a one-line canonical-value error in a file this session had written. The agent
hand-edited
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/002/state.md while recording T019
and T034/T035 progress and set `Current Phase` to `implement`, which is not in the canonical set
(`specify`, `clarify`, `plan`, `tasks`, `before-implement`, `review-signoff`, `retro`,
`iteration-closeout`, `feature-closeout`). `Test-SessionStateBoundaryCanonical` failed on it. The
governance command is one command in the verification plan, so its red result halted the preflight.

**Why this is the interesting half**: the seal behaved correctly — it is a security property, and
`diagnostics-require-command-scoped-disclosure` names the bounded-disclosure door rather than printing
command output. But the derived-diagnosis layer that rides beside the seal did not name the failing
check or its message. The agent read a correct security message as a machinery fault, misdiagnosed it
as a Specrew defect, paused implementation, and escalated to the human twice — for an error the agent
itself had introduced and could have fixed in one line. The failure was self-inflicted and
self-repairable, and the sealed message routed it away from the only party who could act on it. The
human resolved it in one step by running the validator directly, which is the diagnostic path the
agent had available and did not take.

**Consequence if unfixed**: any red configured verification command presents to an agent as an opaque
machinery failure. The expected behaviour under that ambiguity is exactly what happened here — halt
and escalate — so the cost is paid in stalled implementation and human interrupts, on failures that
are frequently the agent's own trivially-fixable record errors.

- **Class closure**: NONE — the fix belongs in the Specrew repository: have
  `Get-ContinuousCoReviewVerificationFailureDiagnosis` carry the failing check name and its assertion
  message (facts the controller already owns, not command output) beside the unchanged reason token,
  so the seal stays intact and the diagnosis reaches the agent. Handed to the maintainer.

**Secondary finding (same run, recorded for the maintainer)**: the canonical repair path could not be
used. `sync-boundary-state.ps1` halts on its markdownlint gate over
file:///C:/Dev/KeyContextAI/.specrew/handover/session-handover.md, which a PostToolUse hook rewrites
after every tool call — so each commit of the gate's own auto-fix produces a fresh dirty copy and the
gate cannot converge from inside a live session. The phase value was therefore repaired by hand, with
the human's explicit prior authorization, and the validator re-run to confirm iteration 002 PASS.

**Status**: open upstream. No review round was spent on either failed run.

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
