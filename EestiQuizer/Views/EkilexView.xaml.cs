using EestiQuizer.Common;
using EestiQuizer.Ekilex;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace EestiQuizer.Views; 


public partial class EkilexView : UserControl {
    public ObservableCollection<CardData> EkilexCardDataCollection { get; set; } = new();
    Settings settings;

    public EkilexView() {
        InitializeComponent();
        settings = Settings.Load(); //TODO: just temp location, probably should be called sooner, since settings is not UI related.
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
        var processor = new Processor(settings);
        var wordIds = processor.DetermineWordIds(wordToLoad.Word);
        foreach(var wordId in wordIds) {
            WriteLine($"{wordId}");
            var cardData = processor.LoadWord(wordToLoad, wordId);
            EkilexCardDataCollection.Add(cardData);
        }
    }
}
