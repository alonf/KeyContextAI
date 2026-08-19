# Data Model: KeyContext AI — Keyboard Layout Auto-Correction

**Feature**: 001-layout-autocorrect
**Date**: 2026-08-19
**Purpose**: Define the entities, attributes, relationships, and validation rules for iteration 001.

## Persistence posture

Most of this model is **transient by requirement**. FR-004 forbids writing typed text to disk, so the
entities describing typing live only in memory and are destroyed on focus change, password focus, pause,
or exit. Only three entities persist: key maps and dictionaries (reference data), the user's own
dictionary additions (which FR-009b limits to words the user affirmed), and settings.

---

## Entity: KeyEvent

**Purpose**: One observed keystroke, as published by the keystroke accessor. Transient.

### Attributes

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `ScanCode` | int | yes | > 0 | Hardware scan code, layout-independent |
| `VirtualKey` | int | yes | > 0 | Windows virtual-key code |
| `Character` | char? | no | — | The character produced under the active layout, when one is |
| `LayoutId` | LayoutId | yes | must be an installed layout | Which layout was active when the key was pressed |
| `Kind` | enum | yes | `Character`, `Committing`, `Editing`, `Modifier`, `Other` | Drives word completion (FR-005b) |
| `IsSelfInjected` | bool | yes | — | True for keys this tool injected; such events never re-enter the pipeline (FR-013) |
| `TimestampTicks` | long | yes | monotonic | Ordering and typing-speed measurement |

### Lifecycle / Relationships

Created by `KeystrokeAccessor` on every hook callback, written to the channel, consumed by
`CorrectionManager`, and appended to the transcript. Never persisted, never logged in standard
diagnostic mode. A `KeyEvent` with `IsSelfInjected = true` is discarded before reaching the engines.

---

## Entity: TranscriptEntry

**Purpose**: One word (or word-in-progress) in the rolling journal, with the provenance needed to
correct a multi-word run. Transient.

### Attributes

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `Text` | string | yes | non-empty once complete | The characters as typed |
| `ScanCodes` | int[] | yes | same length as `Text` | Retained so re-mapping does not depend on the produced characters |
| `TypedInLayout` | LayoutId | yes | installed layout | The layout active while it was typed |
| `StartOffset` | int | yes | >= 0 | Position within the current epoch, used to compute backspace counts |
| `State` | enum | yes | `InProgress`, `Complete`, `VerdictReady`, `Corrected`, `Rejected` | `VerdictReady` is the Option B state: evaluated but uncommitted |
| `Verdict` | CorrectionVerdict? | no | present iff `State` is `VerdictReady` or `Corrected` | The pre-computed decision |
| `EpochId` | int | yes | >= 0 | Increments on every layout switch and focus change |

### Lifecycle / Relationships

Appended by `TranscriptEngine` as `WordAssemblyEngine` reports characters and completions. The engine
widens a failing entry into the maximal consecutive suspect span (FR-007) and computes the trailing
remap for entries typed after a verdict was reached (FR-011). The whole journal is discarded on focus
change, password focus, pause, and exit (FR-003).

**Invariant**: at most one entry may be in `VerdictReady` state at a time — it is the word the armed
flag refers to.

---

## Entity: CorrectionVerdict

**Purpose**: The detection engine's decision about a span. Transient.

### Attributes

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `Outcome` | enum | yes | `Correct`, `Notify`, `Ignore` | `Notify` is the notify-only mode result |
| `TextAsTyped` | string | yes | non-empty | What the user produced |
| `TextIntended` | string | yes when `Outcome != Ignore` | non-empty | The corrected form |
| `TargetLayout` | LayoutId | yes when `Outcome != Ignore` | installed layout | Resolved per FR-005a |
| `Confidence` | double | yes | 0.0 – 1.0 | Compared against the caution level's threshold (FR-006) |
| `Tier` | enum | yes | `Dictionary`, `Ai` | Drives the distinct feedback sound (FR-023); `Ai` is iteration 002 |
| `TransactionId` | Guid | yes | unique | Lets a late or superseded result be discarded (FR-018) |

### Lifecycle / Relationships

Produced by `DetectionEngine` from candidates supplied by `MappingEngine`, held on the transcript entry,
consumed by `CorrectionManager` to build the transaction. Discarded when its transaction is superseded.

---

## Entity: CorrectionTransaction

**Purpose**: The atomic unit of change applied to the user's text. Transient.

