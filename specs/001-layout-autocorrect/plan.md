# Implementation Plan: KeyContext AI — Keyboard Layout Auto-Correction

**Branch**: `001-layout-autocorrect` | **Date**: 2026-08-19 | **Spec**: file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/spec.md

**Input**: Feature specification from file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/spec.md

**Design decision**: Option B — speculative pre-decision, then suppress and re-inject — approved by the
human at the design-analysis stop, recorded in
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/design-analysis.md (decision
commit `6e2ea85`). This plan realizes that option; it does not re-open it.

## Summary

KeyContext AI observes keystrokes system-wide, decides whether a word was typed on the wrong keyboard
layout, and replaces it in place while switching the layout — conservatively, because a false correction
is worse than a missed one, and privately, because keystrokes never leave memory.

Iteration 001 delivers the **correcting core**: capture, transcript, mapping, dictionary-tier detection,
single- and multi-word correction including the trailing remap and the Option B committing-key path,
layout switching, sound and bubble feedback, the tray surface, the flip hotkey, learning from rejected
corrections, the privacy lifecycle, and the local diagnostic log. Iteration 002 adds the AI tier and
release machinery. That split was agreed at the design-analysis stop so the false-correction target can
be measured against real typing before anything is built on top of it.

The technical approach follows the bindings agreed across the ten-lens intake workshop: strict IDesign
decomposition with enforced call rules, a single tray-app process, a managed low-level keyboard hook
behind a swap-ready accessor contract, a `System.Threading.Channels` pipeline feeding a serialized
correction executor, and .NET 10 with WPF.

## Technical Context

**Language/Version**: C# on .NET 10 LTS, `LangVersion latest`, nullable reference types enabled,
warnings-as-errors, file-scoped namespaces, records for pipeline messages and DTOs.

**Primary Dependencies** (iteration 001): `Microsoft.Extensions.DependencyInjection` and `Hosting` 10.x
for composition; WPF (.NET 10 Windows Desktop) for the tray and overlay clients. `Microsoft.Agents.AI`
(MAF 1.0), `Microsoft.Agents.AI.GitHub.Copilot`, and Polly 8.x are recorded in the dependency policy but
enter in iteration 002 with the AI tier.

