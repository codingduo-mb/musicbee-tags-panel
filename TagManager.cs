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
                foreach (var tag in ReadTagsFromFile(fileName, tagsStorage.GetMetaDataType()))
                {
                    tagCounts[tag] = tagCounts.TryGetValue(tag, out var count) ? count + 1 : 1;
                }
            }

            int totalFiles = fileNames.Length;
            return tagCounts.ToDictionary(entry => entry.Key, entry => entry.Value == totalFiles ? CheckState.Checked : CheckState.Indeterminate);
        }

        public string SortTagsAlphabetical(string tags)
        {
            return string.Join(Separator.ToString(), tags.Split(Separator).Distinct().OrderBy(tag => tag));
        }

        public string RemoveTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(Separator));
            tagList.Remove(selectedTag.Trim());
            return string.Join(Separator.ToString(), tagList);
        }

        public string AddTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            var tagList = new HashSet<string>(GetTags(fileUrl, metaDataType).Split(Separator)) { selectedTag.Trim() };
            return string.Join(Separator.ToString(), tagList);
        }

        public bool IsTagAvailable(string tagName, string fileUrl, MetaDataType metaDataType)
        {
            return GetTags(fileUrl, metaDataType).Split(Separator).Contains(tagName);
        }

        public string GetTags(string fileUrl, MetaDataType metaDataType)
        {
            return string.Join(Separator.ToString(), ReadTagsFromFile(fileUrl, metaDataType));
        }

        public void SetTagsInFile(string[] fileUrls, CheckState selected, string selectedTag, MetaDataType metaDataType)
        {
            foreach (var fileUrl in fileUrls)
            {
                var tagsFromFile = selected == CheckState.Checked ? AddTag(selectedTag, fileUrl, metaDataType) : RemoveTag(selectedTag, fileUrl, metaDataType);
                var sortedTags = _settingsStorage.RetrieveTagsStorageByTagName(metaDataType.ToString()).Sorted ? SortTagsAlphabetical(tagsFromFile) : tagsFromFile;

                _mbApiInterface.Library_SetFileTag(fileUrl, metaDataType, sortedTags);
                _mbApiInterface.Library_CommitTagsToFile(fileUrl);
            }

            _mbApiInterface.MB_SetBackgroundTaskMessage("Tags updated");
        }

        public string[] ReadTagsFromFile(string fileName, MetaDataType metaDataField)
        {
            if (string.IsNullOrEmpty(fileName) || metaDataField == 0)
            {
                return Array.Empty<string>();
            }

            return _mbApiInterface.Library_GetFileTag(fileName, metaDataField)
                .Split(Separator)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToArray();
        }

        public Dictionary<string, CheckState> UpdateTagsFromFile(string sourceFileUrl, MetaDataType metaDataType)
        {
            return ReadTagsFromFile(sourceFileUrl, metaDataType).ToDictionary(tag => tag, _ => CheckState.Checked);
        }
    }
}