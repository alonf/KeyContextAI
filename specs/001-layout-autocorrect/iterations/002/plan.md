# Iteration Plan: 002

**Schema**: v1
**Spec**: [../../spec.md](../../spec.md)
**Status**: planning
**Capacity**: 17.5/20 story_points
**Started**: 2026-08-22
**Completed**:

<!--
  Validator schema (canonical, enforced by validate-governance.ps1):
  - Iteration Status MUST be one of:
      planning | executing | reviewing | retro | complete | abandoned
    (Common mistakes the validator REJECTS: `approved`, `in-progress`, `done`, `ready`.)
  - Capacity format MUST be `<consumed>/<cap> <effort_unit>` with NO trailing prose on that line.
    Append explanatory notes in the Notes section at the bottom instead.
  - Task Status (in the Tasks table) MUST be one of:
      planned | in-progress | done | needs-rework | deferred | blocked
    (Note `in-progress` uses a hyphen, not an underscore. `done` not `completed`.)
-->

## Scope Summary

| Requirement | Summary | Stories |
| ----------- | ------- | ------- |
| FR-001 | The system MUST observe keystrokes across all applications without perceptibly delaying | — |
| FR-002 | The system MUST maintain a rolling in-memory record of recent typing — the characters, | — |
| FR-003 | The system MUST discard that record when window focus changes, when a password field is | — |
| FR-004 | The system MUST NOT write any typed text to disk or transmit it, except the single | — |
| FR-005 | The system MUST evaluate a completed word by translating the keystrokes to each candidate | — |
| FR-006 | The system MUST apply a correction only when confidence exceeds the threshold implied by | — |
| FR-007 | The system MUST widen detection from a single word to the full run of consecutive | — |
| FR-008 | The system MUST support any language pair through data alone, so adding a pair requires | — |
| FR-009 | The system MUST NOT re-correct a word the user has flipped back during that session. | — |
| FR-010 | The system MUST replace the detected text with the intended text and switch the active | — |
| FR-011 | The system MUST also correct characters typed between detection and replacement, so | — |
| FR-012 | The system MUST abandon a correction, changing nothing, if window focus changed after | — |
| FR-013 | The system MUST NOT treat its own injected keystrokes as user input. | — |
| FR-014 | The system MUST leave text exactly as the user typed it if a correction fails partway. | — |
| FR-015 | Users MUST be able to reverse the most recent correction with a hotkey that does not | — |
| FR-016 | When the dictionary tier is not confident and the user has explicitly enabled an AI | — |
| FR-017 | The system MUST never delay typing on an AI response, MUST stop waiting after a fixed | — |
| FR-018 | The system MUST discard an AI response whose correction is no longer applicable. | — |
| FR-019 | The system MUST support cloud providers, a locally hosted model, and reuse of an | — |
| FR-020 | The system MUST NOT enable any AI provider without an explicit user decision, including | — |
| FR-021 | The system MUST store provider credentials encrypted so that another user account on the | — |
| FR-022 | The system MUST confirm each correction with a sound and a brief on-screen indication near | — |
| FR-023 | The system MUST use distinguishable sounds for a correction applied, a detection in | — |
| FR-024 | The on-screen indication MUST NOT take focus, block interaction, or obstruct the text, and | — |
| FR-025 | Users MUST be able to pause the tool, switch between correcting and notify-only, and | — |
| FR-026 | Users MUST be able to configure language pairs, caution level, AI provider, exclusions, | — |
| FR-027 | The tray indicator MUST show whether the tool is working, degraded, or stopped, with a | — |
| FR-028 | The system MUST keep user-added words separate from shipped dictionary data so updates | — |
| FR-029 | The system MUST record its own data files with a version marker and refuse an unrecognized | — |
| FR-030 | The system MUST restore keystroke observation automatically if it is lost, and inform the | — |
| FR-031 | The system MUST provide an optional local diagnostic record, off by default, that contains | — |
| FR-032 | Writing diagnostic data MUST NOT compete with correction performance. | — |
| FR-033 | The system MUST NOT transmit any usage or diagnostic information unless the user has | — |

## Tasks

