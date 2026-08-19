# Contract: KeyContext AI Public Surface

**Feature**: 001-layout-autocorrect
**Stability**: pre-1.0 — internal to the application. NuGet extraction of the correction engine is
deliberately deferred (recorded in the code-implementation workshop), so these interfaces may change
without a compatibility obligation to anyone outside this repository.

Every component sits behind its own interface, composed by the IoC container with singleton lifetimes.
The interfaces below are the contract the architecture test enforces and the seam every unit test uses.

## Call rules (enforced by `KeyContextAI.Architecture.Tests`)

- An **accessor** calls no component in the system. It publishes events outward.
- An **engine** calls no engine and no manager. It returns results, including by callback.
- A **manager** calls no manager. It calls engines and accessors.
- Engines make **no accessor calls at all** — managers hand data in. This is stricter than classic
  IDesign and was agreed deliberately so engines are unit-testable with zero mocks.

## Engines

### `IWordAssemblyEngine`

Collects characters into a word and recognizes completion.

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `Append` | `WordAssemblyResult Append(KeyEvent key)` | Add a keystroke; the result reports whether a word completed and, if so, its text and scan codes | Never throws; an unmappable key returns `NoChange` |
| `Reset` | `void Reset()` | Discard the word in progress (focus change, wipe) | Never throws |

**Invariants**: completion is signalled only on a space, punctuation, or committing key (FR-005b) —
never mid-word. `Append` is pure with respect to everything except the word in progress.

### `ITranscriptEngine`

Owns the rolling journal and every span computation.

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `Append` | `void Append(TranscriptEntry entry)` | Add a completed or in-progress entry | Never throws |
| `SetVerdict` | `void SetVerdict(Guid entryId, CorrectionVerdict verdict)` | Move an entry to `VerdictReady` | Throws if another entry is already `VerdictReady` |
| `ComputeTransaction` | `CorrectionTransaction? ComputeTransaction(Guid entryId, KeyEvent? suppressedKey)` | Widen to the maximal suspect span, compute backspace count and replacement including the trailing remap | Returns `null` when the span is no longer correctable |
| `MarkEpoch` | `void MarkEpoch()` | Advance the epoch after a layout switch | Never throws |
| `Wipe` | `void Wipe()` | Discard everything (FR-003) | Never throws |

**Invariants**: at most one entry is `VerdictReady` at a time. `Wipe` leaves no recoverable trace of
typed text. `ComputeTransaction`'s `BackspaceCount` always equals the rendered length of the span it
replaces — the property the correction executor depends on for not eating neighbouring text.

### `IMappingEngine`

Pure layout translation.

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `Translate` | `IReadOnlyList<Candidate> Translate(IReadOnlyList<int> scanCodes, LayoutId typedIn, IReadOnlyList<LayoutId> targets)` | Produce one candidate per target layout, plus the text as typed | Never throws; unmapped scan codes yield a candidate marked incomplete |

**Invariants**: translation works from scan codes, never from produced characters, so it is independent
of what the active layout rendered. Deterministic and side-effect free.

### `IDetectionEngine`

The verdict algorithm.

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `Evaluate` | `CorrectionVerdict Evaluate(IReadOnlyList<Candidate> candidates, DictionarySnapshot data, CautionLevel caution)` | Score every candidate, resolve the target layout, apply the caution threshold | Never throws; returns `Ignore` when uncertain |

**Invariants**: dictionary data is passed in, never fetched — the engine touches no accessor.
`Ignore` is always a valid answer, and is the answer whenever two candidates are comparably plausible
(FR-005a). Never returns `Correct` for a word in the user's never-correct set.

## Managers

### `ICorrectionManager`

Owns the typing flow and the correction transaction.

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `Start` / `Stop` | `Task StartAsync(CancellationToken)` / `Task StopAsync()` | Subscribe to key and focus events and run the pipeline | Start throws if the hook cannot be installed |
| `Pause` / `Resume` | `void Pause()` / `void Resume()` | Suspend all capture and correction (FR-025) | Never throws |
| `FlipLast` | `bool FlipLast()` | Reverse or re-apply the most recent correction (FR-015); returns false when nothing is armed | Never throws |

**Invariants**: it is the only component that opens a correction transaction; it never lets an engine
call an accessor on its behalf; it wipes the transcript on focus change and password focus before any
other work.

