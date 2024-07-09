using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class ChecklistBoxPanel : UserControl
    {
        private readonly MusicBeeApiInterface mbApiInterface;
        private ItemCheckEventHandler eventHandler;
        private readonly Style controlStyle; // Umbenannt für Klarheit

        private const int PaddingWidth = 5; // Konstante für magische Zahl

        public ChecklistBoxPanel(MusicBeeApiInterface mbApiInterface, Dictionary<string, CheckState> data = null)
        {
            this.mbApiInterface = mbApiInterface;
            controlStyle = new Style(mbApiInterface);

            InitializeComponent();

            data?.Keys.ToList().ForEach(key => AddDataSource(data)); // Vereinfachte Überprüfung und Iteration

            StylePanel();
        }

        public void AddDataSource(Dictionary<string, CheckState> data)
        {
            checkedListBoxWithTags.BeginUpdate();
            checkedListBoxWithTags.Items.Clear();

            foreach (var entry in data)
            {
                checkedListBoxWithTags.Items.Add(entry.Key, entry.Value);
            }

            checkedListBoxWithTags.ColumnWidth = GetLongestStringWidth(data.Keys) + PaddingWidth;
            checkedListBoxWithTags.EndUpdate();
        }

        private int GetLongestStringWidth(IEnumerable<string> strings)
        {
            // Verwendung von var für lokale Variable
            var longestString = strings.Any() ? strings.Max(str => str.Length) : 0;
            return TextRenderer.MeasureText(new string('M', longestString), checkedListBoxWithTags.Font).Width;
        }

        private void StylePanel()
        {
            controlStyle.StyleControl(checkedListBoxWithTags);
            controlStyle.StyleControl(this);
        }

        public void AddItemCheckEventHandler(ItemCheckEventHandler eventHandler)
        {
            this.eventHandler = eventHandler;
            checkedListBoxWithTags.ItemCheck += eventHandler;
        }

        public void RemoveItemCheckEventHandler()
        {
            if (this.eventHandler != null)
            {
                checkedListBoxWithTags.ItemCheck -= this.eventHandler;
                this.eventHandler = null;
            }
        }

        private void CheckedListBox1_KeyUp(object sender, KeyEventArgs e)
        {
            checkedListBoxWithTags.CheckOnClick = true;
        }

        private void CheckedListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            checkedListBoxWithTags.CheckOnClick = false;
        }
    }
}