**Storage**: Plain files under `%LOCALAPPDATA%\KeyContextAI\` — key maps and dictionaries loaded into an
in-memory trie at startup, settings as JSON with credentials as a DPAPI-encrypted blob. SQLite was
deliberately rejected. No keystroke is ever persisted.

**Testing**: xUnit for unit and integration tests. Test-first for the pure engines (`MappingEngine`,
`DetectionEngine`, `TranscriptEngine`, `WordAssemblyEngine`), which take data in and return results and
therefore need no mocks. Test-after for the Win32 accessors, verified by integration tests and manual
smoke. A mandatory **architecture test** enforces the IDesign call rules in CI.

**Target Platform**: Windows 10 and 11, x64 and arm64, running unelevated in the user session.

**Project Type**: Windows desktop application (tray-resident, single process).

**Performance Goals**: Hook callback under 1 ms p99 and allocation-free; dictionary verdict within 10 ms
of word completion; full dictionary correction transaction within 50 ms p95; idle CPU effectively zero
(event-driven, no polling); working set under 150 MB.

**Constraints**: The `LowLevelHooksTimeout` ceiling (300 ms default) removes any hook whose callback
overruns it — this is the binding constraint behind Option B's design. Typing latency must be
indistinguishable from the tool being off. The process never elevates. Capture fails closed when the
password state of a control cannot be determined.

**Scale/Scope**: A single user's typing on one machine. Dictionaries of a few megabytes per language
held in memory. Iteration 001 covers 33 of the specification's 41 functional requirements.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution at file:///C:/Dev/KeyContextAI/.specify/memory/constitution.md is the
Specrew-managed baseline. This plan's compliance:

| Principle area | Status | Evidence |
| --- | --- | --- |
| Spec authority | Pass | Every plan element traces to an FR or SC; the FR-to-test map below is the join. |
| Human-authorized boundaries | Pass | specify, clarify and design-analysis each carry a recorded human verdict. |
| Evidence over assertion | Pass | Latency and correction-accuracy claims are measured by the diagnostic log and benchmarks, not asserted; see the Quality Evidence section. |
| No silent scope growth | Pass | The 001/002 split is recorded in the design analysis with the human's agreement; deferred requirements are named individually below. |
| Reversibility recorded | Pass | Option B's kill-switch degradation to observe-only is a recorded property, not an aspiration. |

No violations requiring justification. The Complexity Tracking table below is therefore empty.

## Requirement-to-Test Mapping

This table is the plan's spine: review-signoff holds the implementation to it. Requirements marked
**002** are specified but deferred to iteration 002 by the agreed slicing.

| Requirement group | FRs | Component(s) | Test vehicle | SC evidence produced |
| --- | --- | --- | --- | --- |
| Keystroke capture | FR-001, FR-013 | `KeystrokeAccessor`, `InputInjectionAccessor` | Integration test with a synthetic input source; benchmark asserting an allocation-free callback | SC-004 (latency indistinguishable), SC-003 |
| Transcript and privacy lifecycle | FR-002, FR-003, FR-004 | `TranscriptEngine`, `CorrectionManager`, `FocusAccessor` | Unit tests on the engine (no mocks); integration test asserting wipe on focus change and password focus; a filesystem assertion that no typed text is written | SC-007 (nothing persisted) |
| Detection | FR-005, FR-005a, FR-005b, FR-006, FR-007, FR-008, FR-008a, FR-009, FR-009a, FR-009b | `MappingEngine`, `DetectionEngine`, `WordAssemblyEngine`, `DictionaryAccessor` | Unit tests over a curated corpus of true positives, true negatives, and ambiguous cases; a golden-file corpus test for false-correction rate; a licence-provenance test asserting every shipped pack declares source and licence | SC-001, SC-002, SC-011, SC-012, SC-013 |
| Correction transaction | FR-010, FR-011, FR-012, FR-014 | `CorrectionManager`, `TranscriptEngine`, `InputInjectionAccessor`, `LayoutAccessor` | Unit tests on span computation and trailing remap; integration tests injecting into a real edit control, including a mid-correction typing case and a focus-change abandon case | SC-006 (multi-word and race correctness) |
| Committing-key path (Option B) | FR-005b | `KeystrokeAccessor`, `TranscriptEngine`, `CorrectionManager`, `InputInjectionAccessor` | Integration test asserting a suppressed key is always eventually delivered, including on every failure path; a chat-like send target in the manual smoke script | SC-006, SC-003 |
| Flip and learning | FR-009, FR-009a, FR-009b, FR-015 | `CorrectionManager`, `TranscriptEngine`, `DictionaryAccessor` | Unit tests on the never-correct set; an integration test asserting a flipped-back word survives a restart; a negative test asserting nothing but affirmed words is written | SC-002, SC-013, SC-007 |
| Feedback surfaces | FR-022, FR-023, FR-024 | `OverlayClient`, `AudioAccessor`, `CorrectionManager` | Manual visual smoke per the quickstart, including an RTL rendering check and a reduce-motion check | SC-008 (five-minute first success) |
| Tray and settings | FR-025, FR-026, FR-027 | `TrayClient`, `SettingsManager`, `SettingsAccessor` | Integration tests on settings round-trip and exclusion behavior; manual smoke on the tray menu | SC-008, SC-010 |
| Data integrity | FR-028, FR-029 | `DictionaryAccessor`, `SettingsAccessor` | Unit tests for unknown `schema_version` rejection; an update-simulation test asserting user words survive | SC-013 |
| Resilience | FR-030, FR-032 | `KeystrokeAccessor`, `CorrectionManager`, diagnostic log | Integration test forcing hook loss and asserting re-registration; a benchmark asserting the quiet-period flush never runs on the correction path | SC-010 (a full day without intervention) |
| Diagnostics | FR-031 | diagnostic log ring buffer | Unit tests on the ring buffer and flush trigger; a content assertion that standard mode contains no typed text | SC-007 |
| AI tier | FR-016 – FR-021 | `LlmAccessor`, `SettingsManager` | **002** | SC-005 |
| Telemetry consent | FR-033 | `SettingsManager`, `TrayClient` | **002** | — |
| Packaging and signing | — | CI release lane | **002** | SC-009 |

Every FR in iteration 001 has at least one test vehicle, and every SC except SC-005 (AI latency) and
SC-009 (installs without warnings) is evidenced within this iteration. Those two are evidenced in 002,
which is why they are named here rather than left silent.

## Project Structure

### Documentation (this feature)

```text
specs/001-layout-autocorrect/
├── spec.md                  # The specification (approved)
├── plan.md                  # This file
├── data-model.md            # Entities, attributes, validation
├── quickstart.md            # Try the feature in five minutes
├── contracts/
│   └── keycontext-ai.md     # The public surface of each component
├── review-diagrams.md       # Component and sequence diagrams for the reviewer
├── implementation-rules.yml # The code-craft manifest guiding implementation
├── lens-applicability.json  # Workshop provenance
├── workshop/                # The eleven design decision records
└── iterations/001/
    ├── design-analysis.md   # The approved Option B decision
    ├── plan.md              # The iteration execution plan (written before implement)
    ├── state.md
    ├── drift-log.md
    └── quality/
