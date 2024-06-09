using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagsPanelSettingsForm : Form
    {
        private const string GITHUBLINK = "https://github.com/mat-st/musicbee-tags-panel";
        private const string TOOLTIPADDTAGPAGE = "Add & select a new tag and a new tabpage";

        private Dictionary<string, TagsPanelSettingsPanel> tagPanels = new Dictionary<string, TagsPanelSettingsPanel>();
        public SettingsStorage SettingsStorage { get; set; }

        public TagsPanelSettingsForm(SettingsStorage settingsStorage)
        {
            SettingsStorage = settingsStorage;
            InitializeComponent();

            Btn_Save.DialogResult = DialogResult.OK;
            Btn_Cancel.DialogResult = DialogResult.Cancel;

            VersionLbl.Text = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            VersionLbl.ForeColor = Color.Black;

            foreach (TagsStorage storage in SettingsStorage.TagsStorages.Values)
            {
                AddPanel(storage);
            }

            toolTipAddTagPage.SetToolTip(btnAddTabPage, TOOLTIPADDTAGPAGE);
        }

        private bool AddPanel(TagsStorage storage)
        {
            string tagName = storage.GetTagName();
            if (tagPanels.ContainsKey(tagName))
            {
                MessageBox.Show("This Metadata Type was already added", "Tag exists already", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            TagsPanelSettingsPanel tagsPanelSettingsPanel = new TagsPanelSettingsPanel(tagName);
            tagPanels[tagName] = tagsPanelSettingsPanel;
            TabPage tabPage = new TabPage(tagName);
            tabPage.Controls.Add(tagsPanelSettingsPanel);
            tabControlSettings.TabPages.Add(tabPage);
            tagsPanelSettingsPanel.SetUpPanelForFirstUse();

            return true;
        }

        private void Btn_AddTagPage_Click(object sender, EventArgs e)
        {
            List<string> usedTags = tagPanels.Keys.ToList();
            using (TabPageSelectorForm form = new TabPageSelectorForm(usedTags))
            {
                DialogResult result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    TagsStorage storage = new TagsStorage();
                    storage.MetaDataType = form.GetMetaDataType();
                    if (storage.MetaDataType != null && AddPanel(storage))
                    {
                        form.Close();
                    }
                }
            }
        }

        private void BtnRemoveTagPage_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("This will remove the current tag page and you will lose your current tag list. Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                TabPage tabToRemove = tabControlSettings.SelectedTab;
                if (tabToRemove != null)
                {
                    string tagName = tabToRemove.Text;
                    tabControlSettings.TabPages.Remove(tabToRemove);
                    SettingsStorage.RemoveTagStorage(tagName);
                    tagPanels.Remove(tagName);
                }
            }
        }

        private void LinkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Tags-Panel Plugin " + Environment.NewLine + "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine +
                "Visit us on GitHub", "About Tags-Panel Plugin",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LinkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(GITHUBLINK);
        }
    }
}
