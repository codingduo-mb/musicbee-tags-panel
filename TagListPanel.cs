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

        public TagListPanel(MusicBeeApiInterface mbApiInterface, SettingsManager settingsManager, string tagName, Dictionary<string, TagListPanel> checklistBoxList, string[] selectedFileUrls, Action<string[]> refreshPanelTagsFromFiles)
        {
            // Enable double buffering to reduce flickering
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            _mbApiInterface = mbApiInterface;
            _controlStyle = new UIManager(mbApiInterface, checklistBoxList, selectedFileUrls, refreshPanelTagsFromFiles);
            _tagsStorage = settingsManager.RetrieveTagsStorageByTagName(tagName);

            InitializeComponent();

            // Set the Name property to ensure the correct tab name
            this.Name = tagName;

            StylePanel();

            // Also enable for the CheckedListBox
            EnableDoubleBufferingForListBox();
        }

        // Method to enable double buffering for the CheckedListBox using reflection
        private void EnableDoubleBufferingForListBox()
        {
            // CheckedListBox doesn't expose DoubleBuffered property, so we use reflection
            if (CheckedListBoxWithTags != null)
            {
                typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                    ?.SetValue(CheckedListBoxWithTags, true);
            }
        }




        /// <summary>
        /// Updates the TagsStorage and refreshes the checklist with new data.
        /// </summary>
        /// <param name="newTagsStorage">The new TagsStorage.</param>
        /// <param name="data">Dictionary containing tags and their check states.</param>
        public void UpdateTagsStorage(TagsStorage newTagsStorage, Dictionary<string, CheckState> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _tagsStorage = newTagsStorage;
            PopulateChecklistBoxesFromData(data);
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
                        .Select(tag => tag.Key.Trim())
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
                    var additionalTags = data.Keys.Except(sortedTagsFromSettings);
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

                CheckedListBoxWithTags.Refresh();
            }
            finally
            {
                CheckedListBoxWithTags.EndUpdate();
            }
        }

        private int CalculateMaxStringPixelWidth(IEnumerable<string> strings)
        {
            int maxWidth = 0;
            if (CheckedListBoxWithTags.IsDisposed)
            {
                // Handle the disposed state, possibly by returning early or reinitializing the control
                return 0;
            }

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
            return maxWidth + SystemInformation.BorderSize.Width * 2 + PaddingWidth;
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