```

### Source Code (repository root)

```text
src/
├── KeyContextAI.App/                 # Composition root, WPF host, tray + overlay clients
│   ├── Clients/                      #   TrayClient, OverlayClient
│   └── Composition/                  #   IoC registration, lifetimes
├── KeyContextAI.Core/                # Managers and engines — no Win32, fully testable
│   ├── Managers/                     #   CorrectionManager, SettingsManager
│   ├── Engines/                      #   WordAssembly, Transcript, Mapping, Detection
│   └── Contracts/                    #   The interfaces every component sits behind
└── KeyContextAI.Platform/            # ResourceAccessors — all Win32 interop lives here
    ├── Input/                        #   KeystrokeAccessor, InputInjectionAccessor
    ├── System/                       #   FocusAccessor, LayoutAccessor, AudioAccessor
    └── Storage/                      #   DictionaryAccessor, SettingsAccessor

data/
├── keymaps/                          # Per-pair scancode maps (en-US↔he-IL first)
└── dictionaries/                     # Per-language word lists with source + licence manifests

tests/
├── KeyContextAI.Core.Tests/          # Unit tests — engines, mock-free by construction
├── KeyContextAI.Platform.Tests/      # Integration tests — real Win32, real edit controls
├── KeyContextAI.Architecture.Tests/  # The IDesign call-rule enforcement test
└── corpus/                           # Golden files for detection accuracy
```

**Structure Decision**: Three projects, split along the boundary that matters most for testability.
`KeyContextAI.Core` contains every manager and engine and references no Win32, so the entire correction
algorithm is unit-testable without a desktop. `KeyContextAI.Platform` holds every accessor and is the
only place P/Invoke appears, which is also what makes the recorded native-hook swap a
single-project change. `KeyContextAI.App` is the composition root and the two WPF clients. The
architecture test asserts the call rules across these boundaries — accessors reference nothing inward,
engines reference no managers, managers reference no other managers.

## Phase 1 — Quality Planning

Resolved from the project quality profile. Iteration 001 is a **hardening-relevant** slice because it
introduces a global input hook, text injection, and a privacy boundary.

| Concern | Why it applies here | Planned evidence | Status |
| --- | --- | --- | --- |
| Security baseline | The tool observes all keystrokes and can withhold and inject them; the password gate and the abandon-on-focus-change rule are security controls, not conveniences. | file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/lenses/security-baseline.md | required |
| Robustness and failure semantics | Option B introduces the only path where a user keystroke can be withheld; every failure must still deliver that key. | file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/hardening-gate.md | required |
| Test integrity | The false-correction target is the product's core claim and cannot be evidenced by smoke tests; it needs a corpus. | file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/quality-evidence.md | required |
| Retry and idempotency | No network calls in iteration 001; correction transactions are single-attempt by design, since retrying an injection into changed text is unsafe. | hardening gate record | not-applicable |

### Lens Activation Plan

| Lens | Activation | Why | Planned evidence |
| --- | --- | --- | --- |
| `security-baseline@v1.0.0` | required | Input capture, injection, and the privacy boundary are the feature's highest-consequence surfaces. | file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/lenses/security-baseline.md |
| `robustness-baseline@v1.0.0` | required | The suppression path's failure semantics decide whether the tool can damage user text. | file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/lenses/robustness-baseline.md |
| `test-integrity@v1.0.0` | required | SC-001 and SC-002 are measurement claims; the corpus and the diagnostic counters are their evidence. | file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/lenses/test-integrity.md |

## Phase 2 — Hardening Targets

These are the concerns the before-implement gate will require to be addressed, deferred with approval,
or marked not-applicable:

1. **A suppressed key is always eventually delivered.** Every failure path in the Option B transaction
   re-injects the committing key. This is the single most important test in iteration 001.
2. **The password gate fails closed.** When the password state of a control cannot be determined,
   capture suspends. Tested by simulating an unresponsive UI Automation provider.
3. **No typed text reaches disk.** Asserted by a test that exercises the full pipeline and then scans
   every file the process wrote.
4. **Self-injection never re-enters the pipeline.** Otherwise a correction could trigger a correction.
5. **The hook survives a day.** Loss is detected and re-registered within 2 seconds; repeated failure
   surfaces to the tray rather than failing silently.
6. **False-correction rate is measured, not claimed.** The corpus test produces a number that can be
   compared against SC-001 before release.

## Explicit Deferrals

Recorded so they are visible rather than silently absent:

- The AI tier (FR-016 – FR-021) and telemetry consent (FR-033) move to iteration 002 with the human's
  agreement at the design-analysis stop. They remain MVP scope.
- Packaging, signing under ZioNet, and the CI release lane move to iteration 002, after the correcting
  core is proven.
- The suppression kill-switch is built in iteration 001 but not surfaced in the settings window, per the
  recorded design-analysis intent.
- Composition-based input languages, a background service component, and dictionary cloud sync remain
  out of the feature entirely.

## Complexity Tracking

> No Constitution Check violations; this table is intentionally empty.
