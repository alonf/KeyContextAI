# Observability-Resilience Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: observability-resilience (light depth, expanded by human input)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (failure-mode map rendered in-band; the human
replaced the Crew's "no telemetry ever" proposal with opt-in telemetry and answered both open
questions)

## Agreed failure modes and user-visible behavior

```text
  Hook dies (OS drop, session lock, app crash)
    └─▶ watchdog notices within 2s ─▶ re-register ─▶ tray stays green
        └─▶ 3 failures in a row ─▶ tray RED "corrections stopped — click to restart"

  Focus/UIA unavailable (unresponsive app)
    └─▶ fail-closed: suspend capture ─▶ silent (no correction, no error noise)

  LLM slow / down / 429 / no network
    └─▶ 2s timeout ─▶ verdict "leave" ─▶ dictionary tier keeps working
        └─▶ circuit opens ─▶ tray AMBER "AI tier offline" ─▶ half-open retry later

  Injection rejected by target app (games, RDP, secured input)
    └─▶ correction abandoned, transcript marked ─▶ 3 strikes in one app
        └─▶ tray suggests "exclude this app?"

  Dictionary/settings file corrupt or wrong schema_version
    └─▶ refuse to load that file, log it, run without it ─▶ tray AMBER + reason
```

## Agreed decisions

1. **The tray icon is the primary observability surface** — green/amber/red with a one-line reason on
   hover. The user's question is always "is it working?" and the answer belongs where they can see it
   without opening anything.
2. **Local diagnostic log — in-memory ring buffer with quiet-period flush (human design).** Events
   accumulate in memory and are written to file only during idle periods (no keystroke for ~2s), so
   file I/O never competes with the correction hot path — and losing the tail of a log on a crash is
   the correct thing to sacrifice. Off by default.
   - **Standard mode**: events and outcomes only, never text — "detection fired, dictionary verdict,
     4ms", "LLM timeout after 2003ms", "injection abandoned: focus changed". A log that cannot leak
     is a log users will actually attach to issues.
   - **Verbose bug-report mode (human-approved)**: explicitly opt-in with a warning, session-scoped
     and self-deleting; MAY include the actual text, because debugging a bad correction without the
     words is nearly impossible.
   - **Research instrument for the maintainer**: the log is designed to answer performance,
     correctness, memory use, LLM invocation rate, double-correction count, and — the human's
     specific interest — **correction quality as a function of typing speed** (fast typists produce
     longer suspect spans and more mid-correction races).
3. **In-memory counters visible in settings** — corrections applied, flip-backs, LLM calls, timeouts
   this session. This is how the <1/1000 false-correction target gets validated in dogfood.
4. **Telemetry: none by default; two separately-toggleable opt-in channels (human decision, replacing
   the Crew's no-telemetry proposal).**
   - **Session ping** — counts unique installs. Payload, documented verbatim in the README:

     ```text
     POST /api/session  (opt-in only)
     {
       "install_id": "b3f1…",     random GUID generated at install, resettable in settings
       "version": "1.2.0",
       "os": "Windows 11",
       "pairs": ["en-US↔he-IL"],  which language pairs are active
       "ai_tier": "copilot-cli" | "azure-openai" | "local" | "off"
     }
     ```

     No username, machine name, app names, or text — ever. "Unique users" is honestly "unique
     installs" because the ID is random and user-resettable.
   - **Diagnostics channel** — aggregate p50/p95 detection latency, LLM timeout rate,
     injection-abandon rate, hook-restart count. No text. Validates the NFR targets in the field
     rather than only on the maintainer's machine.
   - **Transport**: a single HTTPS POST to an **Azure Function**, with Application Insights behind it
     for aggregation. IoT Hub and MQTT/Event Grid were considered and rejected: they are built for
     device fleets with per-device identity, high message rates, and bidirectional command channels,
     and would add a device-registration concept that is precisely what privacy-minded users should
     not have to be reassured about. Event Grid can sit behind the Function later without changing
     the client.
   - **Binding privacy rules**: the endpoint must NOT log or retain IP addresses (personal data under
     GDPR; the user base is heavily EU/Israel), and consent is a **first-run explicit ask** showing
     the exact JSON that would be sent — never a pre-checked box in a settings page.
   - **Scope**: the backend is **post-MVP**. The client-side plumbing and consent UI ship in v1 with
     the switches present and off, so no Azure service is built before there are users.
5. **Recovery patterns** — hook watchdog with re-registration (2s), Polly circuit breaker for the AI
   tier with automatic half-open recovery, and atomic correction transactions so a mid-flight failure
   leaves the text unchanged rather than half-corrected.
