using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    /// <summary>
    /// Manages music file tags including reading, writing, and manipulating tag data.
    /// </summary>
    public class TagManager
    {
        private const char Separator = ';';

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly SettingsManager _settingsStorage;

        public TagManager(MusicBeeApiInterface mbApiInterface, SettingsManager settingsStorage)
        {
            if (mbApiInterface.Equals(default(MusicBeeApiInterface)))
                throw new ArgumentNullException(nameof(mbApiInterface));
            if (settingsStorage == null)
                throw new ArgumentNullException(nameof(settingsStorage));

            _mbApiInterface = mbApiInterface;
            _settingsStorage = settingsStorage;
        }

        /// <summary>
        /// Combines tag lists from multiple files and determines their check state.
        /// </summary>
        /// <param name="fileNames">Array of file paths to process</param>
        /// <param name="tagsStorage">The tags storage configuration</param>
        /// <returns>Dictionary of tags and their checked state</returns>
        public Dictionary<string, CheckState> CombineTagLists(string[] fileNames, TagsStorage tagsStorage)
        {
            if (fileNames == null || fileNames.Length == 0)
                return new Dictionary<string, CheckState>();

            var metaDataType = tagsStorage.GetMetaDataType();
            var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileName in fileNames)
            {
                foreach (var tag in ReadTagsFromFile(fileName, metaDataType))
                {
                    tagCounts.TryGetValue(tag, out int count);
                    tagCounts[tag] = count + 1;
                }
            }

            return tagCounts.ToDictionary(
                entry => entry.Key,
                entry => entry.Value == fileNames.Length ? CheckState.Checked : CheckState.Indeterminate,
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sorts tags alphabetically and removes duplicates.
        /// </summary>
        /// <param name="tags">Semicolon-separated tag string</param>
        /// <returns>Sorted, deduplicated tag string</returns>
        public string SortTagsAlphabetical(string tags)
        {
            if (string.IsNullOrEmpty(tags))
                return string.Empty;

            var tagsWithoutDuplicates = new SortedSet<string>(
                tags.Split(Separator).Where(t => !string.IsNullOrWhiteSpace(t)),
                StringComparer.OrdinalIgnoreCase);

            return string.Join(Separator.ToString(), tagsWithoutDuplicates);
        }

        /// <summary>
        /// Removes a tag from a file's tag list.
        /// </summary>
        public string RemoveTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            if (string.IsNullOrEmpty(selectedTag) || string.IsNullOrEmpty(fileUrl))
                return GetTags(fileUrl, metaDataType);

            var tagList = new HashSet<string>(
                GetTags(fileUrl, metaDataType).Split(Separator).Where(t => !string.IsNullOrWhiteSpace(t)),
                StringComparer.OrdinalIgnoreCase);

            tagList.Remove(selectedTag.Trim());
            return string.Join(Separator.ToString(), tagList);
        }

        /// <summary>
        /// Adds a tag to a file's tag list.
        /// </summary>
        public string AddTag(string selectedTag, string fileUrl, MetaDataType metaDataType)
        {
            if (string.IsNullOrEmpty(selectedTag) || string.IsNullOrEmpty(fileUrl))
                return GetTags(fileUrl, metaDataType);

            var existingTags = GetTags(fileUrl, metaDataType);
            var tagList = new HashSet<string>(
                string.IsNullOrEmpty(existingTags)
                    ? Array.Empty<string>()
                    : existingTags.Split(Separator).Where(t => !string.IsNullOrWhiteSpace(t)),
                StringComparer.OrdinalIgnoreCase);

            tagList.Add(selectedTag.Trim());
            return string.Join(Separator.ToString(), tagList);
        }

        /// <summary>
        /// Checks if a tag is already assigned to a file.
        /// </summary>
        public bool IsTagAvailable(string tagName, string fileUrl, MetaDataType metaDataType)
        {
            if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(fileUrl))
                return false;

            var tagList = new HashSet<string>(
                GetTags(fileUrl, metaDataType).Split(Separator).Where(t => !string.IsNullOrWhiteSpace(t)),
                StringComparer.OrdinalIgnoreCase);

            return tagList.Contains(tagName.Trim());
        }

        /// <summary>
        /// Gets a semicolon-separated string of all tags for a file.
        /// </summary>
        public string GetTags(string fileUrl, MetaDataType metaDataType)
        {
            var tags = ReadTagsFromFile(fileUrl, metaDataType);
            return string.Join(Separator.ToString(), tags);
        }

        /// <summary>
        /// Sets or removes tags for multiple files.
        /// </summary>
        public void SetTagsInFile(string[] fileUrls, CheckState selected, string selectedTag, MetaDataType metaDataType)
        {
            if (fileUrls == null || fileUrls.Length == 0 || string.IsNullOrEmpty(selectedTag))
                return;

            var tagsStorage = _settingsStorage.RetrieveTagsStorageByTagName(metaDataType.ToString());
            var isAdding = selected == CheckState.Checked;

            foreach (var fileUrl in fileUrls)
            {
                if (string.IsNullOrEmpty(fileUrl))
                    continue;

                var tagsFromFile = isAdding
                    ? AddTag(selectedTag, fileUrl, metaDataType)
                    : RemoveTag(selectedTag, fileUrl, metaDataType);

                var sortedTags = tagsStorage.Sorted
                    ? SortTagsAlphabetical(tagsFromFile)
                    : tagsFromFile;

                _mbApiInterface.Library_SetFileTag(fileUrl, metaDataType, sortedTags);
                _mbApiInterface.Library_CommitTagsToFile(fileUrl);
            }

            _mbApiInterface.MB_SetBackgroundTaskMessage($"{(isAdding ? "Added" : "Removed")} tag '{selectedTag}' to {fileUrls.Length} file(s)");
        }

        /// <summary>
        /// Reads and parses tags from a file.
        /// </summary>
        public string[] ReadTagsFromFile(string fileName, MetaDataType metaDataField)
        {
            if (string.IsNullOrEmpty(fileName) || metaDataField == 0)
                return Array.Empty<string>();

            var filetagMetaDataFields = _mbApiInterface.Library_GetFileTag(fileName, metaDataField);

            if (string.IsNullOrEmpty(filetagMetaDataFields))
                return Array.Empty<string>();

            return filetagMetaDataFields
                .Split(Separator)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToArray();
        }

        /// <summary>
        /// Creates a dictionary of tags from a single file, all marked as checked.
        /// </summary>
        public Dictionary<string, CheckState> UpdateTagsFromFile(string sourceFileUrl, MetaDataType metaDataType)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
                return new Dictionary<string, CheckState>();

            return ReadTagsFromFile(sourceFileUrl, metaDataType)
                .ToDictionary(tag => tag, _ => CheckState.Checked, StringComparer.OrdinalIgnoreCase);
        }
    }
}