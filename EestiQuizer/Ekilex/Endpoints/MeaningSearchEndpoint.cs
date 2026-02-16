using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer.Ekilex.Endpoints; 


public static class MeaningSearchEndpoint {

    extension(RequestClient client) {
        internal Response? MeaningSearch(string word) {
            var url = $"https://ekilex.ee/api/meaning/search/{word}";
            return client.RequestSynch<Response>(url);
        }
    }


    // {
    //     "meaningCount": 26,
    //     "wordCount": 71,
    //     "resultCount": 26,
    //     "results": [..26..],
    //     "resultExist": true,
    //     "resultDownloadNow": false,
    //     "resultDownloadLater": true
    // }
    public class Response {
        public int meaningCount { get; set; }
        public int wordCount { get; set; }
        public int resultCount { get; set; }
        public List<Result> results { get; set; } = [];
        public bool resultExist { get; set; }
        public bool resultDownloadNow { get; set; }
        public bool resultDownloadLater { get; set; }
    }


    public class Result {
        public int meaningId { get; set; }
        public bool meaningWordsExist { get; set; }
        public List<MeaningWord> meaningWords { get; set; } = [];

        //>> ??
        // public object meaningDomains { get; set; }
    }


    public class MeaningWord
    {
        public int wordId { get; set; }
        public int homonymNr { get; set; }
        public required string wordValue { get; set; }
        public required string wordValuePrese { get; set; }
        public required string lang { get; set; }
        public bool prefixoid { get; set; }
        public bool suffixoid { get; set; }
        public bool foreign { get; set; }
        public bool matchingWord { get; set; }
        public bool mostPreferred { get; set; }
        public bool leastPreferred { get; set; }
        public List<string> datasetCodes { get; set; } = [];
        public bool @public { get; set; }

        //>> what is this, was null when created this.
        // public object wordTypeCodes { get; set; }
    }
}
