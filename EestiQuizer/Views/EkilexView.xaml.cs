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
using System.Windows.Media;
using System.Windows.Threading;


namespace EestiQuizer.Views; 


public partial class EkilexView : UserControl {
    public ObservableCollection<CardData> EkilexCardDataCollection { get; set; } = new();
    public ObservableCollection<string> Logs { get; set; } = new();
    Settings settings;
    Processor processor;
    RequestClient client;
    Cache cache;

    private DispatcherTimer logAutoScroll;
    private ScrollViewer? logScrollViewer;

    public EkilexView() {
        InitializeComponent();
        settings = Settings.Load(); //TODO: just temp location, probably should be called sooner, since settings is not UI related.
        client = new RequestClient(settings.EkilexApiKey, settings.ImageCachePath);
        cache = new Cache(settings.WordIdsCachePath, settings.WordDetailsCachePath);
        processor = new Processor(client, settings, cache);

        //>> Find the scroll viewer of the Log listbox so that we can auto scroll sanely without jumps.
        this.Loaded += (object sender, RoutedEventArgs e) => {
            // https://biggert.github.io/2009/04/01/virtualized-wpf-listbox-scrolling-because-scrollintoview-doesnt-always-work
            logScrollViewer = GetVisualChild<ScrollViewer>(LogView)!;
        };
        //>> We use a DispatcherTimer which we can start and stop with
        //   `StartAutoScroll` and `StopAutoScroll` (see below).
        logAutoScroll = new(DispatcherPriority.Render);
        logAutoScroll.Interval = TimeSpan.FromMilliseconds(150);
        logAutoScroll.Tick += ProcessBuffer;
    }

    void WriteNewLine() => Logs.Add("");
    void WriteLine(string text) => Logs.Add(text);
    //void Write(string text) { OutputBox.Text += text; }
    //TODO: figure out how to write non-line logs with the list based logging approach
    void AppendToPreviousLine(string text) => Logs[Logs.Count-1] += text;

    //>> Method that scrolls the log ListBox the DispatcherTimer calls to 
    //   enable auto scrolling implementation.
    private void ProcessBuffer(object? sender, EventArgs e) {
        CardDataGrid.ScrollIntoView(CardDataGrid.Items[CardDataGrid.Items.Count - 1]);
        logScrollViewer?.ScrollToBottom();
        LogView.UpdateLayout();
    }


    /// <summary>
    /// The point of this is to be used for finding the ScrollViewer of the ListBox.
    /// I couldn't find another way to scroll without jumps to the right on long lines.
    /// </summary>
    public static T? GetVisualChild<T>(DependencyObject parent) where T : DependencyObject {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T target) return target;
            
