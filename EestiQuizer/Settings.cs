using System.IO;
using System.Text.Json;
using static System.Environment;

namespace EestiQuizer; 


internal static class SettingValues {
    internal static string configFilePath = ExpandEnvironmentVariables(@"%userprofile%\.config\eesti_quizer\config.json");
    internal static string defaultOutputFolderPath = ExpandEnvironmentVariables(@"%userprofile%\Documents\eesti_quizer");
}


internal sealed class Settings {
    public string OutputFolderPath { get; set; } = SettingValues.defaultOutputFolderPath;

    /// <summary>
    /// Use <see cref="Settings.Load"/> instead to obtain an instance.
    /// </summary>
    private Settings() { }

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
        Common.EnsureFileAndWriteAllText(SettingValues.configFilePath, contentToWrite);
        return settings;
    }
}
