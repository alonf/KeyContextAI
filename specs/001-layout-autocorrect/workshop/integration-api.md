# Integration-API Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: integration-api (full depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (contract sequence + provider matrix rendered
in-band; the human challenged the first draft's Claude claim, research was re-run, and the human
replied "accept" to the revised set)

## Verified ground truth (researched 2026-08-19, was `research-needed`, now `known`)

- **MAF 1.0 GA'd 2026-04-03** — stable APIs, LTS commitment, first-party connectors for Microsoft
  Foundry, Azure OpenAI, OpenAI, Anthropic Claude, Amazon Bedrock, Google Gemini, Ollama.
- **GitHub Copilot agent type IS available in .NET**: `Microsoft.Agents.AI.GitHub.Copilot`,
  `CopilotClient` → `AsAIAgent()`. Requires an authenticated Copilot runtime (installed CLI +
  subscription) — **no API key**. This realizes the product-domain "installed-assistant" idea.
- **Claude Agent SDK agent type (`ClaudeAgent`) exists** (`agent-framework-claude`), prerequisite
  "install and configure the Claude Code CLI", no API key — but it is **Python-only and prerelease
  (`--pre`)**. No .NET package today. (The human correctly recalled the integration existing; the
  .NET gap is the load-bearing detail.)
- The **Anthropic model provider** (Claude as the model behind our own agent, API-key based) is a
  distinct integration and IS available in .NET.

## Agreed provider matrix

```text
Route                              .NET?   Auth                        MVP?
──────────────────────────────────────────────────────────────────────────
Anthropic model provider           ✓       Anthropic API key           yes
  (Claude as the model behind
   our own agent)
Azure OpenAI / Foundry / OpenAI    ✓       API key / Entra             yes
Ollama / local endpoint            ✓       none (localhost)            yes
GitHub Copilot agent               ✓       installed Copilot CLI +     yes
  (Microsoft.Agents.AI.                    subscription, NO API key
   GitHub.Copilot)
Claude Agent SDK (ClaudeAgent)     ✗       installed Claude Code CLI,  post-MVP
  Python-only, --pre                       NO API key                  (watch)
```

## Agreed contract sequence

```text
  CorrectionManager        LlmAccessor            MAF Agent          Provider
        │                       │                     │                 │
        │ Detect(request)       │                     │                 │
        │──────────────────────▶│                     │                 │
        │  { typed: "akuo",     │  build prompt +     │                 │
        │    sentence: "Hi Dana,│  run agent          │                 │
        │      akuo",           │────────────────────▶│                 │
        │    from: en-US,       │                     │ chat completion │
        │    to:   he-IL,       │                     │────────────────▶│
        │    candidate: "שלום" }│                     │                 │
        │                       │                     │◀────────────────│
        │                       │◀────────────────────│  structured out │
        │◀──────────────────────│                     │                 │
        │  { verdict: correct | leave,                │                 │
        │    text: "שלום", confidence: 0.0-1.0,       │                 │
        │    tier: cloud|local, elapsed_ms }          │                 │
        │                       │                     │                 │
        │  on timeout (2s) / circuit open:            │                 │
        │  { verdict: leave, reason: unavailable } ── never blocks typing
```

## Agreed decisions

1. **Integration style** — direct in-process library call to MAF; no self-hosted HTTP layer. The
   `ILlmAccessor` interface IS our contract; MAF is a swappable implementation behind it.
2. **Contract shape** — request carries typed text, the one-sentence context, both layouts, and the
   dictionary's candidate; response is a small structured record (verdict, text, confidence, tier,
   elapsed_ms). **Structured output, never prose parsing** — parsing free-form LLM replies in a
   correction path is how false corrections are born.
3. **Provider set for MVP** — Azure OpenAI / Microsoft Foundry (flagship story), OpenAI, Anthropic
   Claude (API key), and a local endpoint (Ollama / OpenAI-compatible). All are MAF first-party
   connectors, so this is configuration rather than integration work.
4. **Installed-assistant discovery (revised after research)** — MVP ships the **Copilot CLI zero-key
   path** (detect → ask → use the user's subscription), which is .NET-available today. The **Claude
   Code CLI route is a watch item**: when `agent-framework-claude` reaches .NET it is a configuration
   addition behind the existing `ILlmAccessor` contract, not a redesign. Discovery always asks and
   never activates silently (bound in security-compliance).
5. **Error and timeout semantics** — 2s hard timeout then verdict `leave` with a reason; Polly retry
   only on transient network/429 with exponential backoff and jitter capped under the timeout
   budget; circuit breaker after repeated failures drops to dictionary-only and flips the tray icon
   amber. Typing never waits on any of this.
6. **Idempotency and ordering** — every request carries a correction-transaction id; a late response
   whose transaction was superseded (user typed on, flipped back, changed focus) is **discarded, not
   applied** — the async analogue of the focus-change abandon rule.
7. **Compatibility testing** — the `ILlmAccessor` contract gets a fake for unit tests plus one
   recorded-response integration test per provider, so a MAF version bump or provider change fails a
   test rather than a user's sentence.

## Design flags recorded

- CLI-harness routes (`CopilotClient.StartAsync()`) spin up a runtime process: hold **one long-lived
  client**, never one per correction, or process startup blows the 500ms budget.
- Those harnesses are coding-oriented with shell/file/URL tools; the default is **deny-all
  permissions and we supply no permission handler**, so a text-classification agent cannot touch the
  filesystem.

## Sources

- <https://devblogs.microsoft.com/agent-framework/microsoft-agent-framework-version-1-0/>
- <https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/github-copilot-agent>
- <https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/claude-agent-sdk>
- <https://devblogs.microsoft.com/agent-framework/build-ai-agents-with-claude-agent-sdk-and-microsoft-agent-framework/>
