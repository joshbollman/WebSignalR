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
        public static string SanitizeMessage(this string message)
        {
            return message.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string ReplaceLineBreaks(this string message)
        {
            return message.Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>");
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
            if (message.StartsWith("$img", StringComparison.InvariantCultureIgnoreCase) && (parts = message.Split('(')).Length > 1)
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
            if (message.StartsWith("$link", StringComparison.InvariantCultureIgnoreCase) && (linkMatch = Regex.Match(message, @"\(([^)]+)\)")).Groups.Count > 1)
            {
                string link = $"<a href=\"{linkMatch.Groups[1].Value}\">{{0}}</a>";

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
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string FormatCode(this string message)
        {
            throw new NotImplementedException();

            string[] parts;
            if (message.StartsWith("$code", StringComparison.InvariantCultureIgnoreCase) && (parts = message.Split(':')).Length > 1)
            {
                string code = "";
                return code;
            }

            return message;
        }
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
