using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagsPanelSettingsForm : Form
    {
        
        const string GITHUBLINK = "https://github.com/mat-st/musicbee-tags-panel";

        const string TOOLTIPADDTAGPAGE = "Add & select a new tag and a new tabpage";


        private Dictionary<string, TagsPanelSettingsPanel> tagPanels = new Dictionary<string, TagsPanelSettingsPanel>();
        private SettingsStorage settingsStorage;

        public SettingsStorage SettingsStorage { get => settingsStorage; set => settingsStorage = value; }

        public TagsPanelSettingsForm(SettingsStorage settingsStorage)
        {
            this.settingsStorage = settingsStorage;
            InitializeComponent();

            this.Btn_Save.DialogResult = DialogResult.OK;
            this.Btn_Cancel.DialogResult = DialogResult.Cancel;

            Version pluginVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            VersionLbl.Text = $"Version: {pluginVersion.ToString()}";
            VersionLbl.ForeColor = Color.Black;

            foreach (TagsStorage storage in SettingsStorage.TagsStorages.Values)
            {
                AddPanel(storage);
            }

            // Tooltips

            toolTipAddTagPage.SetToolTip(this.btnAddTabPage, TOOLTIPADDTAGPAGE);
        }



        private bool AddPanel(TagsStorage storage)
        {
            string tagName = storage.GetTagName();
            if (tagPanels.ContainsKey(tagName))
            {
                ShowWarningMetaDataTypeExists();
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


        private void AddTagPage()
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
                else if (result == DialogResult.Cancel)
                {
                    form.Close();
                }
            }
        }

        private void RemoveTagPage()
        {
            TabPage tabToRemove = tabControlSettings.SelectedTab;
            if (tabToRemove != null)
            {
                string tagName = tabToRemove.Text;
                tabControlSettings.TabPages.Remove(tabToRemove);
                settingsStorage.RemoveTagStorage(tagName);
                tagPanels.Remove(tagName);
            }
        }

        /***************************
        LINKLABELS
        ***************************/

        private void LinkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            MessageBox.Show("Tags-Panel Plugin " + Environment.NewLine + "Version " + version + Environment.NewLine +
                "Visit us on GitHub", "About Tags-Panel Plugin",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void LinkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(GITHUBLINK);
        }

        /***************************
        BUTTONS
        ***************************/

        private void Btn_AddTagPage_Click(object sender, EventArgs e)
        {
            AddTagPage();
        }

        private void BtnRemoveTagPage_Click(object sender, EventArgs e)
        {
            ShowDialogToRemoveTagPage();
        }





        /***************************
        DIALOGS
        ***************************/
        private void ShowDialogToRemoveTagPage()
        {
            DialogResult dialogResult = MessageBox.Show("This will remove the current tag page and you will lose your current tag list. Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                RemoveTagPage();
            }
        }

        private void ShowWarningMetaDataTypeExists()
        {
            MessageBox.Show("This Metadata Type was already added", "Tag exists already",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
                
    }
}
