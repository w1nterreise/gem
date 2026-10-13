using Gem.Properties;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class KeyboardHook : IDisposable {

    #region winAPI

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(Keys vKey);

    #endregion

    #region privates

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN     = 0x0100;
    private const int WM_KEYUP       = 0x0101;
    private const int WM_SYSKEYDOWN  = 0x0104;
    private const int WM_SYSKEYUP    = 0x0105;

    private LowLevelKeyboardProc _proc;
    private Control _targetControl;
    private IntPtr  _hookId = IntPtr.Zero;

    private Keys _targetKey;
    private bool _requireCtrl;
    private bool _requireShift;
    private bool _requireAlt;

    private bool _isKeyPressedNow = false;

    private readonly bool _trackDoubleTap;
    private DateTime      _lastTapTime = DateTime.MinValue;
    private int DOUBLE_TAP_INTERVAL = Settings.Default.DoubleTapInterval;

    #endregion


    public event EventHandler HotKeyPressed;
    public event EventHandler HotKeyReleased;

    public event EventHandler HotKeyDoublePressed;

    public KeyboardHook(Control targetControl, Keys targetKey,
                        bool requireCtrl = false,
                        bool requireShift = false,
                        bool requireAlt = false,
                        bool trackDoubleTap = false) {

        _targetControl  = targetControl ?? throw new ArgumentNullException(nameof(targetControl));
        _targetKey      = targetKey;
        _requireCtrl    = requireCtrl;
        _requireShift   = requireShift;
        _requireAlt     = requireAlt;
        _trackDoubleTap = trackDoubleTap;

        _proc   = HookCallback;
        _hookId = SetHook(_proc);
    }


    #region hook

    private IntPtr SetHook(LowLevelKeyboardProc proc) {

        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule) {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }


    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {

        if (nCode < 0) return CallNextHookEx(_hookId, nCode, wParam, lParam);

        int msg = (int)wParam;
        Keys pressedKey = (Keys)Marshal.ReadInt32(lParam);
        Keys baseKey = NormalizeKey(pressedKey);

        if (baseKey != _targetKey) return CallNextHookEx(_hookId, nCode, wParam, lParam);

        if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN) {
            if (!_isKeyPressedNow && CheckModifiers()) {
                _isKeyPressedNow = true;
                HandleKeyDown();
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        if (msg == WM_KEYUP || msg == WM_SYSKEYUP) {
            if (_isKeyPressedNow) {
                _isKeyPressedNow = false;
                _targetControl.BeginInvoke(new Action(() => HotKeyReleased?.Invoke(this, EventArgs.Empty)));
                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }


    private Keys NormalizeKey(Keys key) {

        if (key == Keys.LShiftKey   || key == Keys.RShiftKey)   return Keys.ShiftKey;
        if (key == Keys.LControlKey || key == Keys.RControlKey) return Keys.ControlKey;
        if (key == Keys.LMenu       || key == Keys.RMenu)       return Keys.Menu;
        return key;
    }

    private bool CheckModifiers() {

        bool isCtrlDown  = (GetAsyncKeyState(Keys.ControlKey) & 0x8000) != 0;
        bool isShiftDown = (GetAsyncKeyState(Keys.ShiftKey) & 0x8000)   != 0;
        bool isAltDown   = (GetAsyncKeyState(Keys.Menu) & 0x8000)       != 0;

        bool expectedShift = _targetKey == Keys.ShiftKey ? false : _requireShift;

        return isCtrlDown == _requireCtrl &&
               isShiftDown == expectedShift &&
               isAltDown == _requireAlt;
    }

    private void HandleKeyDown() {

        if (_trackDoubleTap) {
            double elapsed = (DateTime.UtcNow - _lastTapTime).TotalMilliseconds;
            _lastTapTime = DateTime.UtcNow;

            if (elapsed <= DOUBLE_TAP_INTERVAL) {
                _lastTapTime = DateTime.MinValue;
                _targetControl.BeginInvoke(new Action(() => HotKeyDoublePressed?.Invoke(this, EventArgs.Empty)));
                return;
            }
        }

        _targetControl.BeginInvoke(new Action(() => HotKeyPressed?.Invoke(this, EventArgs.Empty)));
    }

    #endregion

    #region dispose

    public void Dispose() {

        if (_hookId != IntPtr.Zero) {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    ~KeyboardHook() => Dispose();

    #endregion
}
