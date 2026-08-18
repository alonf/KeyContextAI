# Feature Specification: KeyContext AI — Keyboard Layout Auto-Correction

**Feature Branch**: `001-layout-autocorrect`

**Created**: 2026-08-19

**Status**: Draft

**Input**: A Windows background tool that detects wrong-keyboard-layout typing (for example English typed
while the Hebrew layout is active), corrects the mistyped text in place, switches the active keyboard
layout to the intended language, and confirms the action with subtle audio and visual feedback.
Detection is tiered: a fast local dictionary first, with a context-aware AI tier for ambiguous cases.

**Design provenance**: every requirement below traces to a decision recorded in the design workshop
under `specs/001-layout-autocorrect/workshop/` (product-domain plus ten technical lenses), each
confirmed by the human in a typed reply.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A mistyped word fixes itself (Priority: P1)

A bilingual user is typing in a chat window. They meant to write a Hebrew word but the English layout
is still active, so gibberish appears. Before they notice, the word is replaced with what they meant,
the keyboard layout switches to Hebrew, a soft sound confirms it, and a small bubble near the cursor
shows what changed. They keep typing without breaking flow.

**Why this priority**: This is the entire product promise. Without it nothing else has value.

**Independent Test**: Type a known wrong-layout word followed by a space in a plain text field; verify
the text is replaced with the intended word, the layout switched, and both feedback channels fired.

**Acceptance Scenarios**:

1. **Given** the English layout is active and the user intends Hebrew, **When** they type a word whose
   layout-translation is a valid Hebrew word and press space, **Then** the typed text is replaced with
   the Hebrew word, the layout switches to Hebrew, a sound plays, and a bubble appears near the caret.
2. **Given** the user types a legitimate English word, **When** they press space, **Then** nothing is
   changed, no sound plays, and no bubble appears.
3. **Given** a correction has just been applied, **When** the user presses the flip hotkey, **Then**
   the original typed text is restored and that word is not corrected again during the session.

---

### User Story 2 - A whole mistyped phrase fixes itself, even while typing continues (Priority: P1)

The user types several words before noticing the layout is wrong, and keeps typing while the correction
happens. The full run of wrong-layout words is corrected as one unit, and the characters typed during
the correction are corrected too, so the result is never a half-translated sentence.

**Why this priority**: Multi-word runs and mid-correction typing are the normal case for fast typists.
A tool that only handles single isolated words produces mangled text in real use, which is the defined
failure condition for this product.

**Independent Test**: Type three consecutive wrong-layout words, continue typing during the correction
window, and verify the entire run plus the newly typed characters end up correct.

**Acceptance Scenarios**:

1. **Given** the user has typed several consecutive wrong-layout words, **When** detection fires,
   **Then** the whole consecutive run is corrected in one action, not word by word.
2. **Given** the user continues typing while a correction is being applied, **When** the correction
   completes, **Then** the characters typed during that window are also correct, and no character is
   lost, duplicated, or left translated twice.
3. **Given** the user changes to a different window between detection and replacement, **When** the
   correction would be applied, **Then** it is abandoned and no text is injected anywhere.

---

### User Story 3 - The user controls when and where it acts (Priority: P2)

The user opens the tray menu to pause corrections, switch to notify-only mode, or exclude the
application currently in front. They open settings to choose language pairs, how cautious the tool
should be, sounds and bubble visibility, and which AI provider to use.

**Why this priority**: Control is what makes an always-on input tool tolerable, and per-application
exclusion is the escape valve for applications where text injection misbehaves.

**Independent Test**: Exclude the foreground application from the tray, type a wrong-layout word in it,
and verify no correction occurs; re-enable and verify correction resumes.

**Acceptance Scenarios**:

1. **Given** the tool is active, **When** the user selects Pause from the tray, **Then** no correction
   or detection occurs until resumed, and the tray shows the paused state.
2. **Given** notify-only mode is selected, **When** a wrong-layout word is detected, **Then** a distinct
   sound plays and no text is changed.
3. **Given** an application is excluded, **When** the user types wrong-layout text in it, **Then**
   nothing is captured, corrected, or reported for that application.

---

