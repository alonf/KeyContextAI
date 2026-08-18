# DevOps-Operations Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: devops-operations (light depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (delivery topology rendered in-band; the human
asked whether Azure credits enable signing, enabled Copilot PR review, and confirmed the MSIX update
model)

## Detected posture (brownfield check before proposing)

- Remote: **GitHub `alonf/KeyContextAI`** (public).
- CI: none beyond the Specrew methodology gate workflow.
- Branch protection: none configured.
- `.specrew/repository-governance.yml` recorded `release_model: local-only` (inferred) — **stale**,
  since a public GitHub remote now exists. Corrected as part of this lens.

## Agreed delivery topology

```text
  Developer machine                GitHub                        End user
  ┌──────────────┐    push    ┌──────────────────┐          ┌───────────────┐
  │ dotnet build │──────────▶ │ Actions CI       │          │ Windows 11/10 │
  │ dotnet test  │            │  build + test    │          │               │
  └──────────────┘            │  publish win-x64 │          │  install via: │
                              │  + arm64         │          │  1 winget     │
                              └────────┬─────────┘          │  2 .msix/.exe │
                                       │ tag v*             │  3 zip (port.)│
                              ┌────────▼─────────┐          └───────▲───────┘
                              │ GitHub Release   │──────────────────┘
                              │  signed          │   update on new release
                              └──────────────────┘
```

## Agreed decisions

1. **Hosting model** — local desktop tool; nothing server-side in MVP. The post-MVP telemetry Azure
   Function deploys separately with its own minimal IaC.
2. **Packaging** — self-contained **win-x64 and win-arm64**; distributed as an **MSIX installer**
   plus a portable zip; **winget** manifest as the headline install path
   (`winget install KeyContextAI`). MSIX gives clean install/uninstall, update support, and a better
   antivirus story than a loose `.exe`; the zip serves users who avoid installers.
3. **Code signing — researched during this lens (was `research-needed`, now `known`).**
   **Primary path: Azure Artifact Signing** (formerly Trusted Signing) — ~**$9.99/month Basic tier**,
   payable from the human's Azure MVP credits, issuing real Public Trust Authenticode certificates
   with auto-renewing short-lived certs in FIPS 140-3 L3 HSMs and a native GitHub Actions signing
   task. **Eligibility catch**: Public Trust identity validation covers organizations in the US,
   Canada, EU, UK, Australia, New Zealand, Japan, South Korea, Singapore, Switzerland, Norway and
   **Israel**, but **individual developers must be in the US or Canada** — so validation must be done
   under an Israeli company entity, not as an individual. **The human named the entity: ZioNet**,
   an Israeli registered organization, which is on the supported list; Azure MVP credits fund the
   subscription.
   **Fallbacks**: SignPath.io free OSS certificate program if organization validation does not clear;
   Microsoft Store submission as a parallel discovery + trust channel (Microsoft signs); unsigned zip
   with published SHA-256 hashes as the last resort.
4. **CI — GitHub Actions (matching the project's own forge).**
   - PR lane: build, unit tests, the **IDesign architecture test**, markdownlint, Specrew governance
     validation.
   - Release lane on tag: build both architectures, package MSIX + zip, sign, generate SHA-256,
     create the GitHub Release. Reproducible-build flags enabled, honoring the security lens's
     binary↔source verifiability promise.
5. **Update model (human decision)** — MSIX updates are produced by the **CI release script when a
   new release is cut**; updates happen because a release exists, not on a background timer.
6. **Repository governance** — protect the release-truth branch (`main`), blocking direct commits,
   requiring PR + green required status checks, no force-push, no branch deletion. Feature branches
   merge via PR. **Required human approvals: 0** (solo maintainer) but **required status checks: on**
   — protection against accident rather than against the maintainer. **Copilot automated PR review:
   ENABLED** (human has the subscription); it is advisory alongside the required checks.
7. **Release model correction** — `.specrew/repository-governance.yml` updated from `local-only` to a
   GitHub-based release model with published artifacts.

## Open action (not a design question)

Applying branch protection mutates repository security settings and needs the human's explicit
go-ahead before it is executed. The design above is agreed; the application is pending.
