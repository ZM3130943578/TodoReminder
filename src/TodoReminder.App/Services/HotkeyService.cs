using System.Windows;
using System.Windows.Interop;
using TodoReminder.Infrastructure.Windows;

namespace TodoReminder.App.Services;

public class HotkeyService : IDisposable
{
    private readonly Window _window;
    private readonly int _hotkeyId = 1;
    private HwndSource? _hwndSource;
    private HwndSourceHook? _hook;
    private bool _isRegistered;

    public event EventHandler? HotkeyPressed;

    public HotkeyService(Window window)
    {
        _window = window;
    }

    public bool Register(uint modifiers, uint key)
    {
        var helper = new WindowInteropHelper(_window);
        var hwnd = helper.EnsureHandle();

        if (!Win32Api.RegisterHotKey(hwnd, _hotkeyId, modifiers, key))
            return false;

        _hook = WndProc;
        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(_hook);
        _isRegistered = true;
        return true;
    }

    public bool Reregister(uint modifiers, uint key)
    {
        var helper = new WindowInteropHelper(_window);
        var hwnd = helper.EnsureHandle();

        if (_isRegistered)
            Win32Api.UnregisterHotKey(hwnd, _hotkeyId);

        if (!Win32Api.RegisterHotKey(hwnd, _hotkeyId, modifiers, key))
            return false;

        if (_hwndSource == null)
        {
            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hook = WndProc;
            _hwndSource?.AddHook(_hook);
        }

        _isRegistered = true;
        return true;
    }

    public void Unregister()
    {
        if (!_isRegistered) return;

        var helper = new WindowInteropHelper(_window);
        Win32Api.UnregisterHotKey(helper.Handle, _hotkeyId);

        if (_hwndSource != null && _hook != null)
            _hwndSource.RemoveHook(_hook);

        _isRegistered = false;
    }

    public void Dispose()
    {
        Unregister();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Api.WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }
}
