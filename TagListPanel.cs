using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class TagListPanel : UserControl
    {
        private const int PaddingWidth = 10;

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly UIManager _controlStyle;
        private TagsStorage _tagsStorage;

        // Add SettingsManager parameter to the constructor
        public TagListPanel(MusicBeeApiInterface mbApiInterface, SettingsManager settingsManager, string tagName, Dictionary<string, CheckState> data = null)
        {
            _mbApiInterface = mbApiInterface;
            _controlStyle = new UIManager(mbApiInterface, new Dictionary<string, TagListPanel> { { tagName, this } }, new string[0], null);
            _tagsStorage = settingsManager.RetrieveTagsStorageByTagName(tagName);

            InitializeComponent();

            // Set the Name property to ensure the correct tab name
            this.Name = tagName;

            if (data != null)
            {
                PopulateChecklistBoxesFromData(data);
            }

            StylePanel();
        }

        // Add this method to update the TagsStorage and refresh the checklist
        public void UpdateTagsStorage(TagsStorage newTagsStorage)
        {
            _tagsStorage = newTagsStorage;

            // Preserve the current checked states
            var currentCheckStates = CheckedListBoxWithTags.Items.Cast<string>().ToDictionary(
                item => item,
                item => CheckedListBoxWithTags.GetItemCheckState(CheckedListBoxWithTags.Items.IndexOf(item))
            );

            // Re-populate the checklist with updated tags and current check states
            PopulateChecklistBoxesFromData(currentCheckStates);
        }
        public void PopulateChecklistBoxesFromData(Dictionary<string, CheckState> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            CheckedListBoxWithTags.BeginUpdate();
            try
            {
                CheckedListBoxWithTags.Items.Clear();

                var tagsFromSettings = _tagsStorage?.GetTags();

                if (tagsFromSettings != null)
                {
                    // Sort tags based on their indices in TagsStorage
                    var sortedTagsFromSettings = tagsFromSettings
                        .OrderBy(tag => tag.Value)
                        .Select(tag => tag.Key)
                        .ToList();

                    // Add tags from settings in the specified order
                    foreach (var tag in sortedTagsFromSettings)
                    {
                        if (data.TryGetValue(tag, out var checkState))
                        {
                            CheckedListBoxWithTags.Items.Add(tag, checkState);
                        }
                        else
                        {
                            // If the tag is not in the data, add it unchecked
                            CheckedListBoxWithTags.Items.Add(tag, CheckState.Unchecked);
                        }
                    }

                    // Add any additional tags not in the settings
                    var additionalTags = data.Keys.Except(tagsFromSettings.Keys).ToList();
                    foreach (var tag in additionalTags)
                    {
                        CheckedListBoxWithTags.Items.Add(tag, data[tag]);
                    }
                }
                else
                {
                    // If no tags from settings, display tags from data
                    foreach (var kvp in data)
                    {
                        CheckedListBoxWithTags.Items.Add(kvp.Key, kvp.Value);
                    }
                }

                // Adjust the column width based on the longest tag
                CheckedListBoxWithTags.ColumnWidth = CalculateMaxStringPixelWidth(
                    CheckedListBoxWithTags.Items.Cast<string>()) + PaddingWidth;
            }
            finally
            {
                CheckedListBoxWithTags.EndUpdate();
            }
        }

        private int CalculateMaxStringPixelWidth(IEnumerable<string> strings)
        {
            int maxWidth = 0;
            using (Graphics g = CheckedListBoxWithTags.CreateGraphics())
            {
                foreach (var str in strings)
                {
                    int width = TextRenderer.MeasureText(g, str, CheckedListBoxWithTags.Font).Width;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }
            return maxWidth;
        }

        private void StylePanel()
        {
            _controlStyle.ApplySkinStyleToControl(CheckedListBoxWithTags);
            _controlStyle.ApplySkinStyleToControl(this);
        }

        public void RegisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            if (eventHandler != null && !IsHandlerRegistered(eventHandler))
            {
                CheckedListBoxWithTags.ItemCheck += eventHandler;
            }
        }

        private bool IsHandlerRegistered(Delegate handler)
        {
            var eventField = typeof(CheckedListBox).GetField("EventItemCheck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var eventKey = eventField?.GetValue(CheckedListBoxWithTags);
            var eventHandlerList = (EventHandlerList)typeof(Component).GetProperty("Events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(CheckedListBoxWithTags);
            var registeredHandler = eventHandlerList?[eventKey];

            return registeredHandler?.GetInvocationList().Contains(handler) ?? false;
        }

        public void UnregisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            CheckedListBoxWithTags.ItemCheck -= eventHandler;
        }

        private void CheckedListBoxWithTags_KeyUp(object sender, KeyEventArgs e)
        {
            CheckedListBoxWithTags.CheckOnClick = true;
        }

        private void CheckedListBoxWithTags_KeyDown(object sender, KeyEventArgs e)
        {
            CheckedListBoxWithTags.CheckOnClick = false;
        }
    }
}
