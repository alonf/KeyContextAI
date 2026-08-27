# Review: Iteration 002

**Feature**: 001-layout-autocorrect
**Iteration**: 002
**Reviewed**: 2026-08-27
**Commit under review**: `68638b1` (source state `a980312`; partial-signoff delta `a6fab9b` accepted
by the maintainer's typed phrase, recorded below)
**Status**: Complete — review stopped by the maintainer's decision on 2026-08-27; sign-off completed
with the remaining findings recorded as follow-ups
**Overall Verdict**: accepted for what it delivered — six accessor and lifecycle tasks with test
coverage — with nine recorded follow-ups and the honest statement that the correction flow (T021
onward) was not reached. Three models are scheduled for redesign in iteration 003 (DRIFT-013).

## Scope reviewed

The delivered tasks of iteration 002: T017 (KeystrokeAccessor — `WH_KEYBOARD_LL` on a dedicated
message-pumping thread), T018 (InputInjectionAccessor — `SendInput` burst with self-injection
tagging), T019 (LayoutAccessor), T033 (FocusAccessor — focus events, UI Automation password
detection, caret), T034 (privacy lifecycle in CorrectionManager — fail-closed on `Unknown`, wipe on
focus change, pause and exit), T035 (focus-change abandon rule). 128 tests pass across the three
test projects.

Not delivered, stated honestly: the correction flow — T021 (single-word flow), T022 (audio cues),
T023 (overlay), T024/T036/T037 (integration tests) — was not reached. Iteration 002 closes without
it by the maintainer's decision of 2026-08-27.

## Reviewer independence

Both delivered rounds ran on the **codex** host (`codex-cli-file-primary`) under verified
containment — independent of the hosts that wrote the code (copilot and claude). The campaign is
`cmp-001-layout-autocorrect-i002`; authority records live under
file:///C:/Dev/KeyContextAI/.specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i002/ .

## Round history

- **run-20260827-130507200-ca1ad260** (codex, delivered): nine findings on the T034/T035
  security-critical surface. Fixed in `a980312` and verified fix-by-fix; the pause test and the
  fixture window identities were also corrected there (findings 8 and 9).
- Three runs between the delivered rounds failed without delivering: `134657966` (candidate-missing),
  `140159347` (target-digest mismatch — the moving iteration records, DRIFT-011), `141057773`
  (invoked and spent, killed by an operator stop, reconciled to terminal `abandoned` — the
  DRIFT-011 correction records both halves of that error). Two allowance resets were approved by
  the maintainer along the way.
- **run-20260827-143505800-a30101b4** (codex, delivered, on the post-update engine): nine findings —
  **2 blocking, 6 major, 1 minor by the reviewer's grading**, which the maintainer ruled governs
  over the engine's demotion. Full report:
  file:///C:/Dev/KeyContextAI/.specrew/review/authority/campaigns/cmp-001-layout-autocorrect-i002/runs/run-20260827-143505800-a30101b4/report.md

## Disposition of the final round's findings

Six of the nine map to defect classes already fixed in earlier rounds — shared-HWND identity,
keyboard-state thread, hook fast-path latency, suppression race, overflow accounting, injection
non-transactionality. The maintainer's pre-set exit criterion fired: fixes were relocating defects,
not removing them. Recorded as **DRIFT-013** (plan drift — the plan under-modelled the identity,
suppression and injection models); the redesign enters iteration 003 through the design workshop's
architecture and component lenses.

Three findings are new territory:

- **UIA `IsPassword` defaulting to safe** (blocking): fixed immediately as the security exception —
  `a6fab9b` maps anything but an explicit provider boolean to `Unknown`, which fails closed. Four
  unit tests added; all 128 tests green.
- **Layout identity collapse** (major) and **orphan keyup on suppression** (minor): folded into the
  iteration-003 design pass — both are identity/lifecycle-model questions.

All nine findings are saved as recorded follow-ups by the review landing.

## Sign-off

The maintainer chose "stop the review here" on 2026-08-27. Because `a6fab9b` landed after the
delivered round (by the maintainer's explicit ordering — the fail-open was live), sign-off required
and received the typed partial-coverage acceptance: the uncovered delta is exactly that one commit,
it implements the reviewer's own prescription letter for letter, moves only in the fail-safe
direction, and re-covering it would spend a round worth more against the iteration-003 redesign.
The final check then ran on the files exactly as they are and sign-off completed. Round budget at
close: 2 of 4 spent; the remaining rounds are reserved for the redesign.
