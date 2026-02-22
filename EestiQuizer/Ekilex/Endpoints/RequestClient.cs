using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace EestiQuizer.Ekilex.Endpoints; 

public class RequestClient {
    HttpClient client = new HttpClient();
    string apiKeyValue;
    private readonly string imageOutputFolder;
    const string apiKeyHeader = "ekilex-api-key";


    internal RequestClient(string apiKey, string imageOutputFolder) {
        apiKeyValue = apiKey;
        this.imageOutputFolder = imageOutputFolder;
        client.DefaultRequestHeaders.Add(apiKeyHeader, apiKeyValue);
    }


    internal T? RequestSynch<T>(string url) {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = client.Send(request);
        var jsonText = response.Content.ReadAsStringAsync().Result;
        var json = JsonSerializer.Deserialize<T>(jsonText);
        return json;
    }

    HttpClient imageClient = new HttpClient();
    internal void DownloadImage(string url) {
        // ex.: https://sonaveeb.ee/files/images/v6i.svg
        var fileName = url.Split('/').LastOrDefault();
        if (fileName is null) throw new NotImplementedException($"We couldn't get the last slash separated segment of the url: {url}");
        var filePath = Path.Combine(imageOutputFolder, fileName);
        if (File.Exists(filePath) ) return; //<< early return to prevent wasted effort.

        string svgContent = imageClient.GetStringAsync(url).Result;
        using var downloadStream = imageClient.GetStreamAsync(url).Result;
        Directory.CreateDirectory(imageOutputFolder);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        downloadStream.CopyTo(fileStream);
        //
        //<< need this instead of below because it seems that the below can't handle jpg which is an option.
        //
        //Utilities.EnsureFileAndWriteAllText(filePath, svgContent);
        //File.WriteAllText(filePath, svgContent);
    }
}
