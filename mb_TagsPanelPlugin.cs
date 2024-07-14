// A MusicBee plugin that displays a panel with tabpages containing checklistboxes. The user can select tags from the checklistboxes and the plugin will update the tags in the selected files.
// The plugin also has a settings dialog that allows the user to define the tags and the order in which they are displayed.
// The plugin also has a logger that logs errors and information messages. The plugin also has a settings storage class that saves the settings to a file.
// The plugin also has a tags manipulation class that manipulates the tags in the selected files. The plugin also has a plugin info class that contains information about the plugin.
// The plugin also has a tags storage class that contains the tags and the order in which they are displayed. The plugin also has a checklistbox panel class that contains a checklistbox and a style class that styles

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private Logger log;
        private Control _panel;
        private TabControl tabControl;
        private List<MetaDataType> tags = new List<MetaDataType>();
        private Dictionary<string, ChecklistBoxPanel> checklistBoxList = new Dictionary<string, ChecklistBoxPanel>();
        private Dictionary<string, TabPage> _tabPageList = new Dictionary<string, TabPage>();
        private Dictionary<string, CheckState> tagsFromFiles = new Dictionary<string, CheckState>();
        private SettingsStorage settingsStorage;
        private TagsManipulation tagsManipulation;
        private string metaDataTypeName;
        private bool sortAlphabetically;
        private PluginInfo about = new PluginInfo();
        private string[] selectedFileUrls = Array.Empty<string>();
        private bool ignoreEventFromHandler = true;
        private bool ignoreForBatchSelect = true;

        #region Initialise plugin

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            InitializeApi(apiInterfacePtr);

            about = CreatePluginInfo();
            InitializePluginComponents();

            return about;
        }

        private void InitializeApi(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
        }

        private PluginInfo CreatePluginInfo()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return new PluginInfo
            {
                PluginInfoVersion = PluginInfoVersion,
                Name = "Tags-Panel",
                Description = "Creates a dockable Panel with user-defined tabbed pages which let the user choose tags from user-defined lists",
                Author = "kn9ff & The Anonymous Programmer",
                TargetApplication = "Tags-Panel",
                Type = PluginType.General,
                VersionMajor = (short)version.Major,
                VersionMinor = (short)version.Minor,
                Revision = 1,
                MinInterfaceVersion = MinInterfaceVersion,
                MinApiRevision = MinApiRevision,
                ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents | ReceiveNotificationFlags.DataStreamEvents,
                ConfigurationPanelHeight = 0
            };
        }

        private void InitializePluginComponents()
        {
            checklistBoxList = new Dictionary<string, ChecklistBoxPanel>();
            tagsFromFiles = new Dictionary<string, CheckState>();
            _tabPageList = new Dictionary<string, TabPage>();

            InitializeLogger();

            settingsStorage = new SettingsStorage(mbApiInterface, log);
            tagsManipulation = new TagsManipulation(mbApiInterface, settingsStorage);

            LoadPluginSettings();

            InitializeMenu();

            log.Info($"{nameof(InitializePluginComponents)} started");
        }

        public bool Configure(IntPtr panelHandle)
        {
            // save any persistent settings in a sub-folder of this path
            // panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
            ShowSettingsDialog();
            return true;
        }

        private void InitializeLogger()
        {
            log = new Logger(mbApiInterface);
        }

        private void InitializeMenu()
        {
            mbApiInterface.MB_AddMenuItem("mnuTools/Tags-Panel Settings", "Tags-Panel: Open Settings", SettingsMenuClicked);
        }

        private void LoadPluginSettings()
        {
            settingsStorage.LoadSettingsWithFallback();
            UpdateSettingsFromTagsStorage();
        }

        private void UpdateSettingsFromTagsStorage()
        {
            TagsStorage tagsStorage = settingsStorage.GetFirstOne();
            if (tagsStorage != null)
            {
                metaDataTypeName = tagsStorage.MetaDataType;
                sortAlphabetically = tagsStorage.Sorted;
            }
        }

        private void ShowSettingsDialog()
        {
            var settingsCopy = settingsStorage.DeepCopy();
            using (var tagsPanelSettingsForm = new TagsPanelSettingsForm(settingsCopy))
            {
                if (tagsPanelSettingsForm.ShowDialog() == DialogResult.OK)
                {
                    settingsStorage = tagsPanelSettingsForm.SettingsStorage;
                    SavePluginConfiguration();
                    UpdateTabControlVisibility();
                }
            }
        }

        private void HandleSettingsDialogResult(TagsPanelSettingsForm tagsPanelSettingsForm)
        {
            UpdateSettingsAndPanel(tagsPanelSettingsForm.SettingsStorage);
        }

        private void UpdateSettingsAndPanel(SettingsStorage newSettingsStorage)
        {
            settingsStorage = newSettingsStorage;
            SavePluginConfiguration();
            UpdateTabControlVisibility();
        }

        private void UpdateTabControlVisibility()
        {
            tabControl.Visible = tabControl.Controls.Count > 0;
        }

        public void SavePluginConfiguration()
        {
            settingsStorage.SaveAllSettings();
            ApplySortOrderFromSettings();
            RefreshPanelContent();
            LogConfigurationSaved();
        }

        private void ApplySortOrderFromSettings()
        {
            sortAlphabetically = settingsStorage.GetFirstOne()?.Sorted ?? false;
        }

        private void LogConfigurationSaved()
        {
            log.Info("Plugin configuration saved.");
        }

        private void RefreshPanelContent()
        {
            RebuildTabPages();
            InvokeRefreshTagTableData();
        }

        private void RebuildTabPages()
        {
            ClearAllTagPages();
            PopulateTabPages();
        }

        private void AddTagPanelForVisibleTags(string tagName)
        {
            if (!SettingsStorage.TagsStorages.TryGetValue(tagName, out var tagsStorage))
            {
                log.Error("tagsStorage is null");
                return;
            }

            var tabPage = GetOrCreateTagPage(tagName);
            ChecklistBoxPanel checkListBox = GetOrCreateCheckListBoxPanel(tagName);
            checkListBox.PopulateChecklistBoxesFromData(tagsStorage.GetTags());

            checkListBox.Dock = DockStyle.Fill;
            checkListBox.RegisterItemCheckEventHandler(TagCheckStateChanged);

            if (!tabPage.Controls.Contains(checkListBox))
            {
                tabPage.Controls.Add(checkListBox);
            }
            checkListBox.Visible = true;
        }

        private TabPage GetOrCreateTagPage(string tagName)
        {
            if (!_tabPageList.TryGetValue(tagName, out var tabPage))
            {
                tabPage = new TabPage(tagName);
                _tabPageList[tagName] = tabPage;
                tabControl.TabPages.Add(tabPage);
            }
            log.Info($"{nameof(GetOrCreateTagPage)} returned {nameof(tabPage)} for {nameof(tagName)}: {tagName}");
            return tabPage;
        }

        private void PopulateTabPages()
        {
            _tabPageList.Clear();
            if (tabControl != null && tabControl.TabPages != null)
            {
                tabControl.TabPages.Clear();
                foreach (var tagsStorage in SettingsStorage.TagsStorages.Values)
                {
                    AddTagPanelForVisibleTags(tagsStorage.MetaDataType);
                }
            }
        }

        /// <summary>
        /// Removes a tab from the panel.
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="tabPage"></param>
        private void RemoveTabPage(string tagName)
        {
            if (_tabPageList.TryGetValue(tagName, out var tabPage))
            {
                tabPage.Controls.Clear();
                tabControl.TabPages.Remove(tabPage);
                _tabPageList.Remove(tagName);
            }
        }

        private void AddTabPage(string tagName, TabPage tabPage)
        {
            _tabPageList[tagName] = tabPage;
            tabControl.TabPages.Add(tabPage);
        }

        private ChecklistBoxPanel CreateCheckListBoxPanelForTag(string tagName)
        {
            var tagsStorage = SettingsStorage.GetTagsStorage(tagName);
            if (tagsStorage == null)
            {
                log.Error("tagsStorage is null"); // Log the error
                return null;
            }

            ChecklistBoxPanel checkListBox = GetOrCreateCheckListBoxPanel(tagName);
            checkListBox.PopulateChecklistBoxesFromData(tagsStorage.GetTags());

            checkListBox.Dock = DockStyle.Fill;
            checkListBox.RegisterItemCheckEventHandler(TagCheckStateChanged);

            return checkListBox;
        }

        private ChecklistBoxPanel GetOrCreateCheckListBoxPanel(string tagName)
        {
            if (!checklistBoxList.TryGetValue(tagName, out var checkListBox) || checkListBox.IsDisposed)
            {
                checkListBox = new ChecklistBoxPanel(mbApiInterface);
                checklistBoxList[tagName] = checkListBox;
            }

            return checkListBox;
        }

        private void ApplyTagsToSelectedFiles(string[] fileUrls, CheckState selected, string selectedTag)
        {
            MetaDataType metaDataType = GetActiveTabMetaDataType();
            if (metaDataType != 0)
            {
                tagsManipulation.SetTagsInFile(fileUrls, selected, selectedTag, metaDataType);
            }
        }

        private void DeleteFile(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        public MetaDataType GetActiveTabMetaDataType()
        {
            return !string.IsNullOrEmpty(metaDataTypeName) ? (MetaDataType)Enum.Parse(typeof(MetaDataType), metaDataTypeName, true) : 0;
        }

        private TagsStorage GetCurrentTagsStorage()
        {
            MetaDataType metaDataType = GetActiveTabMetaDataType();
            TagsStorage tagsStorage = metaDataType != 0 ? SettingsStorage.GetTagsStorage(metaDataType.ToString()) : null;
            log.Info($"{nameof(GetCurrentTagsStorage)} returned {nameof(tagsStorage)} for {nameof(metaDataType)}: {metaDataType}");
            return tagsStorage;
        }

        private void ClearAllTagPages()
        {
            _tabPageList.Clear();
            if (tabControl != null && tabControl.TabPages != null)
            {
                tabControl.TabPages.Clear();
            }
        }

        private void AddTagsToChecklistBoxPanel(string tagName, Dictionary<String, CheckState> tags)
        {
            if (checklistBoxList.TryGetValue(tagName, out var checklistBoxPanel) && !checklistBoxPanel.IsDisposed && checklistBoxPanel.IsHandleCreated)
            {
                checklistBoxPanel.PopulateChecklistBoxesFromData(tags);
            }
        }

        private void UpdateTagsDisplayFromStorage()
        {
            TagsStorage currentTagsStorage = GetCurrentTagsStorage();
            if (currentTagsStorage == null)
            {
                log.Error($"{nameof(currentTagsStorage)} is null");
                return;
            }

            currentTagsStorage.SortByIndex();
            var allTagsFromSettings = currentTagsStorage.GetTags();

            Dictionary<string, CheckState> data = new Dictionary<string, CheckState>(allTagsFromSettings.Count);
            foreach (var tagFromSettings in allTagsFromSettings)
            {
                if (tagsFromFiles.TryGetValue(tagFromSettings.Key.Trim(), out CheckState checkState))
                {
                    data[tagFromSettings.Key] = checkState;
                }
                else
                {
                    data[tagFromSettings.Key] = CheckState.Unchecked;
                }
            }

            foreach (var tagFromFile in tagsFromFiles)
            {
                if (!data.ContainsKey(tagFromFile.Key))
                {
                    data[tagFromFile.Key] = tagFromFile.Value;
                }
            }

            string tagName = currentTagsStorage.GetTagName();
            AddTagsToChecklistBoxPanel(tagName, data);
        }

        private void InvokeRefreshTagTableData()
        {
            if (_panel == null || _panel.IsDisposed)
            {
                log.Error($"{nameof(_panel)} is null or disposed");
                return;
            }

            // Überprüfen, ob der Aufruf auf dem UI-Thread erfolgt
            if (_panel.InvokeRequired)
            {
                // Wenn nicht, verwenden Sie Invoke, um den Aufruf auf dem UI-Thread auszuführen
                _panel.Invoke((Action)UpdateTagsDisplayFromStorage);
            }
            else
            {
                // Wenn bereits auf dem UI-Thread, führen Sie die Methode direkt aus
                UpdateTagsDisplayFromStorage();
            }
        }

        #endregion Initialise plugin

        #region Event handlers

        public void SettingsMenuClicked(object sender, EventArgs args)
        {
            ShowSettingsDialog();
        }

        private void TagCheckStateChanged(object sender, ItemCheckEventArgs e)
        {
            if (ignoreForBatchSelect || ignoreEventFromHandler)
            {
                return;
            }

            CheckState newState = e.NewValue;
            CheckState currentState = ((CheckedListBox)sender).GetItemCheckState(e.Index);

            // Nur fortfahren, wenn sich der Zustand tatsächlich ändert
            if (newState != currentState)
            {
                string name = ((CheckedListBox)sender).Items[e.Index].ToString();

                ignoreEventFromHandler = true;
                ApplyTagsToSelectedFiles(selectedFileUrls, newState, name);
                mbApiInterface.MB_RefreshPanels();
                ignoreEventFromHandler = false;
            }
        }

        private void TabControlSelectionChanged(Object sender, TabControlEventArgs e)
        {
            if (e.TabPage != null && !e.TabPage.IsDisposed)
            {
                string newMetaDataTypeName = e.TabPage.Text;
                if (metaDataTypeName != newMetaDataTypeName)
                {
                    metaDataTypeName = newMetaDataTypeName;
                    SwitchVisibleTagPanel(metaDataTypeName);
                    RefreshPanelTagsFromFiles(selectedFileUrls); // Aktualisiert die Tags nur, wenn notwendig
                }
            }
        }

        private void SelectedTabPageChanged(Object sender, EventArgs e)
        {
            TabPage selectedTab = tabControl.SelectedTab;
            ChecklistBoxPanel checkListBoxPanel = selectedTab?.Controls.OfType<ChecklistBoxPanel>().FirstOrDefault();

            if (checkListBoxPanel != null)
            {
                UpdateTagsInPanelOnFileSelection();
                RefreshPanelTagsFromFiles(selectedFileUrls);
                checkListBoxPanel.Refresh();
                checkListBoxPanel.Invalidate();
            }
        }

        private void SetPanelEnabled(bool enabled = true)
        {
            if (_panel.InvokeRequired)
            {
                _panel.Invoke(new Action(() => _panel.Enabled = enabled));
            }
            else
            {
                _panel.Enabled = enabled;
            }
        }

        private void UpdateTagsInPanelOnFileSelection()
        {
            ignoreEventFromHandler = true;
            ignoreForBatchSelect = true;
            if (_panel.InvokeRequired)
            {
                _panel.Invoke((Action)InvokeRefreshTagTableData);
            }
            else
            {
                InvokeRefreshTagTableData();
            }
            ignoreEventFromHandler = false;
            ignoreForBatchSelect = false;
        }

        /// <summary>
        /// Sets tags from files contained within a panel based on filenames array
        /// </summary>
        /// <param name="filenames"></param>
        private void RefreshPanelTagsFromFiles(string[] filenames)
        {
            tagsFromFiles.Clear();

            if (filenames == null || filenames.Length == 0)
            {
                return;
            }

            var currentTagsStorage = GetCurrentTagsStorage();
            if (currentTagsStorage != null)
            {
                tagsFromFiles = tagsManipulation.CombineTagLists(filenames, currentTagsStorage);
            }

            if (_panel != null)
            {
                UpdateTagsInPanelOnFileSelection();
            }
            SetPanelEnabled(true);
        }

        private void SwitchVisibleTagPanel(string visibleTag)
        {
            // Hide checklistBox on all panels
            foreach (var checklistBoxPanel in checklistBoxList.Values)
            {
                checklistBoxPanel.Visible = false;
            }

            // Show checklistBox on visible panel
            if (!string.IsNullOrEmpty(visibleTag))
            {
                if (checklistBoxList.TryGetValue(visibleTag, out var visibleChecklistBoxPanel))
                {
                    visibleChecklistBoxPanel.Visible = true;
                }
                RefreshPanelTagsFromFiles(selectedFileUrls);
            }
        }

        private void CreateTabPanel()
        {
            tabControl = (TabControl)mbApiInterface.MB_AddPanel(_panel, (PluginPanelDock)6);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Selected += TabControlSelectionChanged;

            if (tabControl.TabPages.Count == 0)
            {
                PopulateTabPages();
            }
        }

        private void DisplaySettingsPromptLabel()
        {
            if (_panel.InvokeRequired)
            {
                _panel.Invoke(new Action(DisplaySettingsPromptLabel));
                return;
            }

            Label emptyPanelText = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(14, 30),
                Size = new System.Drawing.Size(38, 13),
                TabIndex = 2,
                Text = "Please add a tag in the settings dialog first."
            };

            _panel.SuspendLayout();
            _panel.Controls.Add(emptyPanelText);
            _panel.Controls.SetChildIndex(emptyPanelText, 1);
            _panel.Controls.SetChildIndex(tabControl, 0);

            if (tabControl.TabPages.Count == 0)
            {
                tabControl.Visible = false;
            }

            _panel.ResumeLayout();
        }

        private void AddControls()
        {
            _panel.SuspendLayout();
            CreateTabPanel();
            _panel.Controls.Add(tabControl);
            _panel.Enabled = false;
            _panel.ResumeLayout();
        }

        /// <summary>
        /// MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        /// </summary>
        /// <param name="reason">The reason why MusicBee has closed the plugin.</param>
        public void Close(PluginCloseReason reason)
        {
            log?.Info(reason.ToString("G"));
            log?.Dispose();
            _panel?.Dispose();
            _panel = null;
            log = null;
        }

        /// <summary>
        /// uninstall this plugin - clean up any persisted files
        /// </summary>
        public void Uninstall()
        {
            // Delete settings file
            if (System.IO.File.Exists(settingsStorage.GetSettingsPath()))
            {
                System.IO.File.Delete(settingsStorage.GetSettingsPath());
            }

            // Delete _log file
            if (System.IO.File.Exists(log.GetLogFilePath()))
            {
                System.IO.File.Delete(log.GetLogFilePath());
            }
        }

        /// <summary>
        /// Receive event notifications from MusicBee.
        /// You need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event.
        /// </summary>
        /// <param name="sourceFileUrl"></param>
        /// <param name="type"></param>
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            if (_panel == null || type == NotificationType.ApplicationWindowChanged || GetActiveTabMetaDataType() == 0 || ignoreEventFromHandler) return;

            bool isTagsChanging = type == NotificationType.TagsChanging;
            bool isTrackChanged = type == NotificationType.TrackChanged;

            if (isTagsChanging)
            {
                ignoreForBatchSelect = true;
                mbApiInterface.Library_CommitTagsToFile(sourceFileUrl);
            }

            if (isTagsChanging || isTrackChanged)
            {
                tagsFromFiles = tagsManipulation.UpdateTagsFromFile(sourceFileUrl, GetActiveTabMetaDataType());
                InvokeRefreshTagTableData();
            }

            ignoreForBatchSelect = false;
        }

        /// <summary>
        /// Event handler that is triggered by MusicBee when a dockable panel has been created.
        /// </summary>
        /// <param name="panel">A reference to the new panel.</param>
        /// <returns>
        /// &lt; 0 indicates to MusicBee this control is resizable and should be sized to fill the panel it is docked to in MusicBee<br/>
        ///  = 0 indicates to MusicBee this control resizeable<br/>
        /// &gt; 0 indicates to MusicBee the fixed height for the control. Note it is recommended you scale the height for high DPI screens(create a graphics object and get the DpiY value)
        /// </returns>
        public int OnDockablePanelCreated(Control panel)
        {
            _panel = panel;

            if (!_panel.Created)
            {
                _panel.CreateControl();
            }

            AddControls();
            DisplaySettingsPromptLabel();
            if (_panel.IsHandleCreated)
            {
                InvokeRefreshTagTableData();
            }

            return 0;
        }

        /// <summary>
        /// Event handler triggered by MusicBee when the user selects files in the library view.
        /// </summary>
        /// <param name="filenames">List of selected files.</param>
        public void OnSelectedFilesChanged(string[] filenames)
        {
            selectedFileUrls = filenames ?? Array.Empty<string>();

            if (selectedFileUrls.Any())
            {
                RefreshPanelTagsFromFiles(selectedFileUrls);
            }
            else
            {
                tagsFromFiles.Clear();
                UpdateTagsInPanelOnFileSelection();
            }
        }

        /// <summary>
        /// The presence of this function indicates to MusicBee that the dockable panel created above will show menu items when the panel header is clicked.
        /// </summary>
        /// <returns>Returns the list of ToolStripMenuItems that will be displayed.</returns>
        public List<ToolStripItem> GetMenuItems()
        {
            var menuItems = new List<ToolStripItem>
            {
                new ToolStripMenuItem("Tag-Panel Settings", null, SettingsMenuClicked),
            };
            return menuItems;
        }

        #endregion Event handlers
    }
}