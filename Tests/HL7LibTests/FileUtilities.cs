using System;
using System.IO;
using System.Text;

namespace HL7LibTests
{
    public class FileUtilities
    {
        /// <summary>
        /// Reads all lines without comments.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        /// <remarks>By default, Encoding.UTF8 is used.</remarks>
        public static string ReadAllLinesWithoutComments(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var line in File.ReadAllLines(fileName))
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }
                sb.Append(line + Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}