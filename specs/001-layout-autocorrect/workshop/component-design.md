# Component-Design Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: component-design (full depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (map + flow rendered in-band; human replied "Agree")

## Agreed component map (strict IDesign)

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
                                                  │ Llm, Audio │
                                                  └────────────┘
                          (all arrows point downward; accessors publish events upward)
```

## Components and responsibilities

**Clients:**

- `TrayClient` — tray icon, enable/disable, status, hosts the settings window
- `OverlayClient` — the floating bubble near the caret: correction text + new language state

**Managers:**

- `CorrectionManager` — the typing flow: subscribes to keystroke/focus events, drives the engines,
  decides detect→correct, executes the correction transaction, publishes feedback events, owns the
  privacy lifecycle (suspend on password fields, wipe on focus change)
- `SettingsManager` — the configuration flow: profiles, language pairs, conservatism level, per-app
  exclusions, BYOK credentials; notifies CorrectionManager of changes

**Engines** (algorithms + state; callback-only, know no managers):

- `WordAssemblyEngine` — collects keystrokes, recognizes end-of-word, returns the word via callback
- `TranscriptEngine` — rolling journal: layout provenance, suspect-span widening, trailing remap
  (the "applied twice" race case), correction transactions, epoch marks, privacy wipe
- `MappingEngine` — pure layout translation over data-driven key maps: text-as-typed → text-as-intended
- `DetectionEngine` — the verdict algorithm: scores original vs mapped candidate against dictionary
  data, conservative confidence threshold, returns detect / correct / ignore

**ResourceAccessors** (external world only; call nothing in the system):

- `KeystrokeAccessor` — wraps WH_KEYBOARD_LL on its own thread; publishes key events; native-swap-ready
- `FocusAccessor` — foreground app/control changes, password-field detection (UIA), caret coordinates
- `InputInjectionAccessor` — SendInput backspaces + replacement text; tags self-injected events
- `LayoutAccessor` — reads the active keyboard layout; switches it
- `DictionaryAccessor` — loads/queries dictionary + key-map data files; persists learned words
- `LlmAccessor` — the MAF/Foundry agent call: single-sentence context in, corrected span out (BYOK/local)
- `AudioAccessor` — plays the tiered feedback sounds
- `SettingsAccessor` — persists settings; DPAPI-encrypts BYOK keys at rest

## Agreed design notes

- **Composition (human amendment, captured during the requirements-nfr turn)**: components are
  .NET in-process services composed via an IoC container with deliberate lifetime management
  (probably singletons for the long-lived managers/engines/accessors); every component has its own
  interface; unit/integration tests mock dependencies through those interfaces.

- **Engine purity**: engines make NO accessor calls — the manager hands data in (e.g., dictionary
  data to DetectionEngine). Engines are unit-testable with zero mocks. (Stricter than classic
  IDesign, which permits engine→accessor; agreed deliberately.)
- **Extension points**: a new language pair is data only (key map + dictionary files) — no new code;
  new detection tiers and AI providers are DI registrations behind the existing contracts.

## Agreed key flow — seamless correction including the race case

```text
  user types "akuo " meaning "שלום"
  -> KeystrokeAccessor publishes events -> CorrectionManager -> WordAssemblyEngine (collects)
  -> end-of-word callback -> CorrectionManager -> TranscriptEngine.append + suspect-span query
  -> MappingEngine (candidate: "שלום") -> DetectionEngine (verdict: wrong-layout, high confidence)
  -> CorrectionManager opens correction transaction -> TranscriptEngine computes span + keystrokes
     typed since detection (remapped too) -> InputInjectionAccessor (backspaces + inject, tagged)
  -> LayoutAccessor (switch to Hebrew) -> TranscriptEngine marks epoch
  -> CorrectionManager publishes feedback event -> OverlayClient bubble + AudioAccessor sound
```
