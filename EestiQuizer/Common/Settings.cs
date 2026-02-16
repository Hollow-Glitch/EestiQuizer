using System.IO;
using System.Text.Json;

using static System.Environment;
using static EestiQuizer.Common.Utilities;

namespace EestiQuizer.Common; 


internal static class SettingValues {
    internal static string configFilePath = ExpandEnvironmentVariables(@"%userprofile%\.config\eesti_quizer\config.json");
    internal static string defaultOutputFolderPath = ExpandEnvironmentVariables(@"%userprofile%\Documents\eesti_quizer");
}


public sealed class Settings {
    public string OutputFolderPath { get; set; } = SettingValues.defaultOutputFolderPath;
    public string EkilexApiKey { get; set; } = String.Empty;

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
                settings = CreateAndSaveNewFile();
            } else {
                settings = potentialConfig;
            }
        } else {
            settings = CreateAndSaveNewFile();
        }

        return settings;
    }


    private static Settings CreateAndSaveNewFile() {
        var settings = new Settings();
        var contentToWrite = JsonSerializer.Serialize(settings);
        EnsureFileAndWriteAllText(SettingValues.configFilePath, contentToWrite);
        return settings;
    }
}