| Task | Title | Requirement | Story | Effort | Owner | Owner File Globs | Status | Agent | Actual | Verdict |
| ---- | ----- | ----------- | ----- | ------ | ----- | ---------------- | ------ | ----- | ------ | ------- |
| T033 | FocusAccessor — foreground/control change events, UI Automation password detection (`Yes`/`No`/`Unknown`), caret coordinates | FR-003, FR-012 | US5 | 2.5 | Implementer | `src/KeyContextAI.Platform/System/FocusAccessor.cs` | planned | — | — | — |
| T017 | KeystrokeAccessor — `WH_KEYBOARD_LL` on a dedicated message-pumping thread, allocation-free callback, self-injection tagging | FR-001, FR-013 | US1 | 3 | Implementer | `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs` | planned | — | — | — |
| T018 | InputInjectionAccessor — `SendInput` backspaces plus replacement text as one burst, self-injected event tagging | FR-010, FR-013, FR-015 | US1 | 2 | Implementer | `src/KeyContextAI.Platform/Input/InputInjectionAccessor.cs` | planned | — | — | — |
| T019 | LayoutAccessor — read active layout, enumerate installed layouts, switch | FR-005a, FR-010 | US1 | 1 | Implementer | `src/KeyContextAI.Platform/System/LayoutAccessor.cs` | planned | — | — | — |
| T034 | Privacy lifecycle in CorrectionManager — fail-closed on `Unknown`, transcript wipe on every focus change, wipe on pause and exit | FR-003 | US5 | 1.5 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T035 | Focus-change abandon rule — a correction is never injected into a window that did not produce its keystrokes | FR-012 | US5 | 1 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T021 | CorrectionManager single-word flow — Channel pipeline, engine orchestration, serialized correction executor | FR-010 | US1 | 2.5 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T022 | AudioAccessor with the three distinct feedback cues | FR-022, FR-023 | US1 | 0.5 | Implementer | `src/KeyContextAI.Platform/System/AudioAccessor.cs` | planned | — | — | — |
| T023 | OverlayClient — click-through, caret-anchored, auto-fading bubble with RTL rendering, theme awareness, reduce-motion | FR-022, FR-024 | US1 | 2 | Implementer | `src/KeyContextAI.App/Clients/OverlayClient.cs` | planned | — | — | — |
| T036 | Integration test — fail-closed suspension when password state is `Unknown`, simulating an unresponsive UI Automation provider | FR-003 | US5 | 0.5 | Implementer | `tests/KeyContextAI.Platform.Tests/PasswordGateTests.cs` | planned | — | — | — |
| T037 | Filesystem assertion test — exercise the full pipeline, scan every file the process wrote for typed text | FR-004 | US5 | 0.5 | Implementer | `tests/KeyContextAI.Platform.Tests/NoTextPersistedTests.cs` | planned | — | — | — |
| T024 | End-to-end integration test — inject into a real edit control, assert replacement, layout switch, and feedback | FR-010, FR-022, SC-003 | US1 | 0.5 | Implementer | `tests/KeyContextAI.Platform.Tests/SingleWordCorrectionTests.cs` | planned | — | — | — |

**Sequencing rationale.** The order above is execution order, not id order. T033 leads because the
password gate is a precondition for running a keystroke reader on a real machine at all — the same
reason the privacy lifecycle was pulled forward into this iteration. T017 follows as the input
source, then the two accessors it feeds (T018, T019). The privacy rules (T034, T035) land on
`CorrectionManager` *before* the correction flow (T021) so the fail-closed and abandon paths are
present in the first version of that flow rather than retrofitted onto it. Feedback (T022, T023)
comes after the flow that triggers it, and the three tests close the iteration against real
behaviour.

**Shared-surface note.** T034, T035 and T021 all write
`src/KeyContextAI.Core/Managers/CorrectionManager.cs`. They are sequenced serially and owned by one
role for exactly that reason; no `[P]` parallel marker applies to them even though tasks.md marks
some of their siblings parallel-safe.

## Effort Model

| Setting | Value | Notes |
| ------- | ----- | ----- |
| Effort Unit | story_points | Unit used in task effort, capacity, and retro variance. |
| Capacity per Iteration | 20 | Maximum planned effort before overcommit guidance applies. |
| Iteration Bounding | scope | `scope` keeps requirements fixed; `time` enforces a time ceiling. |
| Time Limit (hours) | n/a | Only applies when iteration bounding is `time`. |
| Overcommit Threshold | 1.0 | Warn planners when total estimated effort exceeds 20 story_points (capacity 20 x threshold 1.0). |
| Defer Strategy | manual | How planning should choose deferrals when the iteration is over capacity. |
| Calibration Enabled | true | When true, retrospectives should suggest future capacity adjustments. |

## Concurrency Rationale

