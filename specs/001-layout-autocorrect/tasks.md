# Tasks: KeyContext AI — Keyboard Layout Auto-Correction (Iteration 001)

**Feature**: 001-layout-autocorrect | **Iteration**: 001 | **Date**: 2026-08-19
**Spec**: file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/spec.md
**Plan**: file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/plan.md
**Design**: Option B (speculative pre-decision, then suppress and re-inject), approved at the
design-analysis stop.

**Scope**: iteration 001 only — the correcting core. The AI tier (FR-016–FR-021) and telemetry consent
(FR-033) are deferred to iteration 002 by the human-approved slicing and have no tasks here.

**Tests**: requested. The code-implementation workshop bound a test-first posture for the pure engines
and test-after for the Win32 accessors, plus a mandatory architecture test. Test tasks are therefore
first-class, not optional.

---

## Phase 1: Setup

- [ ] T001 Create the three-project solution skeleton (`KeyContextAI.Core`, `KeyContextAI.Platform`, `KeyContextAI.App`) targeting .NET 10 with nullable enabled, warnings-as-errors, and file-scoped namespaces in `KeyContextAI.sln` and the three `src/*/[Project].csproj` files
- [ ] T002 [P] Create the four test projects (`KeyContextAI.Core.Tests`, `KeyContextAI.Platform.Tests`, `KeyContextAI.Architecture.Tests`, plus the `tests/corpus/` data folder) wired to xUnit in `tests/`
- [ ] T003 [P] Add `Directory.Build.props` with the shared analyzer, language-version, and warnings-as-errors settings at the repository root
- [ ] T004 [P] Add the GitHub Actions PR workflow running build, tests, the architecture test, markdownlint, and Specrew governance validation in `.github/workflows/ci.yml`

---

## Phase 2: Foundational (blocking prerequisites)

**These block every user story. Nothing below Phase 2 can start until they are done.**

- [ ] T005 Define every component interface with its XML documentation, exactly as specified in the contract, in `src/KeyContextAI.Core/Contracts/` (`IWordAssemblyEngine`, `ITranscriptEngine`, `IMappingEngine`, `IDetectionEngine`, `ICorrectionManager`, `ISettingsManager`, `IKeystrokeAccessor`, `IInputInjectionAccessor`, `IFocusAccessor`, `ILayoutAccessor`, `IDictionaryAccessor`, `IAudioAccessor`, `ISettingsAccessor`)
- [ ] T006 [P] Implement the domain records (`KeyEvent`, `TranscriptEntry`, `CorrectionVerdict`, `CorrectionTransaction`, `Candidate`, `LayoutId`, `DictionarySnapshot`, `Settings`) per the data model in `src/KeyContextAI.Core/Model/`
- [ ] T007 Write the architecture test enforcing the strict IDesign call rules — accessors reference no system component, engines reference no engine or manager, managers reference no manager, engines make no accessor calls — in `tests/KeyContextAI.Architecture.Tests/CallRuleTests.cs`
- [ ] T008 [P] Implement the IoC composition root with singleton lifetimes for every manager, engine, and accessor in `src/KeyContextAI.App/Composition/ServiceRegistration.cs`
- [ ] T009 [P] Define the key-map and dictionary file formats with `schema_version`, source, and licence fields, and author the `en-US↔he-IL` key map in `data/keymaps/en-US_he-IL.json`
- [ ] T010 Assemble the Hebrew and English dictionary corpus from permissively licensed sources, recording source and licence per pack, plus the golden-file test corpus of true positives, true negatives, and ambiguous cases in `data/dictionaries/` and `tests/corpus/`

---

## Phase 3: User Story 1 — A mistyped word fixes itself (P1)

**Goal**: A single wrong-layout word is replaced, the layout switches, and both feedback channels fire.

**Independent test**: Type a known wrong-layout word followed by a space in a plain text field; the
text is replaced with the intended word, the layout switched, a sound played, and a bubble shown.

