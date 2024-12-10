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
        private readonly TagsStorage _tagsStorage;

        // Add SettingsManager parameter to the constructor
        public TagListPanel(MusicBeeApiInterface mbApiInterface, SettingsManager settingsManager, string tagName, Dictionary<string, CheckState> data = null)
        {
            _mbApiInterface = mbApiInterface;
            _controlStyle = new UIManager(mbApiInterface, new Dictionary<string, TagListPanel> { { tagName, this } }, new string[0], null);
            // Use the provided SettingsManager instance to retrieve TagsStorage
            _tagsStorage = settingsManager.RetrieveTagsStorageByTagName(tagName);

            InitializeComponent();

            if (data != null)
            {
                PopulateChecklistBoxesFromData(data);
            }

            StylePanel();
        }
        public void PopulateChecklistBoxesFromData(Dictionary<string, CheckState> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            CheckedListBoxWithTags.BeginUpdate();
            try
            {
                CheckedListBoxWithTags.Items.Clear();

                var tagsFromSettings = _tagsStorage?.GetTags()?.Keys;
            if (tagsFromSettings != null)
            {
                var sortedTagsFromSettings = tagsFromSettings.OrderBy(tag => tag).ToList();

                foreach (var tag in sortedTagsFromSettings)
                {
                    if (data.TryGetValue(tag, out var checkState))
                    {
                        CheckedListBoxWithTags.Items.Add(tag, checkState);
                    }
                }

                var additionalTags = data.Keys.Except(tagsFromSettings).OrderBy(tag => tag);
                foreach (var tag in additionalTags)
                {
                    CheckedListBoxWithTags.Items.Add(tag, data[tag]);
                }
            }

            CheckedListBoxWithTags.ColumnWidth = CalculateMaxStringPixelWidth(data.Keys) + PaddingWidth;
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
