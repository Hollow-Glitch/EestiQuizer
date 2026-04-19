using EestiQuizer.anki;
using EestiQuizer.Common;
using EestiQuizer.Ekilex;
using EestiQuizer.Ekilex.Endpoints;
using Microsoft.Data.Sqlite;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents.Serialization;
using System.Xaml;


namespace EestiQuizer.Views; 


public partial class EkilexView : UserControl {
    public ObservableCollection<CardData> EkilexCardDataCollection { get; set; } = new();
    Settings settings;
    Processor processor;
    RequestClient client;
    Cache cache;

    const string interFieldSeparator = "|";

    public EkilexView() {
        InitializeComponent();
        settings = Settings.Load(); //TODO: just temp location, probably should be called sooner, since settings is not UI related.
        client = new RequestClient(settings.EkilexApiKey, settings.ImageCachePath);
        cache = new Cache(settings.WordIdsCachePath, settings.WordDetailsCachePath);
        processor = new Processor(client, settings, cache);
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


    private void LoadWord_Click(object sender, RoutedEventArgs e) {
        var word = InputBox.Text;

        var wordToLoad = new WordToLoad(word, []);
        var wordIds = processor.DetermineWordIds(wordToLoad.Word);
        foreach(var wordId in wordIds) {
            WriteLine($"{wordId}");
            var loadWordResult = processor.LoadWord(wordToLoad, wordId);
            if (loadWordResult.cardData is not null) EkilexCardDataCollection.Add(loadWordResult.cardData);
        }
    }


    void Dispatch(Action action) {
        Application.Current.Dispatcher.Invoke(action);
    }


    void WriteSettings() {
        WriteLine($"{nameof(settings.AnkiProfileName)}  = \"{settings.AnkiProfileName}\"");
        WriteLine($"{nameof(settings.OutputFolderPath)} = \"{settings.OutputFolderPath}\"");
        WriteLine($"{nameof(settings.TagForDBChecking)} = \"{settings.TagForDBChecking}\"");
        WriteLine($"{nameof(settings.EkilexApiKey)}     = --purposefully ommited--");
        WriteNewLine();
    }

    [GeneratedRegex(@"^ *%+.*$")]
    private static partial Regex CommentLine { get; }

    [GeneratedRegex(@"^ *@ *(.*?)(?= *%|$)")]
    private static partial Regex NonChainingTagsLine { get; }

    [GeneratedRegex(@"^ *# *([^#].+?)(?= *%|$)")]
    private static partial Regex Level1ChainingTagsLine { get; }

    [GeneratedRegex(@"^ *## *([^#].+?)(?= *%|$)")]
    private static partial Regex Level2ChainingTagsLine { get; }

    [GeneratedRegex(@"^ *### *([^#].+?)(?= *%|$)")]
    private static partial Regex Level3ChainingTagsLine { get; }


    static List<WordToLoad> LoadFile(string filePath) {
        List<WordToLoad> words = [];
        static string[] MatchOfTagFragmentToTags(Match match) =>
            match.Groups[1].Value.Trim().Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        List<string> nonChainingTags = [];
        List<string> level1Tags = [];
        List<string?> level2Tags = [];
        List<string?> level3Tags = [];
        foreach(var line in File.ReadAllLines(filePath) ) {
            // NOTE: I mean speed wise here it is currently unimportant, but it would have been smarter to first detect a non-instruction line
            //       since 90% of the time that is the line that we can expect.
            if (string.IsNullOrWhiteSpace(line) || CommentLine.IsMatch(line) ) {
                continue;
            } else
            if (NonChainingTagsLine.Match(line) is { Success: true } match) {
                var tags = MatchOfTagFragmentToTags(match);
                nonChainingTags.AddRange(tags);
            } else
            if (Level1ChainingTagsLine.Match(line) is { Success: true } matchLevel1) {
                var tags = MatchOfTagFragmentToTags(matchLevel1).ToList();
                level1Tags = tags;
                level2Tags.Clear();
                level3Tags.Clear();
            } else
            if (Level2ChainingTagsLine.Match(line) is { Success: true } matchLevel2) {
                var tags = MatchOfTagFragmentToTags(matchLevel2).ToList();
                level2Tags = tags;
                level3Tags.Clear();
            } else
            if (Level3ChainingTagsLine.Match(line) is { Success: true } matchLevel3) {
                var tags = MatchOfTagFragmentToTags(matchLevel3).ToList();
                level3Tags = tags;
            } else {
                List<string> levelTags = [];
                if ( level1Tags.Any() ) {
                    if ( ! level2Tags.Any() && level3Tags.Any() ) throw new InvalidDataException("lvl2 not defined while lvl3 defined");
                    if ( ! level2Tags.Any() ) level2Tags.Add(null);
                    if ( ! level3Tags.Any() ) level3Tags.Add(null);
                    foreach(var lvl1 in level1Tags) {
                        foreach(var lvl2 in level2Tags) {
                            foreach(var lvl3 in level3Tags) {
                                List<string> tags = [lvl1];
                                if (lvl2 is not null) tags.Add(lvl2);
                                if (lvl3 is not null) tags.Add(lvl3);
                                levelTags.AddRange(tags.StringJoin("::") );
                            }
                        }
                    }
                }
                var resultTags = nonChainingTags.Concat(levelTags);
                var word = line.Trim();
                words.Add(new WordToLoad(word, resultTags) );
            }
        }

        return words;
    }


    List<WordToLoad> LoadFiles(IEnumerable<string> filePaths) {
        List<WordToLoad> loadedWords = [];
        foreach(var filePath in filePaths) {
            loadedWords.AddRange(LoadFile(filePath) );
        }

        var mergedWords = loadedWords
            .GroupBy(item => item.Word)
            .Select(group => new WordToLoad(group.Key, group.SelectMany(x => x.Tags).Distinct() ))
            .ToList();

        return mergedWords;
    }


    async private void LoadFilesFromFolder_Click(object sender, RoutedEventArgs e) {
        WriteSettings();
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
        var sqliteExceptionOccured = false;
        await Task.Run( () => {
            var wordsToLoad = LoadFiles(fileDialog.FileNames);

            using var ankiDb = new AnkiDatabase(settings.AnkiProfileName);

            foreach(var (wordToLoad, idx) in wordsToLoad.Select((w, i) => (w,i+1) ) ) {
                DispatchWrite($"{idx:D3}/{wordsToLoad.Count:D3}  {wordToLoad.Word,-20}  {wordToLoad.Tags.StringJoin(" ")}");
                var wordIds = processor.DetermineWordIds(wordToLoad.Word);
                DispatchWriteNewLine();
                //DispatchWriteLine($"    wordId-s: {wordIds.Count}"); //<< think I don't need this since now I am writing x/y ... x out of y

                bool hasLoadedAtLeastOne = false;
                if (wordIds.Count == 0) {
                    // the loop won't even start, then the FAILED message is written.
                    DispatchWriteLine($"    ... No word id-s found.");
                }
                foreach(var (wordId, wordIdIdx) in wordIds.Select((w,i) => (w,i+1)) ) {
                    DispatchWrite($"    {wordIdIdx:D2}/{wordIds.Count:D2} {wordId,-10}"); //<< assumption for better output, count is less than 10 so no 
                    try {
                        var rows = ankiDb.GetNoteFields(wordId.ToString(), settings.TagForDBChecking).ToList();
                        if (rows.Count != 0) {
                            DispatchWriteLine($" ... already in DB.");
                            foreach(var row in rows) DispatchWriteLine($"        {row}");
                            hasLoadedAtLeastOne = true; //<< finding it in the DB is equivalent to loading it, because then it is not a problematic word.
                            continue;
                        }
                    } catch (SqliteException e) {
                        DispatchWriteNewLine();
                        DispatchWriteLine(e.Message);
                        sqliteExceptionOccured = true;
                        return;
                    }
                    var sw_getWordDetail = Stopwatch.StartNew();
                    var loadWordResult = processor.LoadWord(wordToLoad, wordId);
                    sw_getWordDetail.Stop();
                    const string success = nameof(success);
                    const string failure = nameof(failure);
                    string report;
                    if (loadWordResult.cardData is not null) {
                        Dispatch( () => EkilexCardDataCollection.Add(loadWordResult.cardData) );
                        hasLoadedAtLeastOne = true;
                        report = success;
                    } else {
                        report = failure;
                        report = $"{failure} - {loadWordResult.reason}";
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
        if (sqliteExceptionOccured) return;
        WriteLine($"{nameof(sw_getDetails)} = {sw_getDetails}");

        if (problematicWords.Count is not 0) {
            WriteNewLine();
            WriteLine("The following did not load correctly");
            foreach(var problematic in problematicWords) {
                WriteLine($"    {problematic.Word}");
            }
        }
        WriteNewLine();

        {   // write file with all rows
            if (EkilexCardDataCollection.Count == 0) {
                WriteLine("Nothing to write, no file created.");
                return;
            }
            
            var timePrefix = DateTime.Now.ToString($"yyyy-MM-dd_HH-mm-ss");
            var ankiFilePath = Path.Combine(settings.OutputFolderPath, $"{timePrefix}.txt");
            var logFilePath = Path.Combine(settings.OutputFolderPath, $"{timePrefix}.log");

            var sb = new StringBuilder();
            var adsf = EkilexCardDataCollection.Select(cd => cd.ToAnkiRow(interFieldSeparator) );
            sb.AppendLine(CardData.Header() );
            sb.AppendJoin("\n", adsf);

            Utilities.EnsureFileAndWriteAllText(ankiFilePath, sb.ToString() );
            Utilities.EnsureFileAndWriteAllText(logFilePath, OutputBox.Text);

            WriteLine("Files written:");
            WriteLine($"- {ankiFilePath}");
            WriteLine($"  written {EkilexCardDataCollection.Count} cards/rows.");
            WriteLine($"- {logFilePath}");
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