- Current roster snapshot: Spec Steward, Planner, Implementer, Reviewer, Retro Facilitator
- Technology and scope signals: No single specialty dominates yet; treat the slice as general product work until task decomposition adds sharper evidence.
- Task dependency graph: detailed dependencies are still pending task decomposition in this stub; revisit once the task table is populated.
- Workstream separability: Current scope does not yet prove enough safe parallelism for same-specialty expansion; default to a smaller serial team until tasks are clearer.
- Shared-surface conflict risk: no elevated shared-surface warning inferred yet.
- Prior reviewer ownership/hotspot evidence: Latest reviewer hotspots: .specify/extensions/specrew-speckit/.specrew-extension-runtime.json (851 changed lines); .specify/extensions/specrew-speckit/scripts/shared-governance.ps1 (456 changed lines); .specify/extensions/specrew-speckit/scripts/validate-governance.ps1 (348 changed lines); .specrew/last-start-prompt.md (372 changed lines); data/dictionaries/en-US/words.txt (370079 changed lines); data/dictionaries/he-IL/words.txt (22250 changed lines); specs/001-layout-autocorrect/iterations/001/code-map.md (297 changed lines); specs/001-layout-autocorrect/iterations/001/design-analysis.md (436 changed lines); specs/001-layout-autocorrect/iterations/001/drift-log.md (333 changed lines); specs/001-layout-autocorrect/iterations/001/review.md (306 changed lines); specs/001-layout-autocorrect/tasks.md (270 changed lines); tests/KeyContextAI.Core.Tests/DetectionEngineTests.cs (255 changed lines)
- Recommendation: do not propose Junior/Senior same-specialty expansion until the task table and ownership boundaries make safe parallelism explicit. If a same-specialty pair is approved later, record `Owner File Globs` for the parallel tasks or keep the work serial.

## Phase Baseline

Retro improvement action 1 asked for per-task actuals so variance becomes computable. Iteration 001
recorded none, so this baseline is the first that can be compared against anything. Record an
`Actual` in the task table as each task completes — an empty Actual column at the 002 retro is the
same failure the 001 retro named.

| Phase | Estimated Effort | Notes |
| ----- | ---------------- | ----- |
| Privacy foundation (T033, T034, T035) | 5 | Password gate, transcript wipe, focus-change abandon — the precondition for dogfooding |
| Input path (T017, T018, T019) | 6 | Hook, injection, layout switching — the highest-risk native surface in the feature |
| Correction flow (T021) | 2.5 | The Channel pipeline and serialized executor that joins engines to accessors |
| Feedback (T022, T023) | 2.5 | Sound cues and the caret-anchored overlay |
| Acceptance evidence (T024, T036, T037) | 1.5 | The three integration tests that prove the iteration on real input |
| **Total planned for 002** | **17.5** | **Against a capacity of 20 story points** |
| Review (unbudgeted in 001) | 2 | First iteration to carry a review estimate at all — the 001 retro named its absence as a planning gap |
| Rework buffer | 1 | Not in the 17.5; the native input path is where a needs-rework verdict is most likely |

**Why review and rework sit outside the 17.5.** The capacity line counts delivery tasks, matching
iteration 001 so the two are comparable. The review and rework rows are the estimate the 001 retro
said no planning model carried; they are recorded here to be measured against, not to be added to
the committed scope. If measured review effort again dwarfs its estimate, that is the signal to move
it inside capacity for 003.

## Traceability Summary

The Scope Summary table above is auto-generated from the full specification and lists every FR in the
feature. This section is authoritative for what iteration 002 actually covers.

- **Requirement scope for iteration 002**: FR-001, FR-003, FR-004, FR-005a, FR-010, FR-012,
  FR-013, FR-015 (partial — injection side only; the flip hotkey itself is T041 in 004), FR-022,
  FR-023, FR-024, and SC-003 (evidenced by T024).
- **User stories represented**: User Story 1 (single-word correction) and User Story 5 (privacy).
- **Deferred and unchanged**: FR-002, FR-007, FR-011, FR-014 and SC-006 belong to the multi-word and
  resilience work in iteration 003 (T026–T032, T045–T047). FR-025 through FR-032 belong to iteration
  004's experience slice. The AI tier (FR-016–FR-021) and telemetry consent (FR-033) remain deferred
  per the human-approved slicing at the design-analysis stop.
- **Capacity status**: ok. 17.5 planned against a capacity of 20 at overcommit threshold 1.0. No
  deferral decision is required at this boundary; the split was already made at the tasks boundary
  and this iteration executes it unchanged.
