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

**Total drift events**: 1
**Resolution rate**: 100% (1/1 resolved)
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

### Resolution Strategies (Unused)

The following resolution strategies remain available if drift is detected later in execution:

- **spec-updated**: Update the spec to reflect implementation choice
- **implementation-reverted**: Revert implementation to match spec
- **deferred**: Mark drift as deferred to next iteration
- **human-decision**: Escalate to Alon for resolution

### Notes

- This artifact was scaffolded before review starts so drift can be logged immediately when detected.
- Replace the zero-drift summary with real counts when the first drift event is recorded.
