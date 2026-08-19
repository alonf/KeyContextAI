# Iteration Plan: 001 (Stub)

**Schema**: v1
**Spec**: [../../spec.md](../../spec.md)
**Status**: reviewing
**Capacity**: 19.5/20 story_points
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
| T001 | Solution skeleton, three projects, .NET 10 posture | enabling: all FR | US1 | 1 | Implementer | `src/**`, `KeyContextAI.sln` | done | — | — | — |
| T002 | Test projects and corpus folder | enabling: SC-001, SC-006, SC-007 | US1 | 0.5 | Implementer | `tests/**` | done | — | — | — |
| T003 | Directory.Build.props with analyzers | enabling: FR-014 | US1 | 0.5 | Implementer | `Directory.Build.props` | done | — | — | — |
| T004 | GitHub Actions PR workflow | enabling: SC-001, SC-010 | US1 | 1 | Implementer | `.github/workflows/**` | done | — | — | — |
| T005 | All component interfaces | enabling: all FR | US1 | 1 | Implementer | `src/KeyContextAI.Core/Contracts/**` | done | — | — | — |
| T006 | Domain records per data model | enabling: FR-002, FR-005, FR-010 | US1 | 1 | Implementer | `src/KeyContextAI.Core/Model/**` | done | — | — | — |
| T007 | Architecture test for IDesign call rules | enabling: plan structure | US1 | 1 | Implementer | `tests/KeyContextAI.Architecture.Tests/**` | done | — | — | — |
| T008 | IoC composition root | enabling: all FR | US1 | 0.5 | Implementer | `src/KeyContextAI.App/Composition/**` | done | — | — | — |
| T009 | Key-map format and en-US↔he-IL map | FR-008, SC-011 | US1 | 1 | Implementer | `data/keymaps/**` | done | — | — | — |
| T010 | Dictionary packs and golden corpus | FR-008a, SC-001 | US1 | 3 | Implementer | `data/dictionaries/**`, `tests/corpus/**` | done | — | — | — |
| T011 | MappingEngine tests | FR-005 | US1 | 0.5 | Implementer | `tests/KeyContextAI.Core.Tests/MappingEngineTests.cs` | done | — | — | — |
| T012 | DetectionEngine tests | FR-005a, FR-006, FR-009, SC-012 | US1 | 1 | Implementer | `tests/KeyContextAI.Core.Tests/DetectionEngineTests.cs` | done | — | — | — |
| T013 | WordAssemblyEngine tests | FR-005b | US1 | 0.5 | Implementer | `tests/KeyContextAI.Core.Tests/WordAssemblyEngineTests.cs` | done | — | — | — |
| T014 | MappingEngine | FR-005, FR-008, SC-011 | US1 | 1 | Implementer | `src/KeyContextAI.Core/Engines/MappingEngine.cs` | done | — | — | — |
| T015 | DetectionEngine | FR-005a, FR-006, SC-012 | US1 | 2 | Implementer | `src/KeyContextAI.Core/Engines/DetectionEngine.cs` | done | — | — | — |
| T016 | WordAssemblyEngine | FR-005b | US1 | 1 | Implementer | `src/KeyContextAI.Core/Engines/WordAssemblyEngine.cs` | done | — | — | — |
| T020 | DictionaryAccessor | FR-008, FR-008a, FR-029 | US1 | 1.5 | Implementer | `src/KeyContextAI.Platform/Storage/DictionaryAccessor.cs` | done | — | — | — |
| T025 | Corpus accuracy test | SC-001 | US1 | 1.5 | Implementer | `tests/KeyContextAI.Platform.Tests/CorpusAccuracyTests.cs` | done | — | — | — |

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

Iteration 001 only. Later iterations carry their own phase baselines.

| Phase | Estimated Effort | Notes |
| ----- | ---------------- | ----- |
| Setup (T001-T004) | 3 | Solution, test projects, build props, CI lane |
| Foundational (T005-T010) | 7.5 | Interfaces, records, architecture test, composition, key map, dictionary corpus |
| Detection algorithm (T011-T016) | 6 | The three engines with their tests |
| Dictionary loading (T020) | 1.5 | Needed to load real packs for the corpus measurement |
| Accuracy evidence (T025) | 1.5 | The corpus-driven false-correction measurement — this iteration's deliverable |
| **Total planned for 001** | **19.5** | **Against a capacity of 20 story points** |
| Deferred to 002 | 17.5 | Live single-word correction plus the privacy lifecycle |
| Deferred to 003 | 19 | Multi-word, Option B committing-key path, resilience, diagnostics |
| Deferred to 004 | 14.5 | Tray, settings, flip hotkey, learning, quickstart evidence |

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

### Overcommit resolved by splitting, not by raising capacity (human decision, 2026-08-19)

The 48 tasks sum to **70.5 story points** against a configured capacity of **20**. The human's decision
at the tasks boundary: **keep 20 story points per iteration**. What calibration produces after an
iteration completes is the **actual velocity in days per story point**, which turns the fixed 20-point
capacity into an increasingly accurate time prediction. The capacity is therefore not inflated to fit
the work; the work is split across four iterations of at most 20 points each.

| Iteration | Content | Tasks | SP |
| --- | --- | --- | --- |
| 001 | Setup, foundational, and the detection algorithm proven against the corpus | T001–T016, T020, T025 | 19.5 |
| 002 | Live single-word correction plus the privacy lifecycle | T017–T019, T021–T024, T033–T037 | 17.5 |
| 003 | Multi-word runs, the Option B committing-key path, resilience and diagnostics | T026–T032, T045–T047 | 19 |
| 004 | Tray, settings, flip hotkey, learning, and the quickstart evidence run | T038–T044, T048 | 14.5 |

**Why this order.** Iteration 001 delivers no user-visible behavior, which is normally the wrong shape
for an increment. It is right here because it produces the single number the product rests on: a
measured false-correction rate against a real corpus, evidencing SC-001 before a hook, an overlay, or a
settings window is built on top of an unproven detector. Iteration 002 pulls the privacy lifecycle
forward from its original US-priority position because the password gate is a precondition for
dogfooding a keystroke-reading tool at all, not a feature of it.

**Iteration 001 scope is therefore T001–T016, T020 and T025 — 19.5 story points against a capacity of
20.** Only those 18 tasks appear in this iteration's task table, so the capacity line reflects this
iteration rather than the feature. The other 30 tasks keep their iteration assignment in
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/tasks.md and move into their own iteration
plans when those iterations open — deferred rather than dropped.
