using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gem {

    public partial class Gem : Form {

        private async Task InitializeWebViewWithCustomCache() {

            try {
                var env = await CoreWebView2Environment.CreateAsync(null, _browserCachePath);
                await webView.EnsureCoreWebView2Async(env);

            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }
        }


        private async void webView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e) {

            if (!e.IsSuccess) {
                if (e.InitializationException != null) {
                    MessageBox.Show(
                        $"Ошибка инициализации: {e.InitializationException.Message}\n\n" +
                        $"Код ошибки (HRESULT): {e.InitializationException.HResult}\n\n" +
                        $"Стек:\n{e.InitializationException.StackTrace}",
                        "Критический сбой WebView2"
                    );
                }
                else {
                    MessageBox.Show("Инициализация завершилась неудачей по неизвестной причине (Exception = null).");
                }
                return;
            }

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

            string mouseupListening = Helpers.GetEmbeddedScript("MouseListening.js");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(mouseupListening);

            string hideMicCss = Helpers.GetEmbeddedScript("HideMic.js");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(hideMicCss);

            webView.CoreWebView2.ContextMenuRequested += this.rmbClick;
            webView.CoreWebView2.Navigate(homeUri);
        }


        private void webView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e) {

            try {
                string message = e.TryGetWebMessageAsString();

                if (string.IsNullOrEmpty(message)) return;

                if (message == "CLOSE_MENU") {
                    this.DisposeCurrentMenu();
                    return;
                }

                if (message.StartsWith("AUTO_COPY|||")) {
                    string textToCopy = message.Substring("AUTO_COPY|||".Length);
                    this.Invoke(new Action(() => Clipboard.SetText(textToCopy)));
                }
                if (message.StartsWith("COPY_LINK|||")) {
                    string urlToCopy = message.Substring("COPY_LINK|||".Length);
                    this.Invoke(new Action(() => Clipboard.SetText(urlToCopy)));
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

            try {
                string resetScript = Helpers.GetEmbeddedScript("ResetFocus.js");
                await webView.CoreWebView2.ExecuteScriptAsync(resetScript);
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }

            if (_currentMenu != null) this.DisposeCurrentMenu();

            string clipboardText = "";
            try {
                if (Clipboard.ContainsText()) {
                    clipboardText = Clipboard.GetText() ?? "";
                }
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }

            string targetSection = null;
            if      (modifiers.HasFlag(Keys.Control)) targetSection = "CTRL";
            else if (modifiers.HasFlag(Keys.Shift))   targetSection = "SHIFT";
            else if (modifiers.HasFlag(Keys.Alt))     targetSection = "ALT";

            // terminal mode:
            if (targetSection == null) {
                if (string.IsNullOrWhiteSpace(clipboardText))
                    return;
                string terminalPrompt = PromptManager.PrepareFullPrompt(clipboardText);
                await this.SendPromptAsync(terminalPrompt);
                return;
            }

            // template menu mode:
            var promptsDict = PromptManager.GetPrompts();
            if (!promptsDict.TryGetValue(targetSection, out string[] activePrompts) || activePrompts.Length == 0) return;

            _currentMenu = this.BuildContextMenu(activePrompts, clipboardText);

            if (_currentMenu.Items.Count == 0) {
                this.DisposeCurrentMenu();
                return;
            }

            _currentMenu.Show(Cursor.Position);
        }


        private ContextMenuStrip BuildContextMenu(string[] prompts, string clipboardText) {

            var menu = new ContextMenuStrip();
            bool isClipboardEmpty = string.IsNullOrEmpty(clipboardText);

            foreach (string promptTemplate in prompts) {

                bool hasClip = promptTemplate.Contains(PromptManager.PLACEHOLDER);
                if (hasClip && isClipboardEmpty)
                    continue;

                string menuTitle = this.GetMenuTitle(promptTemplate, hasClip);
                var menuItem = new ToolStripMenuItem(menuTitle);

                menuItem.Click += async (s, ev) => {
                    string finalPrompt = PromptManager.PrepareFullPrompt(promptTemplate, clipboardText);
                    await this.SendPromptAsync(finalPrompt);
                };

                menu.Items.Add(menuItem);
            }

            return menu;
        }


        private string GetMenuTitle(string promptTemplate, bool hasClip) {

            string firstLine = promptTemplate.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

            var lineInfo = new System.Globalization.StringInfo(firstLine);
            if (lineInfo.LengthInTextElements > 45) {
                firstLine = lineInfo.SubstringByTextElements(0, 42) + "...";
            }

            string marker = hasClip ? " [clip]" : "";
            return $"{firstLine}{marker}";
        }


        private void DisposeCurrentMenu() {
            if (_currentMenu != null) {
                _currentMenu.Close();
                _currentMenu.Dispose();
                _currentMenu = null;
            }
        }


        private async Task SendPromptAsync(string prompt) {

            string safePrompt     = _serializer.Serialize(prompt);
            string scriptTemplate = Helpers.GetEmbeddedScript("InsertAndSend.js");
            string finalScript    = scriptTemplate.Replace("/*PLACEHOLDER*/", safePrompt);

            await webView.CoreWebView2.ExecuteScriptAsync(finalScript);
        }


        private async void ClearWebViewCache() {

            if (webView == null || webView.CoreWebView2 == null) return;

            CoreWebView2BrowsingDataKinds dataKinds =
                CoreWebView2BrowsingDataKinds.DiskCache |
                CoreWebView2BrowsingDataKinds.CacheStorage;

            await webView.CoreWebView2.Profile.ClearBrowsingDataAsync(dataKinds);
            await RunCacheCalculationAsync();
        }


        private void btnClearCache_Click(object sender, EventArgs e) => ClearWebViewCache();


        private void btnHome_Click(object sender, EventArgs e) => webView.CoreWebView2.Navigate(homeUri);
    }
}
