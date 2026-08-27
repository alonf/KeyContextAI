using System.Globalization;
using System.Runtime.InteropServices;
using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Platform.System;

/// <summary>
/// Uses the foreground window's thread and installed HKLs to manage keyboard layouts.
/// </summary>
public sealed class LayoutAccessor : ILayoutAccessor
{
    private const uint WmInputLangChangeRequest = 0x0050;
    private const uint HklLangIdMask = 0xFFFF;

    /// <inheritdoc />
    public LayoutId GetActiveLayout()
    {
        var foregroundWindow = GetForegroundWindow();
        var threadId = foregroundWindow == nint.Zero
            ? 0
            : GetWindowThreadProcessId(foregroundWindow, out _);

        return LayoutIdFromKeyboardLayout(GetKeyboardLayout(threadId));
    }

    /// <inheritdoc />
    public IReadOnlyList<LayoutId> GetInstalledLayouts()
    {
        var count = GetKeyboardLayoutList(0, null);
        if (count <= 0)
        {
            return [];
        }

        var handles = new nint[count];
        var returned = GetKeyboardLayoutList(handles.Length, handles);
        var layouts = new List<LayoutId>(returned);
        var seen = new HashSet<LayoutId>();

        for (var index = 0; index < returned; index++)
        {
            var layout = LayoutIdFromKeyboardLayout(handles[index]);
            if (seen.Add(layout))
            {
                layouts.Add(layout);
            }
        }

        return layouts;
    }

    /// <inheritdoc />
    public bool TrySwitchLayout(LayoutId layout)
    {
        var foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == nint.Zero)
        {
            return false;
        }

        var handles = GetKeyboardLayoutHandles();
        foreach (var handle in handles)
        {
            if (LayoutIdFromKeyboardLayout(handle) == layout)
            {
                return PostMessage(foregroundWindow, WmInputLangChangeRequest, nint.Zero, handle);
            }
        }

        return false;
    }

    internal static LayoutId LayoutIdFromKeyboardLayout(nint hkl)
    {
        var languageId = (int)((ulong)hkl & HklLangIdMask);
        try
        {
            return new LayoutId(CultureInfo.GetCultureInfo(languageId).Name);
        }
        catch (CultureNotFoundException)
        {
            return new LayoutId("und");
        }
    }

    private static nint[] GetKeyboardLayoutHandles()
    {
        var count = GetKeyboardLayoutList(0, null);
        if (count <= 0)
        {
            return [];
        }

        var handles = new nint[count];
        var returned = GetKeyboardLayoutList(handles.Length, handles);
        return returned == handles.Length ? handles : handles[..returned];
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetKeyboardLayoutList(int nBuff, [Out] nint[]? lpList);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);
}
