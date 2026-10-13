using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace Gem {

    [System.ComponentModel.DesignerCategory("")]
    public partial class Gem : Form {

        #region fields

        private readonly static string homeUri             = "https://www.google.com/ai";
        private readonly static string _localAppData       = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly static string _browserCachePath   = Path.Combine(_localAppData, "Gem", "WebView2_Cache");
        private readonly JavaScriptSerializer _serializer  = new JavaScriptSerializer();

        private ContextMenuStrip _currentMenu              = null;

        private readonly Timer _opacityTimer               = new Timer();
        private readonly Timer _resizeAnimationTimer       = new Timer();
        private readonly Timer _memoryTimer                = new Timer();
        private readonly Timer _cacheTimer                 = new Timer();
                                                          
        private MouseHook      _mouseHook;                
        private MouseHook      _opacityWheelHook;         
        private MouseHook      _globalWheelHook;          
                                                          
        private KeyboardHook   _collapseHook;             
        private KeyboardHook   _voiceKeyboardHook;
                                                          
        private object         _voiceRecognizerInstance;  
        private MethodInfo     _voiceStartMethod;         
        private MethodInfo     _voiceStopMethod;          
        private bool           _isVoiceModuleReady         = false;
        private bool           _isVoiceRecording           = false;

        private const double   OPACITY_STEP                = 0.05;
        private const double   OPACITY_OPAQUE              = 1.0;
        private const int      ANIMATION_SPEED             = 4;

        private double         _targetOpacity              = 1.0;      
        private double         _opacityTransparent         = 0.6;
               
        private double _OpacityTransparent {
            get => _opacityTransparent;
            set => _opacityTransparent = Math.Max(0.2, Math.Min(value, 0.9));
        }

        private int _targetWidth;
        private int _targetHeight;

        #endregion


        #region initialization


        public Gem() {

            InitializeComponent();

            _targetWidth = this.Width;
            _targetHeight   = this.Height;

            btnOpacity.Text = _OpacityTransparent.ToString();

            #region timers

            _opacityTimer.Interval = 16;
            _opacityTimer.Tick += OpacityTimer_Tick;

            _resizeAnimationTimer.Interval = 15;
            _resizeAnimationTimer.Tick += ResizeAnimationTimer_Tick;

            _memoryTimer.Interval = 2000;
            _memoryTimer.Tick += MemoryTimer_Tick;
            _memoryTimer.Start();

            _cacheTimer.Interval = 120000;
            _cacheTimer.Tick += CacheTimer_Tick;           
            _cacheTimer.Start();

            #endregion

            #region hooks

            _mouseHook = new MouseHook(this);
            _mouseHook.MouseEnter += (s, e) => { OpacityTransition(OPACITY_OPAQUE); };
            _mouseHook.MouseLeave += (s, e) => { OpacityTransition(_OpacityTransparent); };

            _opacityWheelHook = new MouseHook(this.btnOpacity);

            void ChangeOpacityTransparent (double d) {
                _OpacityTransparent += d;
                this.Opacity = _OpacityTransparent;
                btnOpacity.Text = _OpacityTransparent.ToString();
            };

            _opacityWheelHook.MouseOverWheelUp   += (s, e) => { ChangeOpacityTransparent(0.05);  };
            _opacityWheelHook.MouseOverWheelDown += (s, e) => { ChangeOpacityTransparent(-0.05); };

            var settings = Properties.Settings.Default;

            _collapseHook = new KeyboardHook(
                targetControl:  this,
                targetKey:      settings.CollapseKey,
                requireCtrl:    false,
                requireShift:   false,
                requireAlt:     false,
                trackDoubleTap: settings.CollapseRequireDouble
            );

            _collapseHook.HotKeyDoublePressed += (s, e) => { if (settings.CollapseRequireDouble)  ToggleWidgetCollapse(); };
            _collapseHook.HotKeyPressed       += (s, e) => { if (!settings.CollapseRequireDouble) ToggleWidgetCollapse(); };

            _globalWheelHook = new MouseHook(this);

            int dWidth = Screen.FromControl(this).WorkingArea.Width / 32;

            _globalWheelHook.MouseWheelUp   += (s, e) => HandleEdgeWheel(1);
            _globalWheelHook.MouseWheelDown += (s, e) => HandleEdgeWheel(-1);

            #endregion

            #region tooltips

            toolTips.AutoPopDelay = 5000;
            toolTips.InitialDelay = 1000;
            toolTips.ReshowDelay  = 500;
            toolTips.ShowAlways   = true;

            toolTips.SetToolTip(btnHome, "Home");
            toolTips.SetToolTip(btnPin, "Pin");
            toolTips.SetToolTip(btnOpacity, "Adjust opacity with the mouse wheel");
            toolTips.SetToolTip(btnClearCache, "Clear cache");

            string GetVoiceTooltipText() {

                var parts    = new List<string>();

                if (settings.SttRequireCtrl)  parts.Add("Ctrl");
                if (settings.SttRequireAlt)   parts.Add("Alt");
                if (settings.SttRequireShift) parts.Add("Shift");

                parts.Add(settings.SttKey.ToString());
                string shortcut = string.Join("+", parts);

                return $"Hold {shortcut} to speak";
            }

            void ShowStatusToolTip(string text) {

                Point localPos = this.PointToClient(Cursor.Position);
                localPos.Offset(15, 15);
                toolTips.Show(text, this, localPos);
            }

            tsLblSTT   .MouseEnter += (s, e) => ShowStatusToolTip(GetVoiceTooltipText());
            tsLblMemory.MouseEnter += (s, e) => ShowStatusToolTip("Memory usage: main process + WebView2");
            tsLblCache .MouseEnter += (s, e) => ShowStatusToolTip(_browserCachePath);

            tsLblSTT   .MouseLeave += (s, e) => toolTips.Hide(this);
            tsLblMemory.MouseLeave += (s, e) => toolTips.Hide(this);
            tsLblCache .MouseLeave += (s, e) => toolTips.Hide(this);

            #endregion

        }


        private async void Gem_Load(object sender, EventArgs e) {

            var area    = Screen.FromControl(this).WorkingArea;
            this.Width  = (int)(area.Width * 0.42);
            this.Left   = area.Right - this.Width + SystemInformation.FrameBorderSize.Width * 2;
            this.Top    = area.Y;
            this.Height = area.Height + SystemInformation.FrameBorderSize.Height * 2;

            await this.InitializeWebViewWithCustomCache();
            await this.RunCacheCalculationAsync();
            await this.InitVoiceModule();
        }

        #endregion


        #region dispose

        private void Gem_FormClosing(object sender, FormClosingEventArgs e) {

            try {

                _opacityTimer.Stop();
                _opacityTimer.Dispose();

                _resizeAnimationTimer.Stop();
                _resizeAnimationTimer.Dispose();

                _memoryTimer.Stop();
                _memoryTimer.Dispose();

                _cacheTimer.Stop();
                _cacheTimer.Dispose();

                _mouseHook?.Dispose();
                _opacityWheelHook?.Dispose();
                _globalWheelHook?.Dispose();

                _voiceKeyboardHook?.Dispose();
                _collapseHook?.Dispose();

                if (_voiceRecognizerInstance is IDisposable disposableVoice) {
                    disposableVoice.Dispose();
                }
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }
        }

        #endregion
    }
}
