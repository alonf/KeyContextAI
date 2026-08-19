# Review: Iteration 001

**Feature**: 001-layout-autocorrect
**Iteration**: 001
**Reviewed**: 2026-08-19
**Commit under review**: `526d0b2`
**Status**: Complete — awaiting human sign-off
**Overall Verdict**: accepted

The `accepted` verdict is this reviewer's assessment of the **implementation**: it does what iteration
001 promised, with evidence. It is not a claim that the review was independent — see the independence
section below — and it does not pre-empt the automated co-review, whose findings are recorded
separately. Both items that were open when this review was first drafted are now closed: the
dictionary packs are sourced, and the independent co-review ran and passed.

## Scope reviewed

The 18 tasks of iteration 001: the solution skeleton and build posture, the domain records and
component contracts, the mapping, word-assembly and detection engines, the dictionary accessor, the
IoC composition root, the shipped key map and dictionary packs, the golden corpus, the architecture
test, and the CI lane.

Not in scope, because not built: the keyboard hook, text injection, the transcript journal, the
privacy lifecycle, the tray and overlay clients, the AI tier. Those belong to iterations 002–004 by
the human-approved slicing.

## Reviewer independence — resolved, with a finding about review scoping

The code-implementation lens selected **Copilot** as the independent co-review host, recorded at
file:///C:/Dev/KeyContextAI/.specrew/reviewer-hosts.json .

**The independent review ran and passed against this exact tree.** Run
`run-20260819-211204294-86de8c6e`, verdict `pass`, completion `complete`, currentness `current`, zero
findings. Evidence in file:///C:/Dev/KeyContextAI/.specrew/review/authority/ .

Getting there took four attempts and produced a finding worth keeping:

1. An engine version mismatch, cleared by `specrew update` — twice, because the installed engine hash
   moved again mid-session.
2. A spent round allowance. When round 1 completed before any code existed and reported nothing, I
   answered its run-again / stop-here / abandon question with "stop here". That was a review-pacing
   decision belonging to the maintainer, and it closed the campaign's ability to open a round over
   code that had not yet been written. Cleared by `--remediate allowance-reset` with a recorded reason.
3. A stale paused run, cleared by `--reconcile-run`.
4. **Scope, which is the finding that matters.** The first real run
   (`run-20260819-210747148-9bd5980b`) returned fourteen findings, and every one was the same shape:
   "CorrectionManager not implemented", "TranscriptEngine not implemented", "no end-to-end
   orchestration". All true, and all about components that iterations 002–004 build. The reviewer had
   auto-resolved the whole-feature design as its yardstick and judged iteration 001 against the
   finished product. Re-running with `--design-context-ref` pointed at the iteration plan gave it the
   right yardstick, and it passed with nothing found.

**Carry this forward**: a co-review of a sliced iteration must be given the iteration's scope
explicitly, or it measures the code against work that was never in scope and reports the difference as
defects. The first run's output was not wrong — it was answering a question nobody asked.

## Task Verdicts

| Task | Title | Verdict | Evidence |
| --- | --- | --- | --- |
| T001 | Solution skeleton, three projects, .NET 10 posture | pass | Builds clean in Debug and Release with warnings-as-errors |
| T002 | Test projects and corpus folder | pass | Three test projects, 57 tests running |
| T003 | Directory.Build.props with analyzers | pass | Analyzers on; one src finding honoured, two test-only exceptions recorded |
| T004 | GitHub Actions PR workflow | pass | `.github/workflows/ci.yml` runs build, architecture, unit and corpus tests |
| T005 | All component interfaces | pass | Contracts for the three engines and the dictionary accessor, each documented |
| T006 | Domain records per data model | pass | Records match `data-model.md`; `LayoutId` is a value type per the domain-types rule |
| T007 | Architecture test for IDesign call rules | pass | 7 tests; fails the build on a call-rule violation; no new dependency taken |
| T008 | IoC composition root | pass | Singleton lifetimes; engines constructed from loaded data, never loading it themselves |
| T009 | Key-map format and en-US↔he-IL map | pass | 30 keys with schema version; drives every translation test |
| T010 | Dictionary packs and golden corpus | pass | 370,079 English words (Unlicense) and 22,250 Hebrew words (CC0), licence-verified; corpus re-measured against them — DRIFT-001 resolved |
| T011 | MappingEngine tests | pass | 8 tests including unmappable codes, unknown layouts, determinism, data-only new pair |
| T012 | DetectionEngine tests | pass | 16 tests; every ambiguity path asserts `Ignore` |
| T013 | WordAssemblyEngine tests | pass | 13 tests including the explicit mid-word negative test |
| T014 | MappingEngine | pass | Translates from scan codes, never from rendered characters |
| T015 | DetectionEngine | pass | Conservative by construction; a pre-written test caught the scoring gap in F-03 |
| T016 | WordAssemblyEngine | pass | Completion only on separator or committing key |
| T020 | DictionaryAccessor | pass | Refuses unknown schema versions and packs without source and licence |
| T025 | Corpus accuracy test | pass | Produces the measurement; runs through the real accessor against shipped data |

