namespace EestiQuizer.Ekilex.Endpoints; 


internal static class FormSearchEndpoint {
    extension(RequestClient client) {
        internal Response[]? FormSearch(string word) {
            var url = $"https://ekilex.ee/api/form/search/{word}";
            return client.RequestSynch<Response[]>(url);
        }
    }


    //>> ex.:
    // {
    //   "wordId": 247802,
    //   "wordValue": "tulema",
    //   "lang": "est",
    //   "homonymNr": 1
    // },
    public class Response {
        public int wordId { get; set; }
        public int homonymNr { get; set; }

        public required string wordValue { get; set; }
        public required string lang { get; set; }
    }
}
