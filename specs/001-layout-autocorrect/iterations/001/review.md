# Review: Iteration 001

**Feature**: 001-layout-autocorrect
**Iteration**: 001
**Reviewed**: 2026-08-19
**Commit under review**: `526d0b2`
**Status**: Draft — awaiting human sign-off

## Scope reviewed

The 18 tasks of iteration 001: the solution skeleton and build posture, the domain records and
component contracts, the mapping, word-assembly and detection engines, the dictionary accessor, the
IoC composition root, the shipped key map and dictionary packs, the golden corpus, the architecture
test, and the CI lane.

Not in scope, because not built: the keyboard hook, text injection, the transcript journal, the
privacy lifecycle, the tray and overlay clients, the AI tier. Those belong to iterations 002–004 by
the human-approved slicing.

## Reviewer independence — a gap, stated plainly

The code-implementation lens selected **Copilot** as the independent co-review host, and that
authorization is recorded at file:///C:/Dev/KeyContextAI/.specrew/reviewer-hosts.json .

**The independent review did not run over this code.** One review run exists in the authority store
(`run-20260819-061549610-c414c854`), and it executed *before any code was written* — it reviewed the
planning artifacts and found nothing. Two subsequent attempts to start a round over the implemented
code allocated run identifiers but never executed: the tool reported a round awaiting an answer, then
reported no round awaiting an answer when that round was answered. After two attempts the retry was
stopped rather than repeated.

What follows is therefore a **self-review by the implementing agent**, which is a materially weaker
check than an independent one — the same reasoning that produced the code is reviewing it. It is
recorded as such so nobody reads a passing review as an independent verdict. Re-running the
independent review is the first recommended action at sign-off.

## Requirement coverage

Every iteration-001 requirement, and how it is evidenced:

| Requirement | Evidence | Verdict |
| --- | --- | --- |
| FR-005 (evaluate every candidate layout) | `MappingEngineTests`, `DetectionEngineTests` | met |
| FR-005a (two versus more-than-two layout resolution) | `DetectionEngineTests.Evaluate_ThreeLayouts_*`, `CautionLevelTests` | met |
| FR-005b (completion on separator or committing key, never mid-word) | `WordAssemblyEngineTests` — including the explicit mid-word negative test | met |
| FR-006 (caution level sets the bar) | `CautionLevelTests` proves the levels change behaviour, not just state | met |
| FR-008 (a pair is data, not code) | `MappingEngineTests.Translate_ANewPairIsDataOnly` adds a third layout as data | met |
| FR-008a (licence provenance) | `DictionaryAccessor` refuses a pack without source and licence; `ShippedPacks_DeclareSourceAndLicence` | met in mechanism, see DRIFT-001 for data |
| FR-009 (never re-correct an affirmed word) | `DetectionEngineTests.Evaluate_AffirmedWord_IsNeverCorrected` | met |
| FR-013 (self-injected keys never re-enter) | `WordAssemblyEngineTests.Append_SelfInjectedKey_IsIgnored` | met at this layer |
| FR-029 (refuse unknown schema versions) | `DictionaryAccessor.RequireSupportedSchema` throws `DataPackRejectedException` | met |
| SC-001 (false-correction rate) | measured: 0 of 24 must-not-correct cases | **precondition met, criterion not yet evidenced** |
| SC-011 (a new pair is data only) | same as FR-008 | met |
| SC-012 (three or more layouts) | `DetectionEngineTests` clear-winner and ambiguous cases | met |

## Evidence, not assertion

- **Build**: solution builds clean in Debug and Release with `TreatWarningsAsErrors`, zero warnings.
- **Tests**: 57 passing — 45 core, 5 platform and corpus, 7 architecture.
- **Mechanical checks**: `run-mechanical-checks.ps1` reports zero findings.
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

### F-02 — DRIFT-001: the dictionary packs are starters

**Severity**: carried, deferred with a record.

T010 shipped hand-authored CC0 starter lists (roughly 160 English, 110 Hebrew words) rather than
sourced permissive packs, because licence verification of third-party lists was not possible in this
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

**Recommended: approve with the independent review re-run as a follow-up.**

The iteration delivered what it promised: a detection algorithm that is conservative by construction,
enforced architecture rules, and a measurement. Two things keep this from being an unqualified pass,
and both are disclosed rather than discovered: the independent co-review did not execute over the
code, and the dictionary packs are starters. Neither is a defect in what was built; both are limits
on what can be claimed about it.
