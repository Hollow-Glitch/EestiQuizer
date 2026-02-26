using EestiQuizer.Common;
using EestiQuizer.Ekilex;
using EestiQuizer.Ekilex.Endpoints;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace EestiQuizer.Views; 


public partial class EkilexView : UserControl {
    public ObservableCollection<CardData> EkilexCardDataCollection { get; set; } = new();
    Settings settings;
    Processor processor;
    RequestClient client;

    const string interFieldSeparator = "|";

    public EkilexView() {
        InitializeComponent();
        settings = Settings.Load(); //TODO: just temp location, probably should be called sooner, since settings is not UI related.
        client = new RequestClient(settings.EkilexApiKey, settings.ImageCachePath);
        processor = new Processor(client);
        OutputBox.TextChanged += (o,args) => OutputBox.ScrollToEnd();
    }

    void WriteNewLine() { OutputBox.Text += "\n"; }
    void WriteLine(string text) { OutputBox.Text += text + "\n"; }
    void Write(string text) { OutputBox.Text += text; }


    void DispatchWriteNewLine() => Dispatch( WriteNewLine );
    void DispatchWriteLine(string text) => Dispatch( () => WriteLine(text) );
    void DispatchWrite(string text) => Dispatch( () => Write(text) );


    private void ClearOutput_Click(object sender, RoutedEventArgs e) {
        OutputBox.Text = "";
        EkilexCardDataCollection.Clear();
    }

    // ================================================================================


    private void TextBoxToEkilex_Click(object sender, RoutedEventArgs e) {
        var word = InputBox.Text;

        var wordToLoad = new WordToLoad(word, []);
        var wordIds = processor.DetermineWordIds(wordToLoad.Word);
        foreach(var wordId in wordIds) {
            WriteLine($"{wordId}");
            var cardData = processor.LoadWord(wordToLoad, wordId);
            if (cardData is not null) EkilexCardDataCollection.Add(cardData);
        }
    }


    void Dispatch(Action action) {
        Application.Current.Dispatcher.Invoke(action);
    }


    async private void LoadFilesFromFolder_Click(object sender, RoutedEventArgs e) {
        var fileDialog = new OpenFileDialog { Multiselect = true, };
        if (fileDialog.ShowDialog() != true) return;

        WriteLine("File names:");
        foreach (var name in fileDialog.FileNames) {
            WriteLine($"- {name}");
        }

        var saveFolder = Directory.GetParent(fileDialog.FileNames[0]);

        var sw_getDetails = Stopwatch.StartNew();
        //TODO: explore using the new `await foreach` syntax here that utilizes `IAsynchEnumerable` thingies.
        List<WordToLoad> problematicWords = [];
        await Task.Run( () => {
            var allWordsWithoutComments = new List<WordToLoad>();
            foreach (var fileName in fileDialog.FileNames) {
                var wordsOfFile = File.ReadAllLines(fileName)
                    .Where(line => !line.Contains("#") && !string.IsNullOrWhiteSpace(line));
                string[] tags = Path.GetFileNameWithoutExtension(fileName).Split("__") ?? [];
                var wordsToLoad = wordsOfFile.Select(word => new WordToLoad(word, tags));
                foreach (var wordToLoad in wordsToLoad) allWordsWithoutComments.Add(wordToLoad);
            }

            foreach(var (wordToLoad, idx) in allWordsWithoutComments.Select((w, i) => (w,i+1) ) ) {
                var wordIds = processor.DetermineWordIds(wordToLoad.Word);
                DispatchWriteLine($"{idx,3}/{allWordsWithoutComments.Count,3}  {wordToLoad.Word,20}  wordId-s: {wordIds.Count}");
                bool hasLoadedAtLeastOne = false;
                foreach(var (wordId, wordIdIdx) in wordIds.Select((w,i) => (w,i+1)) ) {
                    DispatchWrite($"    {wordIdIdx,2}/{wordIds.Count} {wordId,30}");
                    var sw_getWordDetail = Stopwatch.StartNew();
                    var cardData = processor.LoadWord(wordToLoad, wordId);
                    sw_getWordDetail.Stop();
                    const string success = nameof(success);
                    const string failure = nameof(failure);
                    string report;
                    if (cardData is not null) {
                        Dispatch( () => EkilexCardDataCollection.Add(cardData) );
                        hasLoadedAtLeastOne = true;
                        report = success;
                    } else {
                        report = failure;
                    }
                    DispatchWriteLine($" ... {report}  time=({sw_getWordDetail.ElapsedMilliseconds,5})");
                    Dispatch( () => CardDataGrid.ScrollIntoView(CardDataGrid.Items[CardDataGrid.Items.Count - 1]) );
                }
                if ( ! hasLoadedAtLeastOne) {
                    problematicWords.Add(wordToLoad);
                    DispatchWriteLine("    FAILED.");
                }
            }
        } );
        sw_getDetails.Stop();
        WriteLine($"{nameof(sw_getDetails)} = {sw_getDetails}");

        if (problematicWords.Count is not 0) {
            WriteNewLine();
            WriteLine("The following did not load correctly");
            foreach(var problematic in problematicWords) {
                WriteLine($"    {problematic.Word}");
            }
        }

        {   // write file with all rows
            var sb = new StringBuilder();
            var adsf = EkilexCardDataCollection.Select(cd => cd.ToAnkiRow(interFieldSeparator) );
            sb.AppendLine(CardData.Header() );
            sb.AppendJoin("\n", adsf);
            //var rows =
            //    EkilexCardDataCollection.Select(cd => cd.ToAnkiRow(interFieldSeparator) )
            //    .StringJoin("\n");
            WriteLine($"Writing `{EkilexCardDataCollection.Count}` cards to `{settings.OutputFolderPath}`.");
            
            var timePrefix = DateTime.Now.ToString($"yyyy-MM-dd_HH-mm-ss");
            var fileName = $"{timePrefix}.txt";
            var filePath = Path.Combine(settings.OutputFolderPath, fileName);
            //Utilities.EnsureFileAndWriteAllText(filePath, rows);
            Utilities.EnsureFileAndWriteAllText(filePath, sb.ToString() );
        }
    }


    const string appLocalBase = "https://app.local/";
    Uri AppLocalBasedFileUri(string name) => new Uri(appLocalBase + name);
    bool isInited = false;
    private void InitWebViewForFiles() {
        ImageWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "app.local",
            settings.ImageCachePath,
            CoreWebView2HostResourceAccessKind.Allow
        );
    }


    private void CardDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        var grid = (DataGrid) sender;
        var selection = grid.SelectedItem as CardData;
        if (selection is null) return;

        if ( ! isInited) {
            InitWebViewForFiles();
            isInited = true;
        }

        var uris = selection.ImageNamesInCache
            .Select( AppLocalBasedFileUri )
            .ToList();
        if(uris.Count == 0) ImageWebView.Source = new Uri("about:blank");
        var html = HtmlImageListGenerator.Generate(uris);
        ImageWebView.NavigateToString(html);
    }
}
