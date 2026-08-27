# Retrospective: Iteration 002

**Schema**: v1
**Date**: 2026-08-28 (work spanned 2026-08-22 to 2026-08-28)

## Estimation Accuracy

**Per-task actuals were captured this iteration** — improvement action 1 from the 001 retro landed:
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/002/tasks-progress.yml carries a
real `started_at`/`completed_at` per task. What they show is that implementation minutes were never
the cost:

| Task | Estimated (pts) | Implementer wall time | Delivered |
| ---- | --------------- | --------------------- | --------- |
| T033 | 2.5 | ~1 min | yes |
| T017 | 3 | ~5 min | yes |
| T018 | 2 | ~5 min | yes |
| T019 | 1 | ~7 min | yes |
| T034 | 1.5 | ~8 min | yes |
| T035 | 1 | ~4 min | yes |
| T021 | 2.5 | — | **not reached** |
| T022 | 0.5 | — | **not reached** |
| T023 | 2 | — | **not reached** |
| T036 | 0.5 | — | **not reached** |
| T037 | 0.5 | — | **not reached** |
| T024 | 0.5 | — | **not reached** |

**11 of 17.5 planned points delivered (63%); 6.5 points not reached.** The undelivered remainder is
the correction flow and its evidence — stated honestly at sign-off and at closeout, not silently
rescoped.

**Where the effort actually went**: the review-and-repair cycle. Four fix commits (`d730ce0`,
`7e1427c`, `a980312`, `a6fab9b`) across six calendar days, two delivered independent review rounds,
three failed review runs, two allowance resets, and one engine redeploy. The plan carried this at
zero points. The mis-estimate of 002 is not in any task row — it is DRIFT-013: three models
(typing-context identity, suppression lifecycle, injection terminal states) were budgeted as directly
implementable and each needed a design step.

## Phase Variance

| Phase | Planned (pts) | Outcome |
| ----- | ------------- | ------- |
| Privacy foundation (T033, T034, T035) | 5 | Delivered; survived three review rounds with fixes |
| Input path (T017, T018, T019) | 6 | Delivered; the highest-risk surface, and where all six relocated findings live |
| Correction flow (T021) | 2.5 | Not reached |
| Feedback (T022, T023) | 2.5 | Not reached |
| Acceptance evidence (T024, T036, T037) | 1.5 | Not reached |
| Review / repair | **0** | **Dominated the iteration** |

**The unplanned review phase is a repeated finding, and repetition is the finding.** The 001 retro
recorded "no planning model currently carries a review-phase estimate at all"; 002 repeated the
omission and paid for it with 37% of scope. Per the retro discipline, a lesson recurring verbatim
means it did not stick — see lesson 2, which now has an owner and a gate it cannot slip past.

## Drift Summary

- **Total drift events**: 8 — full detail in
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/002/drift-log.md
- **In-project drift**: 1 (DRIFT-013, plan drift) — resolved by human decision: stop patching,
  design the three models in iteration 003. Not deferral; a scoped design step with reserved
  review rounds.
- **Specrew-side findings**: 7 (DRIFT-008 through DRIFT-012, DRIFT-014, DRIFT-015) — all open
  upstream with the maintainer, all with named class closures in the log.
- **Resolved via spec update / revert**: 0. **Escalated beyond the recorded decision**: 0.

## What Went Well

- **The fail-closed architecture held under adversarial review.** Three rounds attacked the
  privacy/identity surface; the manager's gate structure absorbed the carried-origin and
  control-identity additions without a redesign of its own, and every fix round was independently
  verifiable fix-by-fix. 128 tests green at close, up from the round-1 baseline, with the test
  fakes now exercising the guarantees they previously stubbed (the finding-9 class is closed).
- **Exit criteria set before the round, not after.** The maintainer pre-committed the decision rule
  ("relocated findings a third time = stop patching"), which made the stop-signal mechanical when it
  fired — no post-hoc debate, and the design pass was chosen from evidence, not fatigue.
- **Independent review earned its cost.** The codex reviewer twice found real classes the
  implementing hosts had reasoned past — the shared-HWND identity gap and the IsPassword
  default-safe fail-open, the latter fixed same-day as a security exception (`a6fab9b`).
