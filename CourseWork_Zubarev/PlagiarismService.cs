
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CourseWork_Zubarev
{
    public class ComparisonResult
    {
        public double Similarity { get; set; }

        public double Originality
        {
            get { return 100 - Similarity; }
        }

        public int CommonWords { get; set; }
    }

    public static class PlagiarismService
    {
        private static HashSet<string> GetWords(string text)
        {
            return new HashSet<string>(
                Regex.Matches(
                    text.ToLowerInvariant(),
                    @"[\p{L}\p{N}]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(word => word.Length > 2));
        }

        public static ComparisonResult Compare(
            string currentText,
            IEnumerable<string> savedTexts)
        {
            HashSet<string> currentWords = GetWords(currentText);

            if (currentWords.Count == 0)
            {
                throw new ArgumentException(
                    "У документі немає тексту для перевірки.");
            }

            HashSet<string> knownWords = new HashSet<string>();

            foreach (string text in savedTexts)
            {
                knownWords.UnionWith(GetWords(text));
            }

            int common = currentWords.Intersect(knownWords).Count();

            double similarity =
                common * 100.0 / currentWords.Count;

            if (similarity > 100)
            {
                similarity = 100;
            }

            return new ComparisonResult
            {
                Similarity = similarity,
                CommonWords = common
            };
        }
    }
}
