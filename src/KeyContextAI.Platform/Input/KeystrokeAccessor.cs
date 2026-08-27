using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Platform.Input;

/// <summary>
/// Low-level keyboard hook on a dedicated message-pumping thread.
/// </summary>
public sealed class KeystrokeAccessor : IKeystrokeAccessor, IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeydown = 0x0100;
    private const int WmSyskeydown = 0x0104;
    private const int WmKeyup = 0x0101;
    private const int WmSyskeyup = 0x0105;

    private const uint VkSpace = 0x20;
    private const uint GaRoot = 2;
    private const uint VkTab = 0x09;
    private const uint VkReturn = 0x0D;
    private const uint VkBack = 0x08;
    private const uint VkDelete = 0x2E;
    private const uint VkShift = 0x10;
    private const uint VkControl = 0x11;
    private const uint VkMenu = 0x12;
    private const uint VkLwin = 0x5B;
    private const uint VkRwin = 0x5C;
    private const uint VkCapital = 0x14;
    private const int PendingKeyCapacity = 256;
    private const uint VkEscape = 0x1B;
    private const uint VkPrior = 0x21;
    private const uint VkDown = 0x28;
    private const uint VkInsert = 0x2D;
    private const uint VkF1 = 0x70;
    private const uint VkF24 = 0x87;
    private const uint VkOem1 = 0xBA;
    private const uint VkOemPlus = 0xBB;
    private const uint VkOemComma = 0xBC;
    private const uint VkOemMinus = 0xBD;
    private const uint VkOemPeriod = 0xBE;
    private const uint VkOem3 = 0xC0;
    private const uint VkOem4 = 0xDB;
    private const uint VkOem8 = 0xDF;
    private const uint VkOem102 = 0xE2;

    private readonly object _gate = new();
    private readonly HookProc _hookProc;
    private readonly Channel<PendingKey> _pending = Channel.CreateBounded<PendingKey>(
        new BoundedChannelOptions(PendingKeyCapacity)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest,
        });

    private readonly TaskCompletionSource<bool> _installTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> _uninstallTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Thread? _thread;
    private Thread? _consumerThread;
    private long _sequence;
    private long _lastSequence;
    private int _droppedSinceLastGap;
    private nint _hookHandle;
    private uint _threadId;
    private int _armed;
    private SuppressionToken _token;
    private bool _installed;
    private bool _disposed;

    /// <summary>Creates the accessor without starting the hook.</summary>
    public KeystrokeAccessor()
    {
        _hookProc = HookCallback;
    }

    /// <inheritdoc />
    public event Action<KeyEvent>? KeyObserved;

    /// <inheritdoc />
    public event Action? SequenceGapDetected;

    /// <inheritdoc />
    public void Arm(SuppressionToken token)
    {
        _token = token;
        Interlocked.Exchange(ref _armed, 1);
    }

    /// <inheritdoc />
    public void Disarm()
    {
        Interlocked.Exchange(ref _armed, 0);
    }

    /// <inheritdoc />
    public Task InstallAsync()
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (_installed)
            {
                return Task.CompletedTask;
            }

            _thread = new Thread(MessageLoop)
            {
                IsBackground = true,
                Name = "KeyContextAI.KeyboardHook",
            };
            _thread.Start();

            _consumerThread = new Thread(ConsumePendingKeys)
            {
                IsBackground = true,
                Name = "KeyContextAI.KeyboardHook.Consumer",
            };
            _consumerThread.Start();

            _installed = true;
            return _installTcs.Task;
        }
    }

    /// <inheritdoc />
    public Task UninstallAsync()
    {
        lock (_gate)
        {
            if (!_installed)
            {
                return Task.CompletedTask;
            }

            if (_threadId != 0)
            {
                PostThreadMessage(_threadId, WmQuit, nint.Zero, nint.Zero);
            }

            _pending.Writer.TryComplete();
            _installed = false;
            return _uninstallTcs.Task;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_installed)
        {
            UninstallAsync().GetAwaiter().GetResult();
        }

        GC.SuppressFinalize(this);
    }

    private void MessageLoop()
    {
        _threadId = GetCurrentThreadId();

        var moduleHandle = GetModuleHandle(null);
        _hookHandle = SetWindowsHookEx(WhKeyboardLl, _hookProc, moduleHandle, 0);
        if (_hookHandle == nint.Zero)
        {
            _installTcs.TrySetException(new InvalidOperationException(
                $"Failed to install the keyboard hook. Win32 error: {Marshal.GetLastWin32Error()}"));
            _uninstallTcs.TrySetResult(true);
            return;
        }

        _installTcs.TrySetResult(true);

        try
        {
            while (GetMessage(out var message, nint.Zero, 0, 0))
            {
                _ = TranslateMessage(ref message);
                _ = DispatchMessage(ref message);
            }
        }
        finally
        {
            if (_hookHandle != nint.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = nint.Zero;
            }

            _threadId = 0;
            _uninstallTcs.TrySetResult(true);
        }
    }

    private nint HookCallback(int code, nint wParam, nint lParam)
    {
        if (code < 0)
        {
            return CallNextHookEx(_hookHandle, code, wParam, lParam);
        }

        if (wParam != (nint)WmKeydown && wParam != (nint)WmSyskeydown)
        {
            return CallNextHookEx(_hookHandle, code, wParam, lParam);
        }

        var data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
        if (data.DwExtraInfo == NativeInputTags.SelfInjectionTag)
        {
            return CallNextHookEx(_hookHandle, code, wParam, lParam);
        }

        // The origin is captured HERE, on the callback, because this is the only moment at which
        // the foreground window and layout are known to be the ones the key was typed into. The
        // consumer runs later and on a different thread; sampling there would attribute the key to
        // whatever window happens to be focused by then, which is how a password-field keystroke
        // could be evaluated against an ordinary field's context (FR-003).
        // Normalized to the top-level window with GA_ROOT so it names the same identity the focus
        // stream publishes. GetForegroundWindow already returns a top-level window, but the
        // normalization is explicit so the correlation identity is established the same way on
        // both sides rather than by coincidence of which API each happens to call.
        var rawWindow = GetForegroundWindow();
        var sourceWindow = rawWindow == nint.Zero ? nint.Zero : NormalizeToRoot(rawWindow);
        var sourceThread = sourceWindow == nint.Zero ? 0 : GetWindowThreadProcessId(sourceWindow, out _);
        var layout = GetKeyboardLayout(sourceThread);

        // The focused control is captured alongside the window because within one top-level window
        // an ordinary field and a password field are indistinguishable by window identity alone.
        var sourceControl = TryGetFocusedControl(sourceThread);

        // Modifier and toggle state must be sampled HERE. GetKeyboardState reads the calling
        // thread's input state, and the consumer thread does not process the target application's
        // keyboard messages, so reading it there returns state that is stale or simply absent —
        // Shift+1 would translate as '1' rather than '!', losing the word boundary and making the
        // transcript diverge from the text on screen.
        var keyboardState = new byte[256];
        _ = GetKeyboardState(keyboardState);

        // Suppression consumes the armed state only for the key it was armed for, and only once.
        // Anything else is passed through, so ordinary characters are never swallowed while armed.
        SuppressionToken? consumedToken = null;
        var suppress = false;
        if (Volatile.Read(ref _armed) != 0
            && IsSuppressionEligible(data.VkCode)
            && Interlocked.CompareExchange(ref _armed, 0, 1) == 1)
        {
            consumedToken = _token;
            _token = default;
            suppress = true;
        }

        // Nothing managed beyond a bounded-capacity enqueue happens on this callback. Windows
        // removes a hook whose callback exceeds LowLevelHooksTimeout, and this callback sits on
        // the user's typing path, so translation, word assembly and every subscriber run on the
        // dedicated consumer thread instead.
        var pending = new PendingKey(
            data.VkCode,
            data.ScanCode,
            data.Time,
            sourceWindow,
            sourceThread,
            layout,
            Interlocked.Increment(ref _sequence),
            consumedToken,
            keyboardState,
            sourceControl);

        if (!_pending.Writer.TryWrite(pending))
        {
            // A dropped keystroke means word assembly no longer describes the text that reached
            // the application, so a later correction could replace the wrong span. Losing the
            // sequence is recorded and surfaced rather than silently absorbed.
            Interlocked.Increment(ref _droppedSinceLastGap);
        }

        return suppress ? (nint)1 : CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    private static bool IsSuppressionEligible(uint virtualKey) =>
        ClassifyKey(virtualKey) is KeyEventKind.Committing or KeyEventKind.Separator;

    private void ConsumePendingKeys()
    {
        var reader = _pending.Reader;
        while (true)
        {
            try
            {
                if (!reader.WaitToReadAsync().AsTask().GetAwaiter().GetResult())
                {
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (reader.TryRead(out var pending))
            {
                var dropped = Interlocked.Exchange(ref _droppedSinceLastGap, 0);
                if (dropped > 0 || (_lastSequence != 0 && pending.Sequence != _lastSequence + 1))
                {
                    SequenceGapDetected?.Invoke();
                }

                _lastSequence = pending.Sequence;

                var observed = KeyObserved;
                if (observed is null)
                {
                    continue;
                }

                observed.Invoke(CreateKeyEvent(pending));
            }
        }
    }

    private static KeyEvent CreateKeyEvent(PendingKey data)
    {
        // Every field is derived from the snapshot taken on the callback. Nothing here samples
        // current OS state, so the event describes the moment it was typed.
        var layout = LayoutIdFromKeyboardLayout(data.KeyboardLayout);
        char? character = TryTranslateCharacter(data) is char translated ? translated : null;
        var kind = ClassifyKey(data.VirtualKeyCode, character);

        return new KeyEvent(
            (int)data.ScanCode,
            (int)data.VirtualKeyCode,
            character,
            layout,
            kind,
            false,
            data.Time,
            data.SourceWindow,
            data.SuppressedToken,
            ReadModifiers(data.KeyboardState),
            data.SourceControl);
    }

    private static nint TryGetFocusedControl(uint threadId)
    {
        var info = new GuiThreadInfo
        {
            CbSize = (uint)Marshal.SizeOf<GuiThreadInfo>(),
        };

        return GetGUIThreadInfo(threadId, ref info) ? info.HwndFocus : nint.Zero;
    }

    private static KeyModifiers ReadModifiers(byte[] keyboardState)
    {
        var modifiers = KeyModifiers.None;
        if (IsDown(keyboardState, VkShift))
        {
            modifiers |= KeyModifiers.Shift;
        }

        if (IsDown(keyboardState, VkControl))
        {
            modifiers |= KeyModifiers.Control;
        }

        if (IsDown(keyboardState, VkMenu))
        {
            modifiers |= KeyModifiers.Alt;
        }

        if (IsDown(keyboardState, VkLwin) || IsDown(keyboardState, VkRwin))
        {
            modifiers |= KeyModifiers.Windows;
        }

        return modifiers;
    }

    // The high bit is the held bit; the low bit is the toggle state, which is not a chord.
    private static bool IsDown(byte[] keyboardState, uint virtualKey) =>
        (keyboardState[virtualKey] & 0x80) != 0;

    private static nint NormalizeToRoot(nint hwnd)
    {
        var root = GetAncestor(hwnd, GaRoot);
        return root != nint.Zero ? root : hwnd;
    }

    private static LayoutId LayoutIdFromKeyboardLayout(nint hkl)
    {
        var langId = (int)((ulong)hkl & 0xFFFF);
        try
        {
            return new LayoutId(CultureInfo.GetCultureInfo(langId).Name);
        }
        catch (CultureNotFoundException)
        {
            return new LayoutId("und");
        }
    }

    private static char? TryTranslateCharacter(PendingKey data)
    {
        // The state was captured on the hook callback and travels with the key. Calling
        // GetKeyboardState here instead would read this consumer thread's own input state, which
        // never sees the target application's modifier keys.
        Span<char> buffer = stackalloc char[8];
        var translated = ToUnicodeEx(
            data.VirtualKeyCode,
            data.ScanCode,
            data.KeyboardState,
            buffer,
            buffer.Length,
            0,
            data.KeyboardLayout);

        return translated > 0 ? buffer[0] : null;
    }

    internal static KeyEventKind ClassifyKeyForTest(uint virtualKey, char? character = null) =>
        ClassifyKey(virtualKey, character);

    private static KeyEventKind ClassifyKey(uint virtualKey, char? character = null)
    {
        var byVirtualKey = ClassifyVirtualKey(virtualKey);
        if (byVirtualKey != KeyEventKind.Character)
        {
            return byVirtualKey;
        }

        // Virtual-key ranges cannot see shifted punctuation: !@#$%^&*() keep VK_0..VK_9, so they
        // would fall through as word content and never complete the word at the punctuation
        // boundary. The translated character is what the user actually typed, so boundaries are
        // decided from it. Digits stay word content.
        if (character is { } ch && !char.IsLetterOrDigit(ch) && !char.IsControl(ch))
        {
            return KeyEventKind.Separator;
        }

        return KeyEventKind.Character;
    }

    private static KeyEventKind ClassifyVirtualKey(uint virtualKey)
    {
        return virtualKey switch
        {
            // FR-005b: Tab and Enter both end the word and may submit input to the application.
            VkReturn or VkTab => KeyEventKind.Committing,

            // Space and punctuation end the current word without submitting it. Punctuation
            // previously fell through as Character and was appended into the word in progress.
            VkSpace => KeyEventKind.Separator,
            >= VkOem1 and <= VkOem3 => KeyEventKind.Separator,
            >= VkOem4 and <= VkOem8 => KeyEventKind.Separator,
            VkOem102 => KeyEventKind.Separator,

            // Backspace shortens the word behind the caret. Forward Delete removes text ahead of
            // it, so it does not shorten the assembled word and must not be treated as Editing.
            VkBack => KeyEventKind.Editing,

            VkShift or VkControl or VkMenu or VkLwin or VkRwin or VkCapital => KeyEventKind.Modifier,

            // Navigation, editing-position and function keys move or invalidate the caret, so the
            // word in progress is no longer contiguous with what is on screen and must be reset.
            VkDelete or VkEscape or VkInsert => KeyEventKind.Other,
            >= VkPrior and <= VkDown => KeyEventKind.Other,
            >= VkF1 and <= VkF24 => KeyEventKind.Other,

            _ => KeyEventKind.Character,
        };
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(KeystrokeAccessor));
    }

    private static uint GetForegroundThreadId()
    {
        var foreground = GetForegroundWindow();
        return foreground == nint.Zero ? 0 : GetWindowThreadProcessId(foreground, out _);
    }

    private const int WmQuit = 0x0012;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hmod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetKeyboardState(Span<byte> lpKeyState);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ToUnicodeEx(
        uint wVirtKey,
        uint wScanCode,
        Span<byte> lpKeyState,
        Span<char> pwszBuff,
        int cchBuff,
        uint wFlags,
        nint dwhkl);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetAncestor(nint hwnd, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

    [StructLayout(LayoutKind.Sequential)]
    private struct GuiThreadInfo
    {
        public uint CbSize;
        public uint Flags;
        public nint HwndActive;
        public nint HwndFocus;
        public nint HwndCapture;
        public nint HwndMenuOwner;
        public nint HwndMoveSize;
        public nint HwndCaret;
        public GuiRect CaretRect;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GuiRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetCurrentThreadId();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMessage(out Msg lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TranslateMessage(ref Msg lpMsg);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint DispatchMessage(ref Msg lpMsg);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostThreadMessage(uint idThread, int msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public nint Hwnd;
        public uint Message;
        public nuint WParam;
        public nint LParam;
        public uint Time;
        public Point Pt;
        public uint LPrivate;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public uint VkCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public nuint DwExtraInfo;

        public uint VirtualKeyCode => VkCode;
    }

    /// <summary>
    /// The minimal native payload copied off the hook callback, together with the origin captured
    /// at that same instant. Only blittable values and an immutable snapshot cross the channel, so
    /// the callback never touches the managed pipeline and the consumer never samples stale state.
    /// </summary>
    private readonly record struct PendingKey(
        uint VirtualKeyCode,
        uint ScanCode,
        uint Time,
        nint SourceWindow,
        uint SourceThread,
        nint KeyboardLayout,
        long Sequence,
        SuppressionToken? SuppressedToken,
        byte[] KeyboardState,
        nint SourceControl);

    private delegate nint HookProc(int code, nint wParam, nint lParam);
}
