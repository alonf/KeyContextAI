# Data-Storage Workshop Record — KeyContext AI (001-layout-autocorrect)

**Lens**: data-storage (medium depth)
**Conducted**: 2026-08-19
**Confirmation**: human-confirmed / lens-question (data map rendered in-band; human replied "Yes, agree")

## Agreed data ownership and lifetime

```text
  ON DISK (per-user, %LOCALAPPDATA%\KeyContextAI\)
  ┌─────────────────────────────────────────────────────────────────┐
  │ KeyMap (per layout pair)          ships with app, read-only     │
  │   pair_id: "en-US↔he-IL"                                        │
  │   scancode → { char_a, char_b }                                 │
  │                          │ referenced by pair_id                │
  │ Dictionary (per language)◀┘       ships with app + user-extended│
  │   lang: "he-IL"                                                 │
  │   words (trie//frequency), user_added[], never_correct[]        │
  │                                                                 │
  │ Settings (single file)            user-owned, small             │
  │   active_pairs[], mode, conservatism, sounds, bubble,           │
  │   excluded_apps[], hotkey, ai_provider                          │
  │      └── ai_credentials  ← DPAPI-encrypted blob, never plaintext│
  └─────────────────────────────────────────────────────────────────┘

  IN MEMORY ONLY (wiped on focus change, password field, pause, exit)
  ┌─────────────────────────────────────────────────────────────────┐
  │ Transcript journal: recent keystrokes + words, layout           │
  │ provenance, correction status, epoch marks                      │
  │ Session never-correct set (from flip-backs)                     │
  └─────────────────────────────────────────────────────────────────┘

  NEVER WRITTEN ANYWHERE: raw keystroke logs, per-correction history,
  window titles, anything from a password field, LLM request/response bodies
```

## Agreed decisions

1. **Storage technology — plain files, not SQLite.** Dictionaries load into an in-memory trie at
   startup (serving the <10ms lookup requirement); the on-disk form is a compact text/binary word
   list per language. SQLite rejected deliberately: it would add a dependency, a schema, and
   migrations for data never queried relationally. **Flip trigger recorded**: dictionaries too large
   to hold in memory (word lists are a few MB, so this is not expected).
2. **Settings format — JSON**, human-readable and hand-editable, with the API-key field stored as a
   DPAPI-encrypted blob inside it.
3. **Ownership** — `DictionaryAccessor` owns key maps + dictionaries; `SettingsAccessor` owns
   settings and credentials; `TranscriptEngine` owns all in-memory typing state. No component reads
   another component's files.
4. **Schema evolution** — every file carries a `schema_version`; unknown-version files are rejected
   with a clear message rather than silently misread. User-added words live in a **separate file**
   from the shipped dictionary so an app update never clobbers them — which also makes the deferred
   OneDrive/Google Drive sync a matter of syncing that one file.
5. **Learned words are inspectable** — a flipped-back correction adds the word to `never_correct[]`
   in the user's own plain-text file, which they can read and edit. For a keystroke-adjacent tool,
   "you can read everything we stored about you" is a trust feature.
