using System;
using System.IO;

namespace LiteDbExplorer.Presentation
{
    public static class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="absolutePath">The path to compress</param>
        /// <param name="limit">The maximum length</param>
        /// <param name="delimiter">The character(s) to use to imply incompleteness</param>
        /// <returns></returns>
        public static string ShrinkPath(this string absolutePath, int limit, string delimiter = "…")
        {
            //no path provided
            if (string.IsNullOrEmpty(absolutePath))
            {
                return string.Empty;
            }
 
            var name = Path.GetFileName(absolutePath);
            var nameLength = name.Length;
            var pathLength = absolutePath.Length;
            var dir = absolutePath.Substring(0, pathLength - nameLength);
 
            var delimiterLength = delimiter.Length;
            var idealMinLength = nameLength + delimiterLength;
 
            var slash = (absolutePath.IndexOf("/", StringComparison.Ordinal) > -1 ? "/" : "\\");
 
            //less than the minimum amt
            if (limit < ((2 * delimiterLength) + 1))
            {
                return "";
            }
 
            //fullpath
            if (limit >= pathLength)
            {
                return absolutePath;
            }
 
            //file name condensing
            if (limit < idealMinLength)
            {
                return delimiter + name.Substring(0, (limit - (2 * delimiterLength))) + delimiter;
            }
 
            //whole name only, no folder structure shown
            if (limit == idealMinLength)
            {
                return delimiter + name;
            }
 
            return dir.Substring(0, (limit - (idealMinLength + 1))) + delimiter + slash + name;
        }
    }
}