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

        var correction = SendCorrectionBurst(tx.BackspaceCount, tx.ReplacementText);
        if (!correction.Succeeded)
        {
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
        var steps = new List<InjectionStep>(2);
        AddKeyPress(steps, key);
        return steps;
    }

    private static async Task<InjectionResult> ReinjectKeyInternalAsync(KeyEvent key)
    {
        var result = SendBurst(BuildReinjectSteps(key));
        return await Task.FromResult(result).ConfigureAwait(false);
    }

    internal static InjectionResult SendCorrectionBurstForTest(
        int backspaceCount,
        string replacementText,
        Func<INPUT[], int> sender) =>
        SendCorrectionBurst(backspaceCount, replacementText, sender);

    private static InjectionResult SendCorrectionBurst(
        int backspaceCount,
        string replacementText,
        Func<INPUT[], int>? sender = null)
    {
        var steps = BuildCorrectionSteps(backspaceCount, replacementText);
        var sent = SendSteps(steps, out var error, sender);
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

        var sent = SendSteps(steps, out var error);
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
        out int win32Error,
        Func<INPUT[], int>? sender = null)
    {
        win32Error = 0;
        if (steps.Count == 0)
        {
            return 0;
        }

        var inputs = new INPUT[steps.Count];
        for (var i = 0; i < steps.Count; i++)
        {
            inputs[i] = steps[i].ToInput();
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

    private static InjectionResult SendBurst(IReadOnlyList<InjectionStep> steps)
    {
        if (steps.Count == 0)
        {
            return InjectionResult.Success();
        }

        var sent = SendSteps(steps, out var error);
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
