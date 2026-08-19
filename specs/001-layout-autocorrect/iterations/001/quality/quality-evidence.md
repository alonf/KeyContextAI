# Quality Evidence: Iteration 001

**Feature**: 001-layout-autocorrect
**Iteration**: 001
**Recorded**: 2026-08-19
**Build**: .NET 10.0.400, Release and Debug, warnings-as-errors, zero warnings

## The number this iteration exists to produce

Iteration 001's deliverable is a measurement, not a behaviour. Here it is.

| Measure | Result | Source |
| --- | --- | --- |
| False corrections | **0 of 24** must-not-correct cases | `CorpusAccuracyTests.FalseCorrectionRate_IsMeasuredAndReported` |
| True positives corrected | **17 of 17**, all to the intended text | `CorpusAccuracyTests.TruePositives_AreCorrectedToTheIntendedText` |
| Corrections to the *wrong* text | **0** | same test — tracked separately because a wrong correction is as severe as a false one |
| Caution monotonicity | holds: conservative ≤ balanced ≤ aggressive | `CorpusAccuracyTests.ConservativeCaution_NeverCorrectsMoreThanBalanced` |

The corpus at file:///C:/Dev/KeyContextAI/tests/corpus/en-he-corpus.json holds 41 cases: 17 wrong-layout
runs that must be corrected, 18 correctly-typed words that must not, 1 word that is real in both
readings, and 5 unrecognized strings (an identifier, a proper noun, keyboard mashing).

## What this measurement does and does not establish

**It establishes** that the detection algorithm is conservative in the way the requirements demand.
Every category that must be left alone was left alone, including the genuinely ambiguous case
(`so` renders as `דם` in Hebrew — both are real words, and the engine declines rather than guessing).

**It does not establish SC-001.** SC-001 constrains false corrections to fewer than 1 in 1,000 across
sustained real use. A 41-case corpus cannot measure a 1-in-1,000 rate — at best it shows the engine
is not obviously wrong. The honest reading is: the algorithm behaves correctly on every case we
thought to write down, which is a precondition for SC-001 rather than evidence of it. SC-001 becomes
measurable when the dictionary tier runs against real typing in a later iteration.

## Test inventory

| Suite | Tests | Covers |
| --- | --- | --- |
| `KeyContextAI.Core.Tests` | 45 | Mapping, word assembly, detection, and caution-level behaviour |
| `KeyContextAI.Platform.Tests` | 5 | Dictionary loading through the real accessor, plus the corpus measurement |
| `KeyContextAI.Architecture.Tests` | 7 | The IDesign call rules |
| **Total** | **57** | all passing |

Negative-path coverage, required by the hardening gate's test-integrity concern:

- Unmappable scan code returns an incomplete candidate rather than throwing.
- Unknown target layout is skipped rather than throwing.
- Empty input, absent dictionaries, and a candidate with no matching dictionary all return `Ignore`.
- A word valid in both layouts returns `Ignore` at every caution level.
- Nothing recognized anywhere returns `Ignore` at every caution level.
- A user-affirmed word is never corrected.
- A self-injected keystroke never enters word assembly (FR-013).
- Backspace on an empty word, and a separator with no word in progress, are both harmless.
- An unrecognized `schema_version` causes the pack to be refused rather than best-effort parsed.

## Architecture rules, enforced rather than documented

`KeyContextAI.Architecture.Tests` fails the build if any of these is violated:

- Engines depend on no accessor — the stricter-than-classic-IDesign rule agreed at the component lens.
- Engines depend on no manager and on no other engine.
- Managers depend on no other manager.
- `KeyContextAI.Core` does not reference the platform assembly, so the correction algorithm stays
  testable without a desktop and the recorded native-hook swap remains a one-project change.
- Every engine sits behind a contract interface.

Implemented with plain reflection: the dependency policy is "earned dependencies only", and no
package was needed to answer these questions.

## Open gap: the dictionary packs are starters, not the sourced packs

**T010 is partially complete and this is recorded rather than glossed.** The task called for word
lists assembled from permissively licensed sources. What shipped is a hand-authored CC0 starter list
per language (roughly 160 English and 110 Hebrew words) with a full licence manifest, because the
sourcing and licence verification of real word lists could not be done in this environment.

What this means concretely:

- The format, the manifest, the licence-provenance enforcement (FR-008a) and the loading path are
  complete and tested.
- The **data volume** is not production-grade. A real pack is tens of thousands of words, and
  detection accuracy on real typing will differ from the corpus result above.
- The corpus measurement remains valid for what it tests — the algorithm's decisions — because every
  corpus word is present in the starter packs by construction.

Recorded in the iteration drift log at
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/iterations/001/drift-log.md and carried as
the first item of the next iteration.

## Deferred to later iterations by the approved slicing

Not evidenced here because not built here: the keyboard hook and latency benchmarks, text injection,
the fail-closed password gate, suppressed-key delivery, the transcript journal, the AI tier. Each is
gated in the iteration that builds it, and the hardening gate for this iteration says so explicitly
rather than inheriting a pass.
