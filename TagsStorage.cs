using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    /// <summary>
    /// Manages the storage and retrieval of tags.
    /// </summary>
    public class TagsStorage
    {
        private string _tagMetaDataType;
        private Dictionary<string, int> _tagList = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets a value indicating whether to show tags not in the list.
        /// </summary>
        public bool ShowTagsNotInList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tags should be sorted.
        /// </summary>
        public bool Sorted { get; set; }

        /// <summary>
        /// Clears all tags from the storage.
        /// </summary>
        public void Clear()
        {
            _tagList.Clear();
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
            else
            {
                // Return tags in their original order
                return _tagList.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            }
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
        /// <returns>The metadata type of the tag.</returns>
        public MetaDataType GetMetaDataType()
        {
            return Enum.TryParse(_tagMetaDataType, true, out MetaDataType result) ? result : default;
        }

        /// <summary>
        /// Swaps the element in the tag list with the specified key to a new index.
        /// </summary>
        /// <param name="key">The key of the element to swap.</param>
        /// <param name="newIndex">The new index of the element.</param>
        public void SwapElement(string key, int newIndex)
        {
            if (_tagList.ContainsKey(key))
            {
                _tagList[key] = newIndex;
            }
        }

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
    }
}