using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
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

    private readonly object _gate = new();
    private readonly HookProc _hookProc;
    private readonly TaskCompletionSource<bool> _installTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> _uninstallTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Thread? _thread;
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

        var observed = KeyObserved;
        if (observed is not null)
        {
            observed.Invoke(CreateKeyEvent(data));
        }

        return Volatile.Read(ref _armed) != 0
            ? (nint)1
            : CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    private static KeyEvent CreateKeyEvent(KbdLlHookStruct data)
    {
        var foregroundThreadId = GetForegroundThreadId();
        var layout = LayoutIdFromKeyboardLayout(GetKeyboardLayout(foregroundThreadId));
        var kind = ClassifyKey(data.VirtualKeyCode);
        char? character = TryTranslateCharacter(data, foregroundThreadId) is char translated ? translated : null;

        return new KeyEvent(
            (int)data.ScanCode,
            (int)data.VirtualKeyCode,
            character,
            layout,
            kind,
            false,
            data.Time);
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

    private static char? TryTranslateCharacter(KbdLlHookStruct data, uint threadId)
    {
        Span<byte> keyboardState = stackalloc byte[256];
        if (!GetKeyboardState(keyboardState))
        {
            return null;
        }

        Span<char> buffer = stackalloc char[8];
        var layoutHandle = GetKeyboardLayout(threadId);
        var translated = ToUnicodeEx(
            data.VirtualKeyCode,
            data.ScanCode,
            keyboardState,
            buffer,
            buffer.Length,
            0,
            layoutHandle);

        return translated > 0 ? buffer[0] : null;
    }

    private static KeyEventKind ClassifyKey(uint virtualKey)
    {
        return virtualKey switch
        {
            VkSpace or VkTab => KeyEventKind.Separator,
            VkReturn => KeyEventKind.Committing,
            VkBack or VkDelete => KeyEventKind.Editing,
            VkShift or VkControl or VkMenu or VkLwin or VkRwin or VkCapital => KeyEventKind.Modifier,
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

    private delegate nint HookProc(int code, nint wParam, nint lParam);
}
