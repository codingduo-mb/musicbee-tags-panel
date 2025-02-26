using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface _mbApiInterface;
        private Logger _logger;
        private Control _tagsPanelControl;
        private TabControl _tabControl;
        private List<MetaDataType> _availableMetaTags = new List<MetaDataType>();
        private readonly Dictionary<string, TagListPanel> _checklistBoxList = new Dictionary<string, TagListPanel>();
        private readonly Dictionary<string, TabPage> _tabPageList = new Dictionary<string, TabPage>();
        private Dictionary<string, CheckState> _tagsFromFiles = new Dictionary<string, CheckState>();
        private SettingsManager _settingsManager;
        private TagManager _tagManager;
        private UIManager _uiManager;
        private bool _showTagsNotInList;
        private string _metaDataTypeName;
        private bool _sortAlphabetically;
        private readonly PluginInfo _pluginInformation = new PluginInfo();
        private string[] _selectedFilesUrls = Array.Empty<string>();
        private bool _ignoreEventFromHandler = true;
        private bool _excludeFromBatchSelection = true;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            InitializeApi(apiInterfacePtr);
            var pluginInformation = CreatePluginInfo();
            InitializePluginComponents();
            return pluginInformation;
        }

        private void InitializeApi(IntPtr apiInterfacePtr)
        {
            _mbApiInterface = new MusicBeeApiInterface();
            _mbApiInterface.Initialise(apiInterfacePtr);
        }

        private PluginInfo CreatePluginInfo()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return new PluginInfo
            {
                PluginInfoVersion = PluginInfoVersion,
                Name = Messages.PluginNamePluginInfo,
                Description = Messages.PluginDescriptionPluginInfo,
                Author = Messages.PluginAuthorPluginInfo,
                TargetApplication = Messages.PluginTargetApplicationPluginInfo,
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
            InitializeUIManager();
            ClearCollections();
            InitializeLogger();
            InitializeSettingsManager();
            InitializeTagManager();
            LoadPluginSettings();
            InitializeMenu();
            EnsureTabControlInitialized();
            _logger.Info($"{nameof(InitializePluginComponents)} started");
        }

        private void InitializeUIManager()
        {
            _uiManager = new UIManager(_mbApiInterface, _checklistBoxList, _selectedFilesUrls, RefreshPanelTagsFromFiles);
        }

        private void ClearCollections()
        {
            _tagsFromFiles.Clear();
            _tabPageList.Clear();
            _showTagsNotInList = false;
        }

        private void InitializeLogger()
        {
            _logger = new Logger(_mbApiInterface);
        }

        private void InitializeSettingsManager()
        {
            _settingsManager = new SettingsManager(_mbApiInterface, _logger);
        }

        private void InitializeTagManager()
        {
            _tagManager = new TagManager(_mbApiInterface, _settingsManager);
        }

        private void EnsureTabControlInitialized()
        {
            if (_tabControl != null)
            {
                _tabControl.SelectedIndexChanged -= TabControlSelectionChanged;
                _tabControl.SelectedIndexChanged += TabControlSelectionChanged;
            }
        }
        public bool Configure(IntPtr panelHandle)
        {
            // save any persistent settings in a sub-folder of this path
            // panelHandle will only be set if you set _pluginInformation.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if _pluginInformation.ConfigurationPanelHeight is set to 0, you can display your own popup window
            ShowSettingsDialog();
            return true;
        }
        private void InitializeMenu()
        {
            _mbApiInterface.MB_AddMenuItem("mnuTools/Tags-Panel Settings", "Tags-Panel: Open Settings", OnSettingsMenuClicked);
        }

        private void LoadPluginSettings()
        {
            if (_settingsManager == null)
            {
                _logger?.Error("SettingsManager is not initialized.");
                return;
            }

            try
            {
                _settingsManager.LoadSettingsWithFallback();
                UpdateSettingsFromTagsStorage();
                _logger?.Info("Plugin settings loaded successfully.");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger?.Error($"File access error while loading plugin settings: {ex.GetType().Name} - {ex.Message}");
                ShowErrorMessage($"Unable to access settings file: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Unexpected error in {nameof(LoadPluginSettings)}: {ex}");
                ShowErrorMessage("An unexpected error occurred while loading settings.");
            }
        }

        private void UpdateSettingsFromTagsStorage()
        {
            var tagsStorage = _settingsManager.RetrieveFirstTagsStorage();
            if (tagsStorage != null)
            {
                _metaDataTypeName = tagsStorage.MetaDataType;
                _sortAlphabetically = tagsStorage.Sorted;
            }
            else
            {
                _logger.Warn("No TagsStorage found in SettingsManager.");
                _metaDataTypeName = string.Empty;
                _sortAlphabetically = false;
            }
        }

        private void ShowSettingsDialog()
        {
            var settingsCopy = _settingsManager.DeepCopy();
            using (var settingsForm = new TagListSettingsForm(settingsCopy))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // When the settings dialog is closed via "Save Settings", update and save the settings.
                    _settingsManager = settingsForm.SettingsStorage;
                    SavePluginConfiguration();
                    UpdateTabControlVisibility();
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateTabControlVisibility()
        {
            if (_tabControl != null)
            {
                _tabControl.Visible = _tabControl.Controls.Count > 0;
            }
        }

        private void SavePluginConfiguration()
        {
            _settingsManager.SaveAllSettings();
            ApplySortOrderFromSettings();

            foreach (var tagPanel in _checklistBoxList.Values)
            {
                var tagName = tagPanel.Name;
                var newTagsStorage = _settingsManager.RetrieveTagsStorageByTagName(tagName);
                if (newTagsStorage != null)
                {
                    var data = GetTagsFromStorage(newTagsStorage);
                    tagPanel.UpdateTagsStorage(newTagsStorage, data);
                }
            }

            RefreshPanelContent();
            _logger.Info("Plugin configuration saved.");
        }

        private void ApplySortOrderFromSettings()
        {
            _sortAlphabetically = _settingsManager.RetrieveFirstTagsStorage()?.Sorted ?? false;
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
            if (!_settingsManager.TagsStorages.TryGetValue(tagName, out var tagsStorage))
            {
                _logger.Error("TagsStorage is null for tag: " + tagName);
                return;
            }

            var tabPage = new TabPage(tagName);
            var tagListPanel = new TagListPanel(_mbApiInterface, _settingsManager, tagName, _checklistBoxList, _selectedFilesUrls, RefreshPanelTagsFromFiles)
            {
                Dock = DockStyle.Fill
            };

            tagListPanel.RegisterItemCheckEventHandler(TagCheckStateChanged);
            tabPage.Controls.Add(tagListPanel);

            _checklistBoxList[tagName] = tagListPanel;
            _tabPageList[tagName] = tabPage;
            _tabControl.TabPages.Add(tabPage);
        }

        private TabPage GetOrCreateTagPage(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                _logger.Error($"{nameof(GetOrCreateTagPage)}: tagName parameter is null or empty");
                throw new ArgumentNullException(nameof(tagName));
            }

            try
            {
                if (_tabPageList == null || _tabControl == null)
                {
                    _logger.Error($"{nameof(GetOrCreateTagPage)}: Required controls not initialized");
                    throw new InvalidOperationException("TabPage collection or TabControl is not initialized");
                }

                TabPage tabPage;
                if (!_tabPageList.TryGetValue(tagName, out tabPage) || tabPage == null || tabPage.IsDisposed)
                {
                    _logger.Info($"{nameof(GetOrCreateTagPage)}: Creating new tab page for tag: {tagName}");
                    tabPage = new TabPage(tagName);
                    _tabPageList[tagName] = tabPage;
                    _tabControl.TabPages.Add(tabPage);
                }
                else
                {
                    _logger.Info($"{nameof(GetOrCreateTagPage)}: Using existing tab page for tag: {tagName}");
                }

                return tabPage;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in {nameof(GetOrCreateTagPage)} for tag '{tagName}': {ex.Message}");
                throw;
            }
        }

        private void PopulateTabPages()
        {
            try
            {
                _logger.Info($"Starting {nameof(PopulateTabPages)}...");

                // Clear existing collections
                _tabPageList.Clear();
                _checklistBoxList.Clear();

                if (_tabControl == null || _tabControl.IsDisposed)
                {
                    _logger.Error($"{nameof(PopulateTabPages)}: TabControl is null or disposed");
                    return;
                }

                _tabControl.TabPages.Clear();

                // Exit early if no tags storage
                if (_settingsManager?.TagsStorages == null || !_settingsManager.TagsStorages.Any())
                {
                    _logger.Warn($"{nameof(PopulateTabPages)}: No tags storage available");
                    return;
                }

                // Add visible tag panels based on settings
                int addedCount = 0;
                foreach (var tagsStorage in _settingsManager.TagsStorages.Values)
                {
                    if (tagsStorage != null && !string.IsNullOrEmpty(tagsStorage.MetaDataType))
                    {
                        AddTagPanelForVisibleTags(tagsStorage.MetaDataType);
                        addedCount++;
                    }
                }

                _logger.Info($"{nameof(PopulateTabPages)}: Added {addedCount} tag panels");

                // Show first tab if any exist
                if (_tabControl.TabCount > 0)
                {
                    _tabControl.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in {nameof(PopulateTabPages)}: {ex}");
            }
        }

        private void ApplyTagsToSelectedFiles(string[] fileUrls, CheckState selected, string selectedTag)
        {
            var metaDataType = GetActiveTabMetaDataType();
            if (metaDataType != 0)
            {
                _tagManager.SetTagsInFile(fileUrls, selected, selectedTag, metaDataType);
            }
        }

        public MetaDataType GetActiveTabMetaDataType()
        {
            return Enum.TryParse(_metaDataTypeName, true, out MetaDataType result) ? result : 0;
        }

        private TagsStorage GetCurrentTagsStorage()
        {
            MetaDataType metaDataType = GetActiveTabMetaDataType();
            var tagsStorage = metaDataType != 0 ? _settingsManager.RetrieveTagsStorageByTagName(metaDataType.ToString()) : null;
            _logger.Info($"{nameof(GetCurrentTagsStorage)} returned TagsStorage for metaDataType: {metaDataType}");
            return tagsStorage;
        }

        private void ClearAllTagPages()
        {
            foreach (var tabPage in _tabPageList.Values)
            {
                foreach (var control in tabPage.Controls.OfType<TagListPanel>())
                {
                    control.Dispose();
                }
                tabPage.Controls.Clear();
            }
            _tabPageList.Clear();
            _tabControl?.TabPages.Clear();
        }

        private Dictionary<string, CheckState> GetTagsFromStorage(TagsStorage currentTagsStorage)
        {
            var allTags = currentTagsStorage.GetTags();
            var data = new Dictionary<string, CheckState>(allTags.Count);

            foreach (var tag in allTags)
            {
                string trimmedKey = tag.Key.Trim();
                var checkState = _tagsFromFiles.TryGetValue(trimmedKey, out var state) ? state : CheckState.Unchecked;
                data[trimmedKey] = checkState;
            }
            return data;
        }

        private void UpdateTagsDisplayFromStorage()
        {
            var currentTagsStorage = GetCurrentTagsStorage();
            if (currentTagsStorage == null)
            {
                _logger.Error("Current TagsStorage is null");
                return;
            }

            var data = GetTagsFromStorage(currentTagsStorage);
            foreach (var tagFromFile in _tagsFromFiles)
            {
                if (!data.ContainsKey(tagFromFile.Key))
                {
                    data[tagFromFile.Key] = tagFromFile.Value;
                }
            }

            var tagName = currentTagsStorage.GetTagName();
            if (_checklistBoxList.TryGetValue(tagName, out var tagListPanel))
            {
                if (tagListPanel.InvokeRequired)
                {
                    tagListPanel.Invoke(new Action(() => tagListPanel.UpdateTagsStorage(currentTagsStorage, data)));
                }
                else
                {
                    tagListPanel.UpdateTagsStorage(currentTagsStorage, data);
                }
            }
            else
            {
                _logger.Error($"TagListPanel for tag '{tagName}' not found.");
            }
        }

        private void InvokeRefreshTagTableData()
        {
            if (_tagsPanelControl == null || _tagsPanelControl.IsDisposed)
                return;

            if (_tagsPanelControl.InvokeRequired)
            {
                _tagsPanelControl.Invoke(new Action(UpdateTagsDisplayFromStorage));
            }
            else
            {
                UpdateTagsDisplayFromStorage();
            }
        }

        public void OnSettingsMenuClicked(object sender, EventArgs args)
        {
            ShowSettingsDialog();
        }

        private void TagCheckStateChanged(object sender, ItemCheckEventArgs e)
        {
            if (_excludeFromBatchSelection || _ignoreEventFromHandler)
                return;

            _ignoreEventFromHandler = true;
            try
            {
                var newState = e.NewValue;
                var tagName = ((CheckedListBox)sender).Items[e.Index].ToString();
                ApplyTagsToSelectedFiles(_selectedFilesUrls, newState, tagName);
                _mbApiInterface.MB_RefreshPanels();
            }
            finally
            {
                _ignoreEventFromHandler = false;
            }
        }

        private void TabControlSelectionChanged(object sender, EventArgs e)
        {
            if (!(sender is TabControl tabControl) || tabControl.SelectedTab == null || tabControl.SelectedTab.IsDisposed)
                return;

            var newMetaDataTypeName = tabControl.SelectedTab.Text;
            if (_metaDataTypeName != newMetaDataTypeName)
            {
                _metaDataTypeName = newMetaDataTypeName;
                _uiManager.SwitchVisibleTagPanel(_metaDataTypeName);
                RefreshPanelTagsFromFiles(_selectedFilesUrls);
                UpdateTagsInPanelOnFileSelection();
            }

            var tagListPanel = tabControl.SelectedTab.Controls.OfType<TagListPanel>().FirstOrDefault();
            tagListPanel?.Refresh();
        }

        private void SetPanelEnabled(bool enabled = true)
        {
            if (_tagsPanelControl == null || _tagsPanelControl.IsDisposed)
                return;

            if (_tagsPanelControl.InvokeRequired)
                _tagsPanelControl.Invoke(new Action(() => _tagsPanelControl.Enabled = enabled));
            else
                _tagsPanelControl.Enabled = enabled;
        }

        private void UpdateTagsInPanelOnFileSelection()
        {
            if (_tagsPanelControl == null)
                return;

            if (_tagsPanelControl.InvokeRequired)
                _tagsPanelControl.Invoke(new Action(UpdateTagsInPanelOnFileSelection));
            else
            {
                _ignoreEventFromHandler = true;
                _excludeFromBatchSelection = true;
                InvokeRefreshTagTableData();
                _ignoreEventFromHandler = false;
                _excludeFromBatchSelection = false;
            }
        }

        private void RefreshPanelTagsFromFiles(string[] filenames)
        {
            if (filenames == null || filenames.Length == 0)
            {
                _tagsFromFiles.Clear();
                UpdateTagsInPanelOnFileSelection();
                SetPanelEnabled(true);
                return;
            }

            var currentTagsStorage = GetCurrentTagsStorage();
            if (currentTagsStorage == null)
            {
                _logger.Error("Current TagsStorage is null");
                return;
            }

            _tagsFromFiles = _tagManager.CombineTagLists(filenames, currentTagsStorage);
            UpdateTagsInPanelOnFileSelection();
            SetPanelEnabled(true);
        }

        private bool ShouldClearTags(string[] filenames)
        {
            return filenames == null || filenames.Length == 0;
        }

        private void ClearTagsAndUpdateUI()
        {
            _tagsFromFiles.Clear();
            UpdateTagsInPanelOnFileSelection();
            SetPanelEnabled(true);
        }

        private void UpdateTagsFromFiles(string[] filenames)
        {
            var currentTagsStorage = GetCurrentTagsStorage();
            if (currentTagsStorage != null)
            {
                _tagsFromFiles = _tagManager.CombineTagLists(filenames, currentTagsStorage);
                UpdateTagsInPanelOnFileSelection();
                SetPanelEnabled(true);
            }
        }

        private void CreateTabPanel()
        {
            _tabControl = (TabControl)_mbApiInterface.MB_AddPanel(_tagsPanelControl, (PluginPanelDock)6);
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.SelectedIndexChanged += TabControlSelectionChanged;

            if (_tabControl.TabPages.Count == 0)
            {
                PopulateTabPages();
            }
        }

        private void AddControls()
        {
            if (_tagsPanelControl.InvokeRequired)
            {
                _tagsPanelControl.Invoke(new Action(AddControls));
                return;
            }

            _tagsPanelControl.SuspendLayout();
            CreateTabPanel();
            _tagsPanelControl.Controls.Add(_tabControl);
            _tagsPanelControl.Enabled = false;
            _tagsPanelControl.ResumeLayout();
        }

        public void Close(PluginCloseReason reason)
        {
            _logger?.Info(reason.ToString("G"));
            _logger?.Dispose();
            _tagsPanelControl?.Dispose();
            _tagsPanelControl = null;
            _logger = null;
        }

        public void Uninstall()
        {
            try
            {
                string settingsPath = _settingsManager.GetSettingsPath();
                if (File.Exists(settingsPath))
                {
                    File.Delete(settingsPath);
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to delete settings file: {ex.Message}");
            }

            try
            {
                string logFilePath = _logger?.GetLogFilePath();
                if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to delete log file: {ex.Message}");
            }
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            if (_tagsPanelControl == null || type == NotificationType.ApplicationWindowChanged ||
                GetActiveTabMetaDataType() == 0 || _ignoreEventFromHandler)
                return;

            bool isTagsChanging = type == NotificationType.TagsChanging;
            bool isTrackChanged = type == NotificationType.TrackChanged;

            if (isTagsChanging)
            {
                _excludeFromBatchSelection = true;
                _mbApiInterface.Library_CommitTagsToFile(sourceFileUrl);
            }

            if (isTrackChanged)
            {
                _tagsFromFiles = _tagManager.UpdateTagsFromFile(sourceFileUrl, GetActiveTabMetaDataType());
                InvokeRefreshTagTableData();
            }

            _excludeFromBatchSelection = false;
        }

        public int OnDockablePanelCreated(Control panel)
        {
            _tagsPanelControl = panel;
            EnsureControlCreated();
            AddControls();
            _uiManager.DisplaySettingsPromptLabel(_tagsPanelControl, _tabControl, "No tags available. Please add tags in the settings.");
            if (_tagsPanelControl.IsHandleCreated)
                InvokeRefreshTagTableData();

            return 0;
        }

        private void EnsureControlCreated()
        {
            if (!_tagsPanelControl.Created)
            {
                _tagsPanelControl.CreateControl();
            }
        }

        public void OnSelectedFilesChanged(string[] filenames)
        {
            _selectedFilesUrls = filenames ?? Array.Empty<string>();

            if (_selectedFilesUrls.Any())
            {
                RefreshPanelTagsFromFiles(_selectedFilesUrls);
            }
            else
            {
                _tagsFromFiles.Clear();
                UpdateTagsInPanelOnFileSelection();
            }
            SetPanelEnabled(true);
        }

        public List<ToolStripItem> GetMenuItems()
        {
            return new List<ToolStripItem>
                {
                    new ToolStripMenuItem("Tag-Panel Settings", null, OnSettingsMenuClicked)
                };
        }
    }
}