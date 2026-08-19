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
| T001 | Solution skeleton, three projects, .NET 10 posture | enabling: all FR | — | 1 | Implementer | `src/**`, `KeyContextAI.sln` | planned | — | — | — |
| T002 | Test projects and corpus folder | enabling: SC-001, SC-006, SC-007 | — | 0.5 | Implementer | `tests/**` | planned | — | — | — |
| T003 | Directory.Build.props with analyzers | enabling: FR-014 | — | 0.5 | Implementer | `Directory.Build.props` | planned | — | — | — |
| T004 | GitHub Actions PR workflow | enabling: SC-001, SC-010 | — | 1 | Implementer | `.github/workflows/**` | planned | — | — | — |
| T005 | All component interfaces | enabling: all FR | — | 1 | Implementer | `src/KeyContextAI.Core/Contracts/**` | planned | — | — | — |
| T006 | Domain records per data model | enabling: FR-002, FR-005, FR-010 | — | 1 | Implementer | `src/KeyContextAI.Core/Model/**` | planned | — | — | — |
| T007 | Architecture test for IDesign call rules | enabling: plan structure | — | 1 | Implementer | `tests/KeyContextAI.Architecture.Tests/**` | planned | — | — | — |
| T008 | IoC composition root | enabling: all FR | — | 0.5 | Implementer | `src/KeyContextAI.App/Composition/**` | planned | — | — | — |
| T009 | Key-map format and en-US↔he-IL map | FR-008, SC-011 | — | 1 | Implementer | `data/keymaps/**` | planned | — | — | — |
| T010 | Dictionary packs and golden corpus | FR-008a, SC-001 | — | 3 | Implementer | `data/dictionaries/**`, `tests/corpus/**` | planned | — | — | — |
| T011 | MappingEngine tests | FR-005 | US1 | 0.5 | Implementer | `tests/KeyContextAI.Core.Tests/MappingEngineTests.cs` | planned | — | — | — |
| T012 | DetectionEngine tests | FR-005a, FR-006, FR-009, SC-012 | US1 | 1 | Implementer | `tests/KeyContextAI.Core.Tests/DetectionEngineTests.cs` | planned | — | — | — |
| T013 | WordAssemblyEngine tests | FR-005b | US1 | 0.5 | Implementer | `tests/KeyContextAI.Core.Tests/WordAssemblyEngineTests.cs` | planned | — | — | — |
| T014 | MappingEngine | FR-005, FR-008, SC-011 | US1 | 1 | Implementer | `src/KeyContextAI.Core/Engines/MappingEngine.cs` | planned | — | — | — |
| T015 | DetectionEngine | FR-005a, FR-006, SC-012 | US1 | 2 | Implementer | `src/KeyContextAI.Core/Engines/DetectionEngine.cs` | planned | — | — | — |
| T016 | WordAssemblyEngine | FR-005b | US1 | 1 | Implementer | `src/KeyContextAI.Core/Engines/WordAssemblyEngine.cs` | planned | — | — | — |
| T017 | KeystrokeAccessor hook | FR-001, FR-013 | US1 | 2 | Implementer | `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs` | planned | — | — | — |
| T018 | InputInjectionAccessor | FR-010, FR-013, FR-015 | US1 | 1.5 | Implementer | `src/KeyContextAI.Platform/Input/InputInjectionAccessor.cs` | planned | — | — | — |
| T019 | LayoutAccessor | FR-005a, FR-010 | US1 | 1 | Implementer | `src/KeyContextAI.Platform/System/LayoutAccessor.cs` | planned | — | — | — |
| T020 | DictionaryAccessor | FR-008, FR-008a, FR-029 | US1 | 1.5 | Implementer | `src/KeyContextAI.Platform/Storage/DictionaryAccessor.cs` | planned | — | — | — |
| T021 | CorrectionManager single-word flow | FR-010, FR-022 | US1 | 2 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T022 | AudioAccessor with three cues | FR-022, FR-023 | US1 | 0.5 | Implementer | `src/KeyContextAI.Platform/System/AudioAccessor.cs` | planned | — | — | — |
| T023 | OverlayClient click-through bubble | FR-022, FR-024 | US1 | 2.5 | Implementer | `src/KeyContextAI.App/Clients/OverlayClient.cs` | planned | — | — | — |
| T024 | Single-word correction integration test | FR-010, FR-022, SC-003 | US1 | 1.5 | Implementer | `tests/KeyContextAI.Platform.Tests/SingleWordCorrectionTests.cs` | planned | — | — | — |
| T025 | Corpus accuracy test | SC-001 | US1 | 1.5 | Implementer | `tests/KeyContextAI.Core.Tests/CorpusAccuracyTests.cs` | planned | — | — | — |
| T026 | TranscriptEngine tests | FR-002, FR-007, FR-011 | US2 | 1.5 | Implementer | `tests/KeyContextAI.Core.Tests/TranscriptEngineTests.cs` | planned | — | — | — |
| T027 | TranscriptEngine | FR-002, FR-007, FR-011 | US2 | 3 | Implementer | `src/KeyContextAI.Core/Engines/TranscriptEngine.cs` | planned | — | — | — |
| T028 | Multi-word span correction | FR-007 | US2 | 1.5 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T029 | Option B armed-flag suppression path | FR-005b | US2 | 2.5 | Implementer | `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs`, `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T030 | Suppressed-key re-injection and compensation | FR-011, FR-014 | US2 | 1.5 | Implementer | `src/KeyContextAI.Platform/Input/InputInjectionAccessor.cs` | planned | — | — | — |
| T031 | Suppressed-key delivery test | FR-012, FR-014, SC-006 | US2 | 2 | Implementer | `tests/KeyContextAI.Platform.Tests/SuppressedKeyDeliveryTests.cs` | planned | — | — | — |
| T032 | Race-safe correction test | FR-011, SC-006 | US2 | 2 | Implementer | `tests/KeyContextAI.Platform.Tests/RaceSafeCorrectionTests.cs` | planned | — | — | — |
| T033 | FocusAccessor with password detection | FR-003 | US5 | 2 | Implementer | `src/KeyContextAI.Platform/System/FocusAccessor.cs` | planned | — | — | — |
| T034 | Privacy lifecycle in CorrectionManager | FR-003, FR-004 | US5 | 1.5 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T035 | Focus-change abandon rule | FR-012 | US5 | 1 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T036 | Fail-closed password gate test | FR-003 | US5 | 1 | Implementer | `tests/KeyContextAI.Platform.Tests/PasswordGateTests.cs` | planned | — | — | — |
| T037 | No-text-persisted filesystem test | FR-004, FR-009b, SC-007 | US5 | 1 | Implementer | `tests/KeyContextAI.Platform.Tests/NoTextPersistedTests.cs` | planned | — | — | — |
| T038 | SettingsAccessor with DPAPI | FR-026, FR-028, FR-029 | US3 | 1.5 | Implementer | `src/KeyContextAI.Platform/Storage/SettingsAccessor.cs` | planned | — | — | — |
| T039 | SettingsManager | FR-006, FR-025 | US3 | 1 | Implementer | `src/KeyContextAI.Core/Managers/SettingsManager.cs` | planned | — | — | — |
| T040 | TrayClient | FR-025, FR-027 | US3 | 2 | Implementer | `src/KeyContextAI.App/Clients/TrayClient.cs` | planned | — | — | — |
| T041 | Flip hotkey | FR-009, FR-015, SC-002 | US3 | 2 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs` | planned | — | — | — |
| T042 | Learning from rejected corrections | FR-009a, FR-009b, FR-028, SC-013 | US3 | 2 | Implementer | `src/KeyContextAI.Core/Managers/CorrectionManager.cs`, `src/KeyContextAI.Platform/Storage/DictionaryAccessor.cs` | planned | — | — | — |
| T043 | Settings window | FR-026 | US3 | 3 | Implementer | `src/KeyContextAI.App/Clients/SettingsWindow.xaml` | planned | — | — | — |
| T044 | Settings and learning integration tests | FR-009a, FR-028, SC-002, SC-013 | US3 | 1.5 | Implementer | `tests/KeyContextAI.Platform.Tests/SettingsAndLearningTests.cs` | planned | — | — | — |
| T045 | Diagnostic log ring buffer | FR-031, FR-032, SC-007 | — | 2 | Implementer | `src/KeyContextAI.Core/Diagnostics/DiagnosticLog.cs` | planned | — | — | — |
| T046 | Hook watchdog | FR-027, FR-030, SC-010 | — | 1.5 | Implementer | `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs` | planned | — | — | — |
| T047 | Latency benchmarks | FR-001, FR-032, SC-003, SC-004 | — | 1.5 | Implementer | `tests/KeyContextAI.Platform.Tests/LatencyBenchmarks.cs` | planned | — | — | — |
| T048 | Quickstart evidence run | SC-001, SC-008, SC-010 | — | 1.5 | Implementer | `specs/001-layout-autocorrect/iterations/001/quality/quality-evidence.md` | planned | — | — | — |

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
| Setup (T001-T004) | 3 | Solution, test projects, build props, CI lane |
| Foundational (T005-T010) | 7.5 | Interfaces, records, architecture test, composition, key map, dictionary corpus |
| US1 single-word correction (T011-T025) | 20 | The correcting core; on its own it already consumes the whole configured capacity |
| US2 multi-word and Option B (T026-T032) | 14 | Transcript engine, suppression path, the two hardest tests |
| US5 privacy (T033-T037) | 6.5 | Focus accessor, privacy lifecycle, the two trust tests |
| US3 control and learning (T038-T044) | 13 | Settings, tray, flip hotkey, learning |
| Polish (T045-T048) | 6.5 | Diagnostic log, watchdog, benchmarks, quickstart evidence |
| **Total planned** | **70.5** | **Against a configured capacity of 20 story points** |

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

### Overcommit: 70.5 planned against a capacity of 20

The 48 tasks sum to **70.5 story points** against a configured capacity of **20** with an overcommit
threshold of 1.0. This is a 3.5x overrun and it is recorded here rather than absorbed. The three
deferral candidates named in tasks.md (the settings window, repeated-use learning, and the diagnostic
log) total only 7 points, so deferral alone does not close the gap — the shape of the iteration or the
capacity itself has to change. This decision belongs to the human and is raised at the tasks boundary.

Two facts worth separating before deciding:

1. **The capacity of 20 is an uncalibrated default** shipped by the Specrew scaffold. No velocity has
   ever been measured for this project, so 20 is a placeholder rather than an observation. The
   configuration enables calibration, which means the first completed iteration is what makes the
   number real.
2. **US1 alone is 20 points** — the correcting core exactly fills the configured capacity before
   multi-word behavior, privacy lifecycle, or any user control is added. That is a genuine signal about
   the iteration's shape, independent of whether the number 20 is right.
