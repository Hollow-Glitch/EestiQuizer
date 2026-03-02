using CommunityToolkit.Mvvm.ComponentModel;
using EestiQuizer.Common;
using System.Windows;
using System.Windows.Controls;

namespace EestiQuizer.Views;


[ObservableObject]
public partial class SettingsView : UserControl {

    [ObservableProperty]
    private Settings settings;

    public SettingsView() {
        InitializeComponent();
        Settings = Settings.Load();

    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e) {
        Settings.Save();
    }
}
