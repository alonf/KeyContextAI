using System.Runtime.InteropServices;
using System.Text;
using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Platform.Input;

/// <summary>
/// Injects backspaces and replacement text in one burst and tags all self-generated input.
/// </summary>
public sealed class InputInjectionAccessor : IInputInjectionAccessor
{
    private const ushort VkBack = 0x08;
    private const ushort VkShift = 0x10;
    private const ushort VkControl = 0x11;
    private const ushort VkMenu = 0x12;
    private const ushort VkLwin = 0x5B;
    private const uint GaRoot = 2;
    private const uint MapVkToVsc = 0;
    private const uint InputKeyboard = 1;
    private const uint KeyeventfExtendedkey = 0x0001;
    private const uint KeyeventfKeyup = 0x0002;
    private const uint KeyeventfUnicode = 0x0004;
    private const uint KeyeventfScancode = 0x0008;

    /// <inheritdoc />
    public async Task<InjectionResult> ApplyCorrectionAsync(CorrectionTransaction tx)
    {
        ArgumentNullException.ThrowIfNull(tx);

        if (tx.BackspaceCount < 0)
        {
            return InjectionResult.Failure("BackspaceCount cannot be negative.");
        }

        if (string.IsNullOrEmpty(tx.ReplacementText))
        {
            return InjectionResult.Failure("ReplacementText must be non-empty.");
        }

        // SendInput delivers to whatever window is foreground at the instant it is called, not to
        // the window this transaction was built for. The manager's applicability check ran earlier
        // and on another thread, so focus can have moved since. The target is therefore
        // revalidated inside the send path, immediately before SendInput, which is what stops a
        // burst of backspaces from deleting text in an unrelated application (FR-012).
        var correction = SendCorrectionBurst(tx.BackspaceCount, tx.ReplacementText, tx.TargetWindowHandle);
        if (!correction.Succeeded)
        {
            // The target was lost before anything mutated. Re-delivering the suppressed key now
            // would type it into whichever window took focus, so the whole transaction is
            // abandoned with the user's text untouched.
            if (correction.FailureKind == InjectionFailureKind.TargetLost)
            {
                return correction;
            }

            // A partial burst has already mutated the user's document. Restoring the original span
            // takes priority over re-delivering the suppressed key: the documented control is that
            // an injection failure leaves the text as it was.
            if (correction.FailureKind == InjectionFailureKind.PartiallyApplied)
            {
                correction = CompensatePartialBurst(correction, tx);
            }

            if (tx.SuppressedKey is null)
            {
                return correction;
            }

            var reinjected = await ReinjectKeyInternalAsync(tx.SuppressedKey).ConfigureAwait(false);
            return reinjected.Succeeded ? correction : reinjected;
        }

        if (tx.SuppressedKey is null)
        {
            return InjectionResult.Success();
        }

        var reinjection = await ReinjectKeyInternalAsync(tx.SuppressedKey).ConfigureAwait(false);
        return reinjection.Succeeded ? InjectionResult.Success() : reinjection;
    }

    /// <inheritdoc />
    public Task ReinjectKeyAsync(KeyEvent key) => ReinjectKeyInternalAsync(key);

    private static bool IsStillTargeting(nint targetWindow)
    {
        var foreground = GetForegroundWindow();
        if (foreground == nint.Zero)
        {
            return false;
        }

        var root = GetAncestor(foreground, GaRoot);
        return (root != nint.Zero ? root : foreground) == targetWindow;
    }

    internal static IReadOnlyList<InjectionStep> BuildCorrectionSteps(int backspaceCount, string replacementText)
    {
        if (backspaceCount < 0)
        {
            return [];
        }

        replacementText ??= string.Empty;

        var steps = new List<InjectionStep>(backspaceCount * 2 + replacementText.Length * 2);

        for (var i = 0; i < backspaceCount; i++)
        {
            AddVirtualKeyPress(steps, VkBack, GetVirtualKeyScanCode(VkBack));
        }

        foreach (var ch in replacementText)
        {
            AddUnicodePress(steps, ch);
        }

        return steps;
    }

