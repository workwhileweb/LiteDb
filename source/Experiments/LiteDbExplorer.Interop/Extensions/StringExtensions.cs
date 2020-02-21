using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LiteDbExplorer.Core
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool NotEmpty(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static string ReplaceSpaces(this string str, string replacement)
        {
            return Regex.Replace(str, @"\s+", replacement, RegexOptions.Compiled);
        }

        public static string RemoveSpaces(this string str)
        {
            return ReplaceSpaces(str, string.Empty);
        }

        public static string ToUtf8(this string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Remove Diacritics from a string
        /// This converts accented characters to nonaccented, which means it is
        /// easier to search for matching data with or without such accents.
        /// Respaced and converted to an Extension Method
        /// <example>
        ///    aàáâãäåçc
        /// is converted to
        ///    aaaaaaacc
        /// </example>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string WithoutDiacritics(this string str)
        {
            var normalizedString = str.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}