- [ ] T011 [P] [US1] Write failing unit tests for `MappingEngine` — scan-code translation, unmapped codes, determinism — in `tests/KeyContextAI.Core.Tests/MappingEngineTests.cs`
- [ ] T012 [P] [US1] Write failing unit tests for `DetectionEngine` — candidate scoring, the caution thresholds, the never-correct set, `Ignore` on ambiguity, the two-versus-more-than-two layout rule — in `tests/KeyContextAI.Core.Tests/DetectionEngineTests.cs`
- [ ] T013 [P] [US1] Write failing unit tests for `WordAssemblyEngine` — completion only on space, punctuation, or a committing key, never mid-word — in `tests/KeyContextAI.Core.Tests/WordAssemblyEngineTests.cs`
- [ ] T014 [US1] Implement `MappingEngine` over data-driven key maps in `src/KeyContextAI.Core/Engines/MappingEngine.cs`
- [ ] T015 [US1] Implement `DetectionEngine` with caution-level thresholds and target-layout resolution in `src/KeyContextAI.Core/Engines/DetectionEngine.cs`
- [ ] T016 [US1] Implement `WordAssemblyEngine` with the word-completion rules in `src/KeyContextAI.Core/Engines/WordAssemblyEngine.cs`
- [ ] T017 [US1] Implement `KeystrokeAccessor` — the `WH_KEYBOARD_LL` hook on a dedicated message-pumping thread, allocation-free callback, self-injection tagging — in `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs`
- [ ] T018 [P] [US1] Implement `InputInjectionAccessor` — `SendInput` backspaces plus replacement text as a single burst, self-injected event tagging — in `src/KeyContextAI.Platform/Input/InputInjectionAccessor.cs`
- [ ] T019 [P] [US1] Implement `LayoutAccessor` — read the active layout, enumerate installed layouts, switch — in `src/KeyContextAI.Platform/System/LayoutAccessor.cs`
- [ ] T020 [P] [US1] Implement `DictionaryAccessor` — trie load, query, `schema_version` rejection, licence-manifest validation — in `src/KeyContextAI.Platform/Storage/DictionaryAccessor.cs`
- [ ] T021 [US1] Implement `CorrectionManager`'s single-word flow — the Channel pipeline, engine orchestration, and the serialized correction executor — in `src/KeyContextAI.Core/Managers/CorrectionManager.cs`
- [ ] T022 [P] [US1] Implement `AudioAccessor` with the three distinct feedback cues in `src/KeyContextAI.Platform/System/AudioAccessor.cs`
- [ ] T023 [US1] Implement `OverlayClient` — the click-through, caret-anchored, auto-fading bubble with RTL rendering, theme awareness, and reduce-motion support — in `src/KeyContextAI.App/Clients/OverlayClient.cs`
- [ ] T024 [US1] Write the end-to-end integration test injecting into a real edit control and asserting replacement, layout switch, and feedback in `tests/KeyContextAI.Platform.Tests/SingleWordCorrectionTests.cs`
- [ ] T025 [US1] Write the corpus-driven accuracy test producing a measured false-correction rate against the golden files in `tests/KeyContextAI.Core.Tests/CorpusAccuracyTests.cs`

---

## Phase 4: User Story 2 — A whole mistyped phrase fixes itself while typing continues (P1)

**Goal**: Multi-word runs correct as one action, and characters typed during the correction are
corrected too — including the Option B committing-key path.

**Independent test**: Type three consecutive wrong-layout words, keep typing during the correction, and
verify the whole run plus the newly typed characters end up correct.

- [ ] T026 [P] [US2] Write failing unit tests for `TranscriptEngine` — suspect-span widening, the trailing remap, backspace-count equals rendered span length, epoch marking, the at-most-one-`VerdictReady` invariant — in `tests/KeyContextAI.Core.Tests/TranscriptEngineTests.cs`
- [ ] T027 [US2] Implement `TranscriptEngine` including the `VerdictReady` state and `ComputeTransaction` in `src/KeyContextAI.Core/Engines/TranscriptEngine.cs`
- [ ] T028 [US2] Extend `CorrectionManager` to widen a failing word into the maximal consecutive suspect span and correct it as one transaction in `src/KeyContextAI.Core/Managers/CorrectionManager.cs`
- [ ] T029 [US2] Implement the Option B armed-flag path — speculative per-keystroke evaluation off the hook thread, `Arm`/`Disarm`, and the O(1) suppress decision in the callback — in `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs` and `src/KeyContextAI.Core/Managers/CorrectionManager.cs`
- [ ] T030 [US2] Implement suppressed-key re-injection on every exit path, including the compensating re-inject-alone path on failure or focus change, in `src/KeyContextAI.Platform/Input/InputInjectionAccessor.cs`
- [ ] T031 [US2] Write the integration test proving a suppressed key is always eventually delivered across every failure path — the first Phase 2 hardening target — in `tests/KeyContextAI.Platform.Tests/SuppressedKeyDeliveryTests.cs`
- [ ] T032 [US2] Write the integration test for mid-correction typing and the trailing remap, including a fast-typist timing case, in `tests/KeyContextAI.Platform.Tests/RaceSafeCorrectionTests.cs`

---

## Phase 5: User Story 5 — The user can trust it with everything they type (P1)

**Goal**: The privacy guarantees hold absolutely and are demonstrable.

