# Security-Compliance Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: security-compliance (medium depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (trust-boundary diagram rendered in-band; human
agreed and sharpened the opt-in rule for discovered assistants)

## Threat framing

KeyContext AI is, mechanically, a keylogger with a text injector and a network connection. The only
thing distinguishing it from malware is discipline designed in and provable by anyone reading the
source. Verifiable claims are the security model.

## Agreed trust boundaries and attack surface

```text
  ┌─── EVERY APP THE USER TYPES IN (banks, password managers, IDEs) ───┐
  │  keystrokes ──▼── injected text                                    │
  └───────────────┼─────────────────────────────────────────────────────┘
        BOUNDARY 1 │ capture + injection  ← highest-consequence boundary
  ┌───────────────▼─────────────────────────────────────────────────────┐
  │  KeyContext AI process (user privilege, NOT elevated)               │
  │                                                                     │
  │   KeystrokeAccessor ─▶ Transcript (RAM only) ─▶ Detection           │
  │        ▲                      │                     │               │
  │   FocusAccessor: password-field / excluded-app gate  │               │
  │                               │                     │               │
  │   SettingsAccessor ─ DPAPI ──▶│                     │               │
  └───────────────────────────────┼─────────────────────┼───────────────┘
                    BOUNDARY 2    │ disk        BOUNDARY 3 │ network (TLS)
                  ┌───────────────▼──────┐    ┌────────────▼─────────────┐
                  │ %LOCALAPPDATA% files │    │ LLM provider (BYOK/local)│
                  │ dictionaries,        │    │ ONE sentence, no ID,     │
                  │ settings, DPAPI key  │    │ no retention, opt-in     │
                  └──────────────────────┘    └──────────────────────────┘
```

## Agreed controls

1. **Boundary 1 — capture.** The password gate is **fail-closed**: if `FocusAccessor` cannot
   determine whether a control is a password field (UIA unavailable, unresponsive app, unknown
   control), capture suspends rather than guesses. The app runs **unelevated**, so elevated windows
   are invisible to it — the correct behavior. The transcript wipes on every focus change, so text
   never crosses between applications.
2. **Boundary 1 — injection.** Correction is injected only into the window that produced the
   keystrokes being corrected. If focus changed between detection and injection, the correction is
   **abandoned**, not applied — a security control preventing our text landing in a different app,
   not merely a correctness nicety.
3. **Boundary 2 — disk.** DPAPI `CurrentUser` scope for API keys, so another local user account
   cannot decrypt them; no keystrokes, no correction history, no window titles written to disk, ever.
4. **Boundary 3 — network.** The AI tier is **opt-in and off by default**. What leaves the machine
   is one sentence of context with no user identifier, no app name, no window title. TLS only; a
   provider without TLS is unsupported. Local models (Ollama/ONNX) close this boundary entirely.
   **Opt-in definition (human-sharpened):** the opt-in is either the user configuring a provider/key
   OR the user answering yes when we detect an installed Claude/Copilot CLI or desktop app and ASK
   whether to use it. Detection alone never activates anything — silent reuse of a credential the
   user did not point at us is forbidden. A one-time confirmation before the first cloud call states
   exactly what will be sent.
5. **Supply chain and the malware-resemblance problem.** Minimal dependency count with pinned
   versions (every dependency is keystroke-adjacent risk); reproducible builds from public CI so the
   published binary matches the public source; `SECURITY.md` plus a plain-language "what this tool
   can and cannot see" section in the README.