    internal static IReadOnlyList<InjectionStep> BuildCorrectionSteps(CorrectionTransaction tx)
    {
        var steps = new List<InjectionStep>(tx.BackspaceCount * 2 + tx.ReplacementText.Length * 2
            + (tx.SuppressedKey is null ? 0 : 2));

        steps.AddRange(BuildCorrectionSteps(tx.BackspaceCount, tx.ReplacementText));
        if (tx.SuppressedKey is not null)
        {
            AddKeyPress(steps, tx.SuppressedKey);
        }

        return steps;
    }

    internal static IReadOnlyList<InjectionStep> BuildReinjectSteps(KeyEvent key)
    {
        // The suppressed key is re-delivered as the gesture the user made, not as a bare keypress.
        // The user is free to release Shift or Ctrl while the correction runs, so replaying only
        // the key would turn Shift+Enter into Enter — which in a chat client sends the message the
        // user meant to break a line in. The chord is rebuilt from what was captured with the key.
        var modifiers = ModifierKeys(key.Modifiers);
        var steps = new List<InjectionStep>(2 + modifiers.Count * 2);

        foreach (var modifier in modifiers)
        {
            AddVirtualKeyDown(steps, modifier, GetVirtualKeyScanCode(modifier));
        }

        AddKeyPress(steps, key);

        for (var i = modifiers.Count - 1; i >= 0; i--)
        {
            AddVirtualKeyUp(steps, modifiers[i], GetVirtualKeyScanCode(modifiers[i]));
        }

        return steps;
    }

    private static List<ushort> ModifierKeys(KeyModifiers modifiers)
    {
        var keys = new List<ushort>(4);
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            keys.Add(VkControl);
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            keys.Add(VkMenu);
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            keys.Add(VkShift);
        }

        if (modifiers.HasFlag(KeyModifiers.Windows))
        {
            keys.Add(VkLwin);
        }

