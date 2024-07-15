using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class SavedSettingsType
    {
        public Dictionary<string, TagsStorage> TagStorages { get; set; }
    }

    public class SettingsManager
    {
        private const char SettingsSeparator = ';';
        private static Dictionary<string, TagsStorage> _storages;
        private const string SettingsFileName = "mb_tags-panel.Settings.json";
        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly Logger _log;

        public static Dictionary<string, TagsStorage> TagsStorages { get; set; }

        public SettingsManager(MusicBeeApiInterface mbApiInterface, Logger log)
        {
            this._mbApiInterface = mbApiInterface;
            this._log = log;
            TagsStorages = new Dictionary<string, TagsStorage>();
        }

        public void LoadSettingsWithFallback()
        {
            LoadSettings();

            if (TagsStorages == null)
                return;

            TagsStorages = TagsStorages.ToDictionary(storage => storage.Value.GetTagName(), storage => storage.Value);
        }

        private void LoadSettings()
        {
            string filename = GetSettingsPath();

            if (!File.Exists(filename) || new FileInfo(filename).Length == 0)
            {
                // Create a default settings file
                InitializeEmptySettingsFile(filename);
            }

            var json = File.ReadAllText(filename, Encoding.UTF8);
            TagsStorages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(json);
        }

        private void InitializeEmptySettingsFile(string filename)
        {
            var defaultSettings = new Dictionary<string, TagsStorage>();

            var json = JsonConvert.SerializeObject(defaultSettings);

            // Write JSON directly to the file without creating a string first
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.Write(json);
            }

            _log.Info($"{nameof(InitializeEmptySettingsFile)} executed");
        }

        public string GetSettingsPath()
        {
            return Path.Combine(_mbApiInterface.Setting_GetPersistentStoragePath(), SettingsFileName);
        }

        public void SaveAllSettings()
        {
            string settingsPath = GetSettingsPath();

            var json = JsonConvert.SerializeObject(TagsStorages);
            File.WriteAllText(settingsPath, json, Encoding.UTF8);

            _mbApiInterface.MB_SetBackgroundTaskMessage("Settings saved");
        }

        public TagsStorage GetFirstTagsStorage()
        {
            return TagsStorages.FirstOrDefault().Value;
        }

        public static TagsStorage GetTagsStorage(string tagName)
        {
            if (!TagsStorages.TryGetValue(tagName, out TagsStorage tagStorage))
            {
                tagStorage = new TagsStorage
                {
                    MetaDataType = tagName
                };
                TagsStorages.Add(tagName, tagStorage);
            }

            return tagStorage;
        }

        public void SetTagsStorage(TagsStorage tagsStorage)
        {
            TagsStorages[tagsStorage.GetTagName()] = tagsStorage;
        }

        public TagsStorage GetFirstOne()
        {
            return TagsStorages.Values.FirstOrDefault();
        }

        public void RemoveTagStorage(string tagName)
        {
            TagsStorages.Remove(tagName);
        }

        public SettingsManager DeepCopy()
        {
            SettingsManager other = (SettingsManager)this.MemberwiseClone();
            _storages = JsonConvert.DeserializeObject<Dictionary<string, TagsStorage>>(JsonConvert.SerializeObject(TagsStorages));
            return other;
        }

       
    }
}
