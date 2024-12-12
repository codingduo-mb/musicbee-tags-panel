using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class TagManager
    {
        private const char Separator = ';';

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly SettingsManager _settingsStorage;

        public TagManager(MusicBeeApiInterface mbApiInterface, SettingsManager settingsStorage)
        {
            _mbApiInterface = mbApiInterface;
            _settingsStorage = settingsStorage;
        }

        public Dictionary<string, CheckState> CombineTagLists(string[] fileNames, TagsStorage tagsStorage)
        {
            var tagCounts = new Dictionary<string, int>();

            foreach (var fileName in fileNames)
            {
                var tagsFromFile = ReadTagsFromFile(fileName, tagsStorage.GetMetaDataType());

                foreach (var tag in tagsFromFile)
                {
                    if (tagCounts.ContainsKey(tag))
                    {
                        tagCounts[tag]++;
                    }
                    else
                    {
                        tagCounts[tag] = 1;
                    }
                }
            }

            return tagCounts.ToDictionary(entry => entry.Key, entry => entry.Value == fileNames.Length ? CheckState.Checked : CheckState.Indeterminate);
        }

        public string SortTagsAlphabetical(string tags)
        {
            var tagsWithoutDuplicates = new SortedSet<string>(tags.Split(Separator));
            return string.Join(Separator.ToString(), tagsWithoutDuplicates);
        }

        public string RemoveTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(Separator));
            tagList.Remove(selectedTag.Trim());
            return string.Join(Separator.ToString(), tagList);
        }

        public string AddTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var existingTags = GetTags(fileUrl, metaDataType);
            var tagList = new HashSet<string>(string.IsNullOrEmpty(existingTags) ? new string[0] : existingTags.Split(Separator));
            tagList.Add(selectedTag.Trim());
            return string.Join(Separator.ToString(), tagList);
        }

        public bool IsTagAvailable(string tagName, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(Separator));
            return tagList.Contains(tagName);
        }

        public string GetTags(string fileUrl, MetaDataType metaDataType)
        {
            var tags = ReadTagsFromFile(fileUrl, metaDataType);
            tags = tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToArray();
            // Entfernt TrimStart(Separator), da es nicht mehr benötigt wird
            return string.Join(Separator.ToString(), tags);
        }

        // TagManager.cs (If necessary)

        public void SetTagsInFile(string[] fileUrls, CheckState selected, string selectedTag, MetaDataType metaDataType)
        {
            var tagsStorage = _settingsStorage.RetrieveTagsStorageByTagName(metaDataType.ToString());

            foreach (var fileUrl in fileUrls)
            {
                var tagsFromFile = selected == CheckState.Checked ? AddTag(selectedTag, fileUrl, metaDataType) : RemoveTag(selectedTag, fileUrl, metaDataType);
                var sortedTags = tagsStorage.Sorted ? SortTagsAlphabetical(tagsFromFile) : tagsFromFile;

                _mbApiInterface.Library_SetFileTag(fileUrl, metaDataType, sortedTags);
                _mbApiInterface.Library_CommitTagsToFile(fileUrl);
            }

            _mbApiInterface.MB_SetBackgroundTaskMessage("Applied tags to files");
        }

        public string[] ReadTagsFromFile(string fileName, MetaDataType metaDataField)
        {
            if (string.IsNullOrEmpty(fileName) || metaDataField == 0)
            {
                return Array.Empty<string>();
            }

            var filetagMetaDataFields = _mbApiInterface.Library_GetFileTag(fileName, metaDataField);
            return filetagMetaDataFields.Split(Separator).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToArray();
        }

        public Dictionary<string, CheckState> UpdateTagsFromFile(string sourceFileUrl, MetaDataType metaDataType)
        {
            return ReadTagsFromFile(sourceFileUrl, metaDataType).ToDictionary(tag => tag, _ => CheckState.Checked);
        }
    }
}