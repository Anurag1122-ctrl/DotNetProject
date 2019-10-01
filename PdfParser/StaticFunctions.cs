using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PdfParser
{
    public static class StaticFunctions
    {
        public static string StrigHtml(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static int ToInt(this string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static double ToDouble(this string value)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static DateTime ToDateTime(this string value)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public static void AddOrReplace(this IDictionary<string, object> dict, string key, object value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static string ParseStringInTwoSteps(this string s, string start1, string stop1, string start2, string stop2)
        {
            string match;
            int pos = Parse(s, 0, start1, stop1, out match);
            if (pos > 0)
                Parse(match, 0, start2, stop2, out match);
            return match;
        }

        public static int Parse(string inText, int startIndex, string startString, string stopString, out string match)
        {
            return Parse(inText, startIndex, startString, stopString, out match, false);
        }

        public static int Parse(string inText, int startIndex, string startString, string stopString, out string match, bool caseSensitive)
        {
            match = "";
            if (startIndex < 0 || string.IsNullOrEmpty(inText) || startIndex >= inText.Length)
                return -1;
            int num1 = !string.IsNullOrEmpty(startString) ? (!caseSensitive ? inText.IndexOfCaseInsensitive(startString, startIndex) : inText.IndexOf(startString, startIndex, StringComparison.Ordinal)) : startIndex;
            if (num1 < 0)
                return -1;
            int startIndex1 = num1 + startString.Length;
            for (int index = startIndex1; index < inText.Length; ++index)
            {
                startIndex1 = index;
                switch (inText[index])
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case ' ':
                        continue;
                    default:
                        goto label_8;
                }
            }
            label_8:
            int num2 = !caseSensitive ? inText.IndexOfCaseInsensitive(stopString, startIndex1) : inText.IndexOf(stopString, startIndex1, StringComparison.Ordinal);
            if (num2 < 0)
            {
                match = inText.SubstringLimit(startIndex1).Trim();
                return startIndex1;
            }
            string str = inText.SubstringLimit(startIndex1, num2 - startIndex1).Trim();
            if (str.Length <= 0)
                return -1;
            match = str;
            return startIndex1;
        }

        public static string SubstringLimit(this string s, int startIndex)
        {
            return s.SubstringLimit(startIndex, s.Length);
        }

        public static string SubstringLimit(this string s, int startIndex, int maxLength)
        {
            if (string.IsNullOrEmpty(s) || startIndex < 0 || startIndex >= s.Length)
                return "";
            int length = Math.Min(s.Length - startIndex, maxLength);
            if (length > 0)
                return s.Substring(startIndex, length);
            return "";
        }

        public static int IndexOfCaseInsensitive(this string s, string compare)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(compare))
                return -1;
            return s.ToUpper().IndexOf(compare.ToUpper(), StringComparison.Ordinal);
        }

        public static int IndexOfCaseInsensitive(this string s, string compare, int startIndex)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(compare))
                return -1;
            return s.ToUpper().IndexOf(compare.ToUpper(), startIndex, StringComparison.Ordinal);
        }
    }
}