### Attributes

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `TransactionId` | Guid | yes | unique | Correlates verdict, injection, and feedback |
| `BackspaceCount` | int | yes | >= 0, must equal the span's rendered length | Characters to remove |
| `ReplacementText` | string | yes | non-empty | Text to inject as one burst (FR-015 undo-friendliness) |
| `TargetLayout` | LayoutId | yes | installed layout | Layout to switch to |
| `SuppressedKey` | KeyEvent? | no | present only on the Option B path | The committing key to re-inject afterwards |
| `TargetWindowHandle` | IntPtr | yes | non-zero | Correction is abandoned if focus is no longer here (FR-012) |
| `SpanEntries` | TranscriptEntry[] | yes | non-empty | The entries this transaction covers |

### Lifecycle / Relationships

Created by `CorrectionManager` from a `VerdictReady` span, executed by the serialized correction
executor, then either committed (epoch advances) or abandoned. **Invariant**: if a transaction carries a
`SuppressedKey`, that key is re-injected on every exit path, success or failure (FR-014, and the first
Phase 2 hardening target).

---

## Entity: LayoutPair *(persisted, shipped)*

**Purpose**: The scan-code mapping that makes translation between two layouts possible.

### Attributes

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `PairId` | string | yes | `<locale-a>↔<locale-b>` | e.g. `en-US↔he-IL` |
| `SchemaVersion` | int | yes | known version or the file is rejected (FR-029) | Format marker |
| `Mappings` | map<int, (char, char)> | yes | every scan code distinct | Scan code to the character each layout produces |

### Lifecycle / Relationships

Shipped as read-only data, loaded once at startup by `DictionaryAccessor`. Adding a language pair means
adding one of these plus the dictionaries — no code change (FR-008).

---

## Entity: Dictionary *(persisted; shipped part read-only, user part writable)*

**Purpose**: The word data for one language, in two separately stored parts so an application update can
never discard the user's own words (FR-028).

### Attributes — shipped part

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `Language` | string | yes | BCP-47 tag | e.g. `he-IL` |
| `SchemaVersion` | int | yes | known version or rejected | Format marker |
| `Source` | string | yes | non-empty (FR-008a) | Where the word list came from |
| `Licence` | string | yes | must permit MIT redistribution (FR-008a) | The pack does not ship without this |
| `Words` | trie | yes | non-empty | Loaded into memory for sub-10 ms lookup |
| `Frequencies` | map<string, int> | no | — | Used to break ties between candidates |

### Attributes — user part

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `UserAdded` | string[] | yes | may be empty | Words the user affirmed as valid (FR-009a) |
| `NeverCorrect` | string[] | yes | may be empty | Words never to correct again, from flip-backs |

### Lifecycle / Relationships

The shipped part is read-only and replaced by updates. The user part is written only when the user
affirms a word — by flipping back a correction, or by repeated use — and never for any other observed
text (FR-009b). It is plain text so the user can read and edit it.

---

## Entity: Settings *(persisted)*

**Purpose**: The user's configuration.

### Attributes

| Attribute | Type | Required | Validation Rules | Description |
| --- | --- | --- | --- | --- |
| `SchemaVersion` | int | yes | known version or rejected | Format marker |
| `ActivePairs` | string[] | yes | each a known `PairId` | Which pairs are enabled |
| `Mode` | enum | yes | `Correct`, `NotifyOnly` | FR-025 |
| `CautionLevel` | enum | yes | `Conservative`, `Balanced`, `Aggressive` | Sets both the confidence bar and AI escalation (FR-006) |
| `ExcludedApps` | string[] | yes | may be empty | Executable names excluded from capture (FR-025) |
| `FlipHotkey` | HotkeySpec | yes | must be bindable | Default: double-tap Ctrl |
| `SoundsEnabled` / `BubbleEnabled` | bool | yes | — | Independently mutable (FR-022) |
| `DiagnosticMode` | enum | yes | `Off`, `Standard`, `Verbose` | Verbose is session-scoped and self-deleting (FR-031) |
| `AiCredentials` | encrypted blob | no | DPAPI CurrentUser scope (FR-021) | Iteration 002; never plaintext |

### Lifecycle / Relationships

Owned by `SettingsAccessor`, surfaced by `SettingsManager`, changed through the settings window and the
tray. A change notifies `CorrectionManager`, which applies it without restart.

---

## Relationship summary

```text
  KeyEvent ──appended to──▶ TranscriptEntry ──span──▶ CorrectionVerdict
                                   │                          │
                                   └──────────┬───────────────┘
                                              ▼
                                    CorrectionTransaction ──re-injects──▶ KeyEvent (suppressed)

  LayoutPair ──feeds──▶ MappingEngine ──candidates──▶ DetectionEngine ◀──consults── Dictionary
  Settings ──configures──▶ everything above
```