### User Story 4 - Ambiguous cases get contextual help (Priority: P2)

A wrong-layout run does not resolve confidently against the dictionary — a proper noun, a rare word, or
a phrase whose translation is ambiguous. The tool consults the user's configured AI provider with a
single sentence of context and applies the result only if it is confident. If the provider is slow or
unavailable, the tool stays silent and typing is never delayed.

**Why this priority**: This is the product's differentiator and the reason the AI tier exists, but the
dictionary tier must deliver value on its own for users who never configure a provider.

**Independent Test**: Configure a provider, type a phrase the dictionary cannot resolve, and verify a
correct result arrives; then disable the network and verify typing is unaffected and the tool degrades
to dictionary-only with a visible status change.

**Acceptance Scenarios**:

1. **Given** the dictionary tier returns no confident verdict and an AI provider is configured,
   **When** the user finishes the phrase, **Then** the AI tier is consulted with at most one sentence
   of context and applies a correction only above the confidence threshold.
2. **Given** the AI provider does not answer within the time limit, **When** the limit elapses,
   **Then** no correction is applied, typing is unaffected, and no error is shown to the user.
3. **Given** repeated provider failures, **When** the failure threshold is reached, **Then** the tool
   continues on the dictionary tier alone and the tray status indicates the AI tier is offline.
4. **Given** no AI provider is configured, **When** the user types, **Then** the dictionary tier works
   fully with no configuration and no prompts.

---

### User Story 5 - The user can trust it with everything they type (Priority: P1)

The user types a password, a private message, and a banking detail. None of it is stored, transmitted,
or retained anywhere. When focus moves to a password field, capture stops entirely.

**Why this priority**: A tool that reads every keystroke is only adoptable if its privacy behavior is
absolute and demonstrable. This is a shipping condition, not a feature.

**Independent Test**: Focus a password field, type, and verify no capture, correction, or record of any
kind; inspect all files the tool writes and confirm no typed text is present.

**Acceptance Scenarios**:

1. **Given** focus moves to a password field, **When** the user types, **Then** nothing is captured or
   corrected.
2. **Given** the tool cannot determine whether a field is a password field, **When** the user types,
   **Then** capture is suspended rather than assumed safe.
3. **Given** the user switches windows, **When** the switch completes, **Then** all retained typing
   context from the previous window is discarded.
4. **Given** no AI provider has been explicitly enabled, **When** the user types anything, **Then**
   nothing leaves the machine.

---

### Edge Cases

- The user types a word that is valid in **both** layouts — the tool must leave it alone rather than
  guess, because a false correction is worse than a missed one.
- The target application rejects or mishandles injected text (remote desktop sessions, games, secured
  input surfaces) — the correction is abandoned and, after repeated failures in the same application,
  the user is offered an exclusion.
- The user presses the flip hotkey when no correction is pending — nothing happens.
- A dictionary or settings file is corrupt or from an unknown version — the tool refuses that file with
  a visible reason and continues with what remains valid, rather than silently misreading it.
- The keyboard hook is dropped by the operating system — the tool restores it automatically and only
  alerts the user if restoration repeatedly fails.
- An AI response arrives after the user has already typed on, flipped back, or changed focus — the
  stale response is discarded, never applied.
- The user types extremely fast, so more words accumulate during a correction than after a slow typist's
  — the correction accounts for everything typed in the window regardless of speed.
- The user has more than two layouts installed — the intended layout is resolved by comparing every
  candidate translation, not by assuming "the other" layout, and ambiguity between two plausible
  candidates leaves the text untouched.

## Requirements *(mandatory)*

### Functional Requirements

**Capture and context**

- **FR-001**: The system MUST observe keystrokes across all applications without perceptibly delaying
  the user's typing.
- **FR-002**: The system MUST maintain a rolling in-memory record of recent typing — the characters,
  which layout produced them, and their correction state — sufficient to correct multi-word runs.
- **FR-003**: The system MUST discard that record when window focus changes, when a password field is
  focused, when the user pauses the tool, and when the tool exits.
- **FR-004**: The system MUST NOT write any typed text to disk or transmit it, except the single
  sentence of context permitted by FR-016 when the user has explicitly enabled an AI provider.

