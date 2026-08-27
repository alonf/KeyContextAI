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

**Total drift events**: 5 (0 in this project, 5 Specrew-side findings)
**Resolution rate**: no in-project drift detected yet (0/0); all five tooling findings are open upstream
**Specification drift**: None detected

## Events

### DRIFT-012: refusals that report a failed comparison name neither side of it

**Detected**: 2026-08-27, iteration 002 — the second instance in one day.
**Class**: defect pattern (diagnostic completeness) across Specrew's refusal surfaces, generalizing
DRIFT-009 and DRIFT-011. Not spec, plan, or implementation drift in KeyContextAI.
**Requirement**: none. No FR or SC of this feature is affected.
**Status**: open upstream.

**The pattern**: two refusals in one day withheld the one fact that would have made them actionable.
`verification-target-digest-mismatch-before-execution` (DRIFT-011) names that a comparison failed and
neither side of it — both movers were two lines of `git status` away. Earlier the same day, the
governance seal reported `diagnostics-require-command-scoped-disclosure` (DRIFT-009) for what turned
out to be a one-line canonical-value error. In each case the machinery owned both compared values and
the delta between them, and the message carried the reason token alone.

**Consequence if unfixed**: every comparison-shaped gate in the toolchain fails opaque-by-default,
and the cost repeats per gate as it did twice today — spent attempts, misdiagnosis, and human
interrupts on failures whose diagnosis the machinery already held.

- **Class closure**: NONE — the rule belongs in the Specrew repository, applied across refusal
  surfaces rather than per message: a refusal reporting a failed comparison must name both sides and
  what changed between them. Handed to the maintainer.

### DRIFT-011: the preflight digest-mismatch failure names neither the paths that moved nor how to see them

**Detected**: 2026-08-27, on review round run-20260827-140159347-64828b6d of iteration 002 and the
two relaunch attempts that followed it.
**Class**: defect (diagnostic completeness) in Specrew's review preflight, surfaced by this project.
Not spec, plan, or implementation drift in KeyContextAI.
**Requirement**: none. No FR or SC of this feature is affected.
**Status**: open upstream.

**What fired**: the round authorized after the allowance reset failed at preflight with
`verification-target-digest-mismatch-before-execution` — the target digest pinned at request time no
longer matched the tree at execution time. The message states that a mismatch exists and stops there:
it names neither the paths whose content moved nor a way to list them.

**The mover, identified by the maintainer**: two uncommitted iteration records inside the reviewed
surface — file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/002/state.md and
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/002/tasks-progress.yml —
rewritten each turn as task-progress updates. `.specrew/` is excluded from the reviewed-state digest;
`specs/` is not, so the per-turn record rewrites moved the pinned target before every execution.

**How the gap compounded**: with no paths in the message, the agent's diagnosis named the host's
turn-end hook writes — plausible, untested, and wrong; the actual mover was visible in `git status`
the whole time. Three attempts were spent (the failed run and two relaunches built on a settle-delay
theory), none of which could have succeeded while the records stayed uncommitted, because each retry
was cheaper than checking. A failure message naming the changed paths would have ended this at the
first attempt instead of the fourth.

**Consequence if unfixed**: any process that rewrites a file under the reviewed surface between
request and execution makes the preflight fail opaquely and repeatedly, and the message gives the
agent nothing to converge on — the cost is paid in spent attempts and human interrupts on what is a
one-line `git status` diagnosis.

- **Class closure**: NONE — the fix belongs in the Specrew repository: have the preflight failure
  carry the paths that differ between the pinned and live reviewed-state digests (facts the
  controller already owns) beside the unchanged reason token. Handed to the maintainer.

**Correction (2026-08-27, recorded after reconciling the killed run)**: the clause above claiming
none of the three attempts could have succeeded is false for the third. By its run
(run-20260827-141057773-43f224b7, requested 14:10:57Z) the records' content had stabilized at digest
`29720be4` — the same content later committed in `bb7cec1` — so it passed target verification,
invoked the codex reviewer under containment and spent the round. It died because the maintainer's
"stop retrying" instruction was executed against it. Both halves of that error belong in the record:
the executor killed the background task without reading its interim output first, and the
instruction was underspecified — "stop retrying" is safe when every attempt is doomed and
destructive when one is not, and it named no condition to check before killing. A correction that
assigns the whole error to the executor is as inaccurate as one that assigns none. The killed run
was reconciled to terminal (verdict=incomplete, completion=none, runtime_outcome=abandoned) rather
than reset over, so the store carries what happened rather than a topped-up counter over an
unfinished fact.

