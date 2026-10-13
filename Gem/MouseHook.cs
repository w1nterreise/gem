using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace Gem {
    public class MouseHook : IDisposable {

        #region winAPI

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT { public POINT pt; public uint mouseData; public uint flags; public uint time; public IntPtr dwExtraInfo; }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        #region privates

        private const int WH_MOUSE_LL   = 14;
        private const int WM_MOUSEMOVE  = 0x0200;
        private const int WM_MOUSEWHEEL = 0x020A;

        private LowLevelMouseProc _proc;
        private Control _targetControl;
        private bool    _isMouseOver = false;
        private IntPtr  _hookId = IntPtr.Zero;

        #endregion

        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;

        public event EventHandler MouseOverWheelUp;
        public event EventHandler MouseOverWheelDown;

        public event EventHandler MouseWheelUp;
        public event EventHandler MouseWheelDown;

        public MouseHook(Control targetControl) {

            _targetControl = targetControl ?? throw new ArgumentNullException(nameof(targetControl));
            _proc          = HookCallback;
            _hookId        = SetHook(_proc);
        }


        #region hook

        private IntPtr SetHook(LowLevelMouseProc proc) {

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {

            if (nCode >= 0) {
                int msg = (int)wParam;
                if (msg == WM_MOUSEMOVE) {
                    var hs = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    var pt = new Point(hs.pt.x, hs.pt.y);

                    bool isOverNow = _targetControl.RectangleToScreen(_targetControl.ClientRectangle).Contains(pt);

                    if (isOverNow != _isMouseOver) {
                        _isMouseOver = isOverNow;
                        var handler = isOverNow ? MouseEnter : MouseLeave;
                        _targetControl.BeginInvoke(new Action(() => handler?.Invoke(_targetControl, EventArgs.Empty)));
                    }
                }
                else if (msg == WM_MOUSEWHEEL) {

                    var hs = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    short delta = (short)((hs.mouseData >> 16) & 0xFFFF);

                    if (delta != 0) {
                        var globalHandler = delta > 0 ? MouseWheelUp : MouseWheelDown;
                        _targetControl.BeginInvoke(new Action(() => globalHandler?.Invoke(this, EventArgs.Empty)));

                        if (_isMouseOver) {
                            var localHandler = delta > 0 ? MouseOverWheelUp : MouseOverWheelDown;
                            _targetControl.BeginInvoke(new Action(() => localHandler?.Invoke(_targetControl, EventArgs.Empty)));
                        }
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        #endregion

        #region dispose

        public void Dispose() {

            if (_hookId != IntPtr.Zero) {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        ~MouseHook() => Dispose();

        #endregion
    }
}