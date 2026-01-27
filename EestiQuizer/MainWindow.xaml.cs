using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;


namespace EestiQuizer;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e) {
        base.OnContentRendered(e);

        // Commented out most so that have still 1 from each main group but doesn't take too long to load.
        string[] words = [
            // verb
            "mõtlema",  // special verb with alternative da
            //"tulema",   // basic verb

            // noomen
            "õpik",     // basic noun
            //"kõik",     // asesõna
            //"roheline", // adjective
            //"arv",      // number

            //muutumatu
            "muidugi",  // 
            //"muidugi",  // sidesõna
            //"koos",     // eessõna
            //"koos",     // määrsõna
        ];

        //foreach(var word in words) GetFromSonapi(word);
        foreach(var word in words) ConvertToAnkiFormat(word);
    }


    private void GetFromSonapi_Click(object sender, RoutedEventArgs e) {
        if (sender is null) throw new NullReferenceException();
        MenuItem menuItem = (MenuItem)sender;
        GetFromSonapi(InputBox.Text);
    }


    private void ConvertToAnkiFormat_Click(object sender, RoutedEventArgs e) {
        if (sender is null) throw new NullReferenceException();
        MenuItem menuItem = (MenuItem)sender;
        GetFromSonapi(InputBox.Text);
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
        var translations   = result?.SipmleEngTranslations(translationCount)?.StringJoin(", ");
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
        var translations   = result?.SipmleEngTranslations(translationCount)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        Write(translations + interFieldSeparator);

        //>> examples
        const int meaningsToConsider = 3;
        const int maxExampleCount = 4;
        var ex = result?.ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)?.Distinct()?.StringJoin("<br>") ?? "";
        Write(ex + interFieldSeparator);

        //>> tags
        WriteLine("generated");
    }


    const string SgN = nameof(SgN);

    string? getFormSgN(SonapiResponse? response) {
        var formSgN = response?.SearchResults
            ?.FirstOrDefault()
            ?.WordForms
            ?.First(form => form.Code is "SgN")
            .Value;

        return formSgN;
    }

    string? getFormSgG(SonapiResponse? response) {
        var formSgN = response?.SearchResults
            ?.FirstOrDefault()
            ?.WordForms
            ?.First(form => form.Code is "SgG")
            .Value;

        return formSgN;
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