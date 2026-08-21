# Drift Log: Iteration 001

**Schema**: v2

<!--
  Markdown authoring note (Specrew lifecycle convention):

  When you add new drift events to this file, watch for MD032 (blanks-around-lists).
  A sentence ending with a colon, immediately followed by a bullet list, is the most
  common violation. Always put a BLANK LINE between the colon line and the list:

      BAD:                              GOOD:
      Resolution steps:                 Resolution steps:
      - Step one                        <— blank line here
      - Step two                        - Step one
                                        - Step two

  The F-033 pre-boundary markdownlint gate runs markdownlint-cli --fix on .md
  changes before every boundary-sync write, so most violations auto-fix — but the
  blank line you write in the first place avoids the cleanup churn.
-->

## Summary

**Total drift events**: 3 (1 in this project, 2 Specrew-side findings this project surfaced)
**Resolution rate**: 100% of in-project drift resolved (1/1); the 2 tooling findings are open upstream
**Specification drift**: none outstanding

## Events

### DRIFT-001: T010 shipped starter dictionary packs rather than sourced permissive ones

**Detected**: 2026-08-19, during implementation of T010.
**Class**: partial task delivery, not a spec contradiction.
**Requirement**: FR-008a — shipped dictionary data must come only from sources whose licence permits
redistribution in an MIT-licensed product, and each pack must record its source and licence.

**What the task asked for**: word lists assembled from permissively licensed sources (Hunspell
dictionaries, Wiktionary-derived frequency lists) for English and Hebrew, with recorded provenance.

**What was delivered**: the pack format, the licence manifest, the FR-008a provenance enforcement in
the loader, and a **hand-authored CC0 starter list** per language — roughly 160 English and 110
Hebrew words. Sourcing and licence-verifying real third-party word lists could not be done in this
environment, and shipping an unverified third-party list would have violated the very requirement the
task exists to satisfy.

**Why this is drift rather than completion**: the requirement is met in mechanism but not in data
volume. A production pack is tens of thousands of words. Detection accuracy against real typing will
differ from the corpus result, and any inference from the corpus number to real-world behaviour is
therefore unsupported.

**What is genuinely complete**: the format, the loader, the schema-version refusal, the licence
provenance check, and the corpus measurement — which remains valid for what it tests, since it
measures the algorithm's decisions and every corpus word is present in the starter packs.

**Resolution**: **RESOLVED 2026-08-20 — implementation-completed, not deferred.**

The maintainer asked why an OSS dictionary could not be found and downloaded. The honest answer was
that I had never tested it: I asserted the environment had no outbound network access without
checking. It does. That assumption, not the environment, was the actual blocker.

Sourced and licence-verified:

- **en-US**: `dwyl/english-words` (`words_alpha.txt`), **Unlicense** — a public-domain dedication,
  confirmed via the GitHub API's `spdx_id`. 370,079 words after filtering to `^[a-z]{2,}$`.
- **he-IL**: **Wikidata Lexemes**, lemmas where `dct:language` is `wd:Q9288`, retrieved through the
  Wikidata Query Service. **CC0-1.0**, also a public-domain dedication. 22,250 words after excluding
  niqqud-bearing forms, which could never match keyboard output.
- **Rejected**: `eyaler/hebrew_wordlists`, the best-known Hebrew list, is AGPL-3.0 because it derives
  from Hspell. Copyleft, so unusable under FR-008a — which is why the Hebrew pack is an order of
  magnitude smaller than the English one.

Re-running the corpus measurement against real data surfaced two findings the starter packs had
hidden, both recorded in the quality evidence: short words are where layout detection is least
reliable (`kt` and `fi` are genuine English entries, so leaving them alone is correct conservative
behaviour, and both cases were reclassified as ambiguous), and Wikidata's Hebrew coverage has
everyday holes (`עבודה` is absent, kept in the corpus as a marked coverage gap rather than deleted).

The conservative property held across a 1,400-fold increase in dictionary size: still zero false
corrections, still zero corrections to the wrong text.

**Lesson for the retro**: an untested assumption about the environment nearly became a deferred gap
requiring the maintainer's approval. The check that would have prevented it cost one command.

**Class closure**: the loader refuses any pack that does not declare a source and a licence, and
`CorpusAccuracyTests.ShippedPacks_DeclareSourceAndLicence` fails the build if one slips through. That
makes an *unprovenanced* pack impossible. It does not make an *undersized* pack loud — the honest
statement is that pack adequacy is a human judgement recorded here, not a mechanism.

**Status**: resolved. No human defer decision is required, because nothing is being carried.

<!--
  Every new ### DRIFT-... event includes:

  - **Class closure**: the executable mechanism that makes the next instance impossible or loud

  If the change fixes only this instance, write NONE — <why class closure is not in scope>.
  A bare Resolution: FIXED is not class closure.
-->

### DRIFT-002: Specrew labelled this session the implementer from files it never wrote

**Detected**: 2026-08-21, at the review-signoff boundary of this iteration.
**Class**: defect in Specrew's own governance tooling (W34-B, review-record authorship), surfaced by
this project. Not spec, plan, or implementation drift in KeyContextAI.
**Requirement**: none. No FR or SC of this feature is affected.

