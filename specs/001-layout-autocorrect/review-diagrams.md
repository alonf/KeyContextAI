# Review Diagrams: KeyContext AI — Keyboard Layout Auto-Correction

**Feature**: 001-layout-autocorrect
**Phase**: pre-implementation (planning artifact for the reviewer)

These diagrams show what iteration 001 will build, before it exists. The console-ASCII versions are
rendered inline so they are readable in a terminal; the Mermaid versions carry the same content for
hosts that render it.

## Component diagram

```text
  CLIENTS          ┌────────────┐  ┌───────────────┐
                   │ TrayClient │  │ OverlayClient │
                   └─────┬──────┘  └───────▲───────┘
                         │ calls managers  │ feedback events
  MANAGERS         ┌─────▼──────────────┐ ┌┴──────────────────┐
                   │ SettingsManager    │ │ CorrectionManager │◀─┐
                   └─────┬──────────────┘ └┬────────┬─────────┘  │ key/focus
                         │                 │ calls  │ calls      │ events
  ENGINES                │        ┌────────▼──────┐ │            │ (published,
                         │        │ WordAssembly  │ │            │  never called
                         │        │ Transcript    │ │            │  upward)
                         │        │ Mapping       │ │            │
                         │        │ Detection     │ │            │
                         │        └───────┬───────┘ │            │
  ACCESSORS        ┌─────▼─────┐  ┌───────▼─────┐ ┌─▼──────────┐ │
                   │ Settings  │  │ Dictionary  │ │ Keystroke ─┼─┘
                   │ Accessor  │  │ Accessor    │ │ Focus      │
                   └───────────┘  └─────────────┘ │ Injection  │
                                                  │ Layout     │
                                                  │ Audio      │
                                                  └────────────┘
```

```mermaid
flowchart TD
  Tray[TrayClient] --> SM[SettingsManager]
  Overlay[OverlayClient]
  CM[CorrectionManager] -.feedback events.-> Overlay
  SM --> SA[SettingsAccessor]
  CM --> WA[WordAssemblyEngine]
  CM --> TE[TranscriptEngine]
  CM --> ME[MappingEngine]
  CM --> DE[DetectionEngine]
  CM --> KA[KeystrokeAccessor]
  CM --> FA[FocusAccessor]
  CM --> IA[InputInjectionAccessor]
  CM --> LA[LayoutAccessor]
  CM --> DA[DictionaryAccessor]
  CM --> AA[AudioAccessor]
  KA -.publishes key events.-> CM
  FA -.publishes focus events.-> CM
```

The dotted edges are the only upward flow in the system, and they are events, not calls — an accessor
publishes and the manager subscribes. Engines have no outgoing edges at all: `CorrectionManager` hands
them data and receives results.

## Sequence: the canonical correction (word completed by space)

```mermaid
sequenceDiagram
  participant U as User
  participant KA as KeystrokeAccessor
  participant CM as CorrectionManager
  participant WA as WordAssemblyEngine
  participant TE as TranscriptEngine
  participant ME as MappingEngine
  participant DE as DetectionEngine
  participant IA as InputInjectionAccessor
  participant LA as LayoutAccessor

  U->>KA: types "akuo" then space
  KA-->>CM: KeyEvent per keystroke (channel)
  CM->>WA: Append(key)
  WA-->>CM: WordCompleted("akuo")
  CM->>TE: Append(entry)
  CM->>ME: Translate(scanCodes, en-US, [he-IL])
  ME-->>CM: candidates ["akuo", "שלום"]
  CM->>DE: Evaluate(candidates, dictionaryData, caution)
  DE-->>CM: Verdict(Correct, "שלום", he-IL, 0.97)
  CM->>TE: ComputeTransaction(entryId, null)
  TE-->>CM: Transaction(backspaces=5, text="שלום ")
  CM->>IA: ApplyCorrection(tx)
  IA-->>CM: Succeeded
  CM->>LA: Switch(he-IL)
  CM->>TE: MarkEpoch()
  CM-->>U: bubble + sound
```

## Sequence: the committing-key path (Option B — the approved design)

This is the sequence that most needs review, because it is the only place the tool withholds a
keystroke from the user's application.

```mermaid
sequenceDiagram
  participant U as User
  participant KA as KeystrokeAccessor
  participant CM as CorrectionManager
  participant TE as TranscriptEngine
  participant IA as InputInjectionAccessor
  participant LA as LayoutAccessor
  participant App as Target application

  Note over CM,TE: while typing, off the hook thread
  CM->>TE: SetVerdict(entry, verdict)
  CM->>KA: Arm(token)

  U->>KA: presses Enter
  Note over KA: inside the callback: O(1) flag read
  alt not armed
    KA-->>App: return 0 — Enter passes through untouched
  else armed
    KA-->>KA: return 1 — SUPPRESS, post to channel, return
    KA-->>CM: SuppressedKey(Enter)
    CM->>TE: ComputeTransaction(entryId, suppressedKey)
    TE-->>CM: Transaction(+ SuppressedKey)
    CM->>IA: ApplyCorrection(tx)
    alt success
      IA->>App: backspaces + corrected text
      CM->>LA: Switch(targetLayout)
      IA->>App: re-inject Enter
      CM->>TE: MarkEpoch()
    else failure or focus changed
      IA->>App: re-inject Enter ALONE — nothing else changes
    end
  end
```

**The property the reviewer should check the implementation against**: every path that leaves the
`armed` branch delivers the Enter to the application. There is no path where a suppressed key is
dropped — that is Phase 2 hardening target 1 and the most important test in iteration 001.

## Sequence: privacy lifecycle

```mermaid
sequenceDiagram
  participant U as User
  participant FA as FocusAccessor
  participant CM as CorrectionManager
  participant TE as TranscriptEngine
  participant WA as WordAssemblyEngine

  U->>FA: clicks into a password box
  FA-->>CM: FocusChanged(context)
  CM->>FA: IsPasswordContext()
  alt Yes or Unknown
    FA-->>CM: Yes / Unknown
    CM->>TE: Wipe()
    CM->>WA: Reset()
    Note over CM: capture suspended — fail closed
  else No
    FA-->>CM: No
    CM->>TE: Wipe()
    Note over CM: new window, fresh transcript — text never crosses apps
  end
```

Note that the transcript is wiped on **every** focus change, not only on password focus. That is what
stops text from one application ever influencing a correction in another.
