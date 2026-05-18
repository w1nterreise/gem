using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("Tests")]
namespace Gem {
    internal static class PromptParser {

        public static Dictionary<string, string[]> Parse(string filePath)
            => File.Exists(filePath)
                ? ParseLines(File.ReadAllLines(filePath, Encoding.UTF8))
                : new Dictionary<string, string[]>();

        #region pipeline


        public static Dictionary<string, string[]> ParseLines(string[] lines)
            => GroupBySection(
                 SplitIntoRawSections(
                   RemoveComments(lines)));


        private static string[] RemoveComments(string[] lines) 
            => lines
                .Select(line => line.Trim())
                .Where(line => !line.StartsWith("#"))
                .ToArray();
        

        private static List<KeyValuePair<string, List<string>>> SplitIntoRawSections(string[] cleanLines) {

            var sections = new List<KeyValuePair<string, List<string>>>();
            string currentSection = null;
            List<string> currentBlock = null;

            foreach (var line in cleanLines) {
                // looking for section headings:
                if (line.StartsWith("[") && line.EndsWith("]")) {
                    currentSection = line.Substring(1, line.Length - 2).ToUpper();
                    currentBlock = new List<string>();
                    sections.Add(new KeyValuePair<string, List<string>>(currentSection, currentBlock));
                    continue;
                }

                if (currentSection != null) currentBlock.Add(line);
            }

            return sections;
        }


        private static Dictionary<string, string[]> GroupBySection(List<KeyValuePair<string, List<string>>> rawSections)
            => rawSections
                .GroupBy(kv => kv.Key)
                .ToDictionary(
                    group => group.Key,
                    group => {
                        var allLines     = group.SelectMany(kv => kv.Value);
                        var fullText     = string.Join(Environment.NewLine, allLines);
                        var splitPattern = new[] { Environment.NewLine + Environment.NewLine, "\n\n", "\r\n\r\n" };
                        return fullText
                            .Split(splitPattern, StringSplitOptions.RemoveEmptyEntries)
                            .Select(prompt => prompt.Trim())
                            .Where (prompt => !string.IsNullOrEmpty(prompt))
                            .ToArray();
                    });

        #endregion
    }
}
