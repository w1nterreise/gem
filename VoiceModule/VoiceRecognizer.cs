using System;
using System.IO;
using System.Text;
using Vosk;
using NAudio.Wave;

namespace VoiceModule {
    public class VoiceRecognizer : IDisposable {

        #region privates

        private Model          _voskModel;
        private VoskRecognizer _voskRecognizer;
        private WaveInEvent    _waveIn;
        private StringBuilder  _collectedText = new StringBuilder();

        private readonly static string _modelDir       = "model";
        private readonly object        _lockObj        = new object();
        private volatile bool          _canAcceptAudio = false;

        private bool _isDisposed = false;

        #endregion


        public bool Init() {

            try {
                string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "STT", _modelDir);
                if (!Directory.Exists(modelPath)) return false;

                Vosk.Vosk.SetLogLevel(-1);
                _voskModel = new Model(modelPath);
                _voskRecognizer = new VoskRecognizer(_voskModel, 16000.0f);
                return true;
            }
            catch { return false; }
        }

        public void Start() {

            if (_isDisposed) throw new ObjectDisposedException(nameof(VoiceRecognizer));

            lock (_lockObj) {
                _collectedText.Clear();
                _canAcceptAudio = true;
            }

            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = new WaveFormat(16000, 1);

            _waveIn.DataAvailable += (sender, e) => {

                if (!_canAcceptAudio) return;

                lock (_lockObj) {
                    if (!_canAcceptAudio || _voskRecognizer == null) return;

                    try {
                        if (_voskRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded)) {
                            string partialText = ParseTextFromJson(_voskRecognizer.Result());
                            if (!string.IsNullOrEmpty(partialText)) {
                                _collectedText.Append(partialText + " ");
                            }
                        }
                    }
                    catch {
                    }
                }
            };

            _waveIn.StartRecording();
        }

        public string Stop() {

            if (_isDisposed) return string.Empty;

            lock (_lockObj) {
                _canAcceptAudio = false;
            }

            if (_waveIn != null) {
                try {
                    _waveIn.StopRecording();
                }
                catch { }
                _waveIn.Dispose();
                _waveIn = null;
            }

            lock (_lockObj) {
                if (_voskRecognizer != null) {
                    string jsonFinal = _voskRecognizer.FinalResult();
                    string finalText = ParseTextFromJson(jsonFinal);
                    if (!string.IsNullOrEmpty(finalText)) {
                        _collectedText.Append(finalText);
                    }
                }

                return _collectedText.ToString().Trim();
            }
        }

        private string ParseTextFromJson(string json) {
            int start = json.IndexOf("\"text\" : \"") + 10;
            int end = json.LastIndexOf("\"");
            if (start > 9 && end > start) return json.Substring(start, end - start).Trim();
            return string.Empty;
        }


        public void Dispose() {

            if (_isDisposed) return;

            lock (_lockObj) {
                _canAcceptAudio = false;
                _isDisposed = true;

                if (_waveIn != null) {
                    try { _waveIn.StopRecording(); } catch { }
                    _waveIn.Dispose();
                    _waveIn = null;
                }

                if (_voskRecognizer != null) {
                    _voskRecognizer.Dispose();
                    _voskRecognizer = null;
                }

                if (_voskModel != null) {
                    _voskModel.Dispose();
                    _voskModel = null;
                }
            }
        }
    }
}
