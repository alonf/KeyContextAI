# Architecture-Core Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: architecture-core (full depth)
**Conducted**: 2026-08-19, one decision at a time (human chose pacing option 2)
**Confirmation**: human-confirmed / lens-question (typed replies for all 5 decisions + state-model closure)

## Decisions

### 1. Design method: strict IDesign (volatility-based, role call-rules as law)

The human chose IDesign volatility-based decomposition, then sharpened it to **strict IDesign** —
the role call-rules are binding law, enforced in review and by an architecture test in CI:

- **Accessors** decouple the system from the external world. They call **no** component in the
  system. The KeystrokeAccessor *publishes* key events; the manager subscribes.
- **Engines** own the algorithms — the most stable components. Engines call no engines and no
  managers. An engine returns results via callback and never knows the manager.
- **Managers** hold the flow logic. Managers call no managers, but call engines and accessors.

Canonical flow (human-authored): the accessor gets keystrokes and publishes them; the manager
listens and submits each keystroke to a word-assembly engine that collects them, recognizes
end-of-word, and returns the word to the manager in a callback; the manager passes the word to a
detection engine (dictionary-based — pure algorithm) and to an **LLM accessor** (LLM detection is
external-world contact, therefore an accessor, not an engine).

Rejected alternatives: clean/layered architecture; pragmatic modular monolith; modular monolith
with IDesign vocabulary but advisory (non-binding) call rules.

### 2. Process topology: single tray-app process

One user-session process containing the hook thread, async analysis pipeline, serialized
correction executor, and UI. Rejected: two-process hook/app split (IPC cost, two AV surfaces,
two lifecycles — recorded as the future cut line if AV or stability ever force it, along the hook
subsystem's contract); app + Windows Service (session-0 isolation makes the service useless for
the core loop).

### 3. Hook isolation: managed first, native swap-ready

Managed P/Invoke `WH_KEYBOARD_LL` on a dedicated message-pumping thread, callback allocation-free
and enqueue-only. The boundary (keystroke-source accessor contract) is shaped so a native C++
helper DLL can replace it without touching anything above. **Recorded swap triggers**: measured
callback-latency violations, or antivirus false-positive reports from the field.

### 4. Threading / pipeline: .NET Channels + serialized correction executor

Hook callback writes to a `Channel<KeyEvent>` and returns. Async consumers run word assembly →
dictionary detection → (on miss) LLM detection. A single serialized correction executor owns the
correction transaction (backspaces + `SendInput` + layout switch as one atomic sequence) and
suppresses self-injected events from re-entering the pipeline. Rejected: TPL Dataflow (heavier,
parallelism knobs unwanted on an ordered single-source stream); Rx (paradigm tax, overkill).

### 5. Volatility isolation, state ownership, and out-of-scope

Volatilities encapsulated behind stable contracts:

1. **Detection strategy** — dictionary and LLM tiers interchangeable behind one detection contract.
2. **AI backend** — the Microsoft Agent Framework agent behind its own accessor boundary; provider
   changes (Azure OpenAI / Claude / local / installed-assistant discovery) never touch detection logic.
3. **Layout knowledge as data** — keyboard-pair mappings and dictionaries are data artifacts;
   adding a language pair ships data, not code.
4. **Feedback channels** — sound and overlay behind a notification contract.
5. **Keystroke source** — decision 3.

**State ownership (human-raised, agreed):**

- Accessors are stateless beyond resource handles.
- **WordAssemblyEngine** holds only the current word in progress.
- **TranscriptEngine** (new component demanded by the multi-word concern) owns the rolling journal
  of recent keystrokes/words with layout provenance, positions, and correction status. It answers:
  which span is suspect (maximal consecutive wrong-layout span, so multi-word gibberish corrects as
  one transaction); what arrived after detection fired (computes the trailing remap — the "mapping
  applied twice" case); what a correction transaction contains (backspace count + replacement for
  the whole span). Pure algorithm + state; no external calls.
- **Manager holds only flow state** — enabled/suspended, correction-in-flight — and commands the
  transcript wipe on focus change or password field (privacy lifecycle is a manager decision, the
  wipe is an engine operation).

**Multi-word / repeated translation (agreed):** detection operates on a sliding window over the
transcript; a failing word widens to the maximal consecutive suspect span; characters typed between
detection and the layout switch get the same remap as a second pass; after the switch, new
keystrokes are correct-by-construction and the journal marks the epoch boundary.

**Out of scope this iteration:** Windows Service component; IME/composition languages; dictionary
cloud sync; installed-assistant discovery (research item only); language pairs beyond Hebrew↔English.

## Keeper diagram — process topology and pipeline

```text
┌────────────── KeyContext AI — single tray-app process (user session) ─────────────┐
│                                                                                   │
│  Hook thread (msg pump)       Analysis pipeline (async)      UI thread (overlay)  │
│  ┌────────────────────┐      ┌─────────────────────────┐     ┌────────────────┐   │
│  │ WH_KEYBOARD_LL     │      │ Word assembly           │     │ Tray, bubble,  │   │
│  │ callback: enqueue  │─────▶│  ▶ Dictionary detection │────▶│ sounds         │   │
│  │ only, return fast  │      │  ▶ (miss) LLM detection │     └────────────────┘   │
│  └────────────────────┘      │    (MAF agent, async)   │                          │
│        ▲                     └───────────┬─────────────┘                          │
│        │ keystrokes (all apps)           ▼                                        │
│        │                     ┌─────────────────────────┐                          │
│        └─────────────────────│ Correction executor     │                          │
│          (journal feedback)  │ serialized: backspaces, │                          │
│                              │ SendInput, layout switch│                          │
│                              └─────────────────────────┘                          │
└───────────────────────────────────────────────────────────────────────────────────┘
```

(Component naming in this diagram is pre-map shorthand; the full named IDesign component map is
co-designed in the component-design lens.)