**Independent test**: Focus a password field and type; verify no capture, correction, or record. Inspect
every file the tool writes and confirm no typed text is present.

- [ ] T033 [US5] Implement `FocusAccessor` — foreground and control change events, UI Automation password detection returning `Yes`/`No`/`Unknown`, caret coordinates — in `src/KeyContextAI.Platform/System/FocusAccessor.cs`
- [ ] T034 [US5] Implement the privacy lifecycle in `CorrectionManager` — fail-closed on `Unknown`, transcript wipe on every focus change, wipe on pause and exit — in `src/KeyContextAI.Core/Managers/CorrectionManager.cs`
- [ ] T035 [US5] Implement the focus-change abandon rule so a correction is never injected into a window that did not produce its keystrokes in `src/KeyContextAI.Core/Managers/CorrectionManager.cs`
- [ ] T036 [P] [US5] Write the integration test asserting fail-closed suspension when the password state is `Unknown`, simulating an unresponsive UI Automation provider, in `tests/KeyContextAI.Platform.Tests/PasswordGateTests.cs`
- [ ] T037 [P] [US5] Write the filesystem assertion test that exercises the full pipeline and scans every file the process wrote for typed text in `tests/KeyContextAI.Platform.Tests/NoTextPersistedTests.cs`

---

## Phase 6: User Story 3 — The user controls when and where it acts (P2)

**Goal**: Pause, notify-only mode, per-application exclusion, the flip hotkey, learning, and settings.

**Independent test**: Exclude the foreground application from the tray, type a wrong-layout word in it,
verify no correction; re-enable and verify correction resumes.

- [ ] T038 [P] [US3] Implement `SettingsAccessor` — JSON persistence, atomic writes, `schema_version` rejection, DPAPI `CurrentUser` credential encryption — in `src/KeyContextAI.Platform/Storage/SettingsAccessor.cs`
- [ ] T039 [US3] Implement `SettingsManager` — validation, change notification, one-click foreground-app exclusion — in `src/KeyContextAI.Core/Managers/SettingsManager.cs`
- [ ] T040 [US3] Implement `TrayClient` — the status dot with hover reason, pause, Correct/Notify-only mode, one-click exclusion, settings, quit — in `src/KeyContextAI.App/Clients/TrayClient.cs`
- [ ] T041 [US3] Implement the flip hotkey — double-tap Ctrl detection with the `Ctrl+Alt+Z` alternative, armed only while the correction is the most recent edit — in `src/KeyContextAI.Core/Managers/CorrectionManager.cs`
- [ ] T042 [US3] Implement learning from rejected corrections — flip-back and repeated-use affirmation writing to the user dictionary, with the affirmed-words-only limit — in `src/KeyContextAI.Core/Managers/CorrectionManager.cs` and `src/KeyContextAI.Platform/Storage/DictionaryAccessor.cs`
- [ ] T043 [US3] Implement the settings window — language pairs, caution level, exclusions, feedback toggles, hotkey, diagnostic mode — in `src/KeyContextAI.App/Clients/SettingsWindow.xaml` and its code-behind
- [ ] T044 [P] [US3] Write integration tests for the settings round-trip, exclusion behavior, and a flipped-back word surviving a restart in `tests/KeyContextAI.Platform.Tests/SettingsAndLearningTests.cs`

---

## Phase 7: Polish and Cross-Cutting Concerns

- [ ] T045 [P] Implement the diagnostic log — in-memory ring buffer with quiet-period flush, standard mode with no typed text, session-scoped self-deleting verbose mode — in `src/KeyContextAI.Core/Diagnostics/DiagnosticLog.cs`
- [ ] T046 [P] Implement the hook watchdog — loss detection, re-registration within 2 seconds, tray escalation after repeated failure — in `src/KeyContextAI.Platform/Input/KeystrokeAccessor.cs`
- [ ] T047 [P] Write the benchmark asserting an allocation-free, sub-millisecond hook callback and that the diagnostic flush never runs on the correction path in `tests/KeyContextAI.Platform.Tests/LatencyBenchmarks.cs`
- [ ] T048 Run the quickstart script end to end on a real machine, including the chat-send case and a slow-application case, and record the evidence in `specs/001-layout-autocorrect/iterations/001/quality/quality-evidence.md`

---

## Dependencies and Execution Order

```text
Phase 1 (Setup)
   ↓
Phase 2 (Foundational — blocks everything)
   ↓
   ├─▶ Phase 3 (US1, P1) ──▶ Phase 4 (US2, P1)   [US2 extends US1's correction path]
   ├─▶ Phase 5 (US5, P1)                          [independent of US1/US2 after Phase 2]
   └─▶ Phase 6 (US3, P2)                          [needs US1's correction path for the flip]
                    ↓
              Phase 7 (Polish)
```

