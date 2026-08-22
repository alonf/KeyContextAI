# Hardening Gate: Iteration 002

**Schema**: v1
**Gate ID**: `pre-implementation-hardening`
**Feature Ref**: `specs/001-layout-autocorrect/spec.md`
**Iteration Ref**: `specs/001-layout-autocorrect/iterations/002`
**Requested Review Class**: `strongest-available`
**Effective Review Class**: `strongest-available`
**Overall Verdict**: `ready`
**Approval Ref**: `—`
**Reviewed By**: Implementer (planning-time gate fill)
**Reviewed At**: 2026-08-23T02:25:22Z

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
| `security-surface` | `security` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | `Password-state detection gate (Yes/No/Unknown), fail-closed on Unknown, abandon correction when focus changes, ignore self-injected events via native input tags, no typed-text persistence path, no network transmission in this slice.` | `true` | `This iteration observes global input and can inject text. The privacy boundary is enforced by FocusAccessor password-state detection and CorrectionManager gating: Unknown must suspend capture/correction, and any focus drift must cancel injection into a different target window.` | `—` |
| `error-handling-expectations` | `robustness` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | `Single fail-closed state transition for Unknown password-state; transcript wipe on focus-change/pause/exit; correction path aborts on stale focus; injection failure leaves original user text intact and avoids partial replay.` | `true` | `Any uncertainty in focus or password state is treated as unsafe and halts correction. Recovery preserves user intent by dropping the correction attempt rather than attempting best-effort continuation through ambiguous state.` | `—` |
| `retry-idempotency-requirements` | `resilience` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | `Bounded in-memory keystroke buffer only (max 256 chars and max 32 token spans per active focus session), ring-style eviction of oldest entries, wipe on boundary events, no cross-session replay cache.` | `true` | `The critical resilience control in this slice is memory/latency safety, not network retry. Explicit buffer bounds prevent unbounded growth under high-rate typing while keeping correction windows deterministic and disposable on focus/lifecycle transitions.` | `—` |
| `test-integrity-targets` | `verification` | `addressed` | `planning-time-analysis` | `pending-post-implementation` | `FR-003/FR-012/FR-013 validated by T033-T036 focus/password/fail-closed tests; FR-004 validated by T037 filesystem assertions; FR-010 path guarded by injection and focus-change integration checks; add explicit negative-path assertions for Unknown password-state and mid-flow focus loss.` | `true` | `This boundary is only safe when negative-path behavior is measured, not inferred. The review evidence must prove fail-closed suspension, transcript wipe, and no persistence, plus abandonment on focus drift.` | `—` |
| `operational-resilience-concerns` | `operability` | `not-applicable` | `not-applicable` | `not-needed` | `—` | `false` | `Iteration 002 introduces no service process, remote dependency, SLO target, or on-call operational surface. Runtime observability in this slice is limited to local test evidence and reviewer artifacts.` | `—` |

## Lens Activation (Planning Baseline)

| Lens Ref | Activation | Planned Evidence Path |
| --- | --- | --- |
| `security-baseline@v1.0.0` | required | `specs/001-layout-autocorrect/iterations/002/quality/lenses/security-baseline.md` |
| `robustness-baseline@v1.0.0` | required | `specs/001-layout-autocorrect/iterations/002/quality/lenses/robustness-baseline.md` |
| `test-integrity@v1.0.0` | required | `specs/001-layout-autocorrect/iterations/002/quality/lenses/test-integrity.md` |

## Notes

- Replace every `<placeholder>` and every angle-bracketed instruction with iteration-specific content before crossing the `before-implement` boundary.
- After every row in the table is filled in with a canonical Status, flip the metadata `Overall Verdict` to `ready` (if every concern is `addressed` / `not-applicable` / `deferred-with-approval`) or keep `blocked`.
- Runtime evidence (lens execution, test counts, mechanical-findings results) is collected after implementation lands; the gate is a PLANNING-time artifact and that deferral is intentional.