- **Traceability**: every task above cites at least one FR or SC, and every FR claimed in scope has at
  least one covering task. Run specrew-traceability-check before the before-implement gate.
- **SC-001a/SC-001b split**: SC-001a is already evidenced by completed T025 and is re-run whenever dictionary
  data changes. SC-001b is a pre-release sustained-daily-use gate assigned to T048 in iteration 004;
  neither criterion adds work to this 17.5-point iteration-002 slice.

## Notes

- The approved design remains Option B (speculative pre-decision, then suppress and re-inject) from
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/design-analysis.md, decision
  commit 6e2ea85. Iteration 002 builds the single-word path Option B assumes; the speculative
  committing-key path itself is T029 in iteration 003.
- Keep Status: planning until the before-implement gate; the hardening gate at
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/002/quality/hardening-gate.md now
  records concrete controls with `Overall Verdict: ready`, but implementation
  remains blocked until a recorded human verdict authorizes the boundary.
- Before-implement preparation evidence is tracked in boundary commits `9e3a396`
  (hardening controls filled) and the current plan refresh commit; implementation
  remains blocked pending explicit human verdict `approved for before-implement`.
- Preflight note: this plan and the hardening gate were refreshed together in the
  latest boundary prep commit to satisfy owed-artifact validation for before-implement.
- Effort estimates carry no measured history: iteration 001 recorded scope but not per-task actuals,
  so these numbers rest on the task descriptions and the 001 total, not on calibration. That is the
  gap retro action 1 exists to close, and 002 is the iteration that can close it.

### Iteration 002 opening state — carried findings

- **DRIFT-007 is resolved upstream** (Specrew b5c84f48, deployed here 2026-08-22). Recorded in
  iteration 001's closed drift log at
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/drift-log.md as a status-only
  note; the closed record itself stands as it happened.
- **Open finding carried into this iteration's drift log**: the co-review navigator's currency check
  still stales a review run on a records-only delta, because its records-only classifier recognises
  only `.specrew-managed`-marked directories and the Spec-Kit/Squad host mirrors
  (`.github/agents/`, `.github/prompts/`, `.claude/skills/speckit-*/`) are deployed without that
  marker. The validator's W38 now asks the source-aware question; the navigator's gate does not. Owned
  upstream; no review round was run on account of it.

### SC-001a/SC-001b resolution — human-approved at the plan → tasks boundary

**The finding.** The pre-split criterion asked for fewer than 1 in 1,000 applied corrections changing already-correct
text, *measured over sustained daily use*. Iteration 001 delivered T025, the corpus accuracy test, and
recorded the result honestly as **precondition met, criterion not yet evidenced** (review finding
F-01): zero false corrections across the must-not-correct cases, but a corpus of that size cannot
evidence a rate of 1-in-1,000 — and a corpus cannot evidence "sustained daily use" at any size.

**The two paths the retro named**, and what each costs:

1. **Grow the corpus until the rate is measurable.** To evidence 1-in-1,000 with any confidence needs
   on the order of 10,000+ realistic correction opportunities per language. That is a data-acquisition
   project, it competes directly with the 17.5 points above, and it still would not satisfy "sustained
   daily use" — a corpus is not use.
2. **Restate the criterion to match what this project intends to measure.** The spec's own assumptions
   section already says the false-correction and reversal targets are "design targets to be validated
   by the maintainer's daily use before release, not measurements of an existing system." Its wording
   has been out of step with that assumption since it was written.

**Approved outcome: option 2, restated in two parts** — a corpus-measurable gate that iteration
001's evidence already speaks to, plus a dogfooding criterion that says plainly what daily use must
show and when. Concretely: SC-001a, zero false corrections across the golden must-not-correct corpus,
re-measured whenever dictionary data changes (already evidenced by T025); and SC-001b, fewer than 1
in 1,000 applied corrections reversed as wrong across the maintainer's sustained daily use, validated
before release rather than in any iteration. That records what iteration 001 actually proved, keeps
the product's real bar, and stops the spec asking for a number no iteration was ever going to produce.

**Human authorization.** At the plan → tasks boundary the project owner approved the recommendation,
the privacy-first sequence, and the existing four-iteration slicing. The feature spec now records
SC-001a and SC-001b, and the task artifacts map T025 to SC-001a and the pre-release evidence task T048
to SC-001b without changing iteration 002's 12 tasks or 17.5/20 SP capacity.

**Before-implement evidence anchor.** This iteration plan is intentionally re-saved together with
the hardening gate to satisfy the before-implement owed-artifact preflight in this resume session.
