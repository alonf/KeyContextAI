# Retrospective: Iteration 001

**Schema**: v1
**Date**: 2026-08-21

## Estimation Accuracy

**Per-task actual effort was never captured, so task-level variance cannot be computed for this
iteration.** That is recorded as a finding rather than filled with invented numbers.
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/tasks-progress.yml carries one
synthetic `started_at` (2026-08-19T09:00:00Z) and one synthetic `completed_at` (2026-08-20T08:40:00Z)
on all 18 tasks, so no task has an actual of its own. Improvement action 1 exists to fix this before
002, which matters because iteration 001 is the data that calibrates the three iterations after it.

| Task | Estimated | Actual | Delta |
| ---- | --------- | ------ | ----- |
| T001 | 1 | not captured | — |
| T002 | 0.5 | not captured | — |
| T003 | 0.5 | not captured | — |
| T004 | 1 | not captured | — |
| T005 | 1 | not captured | — |
| T006 | 1 | not captured | — |
| T007 | 1 | not captured | — |
| T008 | 0.5 | not captured | — |
| T009 | 1 | not captured | — |
| T010 | 3 | not captured | — |
| T011 | 0.5 | not captured | — |
| T012 | 1 | not captured | — |
| T013 | 0.5 | not captured | — |
| T014 | 1 | not captured | — |
| T015 | 2 | not captured | — |
| T016 | 1 | not captured | — |
| T020 | 1.5 | not captured | — |
| T025 | 1.5 | not captured | — |

**Average variance**: not computable — see above.

**What the record does support**: 19.5 story points planned, 19.5 delivered, **scope delta 0**, all 18
tasks terminal at `complete`, across three calendar days (first commit 2026-08-19, sign-off
2026-08-21). No task was dropped, deferred, or silently rescoped.

**The one visible mis-estimate is qualitative, not arithmetic.** T010 (dictionary packs and golden
corpus) was the largest task at 3 points and the only one that produced drift, needed a second pass,
and changed the meaning of the iteration's headline measurement. It was estimated as data assembly and
behaved like a research task. Points were not the thing under-estimated; **risk** was.

## Phase Variance

Actuals are unavailable at phase level for the same reason as task level. Planned distribution and the
qualitative read:

| Phase | Estimated | Actual | Delta | Notes |
| ----- | --------- | ------ | ----- | ----- |
| Setup (T001-T004) | 3 | not captured | — | No friction recorded; nothing in review or drift touches these. |
| Foundational (T005-T010) | 7.5 | not captured | — | Contains T010, the iteration's only drift source and its only second pass. |
| Detection algorithm (T011-T016) | 6 | not captured | — | Test-first paid off here: F-03 was caught before the implementation hardened. |
| Dictionary loading (T020) | 1.5 | not captured | — | Schema-version refusal and licence enforcement landed as specified. |
| Accuracy evidence (T025) | 1.5 | not captured | — | Delivered, but see F-01 — it measures less than SC-001 asks for. |
| **Total planned for 001** | **19.5** | **19.5 delivered** | **0** | Scope hit exactly; effort distribution within it is unmeasured. |
| Deferred to 002 | 17.5 | — | — | T017-T019, T021-T024, T033-T037, per the human-approved slicing. |
| Deferred to 003 | 19 | — | — | T026-T032, T045-T047. |
| Deferred to 004 | 14.5 | — | — | T038-T044, T048. |

**Unmeasured phase: review.** Review and sign-off consumed a disproportionate share of elapsed time
relative to its zero planned points — driven by the independence failure below and by tooling defects,
not by the code. No planning model currently carries a review-phase estimate at all.

## Drift Summary

- **Total drift events**: 4
- **In-project drift**: 1 (DRIFT-001) — resolved via implementation completion, **not** deferral
- **Specrew-side findings this project surfaced**: 3 (DRIFT-002, DRIFT-003, DRIFT-004) — referred upstream
- **Resolved via spec update**: 0
- **Resolved via revert**: 0
- **Deferred**: 0
- **Escalated to human decision**: 0 — no defer approval was needed, because nothing was carried

