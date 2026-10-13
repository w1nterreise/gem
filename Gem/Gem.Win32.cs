using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gem {
    public partial class Gem : Form {

        #region win32Api

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]  private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong",    SetLastError = true)]  private static extern int    GetWindowLong32   (IntPtr hWnd, int nIndex);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) =>
            IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));

        [DllImport("user32.dll")]  private static extern bool   SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]  private static extern IntPtr GetForegroundWindow();

        private static readonly IntPtr HWND_TOPMOST   = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_BOTTOM    = new IntPtr(1);

        private const uint SWP_NOSIZE     = 0x0001;
        private const uint SWP_NOMOVE     = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;

        private const int  WM_SYSCOMMAND  = 0x0112;
        private const int  SC_RESTORE     = 0xF120;
        private const int  WM_ACTIVATE    = 0x0006;
        private const int  SC_MINIMIZE    = 0xF020;

        private const int  GWL_EXSTYLE    = -20;
        private const uint WS_EX_TOPMOST  = 0x00000008;

        #endregion


        private void ToggleWidgetCollapse() {

            void FadeIn() { Opacity = 0; OpacityTransition(OPACITY_OPAQUE); };

            if (this.WindowState == FormWindowState.Minimized) {
                FadeIn();
                this.WindowState = FormWindowState.Normal;
                return;
            }

            if (this.TopMost) {
                long exStyle = GetWindowLongPtr(this.Handle, GWL_EXSTYLE).ToInt64();
                bool isCurrentlyTopMost = (exStyle & WS_EX_TOPMOST) != 0;

                if (isCurrentlyTopMost) {
                    SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    SetWindowPos(this.Handle, HWND_BOTTOM,    0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
                else {
                    FadeIn();
                    SetWindowPos(this.Handle, HWND_TOPMOST,   0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
            }

            else {
                IntPtr activeWindowHandle = GetForegroundWindow();
                bool isCurrentlyActive = (activeWindowHandle == this.Handle);

                if (isCurrentlyActive) {
                    this.WindowState = FormWindowState.Minimized;
                }
                else {

                    this.WindowState = FormWindowState.Minimized;
                    FadeIn();
                    this.WindowState = FormWindowState.Normal;
                }
            }
        }


        private bool IsWidgetOnSurface() {

            if (this.WindowState == FormWindowState.Minimized) {
                return false;
            }

            if (this.TopMost) {
                long exStyle = GetWindowLongPtr(this.Handle, GWL_EXSTYLE).ToInt64();
                bool isCurrentlyTopMost = (exStyle & WS_EX_TOPMOST) != 0;

                return isCurrentlyTopMost;
            }

            IntPtr activeWindowHandle = GetForegroundWindow();
            return (activeWindowHandle == this.Handle);
        }


        protected override void WndProc(ref Message m) {
            // sync native win32 topmost with .net TopMost property when restored from bottom
            if ((m.Msg == WM_SYSCOMMAND && ((int)m.WParam & 0xFFF0) == SC_RESTORE) || (m.Msg == WM_ACTIVATE && (int)m.WParam != 0)) {
                if (this.TopMost) {
                    SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
                }
            }

            base.WndProc(ref m);
        }


        public override bool PreProcessMessage(ref Message m) {
            // block mouse wheel when resizing by wheel
            const int WM_MOUSEWHEEL = 0x020A;
            if (_resizeAnimationTimer.Enabled && m.Msg == WM_MOUSEWHEEL) {
                return true;
            }
            return base.PreProcessMessage(ref m);
        }


    }
}
