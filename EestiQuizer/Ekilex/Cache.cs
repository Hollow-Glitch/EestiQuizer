using System.IO;
using EestiQuizer.Ekilex.Endpoints;
using System.Text.Json;
using EestiQuizer.Common;


namespace EestiQuizer.Ekilex; 


internal class Cache {
    DirectoryInfo wordIdsCachePath;
    DirectoryInfo wordDetailsCachePath;

    internal Cache(string wordIdsCachePath, string wordDetailsCachePath) {
        //TODO: make the input also a DirInfo and hence change settings as well maybe? Figure this out.
        this.wordIdsCachePath = new DirectoryInfo(wordIdsCachePath);
        this.wordDetailsCachePath = new DirectoryInfo(wordDetailsCachePath);
    }

    internal string PathToWordDetailFileOfWordId(int wordId) {
        var newFileName = $"{wordId}.txt";
        var newFilePath = Path.Combine(wordDetailsCachePath.FullName, newFileName);
        return newFilePath;
    }

    /// <summary>
    /// Example of why we need to sanitize is the input line I have written because it was written like that in Tere: kui vana sa oled?
    /// So we need to remove special chars.
    /// </summary>
    /// <param name="wordOrPhrase"></param>
    /// <returns></returns>
    internal string PathToWordIdCollectionFileOfWordOrPhrase(string wordOrPhrase) {
        var saneWordOrPhrase = Utilities.SanitizeFileName(wordOrPhrase);
        var newFileName = $"{saneWordOrPhrase}.txt";
        var newFilePath = Path.Combine(wordIdsCachePath.FullName, newFileName);
        return newFilePath;
    }

    internal void SaveWordDetails(int wordId, WordDetailsEndpoint.Root wordDetails) {
        var newFilePath = PathToWordDetailFileOfWordId(wordId);
        var content = JsonSerializer.Serialize(wordDetails);
        Utilities.EnsureFileAndWriteAllText(newFilePath, content);
    }

    internal WordDetailsEndpoint.Root? LoadWordDetails(int wordId) {
        //wordDetailsCachePath.Create();
        //var file = wordDetailsCachePath.EnumerateFiles().FirstOrDefault(file => 
        //    Path.GetFileNameWithoutExtension(file.Name).Equals(wordId.ToString() ) 
        //);
        var filePath = PathToWordDetailFileOfWordId(wordId);
        if ( ! File.Exists(filePath) ) return null;

        var content = File.ReadAllText(filePath);
        var wordDetails = JsonSerializer.Deserialize<WordDetailsEndpoint.Root>(content);
        return wordDetails;
    }

    internal void SaveWordIds(string word, List<int> wordIds) {
        var filePath = PathToWordIdCollectionFileOfWordOrPhrase(word);
        var content = JsonSerializer.Serialize(wordIds);
        Utilities.EnsureFileAndWriteAllText(filePath, content);
    }

    internal List<int>? LoadWordIds(string word) {
        var filePath = PathToWordIdCollectionFileOfWordOrPhrase(word);
        if ( ! File.Exists(filePath) ) return null;

        var content = File.ReadAllText(filePath);
        var wordIds = JsonSerializer.Deserialize<List<int>>(content);
        return wordIds;
    }
}
