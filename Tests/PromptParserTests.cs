using Gem;

namespace Tests {
    [TestClass]
    public class PromptParserTests {

        [TestMethod]
        public void Parse_ShouldIgnoreCommentsAndEmptyLines() {

            string[] input = [
                "# Это комментарий в начале",
                "[CTRL]",
                "   # Вложенный комментарий с отступами",
                "Промпт 1",
                "",
                "# Еще один комментарий",
                "Промпт 2"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.IsTrue(result.ContainsKey("CTRL"));
            Assert.AreEqual(2, result["CTRL"].Length);
            Assert.AreEqual("Промпт 1", result["CTRL"][0]);
            Assert.AreEqual("Промпт 2", result["CTRL"][1]);
        }

        [TestMethod]
        public void Parse_ShouldMergeDuplicateSections() {

            string[] input = [
                "[CTRL]",
                "Первый промпт",
                "",
                "[SHIFT]",
                "Локальный промпт",
                "",
                "[CTRL]",
                "Второй промпт из дубликата секции"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.AreEqual(2, result["CTRL"].Length);
            Assert.AreEqual("Первый промпт", result["CTRL"][0]);
            Assert.AreEqual("Второй промпт из дубликата секции", result["CTRL"][1]);
            Assert.AreEqual(1, result["SHIFT"].Length);
        }

        [TestMethod]
        public void Parse_ShouldBeCaseInsensitiveForSections() {

            string[] input = [
                "[ctrl]",
                "Тест нижнего регистра",
                "",
                "[ShIfT]",
                "Тест разного регистра"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.IsTrue(result.ContainsKey("CTRL"));
            Assert.IsTrue(result.ContainsKey("SHIFT"));
            Assert.AreEqual("Тест нижнего регистра", result["CTRL"][0]);
        }

        [TestMethod]
        public void Parse_ShouldKeepNewLinesWithinSinglePrompt() {

            string[] input = [
                "[SHIFT]",
                "Первая строка многострочного промпта",
                "Вторая строка того же промпта",
                "",
                "Новый промпт"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.AreEqual(2, result["SHIFT"].Length);
            string expectedMultiline = "Первая строка многострочного промпта" + Environment.NewLine + "Вторая строка того же промпта";
            Assert.AreEqual(expectedMultiline, result["SHIFT"][0]);
        }

        [TestMethod]
        public void Parse_ShouldReturnEmptyDictionary_WhenNoSectionsFound() {

            string[] input = [
                "# Просто файл из одних комментов",
                "Какая-то строка без секции",
                "Еще одна строка"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Parse_EmptySection_ShouldNotThrowAndReturnEmptyArray() {

            string[] input = [
                "[CTRL]",
                "[SHIFT]",
                "Локальный промпт"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.IsTrue(result.ContainsKey("SHIFT"));
            if (result.ContainsKey("CTRL")) {
                Assert.AreEqual(0, result["CTRL"].Length);
            }
        }

        [TestMethod]
        public void Parse_TextBeforeAnySection_ShouldBeIgnored() {

            string[] input = [
                "Привет, я текст без секции",
                "[CTRL]",
                "Промпт 1"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Промпт 1", result["CTRL"][0]);
        }

        [TestMethod]
        public void Parse_MalformedSectionHeadings_ShouldIgnoreThem() {

            string[] input = [
                "[CTRL",
                "Промпт 1",
                "[SHIFT]",
                "Промпт 2"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.IsFalse(result.ContainsKey("CTRL"));
            Assert.IsTrue(result.ContainsKey("SHIFT"));
            Assert.AreEqual("Промпт 2", result["SHIFT"][0]);
        }

        [TestMethod]
        public void Parse_MultipleEmptyLinesBetweenPrompts_ShouldNotCreateEmptyPrompts() {

            string[] input = [
                "[CTRL]",
                "Промпт 1",
                "", "", "", "",
                "Промпт 2"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.AreEqual(2, result["CTRL"].Length);
            Assert.AreEqual("Промпт 1", result["CTRL"][0]);
            Assert.AreEqual("Промпт 2", result["CTRL"][1]);
        }

        [TestMethod]
        public void Parse_UnixLineEndings_ShouldSplitCorrectly() {

            string unixText = "[CTRL]\nСтрока 1\nСтрока 2\n\nНовый промпт";
            string[] input = unixText.Split('\n');

            var result = PromptParser.ParseLines(input);

            Assert.AreEqual(2, result["CTRL"].Length);
            Assert.IsTrue(result["CTRL"][0].Contains("Строка 2"));
            Assert.AreEqual("Новый промпт", result["CTRL"][1]);
        }

        [TestMethod]
        public void Parse_SpecialCharacters_ShouldPreserveJsonAndSymbols() {

            string[] input = [
                "[CTRL]",
                "{ \"action\": \"test\", \"value\": 10 }",
                "",
                "Промпт -> 2"
            ];

            var result = PromptParser.ParseLines(input);

            Assert.AreEqual("{ \"action\": \"test\", \"value\": 10 }", result["CTRL"][0]);
            Assert.AreEqual("Промпт -> 2", result["CTRL"][1]);
        }

        [TestMethod]
        public void Parse_OnlySpacesAndTabs_ShouldReturnEmptyDictionary() {

            string[] input = [
                "   ",
                "\t",
                "  \t  "
            ];

            var result = PromptParser.ParseLines(input);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
