using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EestiQuizer.Common;


internal static class Utilities {
    internal static string SanitizeFileName(string fileName) {
        // ex.: string fileName = "neplatný/súbor*.txt";
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Vytvorí Regex, ktorý hľadá všetky nelegálne znaky
        string pattern = "[" + Regex.Escape(new string(invalidChars)) + " ]"; //<< ! here we are adding space as well!!!!
        string cleanName = Regex.Replace(fileName, pattern, "_");

        return cleanName;
    }

    /// <summary>
    /// Wrapper arround <see cref="Directory.CreateDirectory(string)"/> and <see cref="File.WriteAllText(string, string?, Encoding)"/>
    /// with defaulting to <see cref="Encoding.UTF8"/> in case the optional param <paramref name="encoding"/> is <code>null</code>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    internal static void EnsureFileAndWriteAllText(string filePath, string content, Encoding? encoding = null) {
        if (encoding is null) {
            encoding = Encoding.UTF8;
        }

        FileInfo fileInfo = new FileInfo(filePath);
        if (fileInfo.DirectoryName is null) {
            throw new ArgumentException($"fileInfo.DirectoryName would fail with null exception; filePath = {filePath}");
        }

        Directory.CreateDirectory(fileInfo.DirectoryName);
        File.WriteAllText(filePath, content, encoding);
    }
}
