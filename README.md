# KeyContext AI

A Windows background tool that detects when you type in the wrong keyboard layout (e.g., English text while the Hebrew layout is active), automatically corrects the text in place, switches the keyboard layout to the intended language, and gives you subtle audio-visual feedback.

## The problem

Multilingual typists constantly type a word or a whole sentence before noticing the keyboard was on the wrong layout — producing gibberish like `akuo` instead of `שלום`. Fixing it means selecting the text, deleting it, switching layouts, and retyping. KeyContext AI removes that friction entirely.

## How it works

1. **Capture** — a low-level keyboard hook maintains a small rolling buffer of recent keystrokes (memory-only, never persisted, suspended on password fields).
2. **Detect** — a fast local dictionary lookup (< 10 ms) maps the typed keystrokes to the other layout and checks whether the result is a real word. Ambiguous cases fall back to a context-aware LLM (local or cloud, bring-your-own-key).
3. **Correct** — the erroneous text is replaced via simulated input, the active keyboard layout is switched to the intended language, and a brief sound plus a floating bubble near the caret confirm what happened.

## Planned technology

- .NET / C# targeting Windows, WinUI 3 or WPF for the tray and overlay UI
- Win32 interop: `SetWindowsHookEx` (WH_KEYBOARD_LL), `SendInput`, keyboard-layout APIs
- Microsoft Semantic Kernel as the LLM abstraction (Azure OpenAI, Anthropic Claude, OpenAI, or local models via Ollama/ONNX)
- Local dictionary via in-memory trie or SQLite for sub-10 ms lookups
- Polly for retry/circuit-breaker resiliency on cloud LLM calls

## Privacy

- Keystrokes live only in a short in-memory context window; nothing is logged or written to disk.
- Capture is suspended on password fields (UI Automation `IsPassword` detection).
- API keys (BYOK) are encrypted at rest with Windows DPAPI.

## Status

Early design phase. This project is developed spec-first under a Specrew-governed lifecycle — specifications, plans, and design artifacts live under `specs/` as they are produced.

## License

[MIT](LICENSE)
