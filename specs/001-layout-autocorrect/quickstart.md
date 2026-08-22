# Quickstart: KeyContext AI — Keyboard Layout Auto-Correction

**Feature**: 001-layout-autocorrect
**Last verified**: not yet — this document is written before implementation and is the script the
reviewer follows once iteration 001 lands.

## Run it

```powershell
# From the repository root
dotnet build
dotnet test                                    # unit + architecture tests, no desktop needed
dotnet run --project src/KeyContextAI.App      # starts the tray app
```

The tray icon appears with a green dot. No configuration is required — the Hebrew↔English pack ships
with the application and the dictionary tier works immediately (SC-008).

## Try the canonical scenario

You need Hebrew and English keyboard layouts installed. Open Notepad.

1. **Make sure English is the active layout.** The tray tooltip shows the active pair.
2. **Type `akuo` and press space.** Those are the keys under the Hebrew letters of `שלום`.
   *Expected*: the text becomes `שלום`, the keyboard layout switches to Hebrew, a soft tick plays, and
   a small bubble appears near the caret reading `akuo → שלום   EN ▸ HE`. The bubble fades after about a
   second and never takes focus.
3. **Type a normal English word and press space.**
   *Expected*: nothing happens — no change, no sound, no bubble. A correction you did not need is worse
   than a correction you did not get (SC-001a).
4. **Double-tap Ctrl within about a third of a second.**
   *Expected*: the correction from step 2 reverts to `akuo`, and that word is not corrected again for
   the rest of the session.

## Verify the edge cases

**The chat-send case — the one Option B exists for.** Open any application that sends on Enter (a chat
window, or a search box). Type a wrong-layout word and press **Enter** without pressing space first.
*Expected*: the text is corrected and then sent — the Enter is briefly withheld, the correction applied,
and the Enter re-injected. *If the correction cannot complete*, the message sends exactly as you typed
it. It must never send half-corrected, and the Enter must never be lost (Phase 2 hardening target 1).

**Multi-word runs with typing continuing.** Type three consecutive wrong-layout words quickly and keep
typing through the correction.
*Expected*: the whole run corrects as one action, and the characters typed during the correction are
correct too — no lost, duplicated, or double-translated characters (SC-006).

**The password field.** Focus any password box and type.
*Expected*: nothing is captured or corrected. Then check the diagnostic log with standard mode enabled —
it must contain no typed text at all (SC-007).

**Focus change mid-correction.** Type a wrong-layout word, press space, and immediately click into a
different window.
*Expected*: the correction is abandoned. Nothing is injected into either window.

**A word valid in both layouts.**
*Expected*: left alone. When both candidates are plausible, the tool does nothing.

**Three or more layouts installed.** Add a third layout and repeat step 2.
*Expected*: the correction still targets the right language; where two candidates are equally plausible
the text is left untouched (SC-012).

## What to check in the diagnostic log

Enable standard diagnostic mode in settings, type for a few minutes, then open the log.

*Expected*: entries record events and timings — detection fired, verdict, elapsed milliseconds,
injection outcome — and contain **no typed words**, no window titles, and no application names. Confirm
the flush timestamps cluster in idle gaps rather than during typing bursts, which is what keeps logging
off the correction path (FR-032).
