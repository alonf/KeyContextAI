# Code-Implementation Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: code-implementation (medium depth; auto-on for a code-writing feature)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question — the human replied "I agree with the default"
to the presented forks and named Copilot as the reviewer.

## Source of code-rules truth

**None provided.** No existing coding guideline and no example project to emulate; Specrew defaults
plus the decisions below apply. (Read from the human's "agree with the default" reply and surfaced
to them explicitly as a filled gap so it can be corrected.)

## Design-time to implement-time flow

```text
  DESIGN TIME (now)                          IMPLEMENT TIME (later)
  ┌──────────────────────────┐               ┌───────────────────────────┐
  │ baseline craft defaults  │               │ specrew-code-rules skill  │
  │   (on unless excepted)   │──┐            │  reads the manifest and   │
  │ decision-prompt forks    │  ├─▶ implementation-rules.yml ─▶ guides   │
  │   (human's calls below)  │  │            │  the coding agent per task│
  │ .NET stack posture       │──┘            └───────────────────────────┘
  │ dependency policy        │
  │ reviewer selection       │
  └──────────────────────────┘
```

## Baseline craft posture

All baseline rules ON with **no exceptions requested**: intent-revealing names, short functions,
shallow nesting, DI, DTOs across boundaries, immutability intent, comments only where they earn
their place, no magic numbers, strong domain types, object invariants, no leaky mutable internals,
normalized state, simple trustworthy tests, SOLID, secure-coding defaults, language-native
constructs.

## Agreed consequential forks

1. **Concurrency** — Channels pipeline (bound in architecture-core); the correction executor is a
   **single consumer loop**, serialized by construction rather than by explicit locking.
2. **Error handling** — exceptions for exceptional cases only; **Result-style returns for expected
   failures** (LLM unavailable, injection refused, detection inconclusive). Expected outcomes on a
   hot path should neither cost exception machinery nor be misreported as faults.
3. **Testing posture** — **test-first for the pure engines** (Mapping, Detection, Transcript,
   WordAssembly) where correctness is the product and no mocks are needed; **test-after for
   accessors** (Win32 wrappers verified by integration and manual smoke); a **mandatory architecture
   test in CI enforcing the IDesign call rules**.
4. **.NET posture** — **.NET 10 LTS**, `LangVersion latest`, nullable reference types on,
   warnings-as-errors, file-scoped namespaces, records for messages and DTOs, analyzers on.
5. **Polymorphism** — interfaces plus DI throughout; **Strategy pattern for detection tiers** rather
   than conditional chains; no behavior-variation inheritance hierarchies.
6. **Packaging of the engine** — **app-internal library with clean seams; NuGet extraction
   deferred.** The Crew had offered no lean on this one, so the recorded value is a Crew-proposed
   default surfaced to the human as a filled gap: a public API surface is cheap to add later and
   expensive to retract.

## Dependency policy — "earned dependencies only"

Earned by earlier lens decisions, with full capture in
file:///C:/Dev/KeyContextAI/specs/001-layout-autocorrect/implementation-rules.yml :

- **Microsoft.Agents.AI (MAF 1.0)** — the AI orchestration edge; isolated behind `ILlmAccessor`.
- **Microsoft.Agents.AI.GitHub.Copilot** — the zero-API-key path via the user's Copilot CLI.
- **Polly 8.x** — retry, timeout, circuit breaker for the LLM tier only.
- **Microsoft.Extensions.DependencyInjection + Hosting 10.x** — composition root.
- **WPF (.NET 10 Windows Desktop)** — the UI framework decision: **WPF chosen for MVP**, WinUI 3
  considered and deferred as heavier than a tray-plus-overlay shape warrants.

Anything else requires justification at implement time.

## Reviewer selection

`specrew review --list-hosts --code-writer-host claude` was run and its output presented verbatim.
Available independent hosts, strongest first: **codex (rank 85, marked DEFAULT)**, copilot (80),
cursor-agent (70), antigravity (65); plus claude (85) as the non-independent code-writer.

**Human selected: `copilot`** — explicitly overriding the default because their codex tokens are
exhausted. Copilot remains independent of the Claude code-writer, so the independence property
holds. Authorization persisted by command:
`specrew review --host copilot --authorization-ref workshop-001-layout-autocorrect` →
file:///C:/Dev/KeyContextAI/.specrew/reviewer-hosts.json
