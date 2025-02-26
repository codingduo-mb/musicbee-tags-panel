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

            // Suspend layout to reduce visual updates
            this.SuspendLayout();
            CheckedListBoxWithTags.BeginUpdate();

            try
            {
                // Prepare all items before adding them to reduce visual updates
                List<object> itemsToAdd = new List<object>();
                List<CheckState> statesToApply = new List<CheckState>();

                var tagsFromSettings = _tagsStorage?.GetTags();

                if (tagsFromSettings != null)
                {
                    // Sort tags based on their indices in TagsStorage
                    var sortedTagsFromSettings = tagsFromSettings
                        .OrderBy(tag => tag.Value)
                        .Select(tag => tag.Key.Trim())
                        .ToList();

                    // Prepare tags from settings in the specified order
                    foreach (var tag in sortedTagsFromSettings)
                    {
                        itemsToAdd.Add(tag);
                        if (data.TryGetValue(tag, out var checkState))
                        {
                            statesToApply.Add(checkState);
                        }
                        else
                        {
                            statesToApply.Add(CheckState.Unchecked);
                        }
                    }

                    // Prepare any additional tags not in the settings
                    var additionalTags = data.Keys.Except(sortedTagsFromSettings).ToList();
                    foreach (var tag in additionalTags)
                    {
                        itemsToAdd.Add(tag);
                        statesToApply.Add(data[tag]);
                    }
                }
                else
                {
                    // If no tags from settings, prepare tags from data
                    foreach (var kvp in data)
                    {
                        itemsToAdd.Add(kvp.Key);
                        statesToApply.Add(kvp.Value);
                    }
                }

                // Calculate column width before adding items
                int maxWidth = CalculateMaxStringPixelWidth(itemsToAdd.Cast<string>());

                // Clear and add all items at once
                CheckedListBoxWithTags.Items.Clear();

                // Add all items at once using object[] to minimize UI updates
                for (int i = 0; i < itemsToAdd.Count; i++)
                {
                    CheckedListBoxWithTags.Items.Add(itemsToAdd[i], statesToApply[i]);
                }

                // Set column width only once
                CheckedListBoxWithTags.ColumnWidth = maxWidth + PaddingWidth;
            }
            finally
            {
                CheckedListBoxWithTags.EndUpdate();
                this.ResumeLayout();
            }
        }

        private int CalculateMaxStringPixelWidth(IEnumerable<string> strings)
        {
            if (CheckedListBoxWithTags.IsDisposed || strings == null || !strings.Any())
            {
                return 0;
            }

            int maxWidth = 0;

            // Use a static bitmap to avoid creating Graphics context from control
            using (var bitmap = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bitmap))
            {
                // Use the font from the control for accurate measurements
                foreach (var str in strings)
                {
                    if (string.IsNullOrEmpty(str)) continue;

                    int width = TextRenderer.MeasureText(g, str, CheckedListBoxWithTags.Font).Width;
                    maxWidth = Math.Max(maxWidth, width);
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