- **Per-task actuals exist now** (001's improvement action delivered), and drift capture kept pace
  with events — eight entries recorded the same day they fired, none reconstructed from memory.

## What Was Hard

- **The patch-and-relocate loop.** Six of the final round's nine findings were prior classes moved
  by their own fixes. Each individual fix was reasonable; the sequence was the signal. It took a
  pre-set exit criterion to stop the loop — worth noticing that nothing in the machinery would have
  stopped it.
- **Tooling friction consumed a working day.** In one session: an undelivered review run
  (candidate-missing), a digest mismatch with no paths named (DRIFT-011), a live run killed by an
  underspecified stop instruction with its spend orphaned (DRIFT-011 correction — the error recorded
  in both directions), an engine-version mismatch mid-session, a double acceptance demand
  (DRIFT-014), and a silently ignored near-miss phrase (DRIFT-015). Each was recovered; the sum was
  the day.
- **The engine's summary contradicted the reviewer's grading for the fifth consecutive round**
  ("nothing needs your attention" over 2 blocking + 6 major) — the standing DRIFT-010 pattern, now
  governed in this project by the maintainer's ruling that the reviewer's grade wins.

## Lessons Learned

1. **Fix-relocation is the design-pass signal.** A defect class that survives two consecutive
   independent rounds by moving is a model problem, not a code problem. *Owner: maintainer +
   coordinator. Action: adopted as a standing exit criterion — applied here to stop round 3, and
   carried into iteration 003's review posture.*
2. **Review and repair get a budget line — binding on what is ours to bind.** Second consecutive
   iteration where the 0-point review phase dominated; the soft version of this lesson has already
   failed twice, so discipline is not the answer. The project rule, binding from the iteration-003
   plan boundary onward: **every iteration plan in this project carries a review-and-repair budget
   line, and planning does not close without one**, calibrated from 002's actuals (2 delivered
   rounds, 4 fix commits, ~6 days elapsed). What this retro cannot do is make Specrew's
   capacity-planning step refuse a plan — that is the tool's behaviour, not this project's to
   legislate, and a downstream retro binding the tool is how a project accumulates private rules
   nobody else inherits. The mechanism is therefore separately recorded as a **Specrew feature
   candidate for the beta4 backlog**: a capacity-planning refusal that names the missing budget
   line and the fix, rather than merely refusing. *Owner: maintainer — the project rule at the 003
   plan boundary; the feature candidate in the Specrew backlog.*
3. **Enumerate observable state before theorizing.** Three doomed retries were built on an untested
   hook-writes theory while `git status` named the real mover (DRIFT-011). *Owner: coordinator
   agents. Action: recorded in session memory and proposed as a reviewer/coordinator instruction —
   on any state-mismatch refusal, list what actually changed before forming a cause.*
4. **Kill instructions name their check; executors read interim state before killing.** The orphaned
   spend had two parents: a stop order with no condition and a kill with no look (DRIFT-011
   correction). *Owner: both, standing practice from here.*
5. **Records inside the reviewed surface are committed before any round.** Uncommitted
   `specs/` iteration records moved the review digest between preflight and execution (DRIFT-011).
   *Owner: coordinator. Action: in session memory; upstream fix requested for the unnamed-paths
   refusal (DRIFT-012 class closure).*

## Reviewer-Instruction Triage

- **PROMOTE — "silence must not read as safe."** The IsPassword class: any mapping where an
  unimplemented provider/property defaults into the permissive branch is a finding. Candidate for
  the code-rules catalog and the reviewer playbook; it would have caught `a6fab9b`'s defect at
  T033's first review.
- **PROMOTE — reviewer grade governs over engine demotion** (maintainer ruling, applied four rounds
  running) — already standing in this project; belongs upstream with DRIFT-010.
- **PROMOTE — hook-latency measurement harness, built before the design pass consumes it.** Round 3
  found the round-1 latency cleanup undone by the round-3 fixes — six synchronous queries and an
  allocation back on the low-level callback. The identity/suppression/injection redesign will touch
  that callback again, and a design that cannot measure its own latency will reintroduce the defect
  it was written to avoid and discover it in round 5. Latency is a constraint on all three models,
  so the instrument comes before the design: a standing follow-up built early in iteration 003, an
  input to the design pass rather than a check behind it.
- **DROP — nothing.** Every candidate this iteration surfaced is promoted.

## Signals for Next Iteration

- **Iteration 003 opens with the design pass**, through the workshop's architecture and component
  lenses, on the three recorded questions (DRIFT-013 class closure): typing-context identity where
  HWNDs are shared and providers inconsistent; the suppressed-key lifecycle as one atomic protocol;
  injection as a transaction with defined terminal states. **Two review rounds are reserved for the
  redesign** — not for the code it replaces.
- **The hook-latency harness is built early in 003, before the design pass** — the design's
  measuring instrument, per the triage promotion above.
- **The 6.5 unreached points (T021–T024, T036, T037) do not auto-carry.** They were scoped against
  three models that turned out to be wrong; re-estimating them after the design pass is not the
  same work as carrying them forward. Re-task after the design outcome, with the review budget
  from lesson 2.
- **The follow-ups carry grouped, not flat** (closeout verdict instruction): each design question
  owns its symptoms — identity (shared-HWND gap, keyboard-state thread, layout LANGID collapse),
  suppression lifecycle (disarm race, orphan keyup), injection terminal states (overflow
  accounting, non-transactionality) — with hook latency as the binding constraint, measured first.
  Nothing carries as an independent ticket; the fixed IsPassword finding carries nothing.
- **Iteration 003 opens in a fresh session on a settled engine build** (closeout verdict): today's
  seven upstream findings go to the maintainer's crew as a single brief and will land as engine
  changes, and a design pass should not update mid-flight. Nothing is lost by waiting — the
  questions are recorded, the harness is scoped, the two rounds are reserved.