**Detection**

- **FR-005**: The system MUST evaluate a completed word by translating the keystrokes to each candidate
  layout and comparing every candidate, including the text as typed, against dictionary data.
- **FR-005a**: When the user has exactly two layouts installed, the intended layout MUST be taken as the
  one not currently active. When more than two are installed, the system MUST determine which layout the
  text was intended for by comparing all candidate translations, and MUST leave the text unchanged when
  no single candidate is a clear winner.
- **FR-006**: The system MUST apply a correction only when confidence exceeds the threshold implied by
  the user's chosen caution level, and MUST leave text unchanged when uncertain.
- **FR-007**: The system MUST widen detection from a single word to the full run of consecutive
  wrong-layout words so a mistyped phrase corrects as one action.
- **FR-008**: The system MUST support any language pair through data alone, so adding a pair requires
  no change to the correcting behavior itself.
- **FR-009**: The system MUST NOT re-correct a word the user has flipped back during that session.

**Correction**

- **FR-010**: The system MUST replace the detected text with the intended text and switch the active
  keyboard layout to the intended language as a single user-visible action.
- **FR-011**: The system MUST also correct characters typed between detection and replacement, so
  continued typing never produces partially translated text.
- **FR-012**: The system MUST abandon a correction, changing nothing, if window focus changed after
  detection.
- **FR-013**: The system MUST NOT treat its own injected keystrokes as user input.
- **FR-014**: The system MUST leave text exactly as the user typed it if a correction fails partway.
- **FR-015**: Users MUST be able to reverse the most recent correction with a hotkey that does not
  conflict with the host application's own undo, and to re-apply it with the same hotkey.

**AI tier**

- **FR-016**: When the dictionary tier is not confident and the user has explicitly enabled an AI
  provider, the system MUST consult it with at most one sentence of context and no identifying
  information about the user, the machine, or the application.
- **FR-017**: The system MUST never delay typing on an AI response, MUST stop waiting after a fixed
  time limit, and MUST continue on the dictionary tier alone when the provider is unavailable.
- **FR-018**: The system MUST discard an AI response whose correction is no longer applicable.
- **FR-019**: The system MUST support cloud providers, a locally hosted model, and reuse of an
  already-installed assistant the user is signed in to, so a user without an API key can still enable
  the AI tier.
- **FR-020**: The system MUST NOT enable any AI provider without an explicit user decision, including
  when an installed assistant is detected — detection MUST ask, never activate.
- **FR-021**: The system MUST store provider credentials encrypted so that another user account on the
  same machine cannot read them.

**Feedback and control**

- **FR-022**: The system MUST confirm each correction with a sound and a brief on-screen indication near
  the text cursor showing what changed and the new language, each independently disableable.
- **FR-023**: The system MUST use distinguishable sounds for a correction applied, a detection in
  notify-only mode, and a correction that came from the AI tier.
- **FR-024**: The on-screen indication MUST NOT take focus, block interaction, or obstruct the text, and
  MUST render right-to-left text correctly.
- **FR-025**: Users MUST be able to pause the tool, switch between correcting and notify-only, and
  exclude the foreground application in one action from the tray.
- **FR-026**: Users MUST be able to configure language pairs, caution level, AI provider, exclusions,
  feedback options, and the flip hotkey.
- **FR-027**: The tray indicator MUST show whether the tool is working, degraded, or stopped, with a
  readable reason.

**Data and resilience**

- **FR-028**: The system MUST keep user-added words separate from shipped dictionary data so updates
  never discard the user's own additions.
- **FR-029**: The system MUST record its own data files with a version marker and refuse an unrecognized
  version with a clear message rather than misreading it.
- **FR-030**: The system MUST restore keystroke observation automatically if it is lost, and inform the
  user only when restoration repeatedly fails.
- **FR-031**: The system MUST provide an optional local diagnostic record, off by default, that contains
  no typed text; a separately opted-in verbose mode MAY include text and MUST be session-scoped and
  self-deleting.
- **FR-032**: Writing diagnostic data MUST NOT compete with correction performance.
- **FR-033**: The system MUST NOT transmit any usage or diagnostic information unless the user has
  explicitly opted in, MUST show exactly what would be sent before the first transmission, and MUST
  offer the usage and diagnostic channels as separate choices.