**What fired**: `validate-governance` reported `review-authored-by-implementer` against
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/review.md — "written by the
session that also wrote source in this project" — for a session that wrote no product code at all.
`src/`, `tests/`, `data/` and `.github/workflows/` are byte-identical between the reviewed tree
`273c69bb` and the boundary commit `613271f9`.

**Two independent causes, each verified by executing the tooling's own functions rather than reading
them**:

1. **Specrew's own deployed runtime is classified as product source.**
   `Test-SpecrewReviewAuthorshipSourcePath` already applies the negative rule W33 uses — everything is
   source unless it is recognisably a record or a document — so `specs/`, `docs/`, dot-directories and
   `.md`/`.txt`/`.rst`/`.adoc` are excluded. But Specrew deploys its continuous-co-review runtime into
   `scripts/internal/continuous-co-review/`, which matches none of those. Measured on this project:
   `review-authority-core.ps1` and `.specrew-runtime.json` classify as source, while
   `specs/001-layout-autocorrect/tasks.md` does not. The session-start redeploy rewrote fifteen of
   those files at 09:42 UTC, so they sat dirty for the whole session.

2. **The observation reads the dirty worktree rather than what the session wrote.**
   `Write-SpecrewReviewAuthorshipObservation` is fed `$materialSignal.changed_paths`, and
   `conformance-turn-delta.ps1` computes that in one of two attribution modes — `exact-turn`, or
   `degraded-worktree` when the capture event is not a turn-start. This session ran degraded
   throughout; the conformance hook said so verbatim: "exact per-turn attribution is unavailable". In
   that mode the path set is everything currently dirty, including files no session touched.

**Why it matters**: cause 2 contradicts W34-B's stated premise. Its own header commits to a fact
"minted by the hook from what it watched the session write", precisely so authorship is observed
rather than asserted — but in degraded mode it is minted from what happens to be dirty. Widening the
exclusion list treats only cause 1; any session leaving anything dirty under a source-shaped path is
still labelled an implementer. That matters most here, because this is the one warning in the set that
exists to catch a failure this feature has already had once — the retracted independence claim
recorded in review.md.

**Resolution**: referred upstream to the Specrew repository by the maintainer on 2026-08-21, with this
diagnosis taken as-is. Deliberately not fixed from inside this project: patching Specrew from a project
it governs would contaminate the evidence this project produces about it.

- **Class closure**: NONE — the recurrence guard belongs in the Specrew repository, outside this
  feature's scope. Carried into the retro as a friction item so it is not lost when the session ends.

**Status**: open upstream. Nothing is carried in this feature's scope, and this iteration's product
code and review evidence are unaffected.

### DRIFT-003: an iteration-001 re-sync silently downgraded the task ledger to pending

**Detected**: 2026-08-21, when boundary-sync refused the review-signoff arrival on its
iteration-state truth gate.
**Class**: defect in Specrew's task-progress derivation, surfaced by this project. Not spec, plan, or
implementation drift in KeyContextAI.
**Requirement**: none. No FR or SC of this feature is affected.

**What fired**: the gate refused because
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/state.md still read
"Execution has not started yet" while every one of the 18 iteration-001 tasks sat at `pending`, even
though review.md and the campaign evidence both describe a completed iteration.

**Cause**: `Get-TaskProgressDerivedStatusHints` treats the feature-root
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/tasks.md as authoritative for iteration 1
only. That file's checkboxes were never ticked during the hand-driven implementation flow, so a
re-sync derived `pending` for all 18 tasks and overwrote a ledger that had recorded them `complete`,
then rewrote state.md's managed summary to match. Specrew's source documents this exact hazard as a
known unfixed follow-up, noting iteration 001 "is not the active summary target in the observed
symptom, so it is out of scope for this slice" — this project is that symptom.

**Why it matters**: the downgrade is silent and self-consistent. The ledger and the managed summary
agreed with each other and disagreed only with the prose beneath them, so nothing surfaced it until a
gate that cross-checks evidence artifacts refused. A state file that quietly rewrites lifecycle truth
is the failure mode the truth gate exists to catch, and here the rewrite came from the tooling itself.

**Repaired in this project on 2026-08-21**, honestly rather than by asserting completion: the full
suite was run first and passes 57 tests (45 core, 5 platform, 7 architecture), matching what review.md
records, and only then were the 18 boxes (T001–T016, T020, T025) ticked and the ledger and state.md
re-derived through the task-progress path. Committed as `a44f3f6`.

- **Class closure**: NONE — the repair restores this instance only. The derivation defect and its
  guard belong in the Specrew repository; any future iteration-001 re-sync in any project with
  unticked boxes will downgrade the ledger again. Carried into the retro as a friction item.

**Status**: instance repaired here; the defect is open upstream.

### Resolution Strategies (Unused)

The following resolution strategies remain available if drift is detected later in execution:

- **spec-updated**: Update the spec to reflect implementation choice
- **implementation-reverted**: Revert implementation to match spec
- **deferred**: Mark drift as deferred to next iteration
- **human-decision**: Escalate to Alon for resolution

### Notes

- This artifact was scaffolded before review starts so drift can be logged immediately when detected.
- Replace the zero-drift summary with real counts when the first drift event is recorded.