**Classification of DRIFT-001**: omission, not gold-plating and not direct violation. T010 shipped the
mechanism (pack format, licence manifest, FR-008a enforcement in the loader) with hand-authored starter
data instead of sourced permissive packs.

**Did drift escape to review?** Partly, and this is the honest reading. DRIFT-001 was self-recorded
during execution on 2026-08-19, so the *detection* did not escape. But its **resolution only happened
because the maintainer challenged the stated reason during review** — asking why an OSS dictionary
could not simply be downloaded. Left alone it would have reached iteration closeout as a deferred gap
requiring approval. Detection was in-phase; correction was human-triggered one phase late.

DRIFT-002, DRIFT-003 and DRIFT-004 were all surfaced during review and retro, and all belong to the
tooling rather than the product. None affects the shipped code.

## What Went Well

- **Test-first caught a real design gap, not a typo.** F-03: the first scorer gave every
  multi-candidate case a flat 0.75 confidence, below the balanced threshold of 0.80, which made the
  frequency tie-break unreachable in practice. A test written before the implementation found it. The
  fix — 0.85, above balanced and below conservative — is *better behaviour than the original design*,
  so a user on conservative never gets frequency arguments deciding their text.
- **The conservative property survived a 1,400-fold increase in dictionary size.** Zero false
  corrections across the must-not-correct cases before and after real packs replaced starters. That is
  the property the product lives or dies on, and it was re-measured rather than assumed.
- **Evidence was reported at its true strength, repeatedly.** SC-001 is recorded as
  *precondition met, criterion not yet evidenced* rather than claimed (F-01). The architecture test's
  guarantee is stated as signature-level rather than "the call rules cannot be broken" (F-06). The
  `עבודה` corpus case was kept and marked as a Wikidata coverage gap rather than deleted to make the
  suite green.
- **The architecture test enforces the IDesign call rules with plain reflection**, taking no new
  dependency to do it (T007).
- **Analyzer discipline held where it counted.** CA1707 and CA1859 are suppressed in `tests/` only,
  with reasons recorded; in `src/` CA1859 was *honoured* by narrowing two private helper signatures
  rather than suppressed.
- **The retraction was made in full.** When the false independence claim was found, review.md kept the
  history visible instead of quietly correcting itself.

## What Didn't Go Well

- **A false independence claim reached the review record, and a human caught it — not a gate.** An
  earlier revision of review.md asserted that a valid independent review had run against the code when
  none had. The proximate cause was accepting a 56-second run that returned `pass` while a 186-second
  run that actually examined the code returned `incomplete`/`partial`. The verdict that said the
  desired thing was believed; the runtime difference was visible and went unweighed. **This is the
  iteration's most serious process failure.** It was caught only because the maintainer asked directly
  whether the reviewer had really run against the code.
- **Once a bogus pass existed, the machinery kept reinforcing it.** Sign-off runs auto-anchor their
  baseline to the last recorded pass and review only what changed since — so every subsequent run
  dutifully reviewed governance files rather than code (GAP-01). Breaking the loop needed a
  full-surface round approved by the maintainer.
- **An untested environment assumption nearly became a deferred gap** (DRIFT-001, F-02). The claim
  that the environment had no outbound network access was never tested; one command disproved it. A
  gap requiring the maintainer's approval was one exchange away from being recorded.
- **Tooling friction consumed a large share of review and retro**, and produced three upstream
  findings: an authorship warning that fires on bookkeeping (DRIFT-002), a re-sync that silently
  downgrades a completed task ledger (DRIFT-003), and a reviewer-artifact scaffold that throws
  (DRIFT-004). The first two cost time; **the third blocks iteration closeout**, because the five
  reviewer artifacts validation requires — `code-map.md`, `coverage-evidence.md`, `reviewer-index.md`,
  `review-diagrams.md`, `dependency-report.md` — have no working generator. Together these cost more
  than any implementation task did.