        return keys;
    }

    private static async Task<InjectionResult> ReinjectKeyInternalAsync(KeyEvent key)
    {
        // The suppressed key is re-delivered only into the window it was typed in, for the same
        // FR-012 reason the correction burst revalidates: a focus change between suppression and
        // re-delivery would otherwise type the key into an unrelated application.
        var result = SendBurst(BuildReinjectSteps(key), key.SourceWindowHandle);
        return await Task.FromResult(result).ConfigureAwait(false);
    }

    internal static InjectionResult SendCorrectionBurstForTest(
        int backspaceCount,
        string replacementText,
        Func<INPUT[], int> sender) =>
        SendCorrectionBurst(backspaceCount, replacementText, nint.Zero, sender);

    private static InjectionResult SendCorrectionBurst(
        int backspaceCount,
        string replacementText,
        nint targetWindow = 0,
        Func<INPUT[], int>? sender = null)
    {
        var steps = BuildCorrectionSteps(backspaceCount, replacementText);
        var sent = SendSteps(steps, targetWindow, out var error, out var targetLost, sender);
        if (targetLost)
        {
            return InjectionResult.Abandoned(
                "Focus left the target window before injection; the correction was abandoned.");
        }

        if (sent == steps.Count)
        {
            return InjectionResult.Success();
        }

        var message = $"SendInput inserted {sent} of {steps.Count} input event(s); Win32 error {error}.";
        if (sent == 0)
        {
            return InjectionResult.Failure(message);
        }

        // Each applied step is accounted for by its own effect on the document, not by halving the
        // event count: a Backspace keydown already deletes a character and a Unicode keydown
        // already inserts one, so an odd trailing event has mutated the text even though its
        // keyup never ran. Assuming otherwise leaves compensation working from the wrong state.
        var appliedBackspaces = 0;
        var appliedText = new StringBuilder();
        for (var i = 0; i < sent; i++)
        {
            switch (steps[i].Kind)
            {
                case InjectionStepKind.KeyDown when steps[i].VirtualKey == VkBack:
                    appliedBackspaces++;
                    break;

                case InjectionStepKind.UnicodeDown when steps[i].UnicodeChar is { } ch:
                    appliedText.Append(ch);
                    break;

                default:
                    break;
            }
        }

        return InjectionResult.PartialFailure(message, sent, appliedBackspaces, appliedText.ToString());
    }

    /// <summary>
    /// Undoes the applied prefix of a failed burst so the caller's document is left as it was:
    /// remove the characters that were inserted, then retype the characters the backspaces ate.
    /// </summary>
    private static InjectionResult CompensatePartialBurst(InjectionResult partial, CorrectionTransaction tx)
    {
        var originalPrefix = OriginalTextPrefix(tx, partial.AppliedBackspaceCount);
        var steps = BuildCorrectionSteps(partial.AppliedReplacementText.Length, originalPrefix);
        if (steps.Count == 0)
        {
            return partial;
        }

        var sent = SendSteps(steps, tx.TargetWindowHandle, out var error, out var targetLost);
        if (targetLost)
        {
            // Compensating into whichever window took focus would damage a second application on
            // top of the first, so the restoration is abandoned and the damage reported instead.
            return partial with
            {
                ErrorMessage = $"{partial.ErrorMessage} Focus left the target window before compensation; the target text is left modified.",
            };
        }

        return sent == steps.Count
            ? partial with
            {
                ErrorMessage = $"{partial.ErrorMessage} The applied prefix was compensated and the original text restored.",
            }
            : partial with
            {
                ErrorMessage = $"{partial.ErrorMessage} Compensation also failed after {sent} of {steps.Count} event(s); Win32 error {error}. The target text is left modified.",
            };
    }

    internal static string OriginalTextPrefixForTest(CorrectionTransaction tx, int backspaceCount) =>
        OriginalTextPrefix(tx, backspaceCount);

    /// <summary>
    /// Reconstructs the trailing characters of the original span that the applied backspaces
    /// removed, reading the span entries back to front.
    /// </summary>
    private static string OriginalTextPrefix(CorrectionTransaction tx, int backspaceCount)
    {
        if (backspaceCount <= 0 || tx.SpanEntries.Count == 0)
        {
            return string.Empty;
        }

        var original = string.Concat(tx.SpanEntries.Select(entry => entry.Text));
        return backspaceCount >= original.Length
            ? original
            : original[^backspaceCount..];
    }

    private static int SendSteps(
        IReadOnlyList<InjectionStep> steps,
        nint targetWindow,
        out int win32Error,
        out bool targetLost,
        Func<INPUT[], int>? sender = null)
    {
        win32Error = 0;
        targetLost = false;
        if (steps.Count == 0)
        {
            return 0;
        }

        var inputs = new INPUT[steps.Count];
        for (var i = 0; i < steps.Count; i++)
        {
            inputs[i] = steps[i].ToInput();
        }

        // The foreground is read here, after every step has been built and marshalled, so nothing
        // but this comparison sits between the read and the SendInput call. Checking any earlier
        // widens the window in which focus can move after the check and before the burst.
        if (targetWindow != nint.Zero && !IsStillTargeting(targetWindow))
        {
            targetLost = true;
            return 0;
        }

        var sent = sender is null
            ? (int)SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>())
            : sender(inputs);

        if (sent != inputs.Length)
        {
            win32Error = sender is null ? Marshal.GetLastWin32Error() : 0;
        }

        return sent;
    }

    private static InjectionResult SendBurst(IReadOnlyList<InjectionStep> steps, nint targetWindow)
    {
        if (steps.Count == 0)
        {
            return InjectionResult.Success();
        }

        var sent = SendSteps(steps, targetWindow, out var error, out var targetLost);
        if (targetLost)
        {
            return InjectionResult.Abandoned(
                "Focus left the source window before the suppressed key could be re-delivered; the key was dropped.");
        }

        return sent == steps.Count
            ? InjectionResult.Success()
            : InjectionResult.Failure(
                $"SendInput inserted {sent} of {steps.Count} input event(s); Win32 error {error}.");
    }

    private static void AddKeyPress(List<InjectionStep> steps, KeyEvent key)
    {
        var scanCode = key.ScanCode > 0 ? (ushort)key.ScanCode : GetVirtualKeyScanCode((ushort)key.VirtualKey);
        var extended = IsExtendedKey((ushort)key.VirtualKey);
        AddVirtualKeyPress(steps, (ushort)key.VirtualKey, scanCode, extended);
    }

    private static void AddVirtualKeyPress(List<InjectionStep> steps, ushort virtualKey, ushort scanCode, bool isExtended = false)
    {
        steps.Add(new InjectionStep(InjectionStepKind.KeyDown, virtualKey, scanCode, null, isExtended, NativeInputTags.SelfInjectionTag));
        steps.Add(new InjectionStep(InjectionStepKind.KeyUp, virtualKey, scanCode, null, isExtended, NativeInputTags.SelfInjectionTag));
    }

    private static void AddVirtualKeyDown(List<InjectionStep> steps, ushort virtualKey, ushort scanCode, bool isExtended = false) =>
        steps.Add(new InjectionStep(InjectionStepKind.KeyDown, virtualKey, scanCode, null, isExtended, NativeInputTags.SelfInjectionTag));

    private static void AddVirtualKeyUp(List<InjectionStep> steps, ushort virtualKey, ushort scanCode, bool isExtended = false) =>
        steps.Add(new InjectionStep(InjectionStepKind.KeyUp, virtualKey, scanCode, null, isExtended, NativeInputTags.SelfInjectionTag));

    private static void AddUnicodePress(List<InjectionStep> steps, char ch)
    {
        steps.Add(new InjectionStep(InjectionStepKind.UnicodeDown, 0, ch, ch, false, NativeInputTags.SelfInjectionTag));
        steps.Add(new InjectionStep(InjectionStepKind.UnicodeUp, 0, ch, ch, false, NativeInputTags.SelfInjectionTag));
    }

    private static ushort GetVirtualKeyScanCode(ushort virtualKey) =>
        (ushort)MapVirtualKey(virtualKey, MapVkToVsc);

    private static bool IsExtendedKey(ushort virtualKey) =>
        virtualKey is 0x21 or 0x22 or 0x23 or 0x24 or 0x25 or 0x26 or 0x27 or 0x28
            or 0x2D or 0x2E or 0xA3 or 0xA5 or 0xA6 or 0xA7 or 0xA8 or 0xA9;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetAncestor(nint hwnd, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    internal readonly record struct InjectionStep(
        InjectionStepKind Kind,
        ushort VirtualKey,
        ushort ScanCode,
        char? UnicodeChar,
        bool IsExtended,
        nuint ExtraInfo)
    {
        internal INPUT ToInput()
        {
            var flags = Kind switch
            {
                InjectionStepKind.KeyDown => IsExtended ? KeyeventfScancode | KeyeventfExtendedkey : KeyeventfScancode,
                InjectionStepKind.KeyUp => (IsExtended ? KeyeventfScancode | KeyeventfExtendedkey : KeyeventfScancode) | KeyeventfKeyup,
                InjectionStepKind.UnicodeDown => KeyeventfUnicode,
                InjectionStepKind.UnicodeUp => KeyeventfUnicode | KeyeventfKeyup,
                _ => throw new InvalidOperationException("Unknown injection step kind."),
            };

            return new INPUT
            {
                Type = InputKeyboard,
                Data = new InputUnion
                {
                    Keyboard = new KEYBDINPUT
                    {
                        WVk = UnicodeChar is null ? VirtualKey : (ushort)0,
                        WScan = UnicodeChar is null ? ScanCode : UnicodeChar.Value,
                        DwFlags = flags,
                        Time = 0,
                        DwExtraInfo = ExtraInfo,
                    },
                },
            };
        }
    }

    internal enum InjectionStepKind
    {
        KeyDown,
        KeyUp,
        UnicodeDown,
        UnicodeUp,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public nuint DwExtraInfo;
    }
}
