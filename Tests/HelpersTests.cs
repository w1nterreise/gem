using Gem;

namespace Tests {
    [TestClass]
    [DoNotParallelize]
    public class HelpersTests {

        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errors.txt");

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void GetEmbeddedScript_WhenResourceDoesNotExist_ShouldThrowFileNotFoundException() {

            Helpers.GetEmbeddedScript("NonExistentFile.js");
        }

        [TestMethod]
        public void GetEmbeddedScript_WhenResourceExists_ShouldReturnContent() {

            string script = Helpers.GetEmbeddedScript("InsertAndSend.js");

            Assert.IsFalse(string.IsNullOrEmpty(script));
            Assert.IsTrue(script.Contains("function"));
        }

        [TestMethod]
        public void LogError_ShouldCreateFileAndAppendText() {

            if (File.Exists(LogPath)) File.Delete(LogPath);

            var exception = new InvalidOperationException("Test error message");
            Helpers.LogError(exception);

            Assert.IsTrue(File.Exists(LogPath));

            string logContent = File.ReadAllText(LogPath);
            Assert.IsTrue(logContent.Contains("InvalidOperationException"));
            Assert.IsTrue(logContent.Contains("Test error message"));
        }

        [TestMethod]
        public void LogError_MultipleCalls_ShouldAppendSuccessfully() {

            if (File.Exists(LogPath)) File.Delete(LogPath);

            Helpers.LogError(new Exception("Error 1"));
            Helpers.LogError(new Exception("Error 2"));

            string[] lines = File.ReadAllLines(LogPath);

            Assert.AreEqual(2, lines.Length);
            Assert.IsTrue(lines[0].Contains("Error 1"));
            Assert.IsTrue(lines[1].Contains("Error 2"));
        }
    }
}