### Key Entities

- **Typing record**: the transient in-memory account of recent keystrokes and words, each with the
  layout that produced it, its position, and whether it has been corrected. Never persisted.
- **Correction**: a proposed change covering a span of the typing record — the text as typed, the text
  intended, the target layout, the confidence, and which tier produced it.
- **Layout pair**: the mapping between two keyboard layouts that makes translation possible. Shipped as
  data; extending the tool to a new pair means adding a pair, not changing behavior.
- **Dictionary**: the word data for one language, in two parts — the shipped set and the user's own
  additions and never-correct entries, kept separate and user-inspectable.
- **Settings**: the user's configuration, including encrypted provider credentials.
- **Application exclusion**: an application the user has designated as off-limits for capture and
  correction.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Fewer than 1 in 1,000 applied corrections change text that was already correct, measured
  over sustained daily use.
- **SC-002**: Users reverse fewer than 2% of applied corrections.
- **SC-003**: A dictionary-tier correction completes within 50 milliseconds of the word ending in 95% of
  cases, so the user perceives it as instantaneous.
- **SC-004**: Typing latency is indistinguishable from having the tool switched off.
- **SC-005**: An AI-tier correction arrives within half a second in the typical case and never blocks
  typing regardless of provider behavior.
- **SC-006**: Multi-word wrong-layout runs, including those where typing continued during correction,
  end in fully correct text with no lost, duplicated, or double-translated characters.
- **SC-007**: No file the tool writes and no data it transmits contains typed text, unless the user has
  explicitly enabled the verbose diagnostic mode.
- **SC-008**: A first-time user gets a working correction within five minutes of install without
  configuring anything.
- **SC-009**: The tool installs and runs on a standard Windows machine without security warnings that
  would deter an ordinary user.
- **SC-010**: The tool remains working across a full day of normal use, including sleep, lock, and
  application restarts, without user intervention.
- **SC-011**: Adding a new language pair requires only new data, verified by adding one without changing
  correction behavior.
- **SC-012**: With three or more layouts installed, corrections target the right language as reliably as
  with two, and ambiguous cases leave text untouched rather than guessing.

## Clarifications

### Session 2026-08-19 (specify boundary)

- **Q: Does v1 ship only the Hebrew↔English pair's data, or several pairs the user can choose from?**
  A: Ship support for multiple languages. The number of installed layouts decides the work: with exactly
  two, the intended layout is known by elimination; with more than two, the system must determine which
  language the gibberish was intended for. Captured as FR-005, FR-005a, SC-012 and the revised
  multi-layout edge case.
- **Q: Is the AI tier really in the MVP given its requirement count?** A: Yes — it stays in v1. It is the
  product's edge and carries the Azure AI Foundry positioning. No spec change; FR-016 through FR-021
  remain in scope.
- **Q: Should branch protection be applied to the repository now?** A: Yes. Applied to `main` on
  2026-08-19: direct pushes blocked, pull request required, zero required approvals, force-push and
  branch deletion disabled, conversation resolution required, admins included. Required status checks
  remain empty until the CI lane exists. Not a spec change; recorded in the devops workshop record.

## Assumptions

- The user has at least two keyboard layouts installed and switches between them; Hebrew and English is
  the first pair shipped and proven, with the design remaining pair-agnostic and multiple pairs
  supported from v1.
- Languages requiring composition-based input methods are out of scope for this release.
- The false-correction and reversal targets in SC-001 and SC-002 are design targets to be validated by
  the maintainer's daily use before release, not measurements of an existing system.
- The tool runs with ordinary user privileges; elevated windows are therefore outside its reach, which
  is the intended behavior.
- Users who enable the AI tier accept that one sentence of context leaves their machine for their chosen
  provider, and are shown exactly what is sent before it happens.
- Distribution is through a public repository release; the maintainer bears no inference costs because
  users supply their own AI access.
- Telemetry infrastructure is out of scope for this release; the user-facing consent and controls ship
  with the feature while the receiving service comes later.
- A concurrent third-party tool performing the same layout correction is not accounted for and would
  conflict.
