# UI-UX Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: ui-ux (medium depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (layouts rendered in-band; human agreed and raised
the Ctrl+Z conflict, which was resolved in a second exchange and agreed)
**UX source of truth**: none — no Figma, sketch, or screenshot exists; the human explicitly delegated
the visual design to the Crew's proposal, which is captured here as the agreed baseline.

## Agreed correction bubble

```text
 Text the user is typing in some other app
 ┌──────────────────────────────────────────────────┐
 │  Hi Dana, akuo| ...                              │
 │                └─ caret                          │
 │      ╭───────────────────────────────╮           │
 │      │  akuo → שלום      EN ▸ HE  ⎌  │  ← bubble │
 │      ╰───────────────────────────────╯           │
 └──────────────────────────────────────────────────┘
   what changed ─┘         new layout ─┘  undo hint ─┘
```

Behavior: click-through (never steals focus or blocks text), auto-fades after ~1.2s, positioned via
UIA caret coordinates with a screen-edge fallback to the cursor position, fully suppressible in
settings for silent operation.

## Agreed tray surface

```text
 ╭──────────────────────────────╮
 │ ● KeyContext AI — Active     │   ● green: active   ◐ amber: LLM offline
 │──────────────────────────────│   ○ grey: paused
 │ Pause corrections            │
 │ Mode ▸  Correct / Notify only│
 │ Exclude "Visual Studio"      │  ← current foreground app, one click
 │──────────────────────────────│
 │ Settings…                    │
 │ Quit                         │
 ╰──────────────────────────────╯
```

## Agreed decisions

1. **Bubble** — as rendered above.
2. **Sound design** — three distinct short cues: soft *tick* for an applied correction, subtler
   *double-tick* for a detection in notify-only mode, and a different tone when the correction came
   from the AI tier rather than the dictionary (the user learns to hear which engine acted). All
   individually mutable.
3. **Flip (undo) hotkey — resolved conflict.** Ctrl+Z was rejected: swallowing it destroys the host
   app's undo, and not swallowing it makes the host app undo at its own granularity (our injected
   text may merge with the user's typing into one undo unit), so Ctrl+Z is unreliable as our
   mechanism. **Agreed: double-tap `Ctrl` (two taps within ~300ms) as the default flip key**, with
   `Ctrl+Alt+Z` as a configurable alternative and the hotkey user-changeable in settings. It is a
   *flip*, not an undo: press once to restore what was typed, press again to re-apply the
   correction. Safety rules: the flip is armed only for a short window (~5s) after a correction and
   while that correction is still the most recent edit; flipping back marks the word "do not correct
   again this session," feeding the conservatism logic. Rejected candidates: `Ctrl+Shift+Z` (redo in
   most editors), `Pause/Break` (missing on many laptops), `Win+Z` (Snap Layouts).
   Additionally: corrections are injected as a **single burst** so a host app's own Ctrl+Z treats
   them as one unit — a decent fallback for users who hit it instinctively.
4. **Settings window** — small WinUI/WPF window opened from the tray (not resident): language pairs,
   conservatism level (aggressive / balanced / conservative), AI provider + key (BYOK), per-app
   exclusions, sound and bubble toggles.
5. **Accessibility & RTL** — the bubble renders Hebrew/Arabic RTL text correctly (a correctness
   requirement given the target audience), respects Windows light/dark themes, honors "reduce
   motion" by skipping the fade, and is never the sole channel — the sound carries the same
   information for low-vision users.