- **US1 → US2**: T028–T030 extend the correction path T021 creates. Serial.
- **US5 is independent** of US1 and US2 once Phase 2 is done, because the privacy lifecycle is a
  manager concern that does not depend on detection working.
- **US3's flip and learning (T041, T042)** need US1's correction path; the tray and settings tasks do
  not and can start earlier.

## Parallel Opportunities

- Phase 1: T002, T003, T004 in parallel after T001.
- Phase 2: T006, T008, T009 in parallel; T010 is the long pole and should start first.
- Phase 3: the three engine test tasks T011–T013 in parallel; then T018, T019, T020, T022 in parallel
  since they are separate accessors in separate files.
- Phase 5: T036 and T037 in parallel.
- Phase 7: T045, T046, T047 in parallel.

## Capacity and Deferral

**Configured capacity**: 20 story points per iteration, overcommit threshold 1.0.

Effort is estimated per task in the iteration plan's task table. The decomposition above is **48 tasks
covering 31 requirements**, which will not fit 20 story points — this is the overrun predicted at the
plan boundary and it is named here rather than absorbed.

**Proposed deferral candidates, in the order I would defer them** (lowest-priority requirement slices
first, per the configured manual defer strategy):

1. **T043 — the settings window** (FR-026). The tray already exposes pause, mode, and exclusion, so the
   window is convenience in iteration 001. Deferring it means editing the JSON settings file by hand,
   which is acceptable for a maintainer dogfooding the tool. **Largest saving, lowest pain.**
2. **T042 — learning from repeated use** (the retyping half of FR-009a). Flip-back learning is the
   unambiguous half and is cheap; the repeated-use inference is the part that needs a conservative
   threshold and its own tests.
3. **T045 — the diagnostic log** (FR-031, FR-032). Valuable as a research instrument but not required
   for the tool to correct text. Deferring it delays the typing-speed analysis.

**What must not be deferred**: T031 (suppressed-key delivery), T036 (fail-closed password gate), T037
(no typed text persisted), and T007 (the architecture test). These are the correctness and trust floor.

The final effort numbers and the deferral decision belong in the iteration plan's task table at
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/plan.md and require the human's
decision before the before-implement gate.

## Requirement Coverage

| Requirement | Covering tasks |
| --- | --- |
| FR-001 | T017, T047 |
| FR-002 | T026, T027 |
| FR-003 | T034, T037 |
| FR-004 | T037, T045 |
| FR-005 | T011, T012, T014, T015 |
| FR-005a | T012, T015, T019 |
| FR-005b | T013, T016, T029 |
| FR-006 | T012, T015, T039 |
| FR-007 | T026, T027, T028 |
| FR-008 | T009, T014, T020 |
| FR-008a | T010, T020 |
| FR-009 | T012, T041 |
| FR-009a | T042, T044 |
| FR-009b | T042, T037 |
| FR-010 | T018, T019, T021, T024 |
| FR-011 | T027, T030, T032 |
| FR-012 | T035, T031 |
| FR-013 | T017, T018 |
| FR-014 | T030, T031 |
| FR-015 | T041, T018 |
| FR-022 | T022, T023, T024 |
| FR-023 | T022 |
| FR-024 | T023 |
| FR-025 | T040, T039 |
| FR-026 | T043, T038 |
| FR-027 | T040, T046 |
| FR-028 | T038, T042, T044 |
| FR-029 | T020, T038 |
| FR-030 | T046 |
| FR-031 | T045 |
| FR-032 | T045, T047 |
| SC-001 | T025, T048 |
| SC-002 | T041, T044 |
| SC-003 | T024, T047 |
| SC-004 | T047 |
| SC-006 | T031, T032 |
| SC-007 | T037, T045 |
| SC-008 | T048 |
| SC-010 | T046, T048 |
| SC-011 | T009, T014 |
| SC-012 | T012, T015 |
| SC-013 | T042, T044 |

Every task traces to at least one requirement, and every iteration-001 requirement and success
criterion has at least one covering task. SC-005 (AI latency) and SC-009 (installs without warnings)
have no tasks here because their requirements are deferred to iteration 002.

## Implementation Strategy

**Minimum viable slice**: Phases 1, 2, and 3 alone produce a tool that corrects single words and
switches layout — usable, demonstrable, and enough to start measuring the false-correction rate.

**Second increment**: Phase 4 adds the multi-word and committing-key behavior, which is what makes the
tool feel reliable rather than occasional.

**Third increment**: Phase 5 makes it safe to leave running all day, which is the precondition for real
dogfooding.

Phases 6 and 7 improve the experience but the tool corrects text without them.
