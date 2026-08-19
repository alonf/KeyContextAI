# Hardening Gate: Iteration 001

**Schema**: v1
**Gate ID**: `pre-implementation-hardening`
**Feature Ref**: `specs/001-layout-autocorrect/spec.md`
**Iteration Ref**: `specs/001-layout-autocorrect/iterations/001`
**Requested Review Class**: `strongest-available`
**Effective Review Class**: `strongest-available`
**Overall Verdict**: `ready`
**Approval Ref**: `tasks-boundary-verdict-2026-08-19`
**Reviewed By**: Crew (planning-time hardening analysis)
**Reviewed At**: 2026-08-19T06:05:00Z

<!--
  Concern Review schema (validator-enforced):
  - Status MUST be one of: `addressed` | `not-applicable` | `deferred-with-approval`. The validator
    rejects placeholder values like `tbd`. Pick a real status per concern before implementation.
  - When Status is `addressed`: EvidenceBasis = `planning-time-analysis`, RuntimeEvidenceStatus =
    `pending-post-implementation`, ExpectedControls = concrete controls you will enforce.
  - When Status is `not-applicable`: EvidenceBasis = `not-applicable`, RuntimeEvidenceStatus =
    `not-needed`, ExpectedControls = `—`. Rationale must explain WHY this concern does not apply.
  - When Status is `deferred-with-approval`: same evidence fields as `addressed`, AND the Approval
    column must reference an approval record (decision or defer) with a recorded human approval.
  - Overall Verdict is computed: `ready` when every concern is addressed/not-applicable/deferred-
    with-approval; `blocked` otherwise. Update the metadata above when you change the table.
-->

## Concern Review

| Concern | Category | Status | Evidence Basis | Runtime Evidence Status | Expected Controls | Blocking | Rationale | Approval |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `security-surface` | `security` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | Dictionary and key-map files are parsed as untrusted input: unknown `schema_version` is rejected outright rather than best-effort parsed; malformed entries are skipped with a recorded reason rather than throwing into the load path; no dynamic code paths, no reflection over file content, no deserialization of arbitrary types. Every shipped pack must declare source and licence or the load fails. No network calls exist in this iteration. Engines take data in and return results — they open no files, so the parsing surface is confined to `DictionaryAccessor`. | `true` | Iteration 001 has a deliberately small trust boundary: no keyboard hook, no text injection, no network, no credentials. The only external input is the dictionary and key-map data on disk, and the only privilege is reading the user's own `%LOCALAPPDATA%`. The high-consequence surfaces the feature is known for — capture, injection, the password gate, credential storage — belong to iterations 002 and later and are gated there, not waved through here. | `—` |
| `error-handling-expectations` | `robustness` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | Failure modes for this iteration and their single handling path: (1) dictionary or key-map file missing → that language pair is unavailable and the reason is surfaced, the application still starts; (2) unknown `schema_version` → the file is refused loudly, other valid packs still load; (3) unmappable scan code → `MappingEngine` returns a candidate marked incomplete rather than throwing; (4) no confident candidate → `DetectionEngine` returns `Ignore`, which is always a valid answer. Negative-path tests are required for each: a missing-file test, an unknown-version test, an unmapped-scan-code test, and ambiguity tests asserting `Ignore`. | `true` | The engines are pure functions over supplied data, so the failure semantics that matter are parse-time and decision-time. The governing rule is that uncertainty produces `Ignore` rather than a guess or an exception — the conservative posture bound at the requirements-nfr lens, where a false correction is worse than a missed one. | `—` |
| `retry-idempotency-requirements` | `resilience` | `not-applicable` | `not-applicable` | `not-needed` | `—` | `false` | Iteration 001 performs no network calls, no injection, and no shared-resource mutation. Dictionary loading is a read at startup, and the engines are deterministic pure functions — re-running them yields the same result by construction, so idempotency has no surface to protect. Retry and circuit-breaker semantics enter with the AI tier in the feature's iteration 002 scope and are gated there. The suppressed-key delivery invariant, which is the feature's genuine resilience concern, belongs to the Option B path in iteration 003 and is named as its blocking hardening target. | `—` |
| `test-integrity-targets` | `verification` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | FR-to-test mapping for this iteration: FR-005 → `MappingEngineTests` + `DetectionEngineTests`; FR-005a → `DetectionEngineTests` (two-layout and three-or-more-layout cases, including the ambiguity case asserting no correction); FR-005b → `WordAssemblyEngineTests` (completion on space, punctuation and committing keys; explicit negative test that no mid-word evaluation occurs); FR-006 → `DetectionEngineTests` (one case per caution level); FR-008 → `MappingEngineTests` plus a data-only new-pair test; FR-008a → a licence-provenance test asserting every shipped pack declares source and licence; FR-009 → `DetectionEngineTests` never-correct-set case; FR-029 → unknown-`schema_version` rejection tests. SC-001 → `CorpusAccuracyTests`, which produces a measured false-correction rate written to `quality/quality-evidence.md`. Every FR above has at least one negative-path assertion; smoke-only coverage is disallowed. | `true` | This iteration's entire deliverable is evidence rather than behavior, so test integrity is not a supporting concern here — it is the product of the iteration. SC-001 in particular must be a measured number from a corpus assembled independently of the detector, not a claim; a corpus built to match the detector would make the central product claim unfalsifiable. | `—` |
| `operational-resilience-concerns` | `operability` | `not-applicable` | `not-applicable` | `not-needed` | `—` | `false` | Iteration 001 ships no long-running process, no server, no telemetry pipeline, and no on-call surface — it produces libraries and a test suite. The operational concerns this feature genuinely has (hook watchdog and re-registration, tray status escalation, the diagnostic ring buffer) are tasks T045–T047, scheduled into iteration 003, where this concern will be `addressed` rather than `not-applicable`. Recording it as not-applicable here reflects this iteration's actual surface, not the feature's. | `—` |

## Lens Activation (Planning Baseline)

| Lens Ref | Activation | Planned Evidence Path |
| --- | --- | --- |
| `security-baseline@v1.0.0` | required | `specs/001-layout-autocorrect/iterations/001/quality/lenses/security-baseline.md` |
| `robustness-baseline@v1.0.0` | required | `specs/001-layout-autocorrect/iterations/001/quality/lenses/robustness-baseline.md` |
| `test-integrity@v1.0.0` | required | `specs/001-layout-autocorrect/iterations/001/quality/lenses/test-integrity.md` |

## Notes

- Replace every `<placeholder>` and every angle-bracketed instruction with iteration-specific content before crossing the `before-implement` boundary.
- After every row in the table is filled in with a canonical Status, flip the metadata `Overall Verdict` to `ready` (if every concern is `addressed` / `not-applicable` / `deferred-with-approval`) or keep `blocked`.
- Runtime evidence (lens execution, test counts, mechanical-findings results) is collected after implementation lands; the gate is a PLANNING-time artifact and that deferral is intentional.
- **Two concerns are `not-applicable` for this iteration but emphatically applicable to the feature.**
  Retry/idempotency becomes real with the AI tier, and operational resilience becomes real with the hook
  watchdog and diagnostic log in iteration 003. They are recorded as not-applicable here because
  iteration 001 ships pure libraries and a test suite with no hook, no injection, no network and no
  running process — not because the feature lacks those dimensions. Each must be re-opened, not
  inherited, when its iteration's gate is authored.
- The highest-consequence surfaces of this feature — keystroke capture, text injection, the fail-closed
  password gate, and suppressed-key delivery — are **not** in iteration 001 and are therefore not
  cleared by this gate. They are gated in the iterations that build them.
