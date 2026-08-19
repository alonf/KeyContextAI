# Iteration Plan: 001 (Stub)

**Schema**: v1
**Spec**: [../../spec.md](../../spec.md)
**Status**: planning
**Capacity**: 0/20 story_points
**Started**: 2026-08-19
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
- Prior reviewer ownership/hotspot evidence: No prior reviewer hotspot signals were found for this feature.
- Recommendation: do not propose Junior/Senior same-specialty expansion until the task table and ownership boundaries make safe parallelism explicit. If a same-specialty pair is approved later, record `Owner File Globs` for the parallel tasks or keep the work serial.

## Phase Baseline

| Phase | Estimated Effort | Notes |
| ----- | ---------------- | ----- |
| Planning | TBD | Populate after task decomposition and approval gating |
| Discovery/Spikes | TBD | Capture any required risk-reduction work revealed during planning |
| Implementation | TBD | Sum planned delivery tasks once the task table is complete |
| Review | TBD | Estimate review/demo effort after verdict flow is defined |
| Rework | TBD | Expected needs-work buffer if review finds gaps |

## Traceability Summary

- Requirement scope for iteration 001: FR-001, FR-002, FR-003, FR-004, FR-005, FR-005a, FR-005b, FR-006, FR-007, FR-008, FR-008a, FR-009, FR-009a, FR-009b, FR-010, FR-011, FR-012, FR-013, FR-014, FR-015, FR-022, FR-023, FR-024, FR-025, FR-026, FR-027, FR-028, FR-029, FR-030, FR-031, FR-032
- Deferred to iteration 002 by the human-approved slicing at the design-analysis stop: FR-016, FR-017, FR-018, FR-019, FR-020, FR-021 (the AI tier) and FR-033 (telemetry consent). They remain MVP scope for the feature; they are not in this iteration.
- User stories represented in current scope: User Story 1 (single-word correction), User Story 2 (multi-word run with typing continuing), User Story 3 (user control), User Story 5 (privacy). User Story 4 (AI-assisted ambiguous cases) is deferred with the AI tier.
- Pending detailed planning: populate the task table, then run specrew-capacity-planning and specrew-traceability-check before approval.
- Overcommit guardrail: compare planned task effort against the configured threshold and record any required deferrals from the lowest-priority requirement slices before leaving planning.

## Notes

- This stub captures the planned scope pending detailed planning in the Specrew Planning ceremony.
- The Scope Summary table above was auto-generated from the full specification and therefore lists the AI-tier and telemetry requirements too. The Traceability Summary is authoritative for what iteration 001 actually covers; see the design-analysis Co-Design Record for the agreed 001/002 split.
- The approved design is Option B (speculative pre-decision, then suppress and re-inject) from file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/design-analysis.md, decision commit 6e2ea85.
- Add task rows only for work that is traceable to the scoped requirements above.
- Keep Status: planning until the plan is fully decomposed and approved.
- If task effort exceeds the configured threshold, make the deferral decision explicit in this plan before execution starts and name the lowest-priority requirement slices proposed for deferral.