No task is `needs-work`. T010 initially shipped hand-authored starter word lists and was recorded as
DRIFT-001; that drift was closed on 2026-08-20 by sourcing and licence-verifying real public-domain
packs, and the measurement was re-run against them.

## Requirement coverage

Every iteration-001 requirement, and how it is evidenced:

| Requirement | Evidence | Verdict |
| --- | --- | --- |
| FR-005 (evaluate every candidate layout) | `MappingEngineTests`, `DetectionEngineTests` | met |
| FR-005a (two versus more-than-two layout resolution) | `DetectionEngineTests.Evaluate_ThreeLayouts_*`, `CautionLevelTests` | met |
| FR-005b (completion on separator or committing key, never mid-word) | `WordAssemblyEngineTests` — including the explicit mid-word negative test | met |
| FR-006 (caution level sets the bar) | `CautionLevelTests` proves the levels change behaviour, not just state | met |
| FR-008 (a pair is data, not code) | `MappingEngineTests.Translate_ANewPairIsDataOnly` adds a third layout as data | met |
| FR-008a (licence provenance) | `DictionaryAccessor` refuses a pack without source and licence; both shipped packs are public-domain dedications with verification recorded in their manifests | met |
| FR-009 (never re-correct an affirmed word) | `DetectionEngineTests.Evaluate_AffirmedWord_IsNeverCorrected` | met |
| FR-013 (self-injected keys never re-enter) | `WordAssemblyEngineTests.Append_SelfInjectedKey_IsIgnored` | met at this layer |
| FR-029 (refuse unknown schema versions) | `DictionaryAccessor.RequireSupportedSchema` throws `DataPackRejectedException` | met |
| SC-001 (false-correction rate) | measured against 392,329 real words: 0 of 26 must-not-correct cases | **precondition met, criterion not yet evidenced** |
| SC-011 (a new pair is data only) | same as FR-008 | met |
| SC-012 (three or more layouts) | `DetectionEngineTests` clear-winner and ambiguous cases | met |

## Evidence, not assertion

- **Build**: solution builds clean in Debug and Release with `TreatWarningsAsErrors`, zero warnings.
- **Tests**: 57 passing — 45 core, 5 platform and corpus, 7 architecture.
- **Mechanical checks**: `run-mechanical-checks.ps1` reports zero findings.
- **Independent co-review**: Copilot, run `run-20260819-211204294-86de8c6e`, verdict `pass`,
  completion `complete`, currentness `current`, zero findings against this exact tree.
- **Measurement**: recorded at
  file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/quality/quality-evidence.md

## Findings

### F-01 — SC-001 is not evidenced by this corpus, and the evidence says so

**Severity**: informational, already disclosed.

The corpus holds 41 cases. SC-001 constrains false corrections to fewer than 1 in 1,000. A 41-case
corpus cannot measure that rate; zero false corrections here means the engine is not obviously wrong,
not that the criterion is met. This is stated in the quality evidence rather than left for a reader
to infer, which is the correct handling — but it means iteration 001's headline deliverable is
weaker than "SC-001 measured". The honest headline is: **the algorithm behaves correctly on every
case we thought to write down**.

### F-02 — DRIFT-001, resolved: an untested assumption nearly became a deferred gap

**Severity**: resolved; the process lesson is the durable part.

T010 first shipped hand-authored starter lists rather than sourced permissive packs, on my stated
belief that the environment had no outbound network access. I never tested that belief. When the
maintainer asked why an OSS dictionary could not simply be downloaded, one command disproved it.

Sourced and licence-verified the same day: 370,079 English words from `dwyl/english-words` under the
**Unlicense**, and 22,250 Hebrew words from **Wikidata Lexemes** under **CC0** — both public-domain
dedications, so redistribution inside an MIT product carries no obligation. `eyaler/hebrew_wordlists`,
the best-known Hebrew list, was rejected as AGPL-3.0 via Hspell, which is why the Hebrew pack is an
order of magnitude smaller than the English one.

Re-measuring against real data produced two findings the starter packs had hidden. Short words are
where layout detection is least reliable: `kt` and `fi` are genuine English entries, so the engine
correctly stopped correcting them, and both corpus cases were reclassified from true positives to
ambiguous. And Wikidata's Hebrew coverage has everyday holes: `עבודה` is absent, so that case is kept
and marked as a coverage gap, counted separately from algorithmic misses rather than deleted to make
the suite green. The conservative property survived a 1,400-fold increase in dictionary size unchanged.

The lesson worth carrying to the retro: an untested environment assumption came within one exchange of
becoming a deferred gap needing the maintainer's approval. The check that would have prevented it was
a single command. Superseded text follows for the record.

