using System.Drawing;
using System.Runtime.InteropServices;
using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;
using System.Windows.Automation;

namespace KeyContextAI.Platform.System;

/// <summary>
/// Watches foreground and control focus changes and reports password state plus caret position.
/// </summary>
public sealed class FocusAccessor : IFocusAccessor, IDisposable
{
    private const uint EventSystemForeground = 0x0003;
    private const uint EventObjectFocus = 0x8005;
    private const uint WinEventOutOfContext = 0x0000;
    private const uint WinEventSkipOwnProcess = 0x0002;

    private readonly WinEventDelegate _callback;
    private readonly nint _foregroundHook;
    private readonly nint _focusHook;
    private FocusContext? _lastContext;
    private bool _disposed;

    /// <summary>Creates the accessor and installs the WinEvent hooks.</summary>
    public FocusAccessor()
    {
        _callback = HandleWinEvent;

        _foregroundHook = SetWinEventHook(
            EventSystemForeground,
            EventSystemForeground,
            nint.Zero,
            _callback,
            0,
            0,
            WinEventOutOfContext | WinEventSkipOwnProcess);

        _focusHook = SetWinEventHook(
            EventObjectFocus,
            EventObjectFocus,
            nint.Zero,
            _callback,
            0,
            0,
            WinEventOutOfContext | WinEventSkipOwnProcess);

        if (_foregroundHook == nint.Zero || _focusHook == nint.Zero)
        {
            DisposeHooks();
            throw new InvalidOperationException("The focus hooks could not be installed.");
        }
    }

    /// <inheritdoc />
    public event Action<FocusContext>? FocusChanged;

    /// <inheritdoc />
    public PasswordState IsPasswordContext()
    {
        return TryReadFocusedAutomationMetadata(out var metadata) && metadata is { }
            ? metadata.PasswordState
            : PasswordState.Unknown;
    }

    /// <inheritdoc />
    public bool TryGetCaretPosition(out Point p)
    {
        var info = new GuiThreadInfo
        {
            CbSize = (uint)Marshal.SizeOf<GuiThreadInfo>(),
        };

        if (!GetGUIThreadInfo(0, ref info) || info.HwndCaret == nint.Zero)
        {
            p = default;
            return false;
        }

        p = new Point(
            (info.CaretRect.Left + info.CaretRect.Right) / 2,
            (info.CaretRect.Top + info.CaretRect.Bottom) / 2);
        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DisposeHooks();
        GC.SuppressFinalize(this);
    }

    private void HandleWinEvent(
        nint hook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint threadId,
        uint time)
    {
        _ = hook;
        _ = idObject;
        _ = idChild;
        _ = threadId;
        _ = time;

        if (_disposed)
        {
            return;
        }

        var windowHandle = hwnd != nint.Zero ? hwnd : GetForegroundWindow();
        if (windowHandle == nint.Zero)
        {
            return;
        }

        if (!TryBuildContext(windowHandle, eventType, out var context))
        {
            return;
        }

        if (Equals(context, _lastContext))
        {
            return;
        }

        _lastContext = context;
        FocusChanged?.Invoke(context);
    }

    private bool TryBuildContext(nint windowHandle, uint eventType, out FocusContext context)
    {
        var threadId = GetWindowThreadProcessId(windowHandle, out var processId);

        var title = GetWindowText(windowHandle);
        var className = GetClassName(windowHandle);
        FocusedAutomationMetadata? metadata = null;
        var passwordState = TryReadFocusedAutomationMetadata(out metadata) && metadata is { }
            ? metadata.PasswordState
            : PasswordState.Unknown;
        Point? caretPosition = TryGetCaretPosition(out var caret) ? caret : null;

        context = new FocusContext(
            windowHandle,
            (int)processId,
            (int)threadId,
            title,
            className,
            metadata?.AutomationId,
            metadata?.ControlType,
            metadata?.AutomationName,
            GetForegroundWindow() == windowHandle,
            eventType == EventObjectFocus,
            passwordState,
            caretPosition);
        return true;
    }

    private static bool TryReadFocusedAutomationMetadata(out FocusedAutomationMetadata? metadata)
    {
        metadata = null;

        try
        {
            var focusedElement = AutomationElement.FocusedElement;
            if (focusedElement is null)
            {
                return false;
            }

            var current = focusedElement.Current;
            metadata = new FocusedAutomationMetadata(
                string.IsNullOrWhiteSpace(current.AutomationId) ? null : current.AutomationId,
                current.ControlType?.ProgrammaticName,
                string.IsNullOrWhiteSpace(current.Name) ? null : current.Name,
                current.IsPassword ? PasswordState.Yes : PasswordState.No);
            return true;
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (COMException)
        {
            return false;
        }
    }

    private static string? GetWindowText(nint windowHandle)
    {
        var length = GetWindowTextLength(windowHandle);
        if (length <= 0)
        {
            return null;
        }

        var buffer = new char[length + 1];
        return GetWindowText(windowHandle, buffer, buffer.Length) > 0
            ? new string(buffer).TrimEnd('\0')
            : null;
    }

    private static string? GetClassName(nint windowHandle)
    {
        var buffer = new char[256];
        return GetClassName(windowHandle, buffer, buffer.Length) > 0
            ? new string(buffer).TrimEnd('\0')
            : null;
    }

    private void DisposeHooks()
    {
        if (_foregroundHook != nint.Zero)
        {
            UnhookWinEvent(_foregroundHook);
        }

        if (_focusHook != nint.Zero)
        {
            UnhookWinEvent(_focusHook);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWinEventHook(
        uint eventMin,
        uint eventMax,
        nint hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWinEvent(nint hWinEventHook);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(nint hWnd, [Out] char[] lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetClassName(nint hWnd, [Out] char[] lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

    private delegate void WinEventDelegate(
        nint hook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint threadId,
        uint time);

    private sealed record FocusedAutomationMetadata(
        string? AutomationId,
        string? ControlType,
        string? AutomationName,
        PasswordState PasswordState);

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
        public Rect CaretRect;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
