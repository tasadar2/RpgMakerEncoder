using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RpgMakerEncoder.IO;

namespace RpgMakerEncoder.Configuration
{
    public static class Settings
    {
        private const string SettingsName = "settings.json";
        public static string GamePath { get; set; }
        public static string SourcePath { get; set; }

        public static void Load()
        {
            if (File.Exists(SettingsName))
            {
                var jsonSettings = JToken.Parse(File.ReadAllText(SettingsName));

                var gamePath = jsonSettings.Value<string>("GamePath");
                if (!string.IsNullOrEmpty(gamePath))
                {
                    GamePath = Path.GetFullPath(gamePath);
                }
                var sourcePath = jsonSettings.Value<string>("SourcePath");
                if (!string.IsNullOrEmpty(sourcePath))
                {
                    SourcePath = Path.GetFullPath(sourcePath);
                }
            }
        }

        public static void Save()
        {
            var gamePath = GamePath;
            if (!string.IsNullOrEmpty(gamePath))
            {
                gamePath = DirectoryHelper.MakeRelative(Environment.CurrentDirectory, gamePath);
            }
            var sourcePath = SourcePath;
            if (!string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = DirectoryHelper.MakeRelative(Environment.CurrentDirectory, sourcePath);
            }

            var jsonSettings = new JObject
            {
                {"GamePath", gamePath},
                {"SourcePath", sourcePath}
            };

            File.WriteAllText(SettingsName, jsonSettings.ToString(Formatting.Indented));
        }
    }
}