- **Estimation data was never captured**, so the first iteration — the one that calibrates the next
  three — contributes a scope number and nothing about effort.

## Improvement Actions

1. **Capture per-task actual effort during execution.** Owner: Implementer. Phase: iteration 002.
   Type: process. Expected effect: task-level variance becomes computable, so the 002 retro can
   calibrate capacity on evidence instead of a single scope figure.
2. **Never cite a review run without first checking its `examined_paths` and its runtime.** Owner:
   Reviewer. Phase: iteration 002 review. Type: process. Expected effect: the failure that produced
   the false independence claim cannot recur silently. Partially mechanised already — review.md now
   carries the derived independent-review block, recomputed from the review store at every validation,
   so a hand-written independence claim no longer stands unchecked.
3. **Test environment assumptions before recording them as constraints.** Owner: Implementer. Phase:
   iteration 002. Type: process. Expected effect: removes the class of drift where a belief about the
   environment, not the environment, is the blocker.
4. **Decide SC-001's fate before 002 planning closes: grow the corpus until the rate is measurable, or
   restate the criterion.** Owner: Spec Steward, with Project Owner approval. Phase: iteration 002
   planning. Type: policy/spec. Expected effect: iteration 001's headline deliverable stops being
   "precondition met" and becomes evidenced — or the spec stops asking for a number this project does
   not intend to measure.
5. **Carry DRIFT-002, DRIFT-003 and DRIFT-004 upstream to the Specrew repository.** Owner: Alon
   (maintainer, already actioned for DRIFT-002). Phase: Specrew repo, outside this feature. Type:
   tooling. Expected effect: the governance signals this project depends on stop crying wolf.

### Reviewer-instruction triage

- **PROMOTE** — "verify a cited run's `examined_paths` and duration before the record cites it" into
  durable reviewer methodology. This iteration proves an unverified citation survives every other check.
- **PROMOTE** — "state a guarantee at its true strength" as a review-record convention. F-01 and F-06
  are the model: both were more useful for being weaker than they could have claimed.
- **DEFER** — IL-level inspection in the architecture test. Signature-level catches the realistic
  violation shape; revisit if one ever escapes (F-06).
- **DROP** — nothing.

## Calibration Suggestion

- **Suggested capacity adjustment: none. Hold 20 story points per iteration.**
- **Rationale**: 19.5 planned against 19.5 delivered with zero scope delta is a clean hit, but it is a
  single data point and effort inside it was never measured. The dashboard's 6.5 SP/day is arithmetic
  over three calendar days, not observed effort, and it self-reports low confidence until four
  iterations have closed. Adjusting capacity on this evidence would be fitting a line to one point.
  Revisit after 002 **with per-task actuals captured** (improvement action 1) — until then the honest
  position is that this project has scope calibration and no effort calibration.
- **One sequencing signal for 002 planning**: the review phase carries no estimate at all, yet consumed
  more elapsed time than several implementation tasks. Consider giving review and sign-off a nominal
  budget so overrun there is visible rather than absorbed.

## Signals for the Next Iteration

- **Hebrew corpus realism** remains an open judgement the maintainer took ownership of at sign-off. The
  Wikidata pack has everyday holes (`עבודה` is absent), and short-word detection is the least reliable
  region of the algorithm. Both bear on whether 002's live correction path behaves as the corpus
  predicts.
- **Iteration 002 builds the runtime components** the earlier co-review round mistook for absences —
  `CorrectionManager`, `TranscriptEngine`, the platform accessors, end-to-end orchestration. The
  reviewer must be scoped to 002's slice explicitly, or it will measure against the whole-feature
  design again.
- **`MainWindow` is removed** when the tray and overlay clients arrive (F-05).

## Notes

- Authored from file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/plan.md ,
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/state.md ,
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/drift-log.md ,
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/review.md and the branch
  commit history — not from session memory.
- The scaffold's seeded "Total drift events: 0" was stale on arrival and has been replaced with the
  drift log's actual contents.
