namespace EestiQuizer.Ekilex.Endpoints; 


public static class WordSearchEndpoint {
    extension(RequestClient client) {
        internal Response? WordSearch(string word) {
            var url = $"https://ekilex.ee/api/word/search/{word}";
            return client.RequestSynch<Response>(url);
        }
    }


    public class Response {
        public int totalCount { get; set; }
        public Word[]? words { get; set; }
    }


    public class Word {
        public int wordId { get; set; }
        public string? wordValue { get; set; }
        public string? wordValuePrese { get; set; }
        public int homonymNr { get; set; }
        public string? lang { get; set; }
        public bool prefixoid { get; set; }
        public bool suffixoid { get; set; }
        public bool foreign { get; set; }
        public string[]? datasetCodes { get; set; }
        public DateTime lastActivityEventOn { get; set; }
        public bool wordPublic { get; set; }
        public bool @public { get; set; }
        public bool wwUnif { get; set; }
        public bool wwLite { get; set; }
        public bool wwOs { get; set; }
    }
}
