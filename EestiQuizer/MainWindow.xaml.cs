using System.Net.Http;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;


namespace EestiQuizer;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    Settings settings;

    public MainWindow()
    {
        InitializeComponent();

        //TODO: just temp location, probably should be called sooner, since settings is not UI related.
        settings = Settings.Load();
    }

    protected override void OnContentRendered(EventArgs e) {
        base.OnContentRendered(e);

        // Commented out most so that have still 1 from each main group but doesn't take too long to load.
        string[] words = [
            // verb
            "mõtlema",  // special verb with alternative da
            "tulema",   // basic verb

            // noomen
            "õpik",     // basic noun
            "kõik",     // asesõna
            "roheline", // adjective
            "arv",      // number

            //muutumatu
            "muidugi",  // 
            "muidugi",  // sidesõna
            "koos",     // eessõna
            "koos",     // määrsõna
        ];

        //foreach(var word in words) GetFromSonapi(word);
        //foreach(var word in words) ConvertToAnkiFormat(word);
        LoadWords(words).Wait();
    }


    private void GetFromSonapi_Click(object sender, RoutedEventArgs e) {
        if (sender is null) throw new NullReferenceException();
        MenuItem menuItem = (MenuItem)sender;
        GetFromSonapi(InputBox.Text);
    }


    private void ConvertToAnkiFormat_Click(object sender, RoutedEventArgs e) {
        if (sender is null) throw new NullReferenceException();
        MenuItem menuItem = (MenuItem)sender;
        LoadWords([InputBox.Text]).Wait();
    }


    private void LoadFilesFromFolder_Click(object sender, RoutedEventArgs e) {
        // var folderDialog = new OpenFolderDialog {
        //     Title = "Vyberte priečinok",
        //     InitialDirectory = @"C:\"
        // };
        // if (folderDialog.ShowDialog() == true) {
        //     string folderPath = folderDialog.FolderName;
        //     // tu spracujte cestu k priečinku
        // }

        var fileDialog = new OpenFileDialog {
            Multiselect = true,
        };
        if (fileDialog.ShowDialog() == true) {
            WriteLine("File names:");
            foreach(var name in fileDialog.FileNames) {
                WriteLine($"- {name}");
            }

            var saveFolder = Directory.GetParent(fileDialog.FileNames[0]);

            var allWordsWithoutComments = fileDialog.FileNames
                .SelectMany(File.ReadAllLines)
                .Where(line => ! line.Contains("#") && ! string.IsNullOrWhiteSpace(line) );
            WriteLine(allWordsWithoutComments.StringJoin("\n") );
            LoadWords(allWordsWithoutComments, saveFolder).Wait();
        }
    }


    private void GetFromSonapi(string payload) {
        //var payload = InputBox.Text;
        var url = $"https://api.sonapi.ee/v2/{payload}";

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = client.Send(request);
        var readAsStringAsync = response.Content.ReadAsStringAsync();

        var responseString = readAsStringAsync.Result;
        //WriteLine(responseString);

        SonapiResponse? result = JsonSerializer
            .Deserialize<SonapiResponse>(responseString);

        const string errorMissingData = "Error - missing data.";
        // //>> Moved to separate function, decide where it should land.
        // // 
        // //var formSgN = result?.SearchResults
        // //    ?.FirstOrDefault()
        // //    ?.WordForms
        // //    ?.First(form => form.Code is "SgN")
        // //    .Value;
        // WriteLine("result:");
        // WriteLine("EstonianWord  = " + result?.EstonianWord  ?? errorMissingData);
        // WriteLine("RequestedWord = " + result?.RequestedWord ?? errorMissingData);
        // WriteLine("formSgN       = " + getFormSgN(result) ?? errorMissingData);
        // WriteLine("formSgN       = " + getFormSgG(result) ?? errorMissingData);
        // WriteLine("------------------------------------------------------------");

        var wordClasses = result
            ?.SearchResults?.FirstOrDefault()
            ?.WordClasses?.Where(wc => wc is not null)
            ??[];

        if (wordClasses.Count() == 0) {
            WriteLine("ERROR  --  no word classes found.");
        }
        if (wordClasses.Count() > 1) {
            WriteLine("WARNING  --  multiple word classes found: " + wordClasses.Select(wc => wc.ToString()).StringJoin(", "));
            WriteLine("         --  The first will be chosen.");
        }

        var chosenWordClass = wordClasses?.FirstOrDefault();

        switch(chosenWordClass) {
            case null:
                WriteLine("ERROR  --  word class is null.");
                break;
            case WordClass.verb: {
                var ma   = result?.WordFormValues(WordFormCode.verb_Ma  )?.StringJoin(", ");
                var da   = result?.WordFormValues(WordFormCode.verb_Da  )?.StringJoin(", ");
                var sg1p = result?.WordFormValues(WordFormCode.verb_Sg1P)?.StringJoin(", ");
                WriteLine("ma   = "  + ma   ?? errorMissingData);
                WriteLine("da   = "  + da   ?? errorMissingData);
                WriteLine("sg1p = "  + sg1p ?? errorMissingData);
            } break;
            case WordClass.noomen: {
                var sgN = result?.WordFormValues(WordFormCode.noomen_SgN)?.StringJoin(", ");
                var sgG = result?.WordFormValues(WordFormCode.noomen_SgG)?.StringJoin(", ");
                var sgP = result?.WordFormValues(WordFormCode.noomen_SgP)?.StringJoin(", ");
                WriteLine("sgN = "  + sgN ?? errorMissingData);
                WriteLine("sgG = "  + sgG ?? errorMissingData);
                WriteLine("sgP = "  + sgP ?? errorMissingData);
            } break;
            case WordClass.muutumatu: {
                var theOnlyForm = result?.WordFormValues(WordFormCode.muutumatu_ID)?.StringJoin(", ");
                WriteLine("Id  = " + theOnlyForm ?? errorMissingData);
            } break;
            default:
                break;
        }

        const int translationCount = 3;
        var translations   = result?.OuterEngTranslations(translationCount)?.StringJoin(", ");
        WriteLine("translations = " + translations   ?? errorMissingData);

        const int meaningsToConsider = 3;
        const int maxExampleCount = 4;
        //const int examplesPerMeaning = 4;
        //var ex = "    " + result?.ExamplesPerConsideredMeanings(examplesPerMeaning, meaningsToConsider)?.StringJoin(",\n    ");
        //var ex = "    " + result?.ExamplesCappedPerMeanings(maxExampleCount, examplesPerMeaning)?.StringJoin(",\n    ");
        //var ex = "    " + result?.ExamplesBalancedPerMeanings(maxExampleCount, meaningsToConsider)?.StringJoin(",\n    ");
        var ex = "    " + result?.ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)?.StringJoin(",\n    ");
        WriteLine("ex =\n" + ex   ?? errorMissingData);

        WriteLine("------------------------------------------------------------");
    }


    private void ConvertToAnkiFormat(string payload) {
        //var payload = InputBox.Text;
        var url = $"https://api.sonapi.ee/v2/{payload}";

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = client.Send(request);
        var readAsStringAsync = response.Content.ReadAsStringAsync();

        var responseString = readAsStringAsync.Result;
        //WriteLine(responseString);

        SonapiResponse? result = JsonSerializer
            .Deserialize<SonapiResponse>(responseString);

        var wordClasses = result
            ?.SearchResults?.FirstOrDefault()
            ?.WordClasses?.Where(wc => wc is not null)
            ??[];

        if (wordClasses.Count() == 0) {
            WriteLine("ERROR  --  no word classes found.");
        }
        if (wordClasses.Count() > 1) {
            WriteLine("WARNING  --  multiple word classes found: " + wordClasses.Select(wc => wc.ToString()).StringJoin(", "));
            WriteLine("         --  The first will be chosen.");
        }

        var chosenWordClass = wordClasses?.FirstOrDefault();
        const string interFieldSeparator = "|";
        const string intraFieldSeparator = ", ";

        //== Write anki txt file rows

        //>> word forms (in anki note type sense): ma-da-sg1p, N-G-P, single-form
        switch(chosenWordClass) {
            case null:
                WriteLine("ERROR  --  word class is null.");
                break;
            case WordClass.verb: {
                var ma   = result?.WordFormValues(WordFormCode.verb_Ma  )?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                var da   = result?.WordFormValues(WordFormCode.verb_Da  )?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                var sg1p = result?.WordFormValues(WordFormCode.verb_Sg1P)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                Write(ma   + interFieldSeparator);
                Write(da   + interFieldSeparator);
                Write(sg1p + interFieldSeparator);
            } break;
            case WordClass.noomen: {
                var sgN = result?.WordFormValues(WordFormCode.noomen_SgN)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                var sgG = result?.WordFormValues(WordFormCode.noomen_SgG)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                var sgP = result?.WordFormValues(WordFormCode.noomen_SgP)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                Write(sgN + interFieldSeparator);
                Write(sgG + interFieldSeparator);
                Write(sgP + interFieldSeparator);
            } break;
            case WordClass.muutumatu: {
                var theOnlyForm = result?.WordFormValues(WordFormCode.muutumatu_ID)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
                Write(theOnlyForm + interFieldSeparator);
            } break;
            default:
                break;
        }

        //>> translation / meaning(in my anki note type sense)
        const int translationCount = 3;
        var translations   = result?.OuterEngTranslations(translationCount)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        Write(translations + interFieldSeparator);

        //>> examples
        const int meaningsToConsider = 3;
        const int maxExampleCount = 4;
        var ex = result?.ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)?.Distinct()?.StringJoin("<br>") ?? "";
        Write(ex + interFieldSeparator);

        //>> tags
        WriteLine("generated");
    }


    async Task LoadWords(IEnumerable<string> words, DirectoryInfo? saveFolder = null) {
        using var client = new HttpClient();
                            
        var sw_asyncSend = Stopwatch.StartNew();
        //>> Send requests
        List<(Task<string> asyncContent, string word)> asyncResponseStrings = [];
        int pauseCounter = 0;
        int pauseTreshold = 5;
        try {
            foreach(var word in words) {
                var url = $"https://api.sonapi.ee/v2/{word}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = client.Send(request);
                var readAsStringAsync = response.Content.ReadAsStringAsync();
                asyncResponseStrings.Add( (readAsStringAsync, word) );
                Thread.Sleep(50);
                if (pauseCounter % pauseTreshold == 0) Thread.Sleep(200);
            }
        } catch(Exception e) {
            WriteLine($"exception: {e.Message}");
            return;
        }
        WriteLine($"{nameof(sw_asyncSend)} = {sw_asyncSend}");

        var sw_await = Stopwatch.StartNew();
        //>> await requests and transform to string
        List<(string jsonContent, string word)> responseWordPair = [];
        foreach(var asyncResponseString in asyncResponseStrings) {
            responseWordPair.Add( (await asyncResponseString.asyncContent, asyncResponseString.word) );
        }
        WriteLine($"{nameof(sw_await)} = {sw_await}");

        // //>> save to cache
        // var serializedJson = JsonSerializer.Serialize(responseWordPair);
        //
        //<< let's keep focus for now on the current blockers and issues.

        var sw_transform = Stopwatch.StartNew();
        //>> deserialize and transform to card data grouped by wordclass
        const string interFieldSeparator = "|";
        const string intraFieldSeparator = ", ";
        const int meaningsToConsider = 3;
        const int maxExampleCount = 4;
        const int translationCount = 3;
        var groupedCardData = responseWordPair
            .Select(pair => 
                (json: JsonSerializer.Deserialize<SonapiResponse>(pair.jsonContent), pair.word)
            )
            .Where(pair => pair.json is not null).Select(r => (sonapiResponse: r.json!, word: r.word)) // filter then tell compiler.
            .Select(response =>
                new CardData(
                    response.word,
                    response.sonapiResponse,
                    intraFieldSeparator,
                    translationCount,
                    meaningsToConsider,
                    maxExampleCount
                )
            )
            .GroupBy(cardData => cardData.myWordClass);
        WriteLine($"{nameof(sw_transform)} = {sw_transform}");

        var sw_save = Stopwatch.StartNew();
        //>> save into files (one file per wordClass)
        foreach (var group in groupedCardData) { 
            var rows = group.Select(cardData => cardData.ToAnkiRow(interFieldSeparator) );

            WriteNewLine();
            WriteLine($"{group.Key}");
            foreach(var row in rows) {
                WriteLine("    " + row);
            }

            if (saveFolder is not null) {
                var timePrefix = DateTime.Now.ToString($"yyyy-MM-dd_HH-mm-ss");
                var fileName = $"{timePrefix}_{group.Key}.txt";
                var filePath = Path.Combine(settings.OutputFolderPath, fileName);
                Common.EnsureFileAndWriteAllText(filePath, rows.StringJoin("\n") );
            }
        }
        WriteLine($"{nameof(sw_save)} = {sw_save}");
    }


    void WriteNewLine() {
        OutputBox.Text += "\n";
    }


    void WriteLine(string text) {
        OutputBox.Text += text + "\n";
    }


    void Write(string text) {
        OutputBox.Text += text;
    }

}