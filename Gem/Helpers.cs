using System;
using System.IO;
using System.Text;




namespace Gem {
    internal static class Helpers {

        private readonly static string logFile = "errors.txt";

        public static string GetEmbeddedScript(string scriptName) {

            var    assembly     = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.scripts.{scriptName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                if (stream == null) throw new FileNotFoundException($"Script resource not found: {resourceName}");
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public static void LogError(Exception ex) {
            try {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
                string logText = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}{3}",
                    DateTime.Now, ex.GetType().Name, ex.Message, Environment.NewLine);

                File.AppendAllText(logPath, logText, Encoding.UTF8);
            }
            catch {

            }
        }
    }
}