The original finding read: licence verification of third-party lists was not possible in this
environment. Shipping an unverified list would have violated FR-008a, the very requirement the task
serves. Recorded at
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/drift-log.md with class
closure noted honestly: the loader makes an *unprovenanced* pack impossible, but pack *adequacy*
remains a human judgement rather than a mechanism.

### F-03 — A test caught a real design gap in the detection engine

**Severity**: resolved during implementation, recorded because the lesson matters.

The first scoring implementation gave every multi-candidate case a flat confidence of 0.75, below the
balanced threshold of 0.80 — which made the frequency tie-break unreachable in practice. A test
written before the implementation caught it. The fix reflects that `ResolveWinner` has already
rejected every non-decisive field, so anything reaching the scorer won by a wide margin. It is now
scored at 0.85: above balanced, below conservative — meaning a user who chose conservative does not
get frequency arguments deciding their text. That is better behaviour than the original design, and
it exists because the test came first.

### F-04 — Analyzer exceptions were taken in test projects

**Severity**: informational, deliberate.

CA1707 (no underscores in member names) and CA1859 (prefer concrete types) are suppressed in
`tests/` only, with the reason recorded in file:///C:/Dev/KeyContextAI/tests/Directory.Build.props .
Both remain enforced in `src/`. The first conflicts with the `Method_Scenario_Expectation` naming
that makes a failing test readable; the second conflicts with testing through the interface seam,
which the contract-first design asks tests to do. In `src/`, CA1859 was honoured rather than
suppressed — two private helper signatures were narrowed to match.

### F-05 — `MainWindow` is a placeholder the product will not have

**Severity**: informational.

The WPF template's window survives because the host needs an entry point while iteration 001 builds
engines. KeyContext AI is tray-resident and has no main window in its finished form. The file says so
in its own documentation so a reader does not mistake it for intended design. It is removed when the
tray and overlay clients arrive.

### F-06 — The architecture test's inspection depth is signature-level

**Severity**: informational, recorded so the guarantee is not overstated.

`CallRuleTests` inspects constructor parameters, fields, and method signatures. That catches a
component *declaring* a collaborator it may not know about, which is how a real violation almost
always appears. It would not catch a violation constructed inside a method body — a `new
SomeAccessor()` in a local variable. Widening to IL inspection is possible and is not worth doing
until a real violation escapes; what matters is that the guarantee is stated at its true strength
rather than as "the call rules cannot be broken".

## Gap Ledger

- **GAP-01 — fixed-now** — the independent co-review had not executed over the implemented code; closed on 2026-08-20 when run `run-20260819-211204294-86de8c6e` completed with verdict `pass` against this exact tree, after being given the iteration plan as scope context (dimension: verification independence).
- **GAP-02 — fixed-now** — dictionary packs were hand-authored starters rather than sourced permissive packs; closed on 2026-08-20 by sourcing 370,079 English words under the Unlicense and 22,250 Hebrew words under CC0, with the corpus measurement re-run against them and the conservative property holding unchanged, as recorded in DRIFT-001 in the iteration drift log (dimension: implemented).

## What a reviewer should check most closely

1. **`DetectionEngine.ResolveWinner` and `ConfidenceFor`** at
   file:///C:/Dev/KeyContextAI/src/KeyContextAI.Core/Engines/DetectionEngine.cs — this is where a
   false correction would originate. The `DecisiveFrequencyRatio` of 100 is a judgement call with no
   empirical backing yet.
2. **The corpus composition** at file:///C:/Dev/KeyContextAI/tests/corpus/en-he-corpus.json — a
   corpus assembled by whoever wrote the detector tends to encode that detector's assumptions, which
   is exactly the risk flagged when this task was planned. A Hebrew-speaking reader is better placed
   than the author to judge whether these cases represent real mistyping.
3. **The architecture test's dependency detection** at
   file:///C:/Dev/KeyContextAI/tests/KeyContextAI.Architecture.Tests/CallRuleTests.cs — it inspects
   constructor parameters, fields, and method signatures. A violation hidden inside a method body
   (a `new SomeAccessor()` in a local) would not be caught.

## Verdict

**Accepted.**

The iteration delivered what it promised, with evidence rather than assertion: a detection algorithm
that is conservative by construction, architecture rules enforced by a test that fails the build, and
a measurement taken against 392,329 real public-domain words.

Both items that qualified this verdict when it was first drafted are now closed. The dictionary packs
are sourced and licence-verified, closing DRIFT-001. The independent co-review ran against this exact
tree and passed with zero findings, closing the independence gap.

One honest limit remains, and it is a property of the corpus rather than a defect in the code: a
41-case corpus cannot evidence a rate of fewer than 1 false correction per 1,000. Zero false
corrections here means the algorithm behaves correctly on every case anyone thought to write down.
SC-001 becomes genuinely measurable when the dictionary tier meets real typing, which is iteration 002
onward.
