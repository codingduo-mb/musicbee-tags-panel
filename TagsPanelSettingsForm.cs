﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagsPanelSettingsForm : Form
    {
        private const string GitHubLink = "https://github.com/mat-st/musicbee-tags-panel";
        private const string TooltipAddTagPage = "Add & select a new tag and a new tabpage";
        private const string TagExistsWarning = "Tag exists already";
        private const string RemoveTagPageWarning = "This will remove the current tag page and you will lose your current tag list. Continue?";
        private const string WarningCaption = "Warning";
        private const string AboutMessage = "Tags-Panel Plugin \nVersion {0}\nVisit us on GitHub";
        private const string AboutCaption = "About Tags-Panel Plugin";

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

            foreach (var storage in SettingsStorage.TagsStorages.Values)
            {
                AddPanel(storage);
            }

            toolTipAddTagPage.SetToolTip(btnAddTabPage, TooltipAddTagPage);
        }

        private void AddPanel(TagsStorage storage)
        {
            var tagName = storage.GetTagName();
            if (tagPanels.ContainsKey(tagName))
            {
                ShowMessageBox($"This Metadata Type was already added", TagExistsWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tagsPanelSettingsPanel = new TagsPanelSettingsPanel(tagName);
            tagPanels[tagName] = tagsPanelSettingsPanel;
            var tabPage = new TabPage(tagName) { Controls = { tagsPanelSettingsPanel } };
            tabControlSettings.TabPages.Add(tabPage);
            tagsPanelSettingsPanel.SetUpPanelForFirstUse();
        }

        private void Btn_AddTagPage_Click(object sender, EventArgs e)
        {
            var usedTags = tagPanels.Keys.ToList();
            using (var form = new TabPageSelectorForm(usedTags))
            {
                var result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    var storage = new TagsStorage { MetaDataType = form.GetMetaDataType() };
                    if (storage.MetaDataType != null)
                    {
                        try
                        {
                            AddPanel(storage);
                        }
                        catch (ArgumentException ex)
                        {
                            ShowMessageBox(ex.Message, TagExistsWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        private void BtnRemoveTagPage_Click(object sender, EventArgs e)
        {
            var tabToRemove = tabControlSettings.SelectedTab;
            if (tabToRemove != null)
            {
                var dialogResult = MessageBox.Show(RemoveTagPageWarning, WarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    var tagName = tabToRemove.Text;
                    tabControlSettings.TabPages.Remove(tabToRemove);
                    SettingsStorage.RemoveTagStorage(tagName);
                    tagPanels.Remove(tagName);
                }
            }
        }

        private void LinkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowMessageBox(string.Format(AboutMessage, VersionLbl.Text), AboutCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LinkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(GitHubLink);
        }

        private void ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, buttons, icon);
        }
    }
}
