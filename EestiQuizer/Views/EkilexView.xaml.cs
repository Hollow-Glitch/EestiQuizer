using EestiQuizer.Common;
using EestiQuizer.Ekilex;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace EestiQuizer.Views; 


public partial class EkilexView : UserControl {
    public ObservableCollection<CardData> EkilexCardDataCollection { get; set; } = new();
    Settings settings;
    Processor processor;

    public EkilexView() {
        InitializeComponent();
        settings = Settings.Load(); //TODO: just temp location, probably should be called sooner, since settings is not UI related.
        processor = new Processor(settings);
    }

    void WriteNewLine() { OutputBox.Text += "\n"; }


    void WriteLine(string text) { OutputBox.Text += text + "\n"; }


    void Write(string text) { OutputBox.Text += text; }


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


    private void LoadFilesFromFolder_Click(object sender, RoutedEventArgs e) {
        var fileDialog = new OpenFileDialog { Multiselect = true, };
        if (fileDialog.ShowDialog() != true) return;

        WriteLine("File names:");
        foreach (var name in fileDialog.FileNames) {
            WriteLine($"- {name}");
        }

        var saveFolder = Directory.GetParent(fileDialog.FileNames[0]);

        var allWordsWithoutComments = new List<WordToLoad>();
        foreach (var fileName in fileDialog.FileNames) {
            var wordsOfFile = File.ReadAllLines(fileName)
                .Where(line => !line.Contains("#") && !string.IsNullOrWhiteSpace(line));
            string[] tags = Path.GetFileNameWithoutExtension(fileName).Split("__") ?? [];
            var wordsToLoad = wordsOfFile.Select(word => new WordToLoad(word, tags));
            foreach (var wordToLoad in wordsToLoad) allWordsWithoutComments.Add(wordToLoad);
        }

        if (false) WriteLine(allWordsWithoutComments.Select(wordToLoad => wordToLoad.Word).StringJoin("\n"));

        var wordsWithIds = allWordsWithoutComments.AsParallel()
            .SelectMany(word => processor.DetermineWordIds(word.Word).Select(id => (word, id)) )
            .ToList();

        foreach(var (word, id) in wordsWithIds) {
            var cardData = processor.LoadWord(word, id);
            if (cardData is not null) EkilexCardDataCollection.Add(cardData);
        }
    }
}