### DRIFT-010: the demotion rule understates invariant-inversion findings, and the summary line reports the demoted grade as if it were the reviewer's

**Detected**: 2026-08-27, on review round run-20260827-115016739-cf026f20 of iteration 002.
**Class**: defect (finding classification and human-facing summary) in Specrew's review grading layer.
**Severity**: sharp — beta4 item. It inverts the meaning of the one line a human is most likely to read.
**Status**: open upstream.

**What happened.** Codex reviewed the current tree and reported six findings, of which it graded five
as blocking or major. Specrew's demotion rule downgraded all five to minor on the ground that they
stated no concrete failure scenario. The run summary then reported:

- `Nothing found that needs your attention.`
- `This round: 6 findings - none block sign-off, 0 need your acceptance, 6 are notes.`
- `Recommendation: Nothing was reported as blocking.`

The last line is false on its face: five findings *were* reported as blocking or major, by the
reviewer. The summary reads off the post-demotion classification rather than the reviewer's, so the
`Recommendation` line contradicts the demotion notice printed four lines above it.

**Why the rule understates, structurally.** The demotion rule requires a concrete failure scenario.
The most serious defects in this class are invariants that invert under a condition — "if the
automation provider stalls, the consumer holds a stale `PasswordState.No` while the hook keeps
feeding a password field." That is a *conditional* statement, which is exactly the shape the rule
discounts, and it is exactly the shape a security invariant failure takes. The rule therefore
systematically demotes the finding class that most warrants escalation.

**Evidence of a pattern, not an incident.** This is the third consecutive understatement, all on
invariant-inversion findings. The maintainer confirmed finding 1 by reading `FocusAccessor.cs`
directly: `TryBuildContext` called `TryReadFocusedAutomationMetadata` before
`FocusChanged?.Invoke(context)`, so during the probe the manager retained the previous context. It
was a real security defect that the grading layer classified as a note.

**Round 2 repeated it, at full strength.** Round `run-20260827-122146212-0e6d630d` reviewed the
corrected tree and returned seven findings. The reviewer graded *all seven* blocking or major;
Specrew demoted all seven, and the summary again opened with `That round found nothing that needs
your attention.` The ratio moved from 5-of-6 to 7-of-7, so the rule is not merely noisy at the
margin — on this finding class it does not survive a single case.

Round 2's first finding is the sharpest evidence available that the demoted class is the dangerous
one: it was a *regression introduced by the round-1 fix*. Moving the pipeline off the hook thread
resolved the original inversion and recreated it one layer down, because the consumer sampled the
then-current foreground window while the focus context arrived on an unrelated WinEvent stream. A
grading layer that demotes invariant inversions cannot catch a fix that relocates one, since the
relocated defect is described in exactly the conditional form the rule discounts.

**Impact.** A human who trusts the summary is told the opposite of what the review found. Here the
demoted findings included a password-capture window, a hook-timeout risk that can silently remove the
keyboard hook, a suppression path that swallows ordinary keystrokes, and a data-integrity defect that
can leave the user's document partially deleted. The failure was caught only because the agent did
not trust the summary and re-read the findings; an agent that honoured the "nothing that needs your
attention" line would have carried all four into dependent work.

- **Class closure**: the summary line must report the reviewer's own grades alongside any demoted
  grade, so `Recommendation: Nothing was reported as blocking` cannot be emitted when the reviewer
  reported blocking findings. Separately, the demotion rule needs a conditional-invariant exemption:
  a finding that names an invariant, the condition under which it inverts, and the code path that
  permits the condition is a concrete failure scenario, whether or not it narrates an incident.

**Resolution in this project**: round 1's four substantive findings were fixed under the maintainer's
instruction to treat the demotions as majors, and round 2's seven were fixed on the same standing
ruling. In this project the demoted grade is not used; the reviewer's own grade governs. The finding
itself is handed upstream.

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