### `ISettingsManager`

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `Current` | `Settings Current { get; }` | The active configuration | — |
| `Update` | `Task UpdateAsync(Action<SettingsDraft> edit)` | Apply and persist a change, notifying subscribers | Throws on a validation failure; the previous settings remain active |
| `ExcludeForegroundApp` | `Task ExcludeForegroundAppAsync()` | The one-click tray exclusion | Never throws |

## ResourceAccessors

### `IKeystrokeAccessor`

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `KeyObserved` | `event Action<KeyEvent>` | Published for every non-self-injected keystroke | — |
| `Arm` / `Disarm` | `void Arm(SuppressionToken token)` / `void Disarm()` | Set or clear the flag the callback reads to decide suppression (Option B) | Never throws |
| `Install` / `Uninstall` | `Task InstallAsync()` / `Task UninstallAsync()` | Manage the hook lifetime, including re-registration after loss | Install throws when the hook cannot be created |

**Invariants**: the callback performs an O(1) atomic flag read and allocates nothing. It **decides
nothing** — it reads a flag a manager set. A suppressed key is always handed to the channel before the
callback returns, so it can never be dropped silently.

### `IInputInjectionAccessor`

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `ApplyCorrection` | `Task<InjectionResult> ApplyCorrectionAsync(CorrectionTransaction tx)` | Backspaces plus replacement text as one burst, then re-inject any suppressed key | Returns a failed result rather than throwing; never partially reports success |
| `ReinjectKey` | `Task ReinjectKeyAsync(KeyEvent key)` | Deliver a suppressed key alone, the compensating path | Never throws |

**Invariants**: injected events are tagged so they never re-enter the pipeline (FR-013). If
`ApplyCorrection` fails at any point after suppression, the suppressed key is still delivered.

### `IFocusAccessor`

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `FocusChanged` | `event Action<FocusContext>` | Foreground window and control changes | — |
| `IsPasswordContext` | `PasswordState IsPasswordContext()` | `Yes`, `No`, or `Unknown` | Never throws |
| `TryGetCaretPosition` | `bool TryGetCaretPosition(out Point p)` | For bubble placement | Never throws |

**Invariants**: `Unknown` is treated as `Yes` by callers — the gate fails closed (a security control,
not a convenience).

### `ILayoutAccessor`, `IDictionaryAccessor`, `IAudioAccessor`, `ISettingsAccessor`

| Symbol | Signature | Purpose | Errors |
| --- | --- | --- | --- |
| `ILayoutAccessor.Active` | `LayoutId Active { get; }` | The active layout for the foreground thread | — |
| `ILayoutAccessor.Installed` | `IReadOnlyList<LayoutId> Installed { get; }` | Drives the two-versus-more-than-two rule (FR-005a) | — |
| `ILayoutAccessor.Switch` | `Task<bool> SwitchAsync(LayoutId target)` | Switch the active layout | Returns false rather than throwing |
| `IDictionaryAccessor.Load` | `DictionarySnapshot Load(string language)` | Load a pack into memory | Throws on an unknown `schema_version` (FR-029) |
| `IDictionaryAccessor.AffirmWord` | `Task AffirmWordAsync(string word, string language)` | Persist a user-affirmed word (FR-009a/FR-009b) | Never throws; failure is logged, not surfaced |
| `IAudioAccessor.Play` | `void Play(FeedbackCue cue)` | The tiered sounds (FR-023) | Never throws |
| `ISettingsAccessor.Read` / `Write` | `Settings Read()` / `Task WriteAsync(Settings s)` | Persist settings; credentials DPAPI-encrypted | Read throws on unknown `schema_version`; Write is atomic |

**Invariants across all accessors**: none calls another component in the system. `AffirmWordAsync` is
the only path that writes a user-typed word, and only for words the user affirmed.

## System-wide invariants

1. No typed text is written to disk or transmitted, except a user-affirmed word to the user dictionary
   and — in iteration 002 — one sentence of context to an explicitly enabled AI provider.
2. A suppressed keystroke is delivered on every exit path.
3. A correction is applied only to the window that produced the keystrokes it corrects.
4. The hook callback never blocks and never allocates.
5. The tool never runs elevated.
