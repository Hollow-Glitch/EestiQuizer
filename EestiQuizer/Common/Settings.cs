using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using static System.Environment;
using static EestiQuizer.Common.Utilities;

namespace EestiQuizer.Common; 


internal static class SettingValues {
    internal static string configFilePath = ExpandEnvironmentVariables(@"%userprofile%\.config\eesti_quizer\config.json");
    internal static string defaultOutputFolderPath = ExpandEnvironmentVariables(@"%userprofile%\Documents\eesti_quizer");
}


/// <summary>
/// This class has multiple purposes:
/// - UI   : hence ObservableObject.
/// - JSON : deserialized from file with JsonSerilizer.
/// </summary>
public partial class Settings : ObservableObject {
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImageCachePath))]
    [NotifyPropertyChangedFor(nameof(WordDetailsCachePath))]
    [NotifyPropertyChangedFor(nameof(WordIdsCachePath))]
    private string outputFolderPath = SettingValues.defaultOutputFolderPath;
    [ObservableProperty]
    private string ekilexApiKey = String.Empty;
    [ObservableProperty]
    private string ankiProfileName = String.Empty;
    [ObservableProperty]
    private string tagForDBChecking = String.Empty;

    /// <summary>
    /// Represents the baseline example/usage sentence length we are looking for,
    /// if not found, then we are looking for shorter and longer sentences
    /// </summary>
    [ObservableProperty]
    private int sentenceLengthOrigin = 2;

    /// <summary>
    /// Number of usage/example sentences we will add to the CardData.
    /// </summary>
    [ObservableProperty]
    private int usageSentencesToTake = 10;

    [JsonIgnore]
    public string ImageCachePath => Path.Combine(OutputFolderPath, "images");

    [JsonIgnore]
    public string WordIdsCachePath => Path.Combine(OutputFolderPath, "wordIds");

    [JsonIgnore]
    public string WordDetailsCachePath => Path.Combine(OutputFolderPath, "wordDetails");

    /// <summary>
    /// Use <see cref="Settings.Load"/> to load from file, this only creates a default instance.
    /// It must be internal so that the JsonSerializer can work with it.
    /// </summary>
    public Settings() { }

    internal static Settings Load() {
        var exists = File.Exists(SettingValues.configFilePath);

        //>> load or create config file
        Settings settings;
        if (exists) {
            var content = File.ReadAllText(SettingValues.configFilePath);
            Settings? potentialConfig = JsonSerializer.Deserialize<Settings>(content);
            if (potentialConfig is null) {
                settings = CreateAndSaveNewSettings();
            } else {
                settings = potentialConfig;
            }
        } else {
            settings = CreateAndSaveNewSettings();
        }

        return settings;
    }


    internal void Save() {
        _ = CreateAndSaveSettings(this);
    }


    private static Settings CreateAndSaveNewSettings() {
        return CreateAndSaveSettings(new Settings() );
    }


    internal static Settings CreateAndSaveSettings(Settings settings) {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var contentToWrite = JsonSerializer.Serialize(settings, options);
        EnsureFileAndWriteAllText(SettingValues.configFilePath, contentToWrite);
        return settings;
    }
}
