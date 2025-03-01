using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    /// <summary>
    /// Manages the storage and retrieval of tags for MusicBee metadata.
    /// </summary>
    public class TagsStorage
    {
        private string _tagMetaDataType;
        private Dictionary<string, int> _tagList;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsStorage"/> class.
        /// </summary>
        public TagsStorage()
        {
            _tagList = new Dictionary<string, int>();
            ShowTagsNotInList = false;
            Sorted = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsStorage"/> class with the specified metadata type.
        /// </summary>
        /// <param name="metadataType">The metadata type for this tag storage.</param>
        public TagsStorage(string metadataType) : this()
        {
            _tagMetaDataType = metadataType;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show tags not in the list.
        /// </summary>
        public bool ShowTagsNotInList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tags should be sorted alphabetically.
        /// </summary>
        public bool Sorted { get; set; }

        /// <summary>
        /// Gets or sets the metadata type of the tag.
        /// </summary>
        public string MetaDataType
        {
            get => _tagMetaDataType;
            set => _tagMetaDataType = value;
        }

        /// <summary>
        /// Gets or sets the tag list.
        /// </summary>
        public Dictionary<string, int> TagList
        {
            get => _tagList;
            set => _tagList = value ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// Clears all tags from the storage.
        /// </summary>
        public void Clear()
        {
            _tagList.Clear();
        }

        /// <summary>
        /// Adds or updates a tag in the list.
        /// </summary>
        /// <param name="tagName">The name of the tag to add or update.</param>
        /// <param name="index">The index value for the tag.</param>
        public void AddTag(string tagName, int index)
        {
            if (string.IsNullOrEmpty(tagName))
                return;

            _tagList[tagName] = index;
        }

        /// <summary>
        /// Removes a tag from the list if it exists.
        /// </summary>
        /// <param name="tagName">The name of the tag to remove.</param>
        /// <returns>True if the tag was removed, false otherwise.</returns>
        public bool RemoveTag(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && _tagList.Remove(tagName);
        }

        /// <summary>
        /// Gets the tags, optionally sorted based on the Sorted property.
        /// </summary>
        /// <returns>A dictionary of tags and their associated values.</returns>
        public Dictionary<string, int> GetTags()
        {
            if (Sorted)
            {
                // Return tags sorted alphabetically
                return _tagList.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            // Return tags in their original order
            return _tagList.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        /// <returns>The name of the tag.</returns>
        public string GetTagName()
        {
            return _tagMetaDataType;
        }

        /// <summary>
        /// Gets the metadata type of the tag.
        /// </summary>
        /// <returns>The metadata type of the tag, or default if the type cannot be parsed.</returns>
        public MetaDataType GetMetaDataType()
        {
            return Enum.TryParse(_tagMetaDataType, true, out MetaDataType result) ? result : default;
        }

        /// <summary>
        /// Checks if a tag exists in the collection.
        /// </summary>
        /// <param name="tagName">The tag name to check.</param>
        /// <returns>True if the tag exists in the collection, false otherwise.</returns>
        public bool ContainsTag(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && _tagList.ContainsKey(tagName);
        }

        /// <summary>
        /// Swaps the element in the tag list with the specified key to a new index.
        /// </summary>
        /// <param name="key">The key of the element to swap.</param>
        /// <param name="newIndex">The new index of the element.</param>
        /// <returns>True if the element was swapped, false if the key doesn't exist.</returns>
        public bool SwapElement(string key, int newIndex)
        {
            if (string.IsNullOrEmpty(key) || !_tagList.ContainsKey(key))
                return false;

            _tagList[key] = newIndex;
            return true;
        }
    }
}