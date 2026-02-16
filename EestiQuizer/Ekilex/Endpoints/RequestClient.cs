using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EestiQuizer.Ekilex.Endpoints; 

public class RequestClient {
    HttpClient client = new HttpClient();
    string apiKeyValue;
    const string apiKeyHeader = "ekilex-api-key";


    internal RequestClient(string apiKey) {
        apiKeyValue = apiKey;
        client.DefaultRequestHeaders.Add(apiKeyHeader, apiKeyValue);
    }


    internal T? RequestSynch<T>(string url) {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = client.Send(request);
        var jsonText = response.Content.ReadAsStringAsync().Result;
        var json = JsonSerializer.Deserialize<T>(jsonText);
        return json;
    }

}
