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
                // Get tags from settings and prepare items for display
                var (itemsToAdd, statesToApply) = PrepareTagsForDisplay(data);

                // Calculate optimal column width for display
                int maxWidth = CalculateMaxStringPixelWidth(itemsToAdd);

                // Update the UI with the prepared items
                UpdateCheckListBoxItems(itemsToAdd, statesToApply);

                // Set column width only once
                CheckedListBoxWithTags.ColumnWidth = maxWidth + PaddingWidth;
            }
            finally
            {
                CheckedListBoxWithTags.EndUpdate();
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// Prepares the tag items and their check states for display in the checklist box
        /// </summary>
        /// <param name="data">Dictionary of tags and their check states</param>
        /// <returns>Tuple containing lists of items and their corresponding check states</returns>
        private (List<string> Items, List<CheckState> States) PrepareTagsForDisplay(Dictionary<string, CheckState> data)
        {
            var itemsToAdd = new List<string>();
            var statesToApply = new List<CheckState>();
            var tagsFromSettings = _tagsStorage?.GetTags();

            if (tagsFromSettings != null && tagsFromSettings.Any())
            {
                // First add tags from settings in their specified order
                AddTagsFromSettings(tagsFromSettings, data, itemsToAdd, statesToApply);

                // Then add any additional tags not in settings
                AddAdditionalTags(tagsFromSettings.Keys.ToList(), data, itemsToAdd, statesToApply);
            }
            else
            {
                // If no tags from settings, add all tags from data
                foreach (var kvp in data)
                {
                    itemsToAdd.Add(kvp.Key);
                    statesToApply.Add(kvp.Value);
                }
            }

            return (itemsToAdd, statesToApply);
        }

        /// <summary>
        /// Adds tags from settings to the display lists in the order specified by their indices
        /// </summary>
        private void AddTagsFromSettings(
            Dictionary<string, int> tagsFromSettings,
            Dictionary<string, CheckState> data,
            List<string> itemsToAdd,
            List<CheckState> statesToApply)
        {
            var sortedTagsFromSettings = tagsFromSettings
                .OrderBy(tag => tag.Value)
                .Select(tag => tag.Key.Trim())
                .ToList();

            foreach (var tag in sortedTagsFromSettings)
            {
                itemsToAdd.Add(tag);
                statesToApply.Add(data.TryGetValue(tag, out var checkState)
                    ? checkState
                    : CheckState.Unchecked);
            }
        }

        /// <summary>
        /// Adds tags from the data that aren't already in the settings
        /// </summary>
        private void AddAdditionalTags(
            List<string> existingTags,
            Dictionary<string, CheckState> data,
            List<string> itemsToAdd,
            List<CheckState> statesToApply)
        {
            var additionalTags = data.Keys
                .Where(key => !existingTags.Contains(key))
                .ToList();

            foreach (var tag in additionalTags)
            {
                itemsToAdd.Add(tag);
                statesToApply.Add(data[tag]);
            }
        }

        /// <summary>
        /// Updates the CheckedListBox with the prepared items and states
        /// </summary>
        /// <param name="items">The items to display</param>
        /// <param name="states">The check states for each item</param>
        private void UpdateCheckListBoxItems(List<string> items, List<CheckState> states)
        {
            CheckedListBoxWithTags.Items.Clear();

            // Add all items at once
            for (int i = 0; i < items.Count; i++)
            {
                CheckedListBoxWithTags.Items.Add(items[i], states[i]);
            }
        }
        

        /// <summary>
        /// Calculates the maximum width in pixels needed to display a collection of strings.
        /// </summary>
        /// <param name="strings">The collection of strings to measure.</param>
        /// <returns>The width in pixels needed for the widest string plus padding and borders.</returns>
        private int CalculateMaxStringPixelWidth(IEnumerable<string> strings)
        {
            // Early return if control is disposed or no strings to measure
            if (CheckedListBoxWithTags == null ||
                CheckedListBoxWithTags.IsDisposed ||
                strings == null ||
                !strings.Any())
            {
                return 0;
            }

            // Get the font to use for measurement
            Font font = CheckedListBoxWithTags.Font;
            if (font == null)
            {
                return PaddingWidth; // Return minimal width if no font available
            }

            int maxWidth = 0;

            // Create measurement objects only once outside the loop
            using (var bitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (var str in strings)
                {
                    if (string.IsNullOrEmpty(str))
                        continue;

                    // Use TextRenderer for accurate Windows Forms text measurement
                    Size textSize = TextRenderer.MeasureText(graphics, str, font);
                    maxWidth = Math.Max(maxWidth, textSize.Width);
                }
            }

            // Add system border width on both sides plus padding
            return maxWidth + (SystemInformation.BorderSize.Width * 2) + PaddingWidth;
        }

        /// <summary>
        /// Applies the MusicBee skin styles to the panel and its contained controls.
        /// This ensures consistent appearance with the rest of the application.
        /// </summary>
        private void StylePanel()
        {
            if (_controlStyle == null)
                return;

            this.SuspendLayout();
            try
            {
                // Apply styling to the checklist box first
                if (CheckedListBoxWithTags != null && !CheckedListBoxWithTags.IsDisposed)
                {
                    _controlStyle.ApplySkinStyleToControl(CheckedListBoxWithTags);
                }

                // Then apply styling to the panel itself
                _controlStyle.ApplySkinStyleToControl(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling panel: {ex.Message}");
            }
            finally
            {
                this.ResumeLayout(true); // true to perform layout if needed
            }
        }

        /// <summary>
        /// Registers an event handler for the ItemCheck event if it hasn't been registered already.
        /// </summary>
        /// <param name="eventHandler">The event handler to register.</param>
        public void RegisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            if (eventHandler == null)
                return;

            // Unregister first to prevent duplicate registrations
            CheckedListBoxWithTags.ItemCheck -= eventHandler;
            CheckedListBoxWithTags.ItemCheck += eventHandler;
        }

        private bool IsHandlerRegistered(Delegate handler)
        {
            var eventField = typeof(CheckedListBox).GetField("EventItemCheck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var eventKey = eventField?.GetValue(CheckedListBoxWithTags);
            var eventHandlerList = (EventHandlerList)typeof(Component).GetProperty("Events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(CheckedListBoxWithTags);
            var registeredHandler = eventHandlerList?[eventKey];

            return registeredHandler?.GetInvocationList().Contains(handler) ?? false;
        }

        /// <summary>
        /// Unregisters an event handler from the ItemCheck event.
        /// </summary>
        /// <param name="eventHandler">The event handler to unregister.</param>
        public void UnregisterItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            if (eventHandler != null)
            {
                CheckedListBoxWithTags.ItemCheck -= eventHandler;
            }
        }

        /// <summary>
        /// Enables CheckOnClick on key up to allow for keyboard navigation without 
        /// immediately toggling the checkbox state.
        /// </summary>
        private void CheckedListBoxWithTags_KeyUp(object sender, KeyEventArgs e)
        {
            CheckedListBoxWithTags.CheckOnClick = true;
        }

        /// <summary>
        /// Disables CheckOnClick on key down to provide better keyboard navigation experience.
        /// </summary>
        private void CheckedListBoxWithTags_KeyDown(object sender, KeyEventArgs e)
        {
            CheckedListBoxWithTags.CheckOnClick = false;
        }
    }
}