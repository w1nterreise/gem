using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace Gem {
    public partial class Gem : Form {


        #region private fields


        private readonly string               homeUri     = "https://www.google.com/search?udm=50";
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();


        private ContextMenuStrip _currentMenu = null;


        //opacity animation:
        private readonly Timer  _opacityTimer       = new Timer();
        private          double _targetOpacity      = 1.0;
        private const    double OPACITY_STEP        = 0.05;
        private const    double OPACITY_TRANSPARENT = 0.4;
        private const    double OPACITY_OPAQUE      = 1.0;


        #endregion


        #region initialization


        public Gem() {

            InitializeComponent();

            _opacityTimer.Interval = 16;
            _opacityTimer.Tick += OpacityTimer_Tick;

            this.InitializeWebViewWithCustomCache();
        }


        private async void InitializeWebViewWithCustomCache() {

            try {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string cachePath = Path.Combine(localAppData, "Gem", "WebView2_Cache");

                var env = await CoreWebView2Environment.CreateAsync(null, cachePath);
                await webView.EnsureCoreWebView2Async(env);
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }
        }


        private void Gem_Load(object sender, EventArgs e) {

            var area = Screen.FromControl(this).WorkingArea;
            this.Width = (int)(area.Width * 0.42);
            this.Left = area.Right - this.Width + SystemInformation.FrameBorderSize.Width * 2;
            this.Top = area.Y;
            this.Height = area.Height + SystemInformation.FrameBorderSize.Height * 2;
        }


        private async void webView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e) {

            if (!e.IsSuccess) return;

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

            string mouseupListening = Helpers.GetEmbeddedScript("MouseListening.js");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(mouseupListening);

            webView.CoreWebView2.ContextMenuRequested += this.rmbClick;

            webView.CoreWebView2.WebResourceResponseReceived += this.webView_WebResourceResponseReceived;

            webView.CoreWebView2.Navigate(homeUri);
        }


        private void webView_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e) {
            //authorization freezing fix
            try {
                if (e.Request.Uri.Contains("accounts.google.com/CheckCookie")) {
                    // if the server returns a 302, then authorization was successful,
                    // but WebView2 often gets stuck processing the session
                    if (e.Response.StatusCode == 302) {
                        this.BeginInvoke(new Action(() => {
                            webView.CoreWebView2.Navigate(homeUri);
                        }));
                    }
                }
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }
        }


        #endregion


        #region major controls


        private void webView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e) {

            try {
                string message = e.TryGetWebMessageAsString();

                if (string.IsNullOrEmpty(message)) return;

                if (message == "CLOSE_MENU") {
                    if (_currentMenu != null) {
                        _currentMenu.Close();
                        _currentMenu = null;
                    }
                    return;
                }

                if (message.StartsWith("AUTO_COPY|||")) {
                    string textToCopy = message.Substring("AUTO_COPY|||".Length);
                    Clipboard.SetText(textToCopy);
                }
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }
        }


        private async void rmbClick(object sender, CoreWebView2ContextMenuRequestedEventArgs e) {

            var modifiers = Control.ModifierKeys;

            if (modifiers.HasFlag(Keys.Control) && modifiers.HasFlag(Keys.Shift))
                return;

            e.Handled = true;

            if (_currentMenu != null) {
                _currentMenu.Dispose();
                _currentMenu = null;
            }

            string clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText)) return;

            string targetSection = null;
            if (modifiers.HasFlag(Keys.Control)) targetSection = "CTRL";
            else if (modifiers.HasFlag(Keys.Shift)) targetSection = "SHIFT";
            else if (modifiers.HasFlag(Keys.Alt)) targetSection = "ALT";

            // terminal mode:
            if (targetSection == null) {
                string terminalPrompt = PromptManager.PrepareFullPrompt(clipboardText);
                await this.SendPromptAsync(terminalPrompt);
                return;
            }

            // template menu mode:
            var promptsDict = PromptManager.GetPrompts();
            if (!promptsDict.TryGetValue(targetSection, out string[] activePrompts) || activePrompts.Length == 0) return;

            _currentMenu = this.BuildContextMenu(activePrompts, clipboardText);
            _currentMenu.Show(Cursor.Position);
        }


        private ContextMenuStrip BuildContextMenu(string[] prompts, string clipboardText) {

            var menu = new ContextMenuStrip();

            for (int i = 0; i < prompts.Length; i++) {
                string promptTemplate = prompts[i];
                string menuTitle = this.GetMenuTitle(promptTemplate, i);

                var menuItem = new ToolStripMenuItem(menuTitle);
                menuItem.Click += async (s, ev) => {
                    string finalPrompt = PromptManager.PrepareFullPrompt(promptTemplate, clipboardText);
                    await this.SendPromptAsync(finalPrompt);
                };

                menu.Items.Add(menuItem);
            }

            return menu;
        }


        private string GetMenuTitle(string promptTemplate, int index) {

            string firstLine = promptTemplate.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

            var lineInfo = new System.Globalization.StringInfo(firstLine);
            if (lineInfo.LengthInTextElements > 45) {
                firstLine = lineInfo.SubstringByTextElements(0, 42) + "...";
            }

            bool hasClip = promptTemplate.Contains(PromptManager.PLACEHOLDER);
            string marker = hasClip ? " [clip]" : "";
            return $"{index + 1}. {firstLine}{marker}";
        }


        private async Task SendPromptAsync(string prompt) {

            string safePrompt     = _serializer.Serialize(prompt);
            string scriptTemplate = Helpers.GetEmbeddedScript("InsertAndSend.js");
            string finalScript    = scriptTemplate.Replace("/*PLACEHOLDER*/", safePrompt);

            await webView.CoreWebView2.ExecuteScriptAsync(finalScript);
        }


        #endregion


        #region minor controls and animation


        private void OpacityTransition(double targetOpacity) {

            _targetOpacity = targetOpacity;
            _opacityTimer.Start();
        }


        private void Gem_Activated(object sender, EventArgs e) => OpacityTransition(OPACITY_OPAQUE);
        private void Gem_Deactivate(object sender, EventArgs e) => OpacityTransition(OPACITY_TRANSPARENT);

        private void OpacityTimer_Tick(object sender, EventArgs e) {

            if (Math.Abs(this.Opacity - _targetOpacity) < 0.001) {
                _opacityTimer.Stop();
                return;
            }

            int direction = this.Opacity < _targetOpacity ? 1 : -1;
            this.Opacity += OPACITY_STEP * direction;
        }


        private void btnHome_Click(object sender, EventArgs e) => webView.CoreWebView2.Navigate(homeUri);


        private void btnPin_CheckedChanged(object sender, EventArgs e) => this.TopMost = btnPin.Checked;

        #endregion
    }
}
