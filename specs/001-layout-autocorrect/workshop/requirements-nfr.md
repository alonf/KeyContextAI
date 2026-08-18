# Requirements-NFR Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: requirements-nfr (medium depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (table rendered in-band; human replied "Agree")

## Agreed quality-attribute priorities and thresholds

```text
 #  Attribute           Threshold (measurable)                                  Driver?
 ─  ─────────────────   ─────────────────────────────────────────────────────   ───────
 1  Correction safety   False-correction rate: <1 per 1,000 corrections in      YES — drives
    (trust)             dogfood; user-undo rate <2% of applied corrections;     conservative
                        a missed correction is never counted as a defect        thresholds, undo
                        of the same severity                                    design
 2  Felt latency        Hook callback <1ms p99 (allocation-free);               YES — drives
                        dictionary verdict <10ms from end-of-word;              pipeline design
                        full dictionary correction transaction <50ms p95;
                        LLM tier: 500ms target, 2s hard timeout -> tier
                        degrades to detection-only sound, never blocks
 3  Privacy             In-memory-only keystrokes; 1-sentence cloud context     YES — but owned
                        max; suspend on password fields                         by security lens
 4  Reliability         Hook survives indefinitely; auto-recover a dropped      YES — drives
                        hook within 2s; a component crash never loses           supervision
                        user-typed text or leaves a half-applied correction     design
 5  Resource footprint  Idle CPU ~0% (event-driven, no polling);                moderate
                        added typing latency not perceivable (<1ms
                        passthrough); working set target <150MB
 6  Compatibility       Corrections work in mainstream edit surfaces            moderate — the
                        (browsers, Office, editors, chat apps); apps where      per-app exclusion
                        injection misbehaves are excludable per-app             list is the valve
```

## The system refuses to

- Correct in password fields.
- Act in excluded apps.
- Persist any keystroke.
- Send more than one sentence to a cloud LLM.
- Apply a correction when confidence is below the user-chosen conservatism level.

## Honest flags (assumed until verified)

- The false-correction numbers are `assumed` until dogfood — the bar is designed toward and
  verified by the maintainer's daily use before release.
- The <50 ms correction transaction is ambitious with `SendInput` backspace bursts in slow apps
  (remote desktop, Electron). The per-app exclusion list is the escape valve, and acceptance tests
  must include a slow-app case, not just Notepad.

## Cross-lens amendment captured in this turn (extends component-design)

Components are .NET in-process services composed via an IoC container with deliberate lifetime
management (probably singletons for the long-lived managers/engines/accessors); **every component
has its own interface**; unit and integration tests mock dependencies through those interfaces
(managers tested with mocked engines/accessors; engines remain pure and mock-free).
