using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;


[assembly: InternalsVisibleTo("Tests")]
namespace Gem {
    internal static class PromptManager {


        #region private fields


        internal static readonly string _configPath;
        private  static DateTime _lastLoadedTime;
        private  static Dictionary<string, string[]> _prompts;

        public const string PLACEHOLDER       = "{clip}";
        public const int    MAX_PROMPT_LENGTH = 6000;

        static PromptManager() {

            _configPath     = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prompts.ini");
            _prompts        = new Dictionary<string, string[]>();
            _lastLoadedTime = DateTime.MinValue;
        }


        #endregion


        #region public methods


        public static Dictionary<string, string[]> GetPrompts() {

            try {
                if (!File.Exists(_configPath)) CreateDefaultPromptFile();

                DateTime currentWriteTime = File.GetLastWriteTime(_configPath);

                if (currentWriteTime != _lastLoadedTime) {
                    _prompts        = PromptParser.Parse(_configPath);
                    _lastLoadedTime = currentWriteTime;
                }
            }
            catch (Exception e) {
                Helpers.LogError(e);
            }

            return _prompts;
        }


        public static string PrepareFullPrompt(string clipboardText)
            => Truncate(clipboardText);


        public static string PrepareFullPrompt(string promptTemplate, string clipboardText) {

            if (string.IsNullOrEmpty(promptTemplate)) {
                return PrepareFullPrompt(clipboardText);
            }

            string fullPrompt = promptTemplate.Contains(PLACEHOLDER)
                ? promptTemplate.Replace(PLACEHOLDER, clipboardText)
                : promptTemplate;

            return Truncate(fullPrompt);
        }


        #endregion


        #region private and auxiliary methods


        private static string Truncate(string text) {

            if (string.IsNullOrEmpty(text)) return string.Empty;

            var si = new System.Globalization.StringInfo(text);

            return si.LengthInTextElements > MAX_PROMPT_LENGTH
                ? si.SubstringByTextElements(0, MAX_PROMPT_LENGTH)
                : text;
        }


        internal static void ResetCache() {
            _lastLoadedTime = DateTime.MinValue;
            _prompts        = new Dictionary<string, string[]>();
        }


        private static void CreateDefaultPromptFile() {

            string defaultContent = Helpers.GetEmbeddedScript("DefaultPrompts.ini");
            var utf8WithoutBom = new UTF8Encoding(false);

            File.WriteAllText(_configPath, defaultContent, utf8WithoutBom);
        }


        #endregion
    }
}