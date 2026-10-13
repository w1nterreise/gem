using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gem {
    public partial class Gem : Form {

        private async Task InitVoiceModule() {

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "STT", "VoiceModule.dll");
            if (!File.Exists(dllPath)) return;

            try {
                Assembly asm = Assembly.LoadFrom(dllPath);
                Type type = asm.GetType("VoiceModule.VoiceRecognizer");

                _voiceRecognizerInstance = Activator.CreateInstance(type);

                MethodInfo initMethod = type.GetMethod("Init");
                _voiceStartMethod     = type.GetMethod("Start");
                _voiceStopMethod      = type.GetMethod("Stop");

                tsLblSTT.Visible = true;
                tsLblSTT.Text    = "Loading STT module...";

                bool initSuccess = await Task.Run(() =>
                     (bool)initMethod.Invoke(_voiceRecognizerInstance, null)
                );

                if (initSuccess) {
                    _isVoiceModuleReady = true;
                    _voiceKeyboardHook = new KeyboardHook(this, Properties.Settings.Default.SttKey,
                        requireCtrl:  Properties.Settings.Default.SttRequireCtrl,
                        requireShift: Properties.Settings.Default.SttRequireShift,
                        requireAlt:   Properties.Settings.Default.SttRequireAlt);
                    _voiceKeyboardHook.HotKeyPressed  += VoiceHotKey_HotKeyPressed;
                    _voiceKeyboardHook.HotKeyReleased += VoiceHotKey_HotKeyReleased;
                    tsLblSTT.Text = "STT ON";
                }
                else {
                    tsLblSTT.Text = "Failed to load STT module.";
                }
            }
            catch (Exception ex) {
                Helpers.LogError(ex);
            }
        }


        private void VoiceHotKey_HotKeyPressed(object sender, EventArgs e) {

            if (!_isVoiceModuleReady || _isVoiceRecording) return;
            _isVoiceRecording = true;
            _voiceStartMethod.Invoke(_voiceRecognizerInstance, null);
        }


        private async void VoiceHotKey_HotKeyReleased(object sender, EventArgs e) {

            if (!_isVoiceModuleReady || !_isVoiceRecording) return;
            _isVoiceRecording = false;
            string resultText = (string)_voiceStopMethod.Invoke(_voiceRecognizerInstance, null);
            if (!string.IsNullOrEmpty(resultText)) await this.SendPromptAsync($"<STT>{resultText}</STT>");
        }
    }
}
