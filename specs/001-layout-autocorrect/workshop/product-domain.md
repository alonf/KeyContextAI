# Product-Domain Workshop Record — KeyContext AI (001-layout-autocorrect)

**Phase**: product-domain (first-stage, pre-technical)
**Depth**: Standard — first feature of a new product (rules out Light); single-maintainer OSS utility, not regulated/multi-team/commercial (rules out Deep). Proposed by the Crew; the human was offered Light/Deep adjustment repeatedly and answered all Standard-depth areas without adjusting.
**Conducted**: 2026-08-18/19, one area at a time (human chose pacing option 2)
**Confirmation**: human-confirmed / lens-question (typed replies for all 8 areas; receipts in `.specrew/runtime/workshop-authority.jsonl`)

## 1. Users, customers, and stakeholders

- Open-source tool for **any language-pair combination** — explicitly NOT just the Hebrew-speaking community (human corrected the initial draft). `known`
- Requires per-language dictionaries plus AI that generalize across layout pairs; **Hebrew↔English is the first shipped, proven pair**. `known`
- The maintainer (Alon) is the first user; community users including non-developers matter — usability, installer and docs polish count. `known`
- Complex input-method languages (IME composition — Chinese/Japanese/Korean) **may be deferred** if they complicate the core. `known`

## 2. Pain, job, and current workaround

- Pain: typing a burst of text on the wrong layout, noticing late, then manually deleting, switching layout, retyping — dozens of times a day of micro-friction. `known`
- Current workaround: **nothing** — users manually fix every time (human confirmed option "Nothing"). `known`
- Cost of doing nothing: constant small interruptions to flow. The bar to beat: faster and less annoying than delete-switch-retype, without ever making things worse. `known`

## 3. Existing system and context

- **New standalone product** — extends, replaces, migrates nothing. `known` (human: "confirm")
- Integrates only with Windows itself (input stack, layout APIs) and the user's chosen LLM provider. `known`

## 4. Constraints (all binding unless noted)

- **Windows-only**; no cross-platform ambition for now. `known`
- **.NET / C# ecosystem** — externally fixed by maintainer preference and expertise. Specific frameworks/libraries stay open for their technical lenses. `known`
- **BYOK economics** — users bring their own AI keys or local models; the project never bears inference costs. `known`
  - Discovery opportunity: detect an already-installed Copilot/Codex/Claude CLI or desktop app and reuse the user's existing license with no login or a simple browser-based login. The Microsoft Agent Framework now supports agents whose backend is the Copilot or Claude SDK rather than a raw API. `research-needed` (load_bearing: false — API-key BYOK remains the guaranteed path; explored in integration-api lens)
- **Privacy hard rule** — keystrokes live only in a short in-memory window, never persisted; capture suspends on password fields. Refinement: sending a **single sentence of context** to a cloud LLM is acceptable, but nothing is ever retained in the cloud. The **dictionary** (words, not keystrokes) may be persisted, updated over time, and synced between the same user's machines via OneDrive/Google Drive. `known`
- **MIT license** — committed. `known`
- **Team**: single developer + AI agents; no external schedule/budget pressure. `known`
- **Antivirus/SmartScreen tolerance** — global hook + text injection + network egress pattern-matches malware; distribution must not trigger quarantine or unknown-publisher scare screens. Zero-budget qualifier: **no paid Authenticode certificate** for an OSS project; use a free/legitimate signing path if one exists. Candidates: SignPath.io OSS program, Azure Trusted Signing low-cost tier, Microsoft Store MSIX. `research-needed` (load_bearing: false — shapes release, not the build)

## 5. Outcomes and success metrics

- **Correction-quality priority**: a false correction (mangling correct text) is worse than a missed correction — the tool is conservative. `known` (human: "Agree")
- **Two-tier feedback**: distinct sounds for *detection-only* (flagged, no auto-action) vs *seamless correction* (replace + layout switch); likely also distinguishing instant dictionary corrections from later-arriving LLM ones. Sound design details belong to ui-ux; the product fact is that audio disambiguates what the tool did. `known`
- **Race-safe correction** (human's example: `"Hן ים' are you today"` → must not become `"How are טםו today"`): detection fires at word-end while the user keeps typing on the wrong layout; the engine must track keystrokes typed during the correction window and remap those too — the mapping may need to apply twice, and continued input after the layout switch must be remapped. Core correctness requirement; mechanics designed in architecture/component lenses. `known`
- **Speed as felt experience**: dictionary-tier corrections feel instantaneous (< 10 ms lookup); LLM-tier corrections useful at < 500 ms round-trip. `known`
- **Personal bar**: the maintainer stops noticing layout mistakes at all — the tool disappears into the OS. `known`
- **Community bar**: real adoption — strangers install it, keep it running, it survives their antivirus; signals: GitHub stars, download counts. `known`

## 6. MVP, non-goals, and vision

**MVP (v1):**

1. Language-pair-agnostic detection engine — dictionary tier, Hebrew↔English first shipped pair
2. Seamless correction: replace + layout switch + audio/visual feedback, race-safe with continued typing
3. Detection-only mode (sound, no auto-action) as a user-selectable conservatism level
4. Tray app: enable/disable, per-app exclusions, settings
5. Password-field suspension; in-memory-only keystroke rule
6. **LLM tier IS in the MVP, built on the Microsoft Agent Framework** — "this is the edge." Strategic: Azure AI Foundry positioning and Foundry-team recognition; the README states the MAF + Foundry foundation (done 2026-08-19). `known`

**Deferred (post-v1):** IME/composition languages; Windows Service component; dictionary sync via OneDrive/Google Drive; installed-assistant discovery (Copilot/Claude SDK reuse); additional dictionary packs beyond the first pairs.

**Failure conditions for v1 even if it "works":** it mangles correct text (false corrections), or antivirus/SmartScreen blocks it for ordinary users. `known`

## 7. Alternatives, competitors, and differentiation

- The human does not care about competition as rivalry. Competitors matter **only as a learning source**: what they can do, what they can't, and why their adoption is poor. `known`
- A short competitive scan (Punto Switcher, AutoHotkey community scripts, Windows text intelligence) will run before the spec to extract capability gaps and adoption-failure causes. `research-needed` (load_bearing: false)
- Differentiators: (1) context-aware AI correction via Microsoft Agent Framework/Foundry; (2) any language pair by design; (3) trustworthy by construction — OSS, memory-only keystrokes, BYOK. Winning dimension: **accuracy/trust**, not feature count. `known`

## 8. Adoption, rollout, and change impact

- Rollout: maintainer dogfoods daily → public GitHub release → possibly winget / Microsoft Store later (reach + signing benefit). `known` (human: "confirm")
- Adoption risks: (a) SmartScreen/AV trust — binding constraint above; (b) AI-tier setup friction (API keys are a wall for non-developers — mitigated by the installed-assistant discovery idea); (c) skepticism toward a keystroke-reading tool — countered by OSS transparency and the memory-only rule. `known`
- Out-of-box experience: dictionary tier works immediately with zero configuration; the AI tier is opt-in enrichment. `known`
- No migration, no training; docs = README + short setup guide; support via GitHub issues. `known`

## Follow-up research carried forward

| Item | Tag | Load-bearing | Owner lens |
| --- | --- | --- | --- |
| Installed-assistant reuse (Copilot/Claude SDK via MAF) | research-needed | no | integration-api |
| Free OSS code-signing path (SignPath / Azure Trusted Signing / Store) | research-needed | no | devops-operations |
| Competitive lessons scan (capabilities + adoption failures) | research-needed | no | pre-spec research |
