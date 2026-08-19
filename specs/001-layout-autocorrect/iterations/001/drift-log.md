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
**Resolution rate**: 0% (0/1 resolved)
**Specification drift**: one task partially delivered, recorded and carried forward

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

**Resolution**: deferred. Carried as the first item of the next iteration: source and licence-verify
real permissive packs, then re-run the corpus measurement against them and compare.

**Class closure**: the loader refuses any pack that does not declare a source and a licence, and
`CorpusAccuracyTests.ShippedPacks_DeclareSourceAndLicence` fails the build if one slips through. That
makes an *unprovenanced* pack impossible. It does not make an *undersized* pack loud — the honest
statement is that pack adequacy is a human judgement recorded here, not a mechanism.

**Status**: deferred-with-record. Requires no human decision now; it is surfaced at review sign-off.

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
