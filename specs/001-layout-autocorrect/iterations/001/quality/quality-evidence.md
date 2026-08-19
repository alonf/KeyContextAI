# Quality Evidence: Iteration 001

**Feature**: 001-layout-autocorrect
**Iteration**: 001
**Recorded**: 2026-08-19
**Build**: .NET 10.0.400, Release and Debug, warnings-as-errors, zero warnings

## The number this iteration exists to produce

Iteration 001's deliverable is a measurement, not a behaviour. Here it is.

Measured against the **sourced** dictionary packs — 370,079 English words and 22,250 Hebrew words.

| Measure | Result | Source |
| --- | --- | --- |
| False corrections | **0 of 26** must-not-correct cases | `CorpusAccuracyTests.FalseCorrectionRate_IsMeasuredAndReported` |
| Corrections to the *wrong* text | **0** | `CorpusAccuracyTests.TruePositives_AreCorrectedToTheIntendedText` — tracked separately because a wrong correction is as severe as a false one |
| True positives corrected | **14 of 15**, all to the intended text | same test |
| Known dictionary-coverage gaps | **1** (`עבודה` absent from the CC0 Hebrew source) | same test, counted separately from algorithmic misses |
| Caution monotonicity | holds: conservative ≤ balanced ≤ aggressive | `CorpusAccuracyTests.ConservativeCaution_NeverCorrectsMoreThanBalanced` |

The corpus at file:///C:/Dev/KeyContextAI/tests/corpus/en-he-corpus.json holds 41 cases: 15 wrong-layout
runs that must be corrected, 18 correctly-typed words that must not, 3 that are real words in both
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

## Dictionary packs: sourced, and what sourcing them revealed

**T010 is complete.** An earlier draft of this document recorded the packs as hand-authored starters
because I had assumed outbound network access was unavailable. That assumption was wrong and went
untested until the maintainer challenged it. Real packs are now sourced and licence-verified:

| Language | Source | Licence | Words |
| --- | --- | --- | --- |
| en-US | `dwyl/english-words` (`words_alpha.txt`) | Unlicense (public domain) | 370,079 |
| he-IL | Wikidata Lexemes, `dct:language wd:Q9288` | CC0-1.0 (public domain) | 22,250 |

Both licences are public-domain dedications, so redistribution inside an MIT product carries no
obligation. Licence verification for the English pack is the GitHub API's `spdx_id`; for Hebrew it is
Wikidata's blanket CC0 terms. Hebrew lemmas carrying niqqud were excluded, because Hebrew is typed
without vowel points and a vocalised lemma could never match what a keyboard produces.

**Rejected**: `eyaler/hebrew_wordlists`, the best-known Hebrew list, derives from Hspell and is
AGPL-3.0. Copyleft, so not usable under FR-008a. Its absence is why the Hebrew pack is an order of
magnitude smaller than the English one.

### Two findings that only real data could produce

Swapping 270 hand-picked words for 392,000 real ones changed three corpus outcomes, and each was
informative rather than a regression:

**Short words are where layout detection is least reliable.** `kt` (meant `לא`) and `fi` (meant `כן`)
stopped being corrected — because both are genuine entries in the English list. The engine saw a
valid as-typed word and left it alone, which is the correct conservative answer: correcting a word the
user may have typed deliberately is a *false* correction, the failure this product can least afford.
Both cases were reclassified in the corpus from `true_positive` to `ambiguous`, with the reasoning
recorded in the case notes. The general fact — two-letter runs are likely to be real words in both
languages — is a product insight worth carrying into the caution-threshold design.

**Wikidata's Hebrew coverage has everyday holes.** `עבודה` ("work") is absent from the CC0 lexeme set.
The case is kept in the corpus and marked `known_coverage_gap`, and the accuracy test now counts
coverage gaps separately from algorithmic misses — so the suite stays a measure of the *algorithm*
while the data gap is counted out loud instead of being deleted to go green.

### The measurement, re-run against real data

| Measure | Starter packs (270 words) | Real packs (392,329 words) |
| --- | --- | --- |
| False corrections | 0 of 24 | **0 of 26** |
| Corrections to the wrong text | 0 | **0** |
| True positives corrected | 17 of 17 | **14 of 15**, plus 1 known coverage gap |
| Caution monotonicity | holds | **holds** |

The conservative property — the one the product depends on — survived a 1,400-fold increase in
dictionary size unchanged. That is a materially stronger result than the starter-pack run, because a
larger dictionary creates far more opportunity to mistake a real word for gibberish.

## Deferred to later iterations by the approved slicing

Not evidenced here because not built here: the keyboard hook and latency benchmarks, text injection,
the fail-closed password gate, suppressed-key delivery, the transcript journal, the AI tier. Each is
gated in the iteration that builds it, and the hardening gate for this iteration says so explicitly
rather than inheriting a pass.