            T? childOfChild = GetVisualChild<T>(child);
            if (childOfChild != null) return childOfChild;
        }
        return null;
    }


    /// <summary>
    /// Use this instead <see cref="logAutoScroll.Start"/>.
    /// </summary>
    private void StartAutoScroll() {
        logAutoScroll.Start();
    }

    /// Use this instead <see cref="logAutoScroll.Stop"/>.
    /// Reason is that this handles the very last moment by scrolling down
    /// so that timing is not an issue.
    private void StopAutoScroll() {
        logAutoScroll.Stop();
        //>> ensures that we don't end up without auto scrolling to the bottom at the end.
        //   Could have happened if the timer did not run at the end before we stopped it.
        LogView.ScrollIntoView(LogView.Items[Logs.Count - 1]);
    }


    void DispatchWriteNewLine() => Dispatch( WriteNewLine );
    void DispatchWriteLine(string text) => Dispatch( () => WriteLine(text) );
    //void DispatchWrite(string text) => Dispatch( () => Write(text) );
    //TODO: figure out how to write non-line logs with the list based logging approach
    //      See commented out `Write` above.


    private void ClearOutput_Click(object sender, RoutedEventArgs e) {
        Logs.Clear();
        EkilexCardDataCollection.Clear();
    }

    // ================================================================================


    private void LoadWord_Click(object sender, RoutedEventArgs e) {
        var word = InputBox.Text;

        var wordToLoad = new WordToLoad(word, [], []);
        var normalizedWords = processor.DetermineWordIds(wordToLoad.Word);
        foreach(var normalizedWord in normalizedWords) {
            WriteLine($"{normalizedWord.BaseForm}  {normalizedWord.Id}");
            var loadWordResult = processor.LoadWord(wordToLoad, normalizedWord.Id);
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
        foreach(var (line, lineNumber) in File.ReadAllLines(filePath).WithIndexBase1() ) {
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
                var resultTags = nonChainingTags.Concat(levelTags).ToList();
                var word = line.Trim();
                var sourceLocation = new SourceLocation(filePath, lineNumber);
                words.Add(new WordToLoad(word, resultTags, [sourceLocation]) );
            }
        }

        return words;
    }


    /// <summary>
    /// Loads words to load from files and merges them together.
    /// This merge happens in the sense that all the info of all the word instances
    /// become part of the new merged instance.
    ///
    /// This does not solve the does not solve the duplication due to potential
    /// multiple alternative forms of the same word!
    /// That is handle separately elsewhere for now.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    List<WordToLoad> LoadFiles(IEnumerable<string> filePaths) {
        List<WordToLoad> loadedWords = [];
        foreach(var filePath in filePaths) {
            loadedWords.AddRange(LoadFile(filePath) );
        }

        var mergedWords = loadedWords
            .GroupBy(item => item.Word)
            .Select(group => {
                var tags = group.SelectMany(x => x.Tags).Distinct().ToList();
                var locations = group.SelectMany(x => x.SourceLocations).Distinct().ToList();
                return new WordToLoad(group.Key, tags, locations);
            })
            .ToList();

         return mergedWords;
    }

    // IDEA
    // ====
    // > LoadFiles
    //   -> List<WordToLoad>
    // > Deduplicate by word while merging tags and source locations.
    //   - memorize duplicates (we call these verbatim duplicates)
    // > Log verbatime duplicates
    // > map each to 0-or-more base form based instances by calling ekilex form api.
    //   -> List<NormalizedWord>
    // > Deduplicate by id while merging tags and source locations.
    //   - memorize duplicates (we call these form duplicates)
    // > Log form duplicates 
    // > Process normalized words

    async private void LoadFilesFromFolder_Click(object sender, RoutedEventArgs e) {
        StartAutoScroll();
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
        List<(WordToLoad wordToLoad, string problem)> problematicWords = [];
        var sqliteExceptionOccured = false;
        await Task.Run( () => {
            using var ankiDb = new AnkiDatabase(settings.AnkiProfileName);

            var wordsToLoad = LoadFiles(fileDialog.FileNames);
            DispatchWriteNewLine();
            DispatchWriteLine("Loading word id-s:");
            DispatchWriteLine("------------------");

            List<(NormalizedWord, WordToLoad)> duplicates = [];
            Dictionary<int, WordToLoad> entriesToLoad = [];
            foreach(var (wordToLoad, idx) in wordsToLoad.WithIndexBase1() ) {
                DispatchWriteLine($"{idx:D3}/{wordsToLoad.Count:D3} {wordToLoad.Word}");
                var normalizedWords = processor.DetermineWordIds(wordToLoad.Word);
                if (normalizedWords.Count == 0) {
                    var problem = "No word id-s found.";
                    DispatchWriteLine($"    {problem}");
                    problematicWords.Add( (wordToLoad, problem) );
                    continue;
                }

                foreach(var normalizedWord in normalizedWords) {
                    DispatchWriteLine($"    [{normalizedWord.Id,8}]  {normalizedWord.BaseForm}");
                    if ( entriesToLoad.TryGetValue(normalizedWord.Id, out var existingWordToLoad) ) {
                        duplicates.Add( (normalizedWord, wordToLoad) );
                        DispatchWriteLine($"        Duplicate.");
                        foreach(var tag in wordToLoad.Tags) {
                            if ( ! existingWordToLoad.Tags.Contains(tag) )
                                existingWordToLoad.Tags.Add(tag);
                        }
                        foreach(var location in wordToLoad.SourceLocations) {
                            if ( ! existingWordToLoad.SourceLocations.Contains(location) ) //<< TODO: this is probably not necessary, we can't visit the word coming from the same line twice, right?
                                existingWordToLoad.SourceLocations.Add(location);
                        }
                    } else {
                        DispatchWriteLine($"        New.");
                        var newWordToLoad = new WordToLoad(normalizedWord.BaseForm, wordToLoad.Tags, wordToLoad.SourceLocations);
                        entriesToLoad.Add(normalizedWord.Id, newWordToLoad);
                    }
                }
            }
            //<< TODO: this hasLoadedAtLeastOne thing is now with the change to id based processing no longer seems fit.

            DispatchWriteNewLine();
            DispatchWriteLine("Duplicates:");
            DispatchWriteLine("----------");
            foreach(var (normalizedWord, wordToLoad) in duplicates) {
                var locations = wordToLoad.SourceLocations.Select(s => $"{s.LineNumber} @ {s.FileName}").StringJoin(" ");
                DispatchWriteLine($"{normalizedWord.BaseForm} {wordToLoad.Word} {locations}");
            }

            DispatchWriteNewLine();
            DispatchWriteLine("Processing words/word-id-s:");
            DispatchWriteLine("--------------------------");
            foreach(var (idWithWordToLoad, idx) in entriesToLoad.WithIndexBase1() ) {
                var id = idWithWordToLoad.Key;
                var wordToLoad = idWithWordToLoad.Value;
                DispatchWriteLine($"{idx:D3}/{entriesToLoad.Count:D3}  [{id,8}] {wordToLoad.Word,-20}  {wordToLoad.Tags.StringJoin(" ")}");
                DispatchWriteLine($"    {wordToLoad.SourceLocations.Select(l => $"{l.LineNumber} @ {l.FileName}").StringJoin(", ")}");
                //DispatchWriteLine($"    wordId-s: {wordIds.Count}"); //<< think I don't need this since now I am writing x/y ... x out of y

                bool hasLoadedAtLeastOne = false;

                try {
                    var rows = ankiDb.GetNoteFields(id.ToString(), settings.TagForDBChecking).ToList();
                    if (rows.Count != 0) {
                        DispatchWriteLine($"    already in DB.");
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
                var loadWordResult = processor.LoadWord(wordToLoad, id);
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
                DispatchWriteLine($"    {report}  time=({sw_getWordDetail.ElapsedMilliseconds,5})");

                if ( ! hasLoadedAtLeastOne) {
                    problematicWords.Add( (wordToLoad, loadWordResult.reason!) );
                    DispatchWriteLine("    FAILED.");
                }
            }
        } );
        sw_getDetails.Stop();
        if (sqliteExceptionOccured) return;
        WriteLine($"{nameof(sw_getDetails)} = {sw_getDetails}");

        if (problematicWords.Count is not 0) {
            WriteNewLine();
            WriteLine("Problematic words/phrases:");
            WriteLine("-------------------------");
            List<WordToLoad> wordsWithoutRepresentant = [];
            foreach(var group in problematicWords.GroupBy(pw => pw.problem) ) {
                WriteLine($":: {group.Key}");
                foreach(var word in group) {
                    WriteLine($"    {word.wordToLoad.Word}");
                    if (EkilexCardDataCollection.Any(c => c.Form1.ToLower().Equals(word.wordToLoad.Word.ToLower()) ) ) {
                        AppendToPreviousLine($" ... has representant.");
                    } else {
                        AppendToPreviousLine($" ... NO REPRESENTANT.");
                        wordsWithoutRepresentant.Add(word.wordToLoad);
                    }
                }
            }

            WriteNewLine();
            WriteLine("Words without representants:");
            WriteLine("---------------------------");
            foreach(var word in wordsWithoutRepresentant) {
                WriteLine($"{word.Word}");
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
            var adsf = EkilexCardDataCollection.Select(cd => cd.ToAnkiRow() );
            sb.AppendLine(CardData.Header() );
            sb.AppendJoin("\n", adsf);

            Utilities.EnsureFileAndWriteAllText(ankiFilePath, sb.ToString() );
            Utilities.EnsureFileAndWriteAllText(logFilePath, Logs.StringJoin(Environment.NewLine) );

            WriteLine("Files written:");
            WriteLine($"- {ankiFilePath}");
            WriteLine($"  written {EkilexCardDataCollection.Count} cards/rows.");
            WriteLine($"- {logFilePath}");
        }
        StopAutoScroll();
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
