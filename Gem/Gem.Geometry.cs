using System;
using System.Windows.Forms;

namespace Gem {
    public partial class Gem : Form {

        [Flags]
        private enum Edges {
            None        = 0,
            Left        = 1,
            Right       = 2,
            Top         = 4,
            Bottom      = 8,
            TopRight    = Top    | Right,
            TopLeft     = Top    | Left,
            BottomRight = Bottom | Right,
            BottomLeft  = Bottom | Left
        }
        private Edges _activeEdges = Edges.None;


        private void btnPin_CheckedChanged(object sender, EventArgs e) => this.TopMost = btnPin.Checked;


        private Edges GetMouseEdges() {

            var area = Screen.FromPoint(Control.MousePosition).Bounds;
            int mx   = Control.MousePosition.X;
            int my   = Control.MousePosition.Y;

            const int tolerance = 5;

            Edges result = Edges.None;

            if (mx <= area.Left + tolerance) result |= Edges.Left;
            if (mx >= area.Right - tolerance) result |= Edges.Right;
            if (my <= area.Top + tolerance) result |= Edges.Top;
            if (my >= area.Bottom - tolerance) result |= Edges.Bottom;

            return result;
        }


        private void HandleEdgeWheel(int direction) {

            _activeEdges = GetMouseEdges();
            if (_activeEdges == Edges.None) return;

            if (!IsWidgetOnSurface()) ToggleWidgetCollapse();

            var    area        = Screen.FromPoint(Control.MousePosition).WorkingArea;
            int    stepX       = area.Width / 32;
            double aspectRatio = (double)area.Width / area.Height;

            bool isCorner = (_activeEdges.HasFlag(Edges.Left) || _activeEdges.HasFlag(Edges.Right)) &&
                            (_activeEdges.HasFlag(Edges.Top)  || _activeEdges.HasFlag(Edges.Bottom));

            if (isCorner) {
                int deltaX = direction * stepX;
                int deltaY = (int)Math.Round(deltaX / aspectRatio);

                _targetWidth  = Math.Max(200, Math.Min(_targetWidth  + deltaX, (int)(area.Width  * 0.6)));
                _targetHeight = Math.Max(200, Math.Min(_targetHeight + deltaY, (int)(area.Height * 0.6)));

                _targetWidth  = (int)Math.Round(_targetHeight * aspectRatio);
            }
            else {
                if (_activeEdges.HasFlag(Edges.Left) || _activeEdges.HasFlag(Edges.Right)) {
                    _targetWidth = Math.Max(200, Math.Min(_targetWidth + (direction * stepX), (int)(area.Width * 0.6)));
                }

                if (_activeEdges.HasFlag(Edges.Top) || _activeEdges.HasFlag(Edges.Bottom)) {
                    int stepY = area.Height / 32;
                    _targetHeight = Math.Max(200, Math.Min(_targetHeight + (direction * stepY), (int)(area.Height * 0.6)));
                }
            }

            if (!_resizeAnimationTimer.Enabled) _resizeAnimationTimer.Start();
        }


        private void ResizeAnimationTimer_Tick(object sender, EventArgs e) {

            if (this.Width == _targetWidth && this.Height == _targetHeight) {
                _resizeAnimationTimer.Stop();
                _activeEdges = Edges.None;
                return;
            }

            int NextDimension(int current, int target) {
                if (current == target) return current;
                int step = (target - current) / ANIMATION_SPEED;
                return current + (step != 0 ? step : Math.Sign(target - current));
            }

            var area = Screen.FromControl(this).WorkingArea;

            int borderX = SystemInformation.FrameBorderSize.Width * 2;
            int borderY = SystemInformation.FrameBorderSize.Height * 2;

            this.SuspendLayout();

            this.Width  = NextDimension(this.Width, _targetWidth);
            this.Height = NextDimension(this.Height, _targetHeight);

            if (_activeEdges.HasFlag(Edges.Right))  this.Left = area.Right  - this.Width + borderX;
            if (_activeEdges.HasFlag(Edges.Left))   this.Left = area.Left   - borderX;
            if (_activeEdges.HasFlag(Edges.Bottom)) this.Top  = area.Bottom - this.Height + borderY;
            if (_activeEdges.HasFlag(Edges.Top))    this.Top  = area.Top;

            if (_activeEdges == Edges.Left || _activeEdges == Edges.Right) {
                this.Top = area.Y;
                _targetHeight = this.Height = area.Height + borderY;
            }

            else if (_activeEdges == Edges.Top || _activeEdges == Edges.Bottom) {
                this.Left = area.Left - borderX;
                _targetWidth = this.Width = area.Width + borderX * 2;
            }

            this.ResumeLayout(true);
        }


        private void Gem_Resize(object sender, EventArgs e) {

            //if (this.WindowState == FormWindowState.Normal || this.WindowState == FormWindowState.Maximized)
            //  await webView.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, document.documentElement.scrollHeight);");

            if (!_resizeAnimationTimer.Enabled) {
                _targetWidth = this.Width;
            }
        }


        private void OpacityTimer_Tick(object sender, EventArgs e) {

            if (Math.Abs(this.Opacity - _targetOpacity) < 0.001) {
                _opacityTimer.Stop();
                return;
            }

            int direction = this.Opacity < _targetOpacity ? 1 : -1;
            this.Opacity += OPACITY_STEP * direction;
        }


        private void OpacityTransition(double targetOpacity) {

            _targetOpacity = targetOpacity;
            _opacityTimer.Start();
        }

    }
}
