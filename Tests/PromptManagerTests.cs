using Gem;

namespace Tests {
    [TestClass]
    [DoNotParallelize]
    public class PromptManagerTests {

        [TestMethod]
        public void GetPrompts_ShouldCreateDefaultFile_WhenFileDoesNotExist() {

            if (File.Exists(PromptManager._configPath)) File.Delete(PromptManager._configPath);

            var prompts = PromptManager.GetPrompts();

            Assert.IsTrue(File.Exists(PromptManager._configPath));
            Assert.IsTrue(prompts.ContainsKey("CTRL"));
            Assert.IsTrue(prompts.ContainsKey("SHIFT"));
            Assert.IsTrue(prompts.ContainsKey("ALT"));
        }

        [TestMethod]
        public void GetPrompts_HotReload_ShouldReflectChangesOnDisk() {

            PromptManager.GetPrompts();

            string customConfig = "[CTRL]\nCustom Prompt 1\n\nCustom Prompt 2";
            File.WriteAllText(PromptManager._configPath, customConfig, System.Text.Encoding.UTF8);
            File.SetLastWriteTime(PromptManager._configPath, DateTime.Now.AddSeconds(5));

            var updatedPrompts = PromptManager.GetPrompts();

            Assert.IsTrue(updatedPrompts.ContainsKey("CTRL"));
            Assert.AreEqual(2, updatedPrompts["CTRL"].Length);
            Assert.AreEqual("Custom Prompt 1", updatedPrompts["CTRL"][0]);
        }

        [TestMethod]
        public void GetPrompts_WhenFileIsEmpty_ShouldReturnEmptyDictionary() {

            File.WriteAllText(PromptManager._configPath, string.Empty, System.Text.Encoding.UTF8);
            PromptManager.ResetCache();

            var result = PromptManager.GetPrompts();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetPrompts_WhenFileContainsOnlyComments_ShouldReturnEmptyDictionary() {

            string config = "# Just a comment\n# Another one\n\n# Empty line with comment";
            File.WriteAllText(PromptManager._configPath, config, System.Text.Encoding.UTF8);
            PromptManager.ResetCache();

            var result = PromptManager.GetPrompts();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetPrompts_WhenFileHasTextButNoSections_ShouldReturnEmptyDictionary() {

            string config = "Translate this text please\n\nAnd fix errors";
            File.WriteAllText(PromptManager._configPath, config, System.Text.Encoding.UTF8);
            PromptManager.ResetCache();

            var result = PromptManager.GetPrompts();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetPrompts_WhenFileIsLocked_ShouldReturnCachedVersion() {

            string validConfig = "[CTRL]\nInitial Prompt";
            File.WriteAllText(PromptManager._configPath, validConfig, System.Text.Encoding.UTF8);
            PromptManager.ResetCache();
            PromptManager.GetPrompts();

            using (var stream = new FileStream(PromptManager._configPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {

                var result = PromptManager.GetPrompts();

                Assert.IsNotNull(result);
                Assert.IsTrue(result.ContainsKey("CTRL"));
                Assert.AreEqual("Initial Prompt", result["CTRL"][0]);
            }
        }

        [TestMethod]
        public void PrepareFullPrompt_TerminalMode_ShouldOnlyTruncate() {

            string shortInput = "Hello";
            string longInput  = new string('A', 7000);

            string shortResult = PromptManager.PrepareFullPrompt(shortInput);
            string longResult  = PromptManager.PrepareFullPrompt(longInput);

            Assert.AreEqual("Hello", shortResult);
            Assert.AreEqual(6000, longResult.Length);
        }

        [TestMethod]
        public void PrepareFullPrompt_WithPlaceholder_ShouldReplacePlaceholder() {

            string template  = "Fix this code: {clip} and make it fast.";
            string clipboard = "Console.WriteLine();";

            string result = PromptManager.PrepareFullPrompt(template, clipboard);

            Assert.AreEqual("Fix this code: Console.WriteLine(); and make it fast.", result);
        }

        [TestMethod]
        public void PrepareFullPrompt_WithoutPlaceholder_ShouldIgnoreClipboard() {

            string template  = "Write a fresh joke about AI.";
            string clipboard = "Some ignored clipboard text";

            string result = PromptManager.PrepareFullPrompt(template, clipboard);

            Assert.AreEqual("Write a fresh joke about AI.", result);
        }

        [TestMethod]
        public void PrepareFullPrompt_WhenTemplateIsEmpty_ShouldFallbackToTerminalMode() {

            string template  = "";
            string clipboard = "Direct text";

            string result = PromptManager.PrepareFullPrompt(template, clipboard);

            Assert.AreEqual("Direct text", result);
        }

        [TestMethod]
        public void PrepareFullPrompt_OneCharacterOverLimit_ShouldTruncateExactlyToOneChar() {

            string template = "Prompt: {clip}";
            string clipboard = new string('A', 6001 - "Prompt: ".Length);

            string result = PromptManager.PrepareFullPrompt(template, clipboard);

            Assert.AreEqual(6000, result.Length);
        }

        [TestMethod]
        public void PrepareFullPrompt_EmptyAndNullInputs_ShouldReturnEmptyString() {

            Assert.AreEqual(string.Empty, PromptManager.PrepareFullPrompt(null));
            Assert.AreEqual(string.Empty, PromptManager.PrepareFullPrompt(string.Empty));
            Assert.AreEqual(string.Empty, PromptManager.PrepareFullPrompt(null, null));
            Assert.AreEqual(string.Empty, PromptManager.PrepareFullPrompt("", ""));
        }

        [TestMethod]
        public void PrepareFullPrompt_MultiplePlaceholders_ShouldReplaceAllOccurrences() {

            string template = "Compare this: {clip} with this: {clip}";
            string clipboard = "Data";

            string result = PromptManager.PrepareFullPrompt(template, clipboard);

            Assert.AreEqual("Compare this: Data with this: Data", result);
        }

    }
}