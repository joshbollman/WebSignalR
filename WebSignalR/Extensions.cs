using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebSignalR
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    static class MyExtensions
    {
        #region Message Formatting and Sanitizing
        public static string ProcessMessage(this string message) => message.SanitizeMessage().FormatCode().FormatLink().FormatImageTag(0, 200).ReplaceLineBreaks();

        /// <summary>
        /// Transforms '&lt;' and '&gt;' to "&amp;lt;" and "&amp;gt;", respectively
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string SanitizeMessage(this string message)
        {
            var m = message.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace("<br>", "\r\n");
            return m.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        /// <summary>
        /// Transforms spaces " " to "&amp;nbsp;"
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string ReplaceSpaces(this string message) => message.Replace(" ", "&nbsp;");

        /// <summary>
        /// Replaces line breaks with 'br' elements. Condenses 2+ consecutive line breaks to 1.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string ReplaceLineBreaks(this string message)
        {
            var lines = message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var newLines = new List<string>();

            int contEmpty = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim()))
                {
                    if (contEmpty++ >= 1)
                        continue;

                    newLines.Add(lines[i]);
                    continue;
                }

                newLines.Add(lines[i]);
                contEmpty = 0;
            }

            return string.Join("<br/>", newLines);
        }

        /// <summary>
        /// Take image link and transform to html element to display
        /// </summary>
        /// <param name="message">Input should look like "$img(link)"</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string FormatImageTag(this string message, int height, int width)
        {
            string[] parts;
            if (message.TrimStart().StartsWith("$img", StringComparison.InvariantCultureIgnoreCase) && (parts = message.Split('(')).Length > 1)
            {
                string img = $"<img src=\"{parts[1].Substring(0,parts[1].Length - 1).Trim()}\" style=\"max-height:{(height == 0 ? "auto" : height.ToString() + "px")};max-width:{(width == 0 ? "auto" : width.ToString()) + "px"};\">";
                return img;
            }

            return message;
        }

        /// <summary>
        /// Take link address and transform to html element to display
        /// </summary>
        /// <param name="message">Input should look like "$link[optional text](link)"</param>
        /// <returns></returns>
        public static string FormatLink(this string message)
        {
            Match linkMatch;
            if (message.TrimStart().StartsWith("$link", StringComparison.InvariantCultureIgnoreCase) && (linkMatch = Regex.Match(message, @"\(([^)]+)\)")).Groups.Count > 1)
            {
                string link = $"<a style=\"text-decoration:underline;\" href=\"{linkMatch.Groups[1].Value}\">{{0}}</a>";

                Match displayMatch;
                if ((displayMatch = Regex.Match(message, @"\[([^)]+)\]")).Groups.Count > 1)
                    link = string.Format(link, displayMatch.Groups[1].Value);
                else
                    link = string.Format(link, linkMatch.Groups[1].Value);

                return link;
            }

            return message;
        }

        /// <summary>
        /// Transforms code chunk to code element.  Replaces spaces with &nbsp; to keep indentation
        /// </summary>
        /// <param name="message">Input should look like $code:public static void Main...</param>
        /// <returns></returns>
        public static string FormatCode(this string message)
        {
            if (message.TrimStart().StartsWith("$code", StringComparison.InvariantCultureIgnoreCase))
            {
                var code = message.Substring(message.IndexOf(':') + 1).TrimStart();
                code = code.ReplaceSpaces();

                string element = $"<code style=\"text-align:left !important;\">{code}</code>";

                return element;
            }

            return message;
        }
        #endregion

        #region Shuffle and Randomize
        public static string ToNumberWord(this int i)
        {
            switch (i)
            {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";
                default:
                    return string.Empty;
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> oList)
        {
            int n = oList.Count();
            var list = oList.ToArray();
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
        #endregion
    }
}
