using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using Newtonsoft.Json;

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

        /// <summary>
        /// Initializes the plugin and prepares it for use with MusicBee.
        /// </summary>
        /// <param name="apiInterfacePtr">Pointer to the MusicBee API interface</param>
        /// <returns>A fully initialized PluginInfo object describing this plugin</returns>
        /// <exception cref="ArgumentException">Thrown when the API interface pointer is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when plugin initialization fails</exception>
        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            try
            {
                _logger?.Debug("Plugin initialization started");

                // Initialize the MusicBee API with proper validation
                if (apiInterfacePtr == IntPtr.Zero)
                {
                    _logger?.Error("Invalid API interface pointer provided");
                    throw new ArgumentException("Invalid MusicBee API interface pointer", nameof(apiInterfacePtr));
                }

                InitializeApi(apiInterfacePtr);

                // Create plugin information for MusicBee
                var pluginInformation = CreatePluginInfo();

                // Initialize all plugin components
                InitializePluginComponents();

                _logger?.Info("Plugin initialization completed successfully");
                return pluginInformation;
            }
            catch (Exception ex) when (!(ex is ArgumentException) && !(ex is InvalidOperationException))
            {
                _logger?.Error($"Unexpected error during plugin initialization: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to initialize Tags Panel plugin", ex);
            }
        }

        /// <summary>
        /// Initializes the MusicBee API interface with the given pointer.
        /// </summary>
        /// <param name="apiInterfacePtr">Pointer to the MusicBee API interface</param>
        /// <exception cref="ArgumentException">Thrown when the API interface pointer is invalid</exception>
        private void InitializeApi(IntPtr apiInterfacePtr)
        {
            if (apiInterfacePtr == IntPtr.Zero)
            {
                _logger?.Error("API interface pointer is null or zero");
                throw new ArgumentException("Invalid MusicBee API interface pointer", nameof(apiInterfacePtr));
            }

            try
            {
                _mbApiInterface = new MusicBeeApiInterface();
                _mbApiInterface.Initialise(apiInterfacePtr);

                // Verify successful initialization by checking version
                if (_mbApiInterface.InterfaceVersion <= 0)
                {
                    _logger?.Error("API interface initialization failed: Invalid interface version");
                    throw new InvalidOperationException("Failed to initialize MusicBee API interface");
                }

                _logger?.Debug($"MusicBee API initialized: v{_mbApiInterface.InterfaceVersion}.{_mbApiInterface.ApiRevision}");
            }
            catch (Exception ex) when (!(ex is ArgumentException) && !(ex is InvalidOperationException))
            {
                _logger?.Error($"Error initializing MusicBee API: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to initialize MusicBee API interface", ex);
            }
        }

        /// <summary>
        /// Creates and initializes the plugin information structure with version details and required settings.
        /// </summary>
        /// <returns>A fully initialized PluginInfo object that describes the plugin to MusicBee</returns>
        private PluginInfo CreatePluginInfo()
        {
            try
            {
                // Get the current assembly version to use for plugin versioning
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
                              ?? new Version(1, 0, 0, 0);

                // Create and return the plugin information structure with all required details
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
                    ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents |
                                            ReceiveNotificationFlags.TagEvents |
                                            ReceiveNotificationFlags.DataStreamEvents,
                    ConfigurationPanelHeight = 0 // Use popup settings dialog instead of embedded panel
                };
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error creating plugin information: {ex.Message}", ex);

                // Provide fallback plugin info in case of an error
                return new PluginInfo
                {
                    PluginInfoVersion = PluginInfoVersion,
                    Name = "Tags Panel",
                    Description = "A panel to manage music file tags",
                    Type = PluginType.General,
                    VersionMajor = 1,
                    VersionMinor = 0,
                    Revision = 0,
                    MinInterfaceVersion = MinInterfaceVersion,
                    MinApiRevision = MinApiRevision
                };
            }
        }

        private void InitializePluginComponents()
        {
            _logger?.Info($"{nameof(InitializePluginComponents)} started");
            InitializeUIManager();
            ClearCollections();
            InitializeLogger();
            InitializeSettingsManager();
            InitializeTagManager();
            LoadPluginSettings();
            InitializeMenu();
            EnsureTabControlInitialized();
        }

        private void InitializeUIManager()
        {
            _uiManager = new UIManager(_mbApiInterface, _checklistBoxList, _selectedFilesUrls, RefreshPanelTagsFromFiles);
        }

        /// <summary>
        /// Clears all collection objects used by the plugin to ensure a clean state.
        /// </summary>
        /// <remarks>
        /// This method resets all collections and state variables to prepare for reinitialization
        /// or when the plugin needs to return to a default state.
        /// </remarks>
        private void ClearCollections()
        {
            // Clear tag-related collections
            _tagsFromFiles?.Clear();
            foreach (var pair in _checklistBoxList)
            {
                pair.Value?.UnregisterItemCheckEventHandler(TagCheckStateChanged);
            }
            _checklistBoxList?.Clear();
            _tabPageList?.Clear();
            _availableMetaTags?.Clear();

            // Reset state variables
            _showTagsNotInList = false;
            _metaDataTypeName = string.Empty;
            _selectedFilesUrls = Array.Empty<string>();
            _ignoreEventFromHandler = true;

            _logger?.Debug("All collections and state variables cleared");
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
                // Remove event handler only if _tabControl exists
                _tabControl.SelectedIndexChanged -= TabControlSelectionChanged;

                // Add the event handler
                _tabControl.SelectedIndexChanged += TabControlSelectionChanged;

                _logger?.Debug("TabControl event handlers initialized");
            }
            else
            {
                _logger?.Warn("EnsureTabControlInitialized called with null TabControl");
            }
        }
        public bool Configure(IntPtr panelHandle)
        {
            try
            {
                _logger?.Debug("Configure method called");

                // panelHandle will only be set if _pluginInformation.ConfigurationPanelHeight is non-zero
                // Here we're using a popup window approach (ConfigurationPanelHeight = 0)

                using (new CursorScope(Cursors.WaitCursor))
                {
                    ShowSettingsDialog();
                    _logger?.Info("Settings dialog closed successfully");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in Configure method: {ex.Message}", ex);
                ShowErrorMessage("An error occurred while opening settings. Please check the log for details.");
                return false;
            }
        }

        /// <summary>
        /// Initializes menu items for the plugin in MusicBee's menu structure.
        /// </summary>
        /// <remarks>
        /// Adds menu entries to allow users to access plugin functionality.
        /// Uses consistent naming conventions and provides error handling.
        /// </remarks>
        private void InitializeMenu()
        {
            try
            {
                _logger?.Debug("Adding plugin menu items");

                // Add main settings menu item in Tools menu
                ToolStripItem added = _mbApiInterface.MB_AddMenuItem(
                     "mnuTools/Tags-Panel Settings",
                     "Tags-Panel: Open Settings",
                     OnSettingsMenuClicked);

                if (added == null && _logger != null)
                {
                    _logger.Warn("Failed to add plugin menu item");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error initializing plugin menu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads plugin settings from storage, with appropriate error handling for different failure scenarios.
        /// </summary>
        /// <remarks>
        /// This method attempts to load settings with fallback options if the primary load fails.
        /// It provides detailed logging and user-friendly error messages for different types of failures.
        /// </remarks>
        private void LoadPluginSettings()
        {
            if (_settingsManager == null)
            {
                _logger?.Error("Cannot load plugin settings: SettingsManager is not initialized.");
                ShowErrorMessage("Unable to initialize plugin settings manager. Please restart MusicBee.");
                return;
            }

            try
            {
                _logger?.Debug("Loading plugin settings...");
                _settingsManager.LoadSettingsWithFallback();
                UpdateSettingsFromTagsStorage();
                _logger?.Info("Plugin settings loaded successfully.");
            }
            catch (JsonException ex)
            {
                _logger?.Error($"Settings file is corrupted or has invalid format: {ex.Message}", ex);
                ShowErrorMessage("Your settings file appears to be corrupted. Default settings will be used.",
                                "Settings Error");

                // Try to initialize with defaults after corruption
                try
                {
                    _settingsManager.LoadSettingsWithFallback();
                    _logger?.Info("Initialized with default settings after corruption.");
                }
                catch (Exception fallbackEx)
                {
                    _logger?.Error($"Failed to initialize default settings: {fallbackEx.Message}", fallbackEx);
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger?.Info($"Settings file not found, will create defaults: {ex.Message}");
                // No error message needed for users - this is expected on first run
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger?.Error($"File access error while loading plugin settings: {ex.GetType().Name} - {ex.Message}", ex);
                ShowErrorMessage($"Unable to access settings file: {ex.Message}\n\nPlease check file permissions.",
                                "Settings Access Error");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Unexpected error in {nameof(LoadPluginSettings)}: {ex}", ex);
                ShowErrorMessage("An unexpected error occurred while loading settings.\n\nPlease check the log file for details.",
                                "Settings Error");
            }
        }

        /// <summary>
        /// Updates internal settings based on the first available TagsStorage.
        /// Retrieves metadata type name and sort order from storage or uses defaults if not available.
        /// </summary>
        private void UpdateSettingsFromTagsStorage()
        {
            try
            {
                var tagsStorage = _settingsManager?.RetrieveFirstTagsStorage();
                if (tagsStorage != null)
                {
                    _metaDataTypeName = string.IsNullOrEmpty(tagsStorage.MetaDataType)
                        ? string.Empty
                        : tagsStorage.MetaDataType;

                    _sortAlphabetically = tagsStorage.Sorted;

                    _logger?.Debug($"Settings updated from storage: MetaDataType={_metaDataTypeName}, Sort={_sortAlphabetically}");
                }
                else
                {
                    _logger?.Warn("No TagsStorage found in SettingsManager. Using default values.");
                    _metaDataTypeName = string.Empty;
                    _sortAlphabetically = false;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to update settings from TagsStorage: {ex.Message}", ex);
                // Use defaults in case of error
                _metaDataTypeName = string.Empty;
                _sortAlphabetically = false;
            }
        }

        /// <summary>
        /// Shows the settings dialog and processes the results if the user saves changes.
        /// </summary>
        private void ShowSettingsDialog()
        {
            try
            {
                _logger?.Debug("Opening settings dialog");

                // Create a deep copy of settings to avoid modifying originals until confirmed
                var settingsCopy = _settingsManager.DeepCopy();

                // Show wait cursor while preparing settings form
                using (new CursorScope(Cursors.WaitCursor))
                using (var settingsForm = new TagListSettingsForm(settingsCopy))
                {
                    _logger?.Debug("Settings dialog initialized");

                    var result = settingsForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        _logger?.Info("User confirmed settings changes - applying updates");

                        // Show wait cursor while applying settings
                        using (new CursorScope(Cursors.WaitCursor))
                        {
                            // Update and save the settings
                            _settingsManager = settingsForm.SettingsStorage;
                            SavePluginConfiguration();
                            UpdateTabControlVisibility();

                            _logger?.Info("Settings applied successfully");
                        }
                    }
                    else
                    {
                        _logger?.Debug("Settings dialog canceled - no changes applied");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in settings dialog: {ex.Message}", ex);
                ShowErrorMessage("An error occurred while processing settings. Please check the log for details.");
            }
        }

        /// <summary>
        /// Displays an error message to the user with standard formatting.
        /// </summary>
        /// <param name="message">The error message to display</param>
        /// <param name="title">Optional title for the error dialog (defaults to "Error")</param>
        /// <param name="buttons">Optional dialog buttons (defaults to OK)</param>
        private void ShowErrorMessage(string message, string title = "Error", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            try
            {
                // Log the error first
                _logger?.Error($"Error displayed to user: {message}");

                // Ensure we're on the UI thread
                if (_tagsPanelControl?.InvokeRequired == true)
                {
                    _tagsPanelControl.BeginInvoke(new Action(() =>
                        MessageBox.Show(message, title, buttons, MessageBoxIcon.Error)));
                }
                else
                {
                    MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Last resort logging if showing the error dialog itself fails
                _logger?.Error($"Failed to show error message dialog: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error showing message box: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the visibility of the tab control based on whether it has any tabs.
        /// </summary>
        private void UpdateTabControlVisibility()
        {
            try
            {
                if (_tabControl != null && !_tabControl.IsDisposed)
                {
                    bool shouldBeVisible = _tabControl.TabCount > 0;
                    if (_tabControl.Visible != shouldBeVisible)
                    {
                        _tabControl.Visible = shouldBeVisible;
                        _logger?.Debug($"TabControl visibility set to {shouldBeVisible} (tab count: {_tabControl.TabCount})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error updating tab control visibility: {ex.Message}");
            }
        }

        private void SavePluginConfiguration()
        {
            try
            {
                if (_settingsManager == null)
                {
                    _logger?.Error("Cannot save plugin configuration: SettingsManager is null");
                    return;
                }

                // Save settings to file
                if (!_settingsManager.SaveAllSettings())
                {
                    _logger?.Warn("SaveAllSettings returned false - settings may not have been saved correctly");
                }

                // Update sort order from settings
                ApplySortOrderFromSettings();

                // Update each tag panel with the latest settings
                foreach (var tagPanel in _checklistBoxList.Values)
                {
                    if (tagPanel == null || tagPanel.IsDisposed)
                    {
                        continue;
                    }

                    var tagName = tagPanel.Name;
                    if (string.IsNullOrEmpty(tagName))
                    {
                        _logger?.Warn("Skipping tag panel with null or empty name");
                        continue;
                    }

                    var newTagsStorage = _settingsManager.RetrieveTagsStorageByTagName(tagName);
                    if (newTagsStorage != null)
                    {
                        var data = GetTagsFromStorage(newTagsStorage);
                        tagPanel.UpdateTagsStorage(newTagsStorage, data);
                    }
                    else
                    {
                        _logger?.Warn($"Could not retrieve TagsStorage for '{tagName}'");
                    }
                }

                // Refresh panel content to reflect changes
                RefreshPanelContent();
                _logger.Info("Plugin configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error saving plugin configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates the sort order setting from the current settings manager configuration.
        /// </summary>
        private void ApplySortOrderFromSettings()
        {
            try
            {
                var tagsStorage = _settingsManager?.RetrieveFirstTagsStorage();
                if (tagsStorage != null)
                {
                    bool newSortOrder = tagsStorage.Sorted;

                    // Only update if the sort order has actually changed
                    if (_sortAlphabetically != newSortOrder)
                    {
                        _logger?.Debug($"Changing sort order from {_sortAlphabetically} to {newSortOrder}");
                        _sortAlphabetically = newSortOrder;

                        // Update tag panels that might need to refresh their sort order
                        foreach (var panel in _checklistBoxList.Values)
                        {
                            if (panel != null && !panel.IsDisposed)
                            {
                                panel.Refresh();
                            }
                        }
                    }
                }
                else
                {
                    _logger?.Warn("ApplySortOrderFromSettings: No TagsStorage available to retrieve sort setting");
                    _sortAlphabetically = false; // Default to unsorted if no storage is available
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error applying sort order from settings: {ex.Message}", ex);
                // Keep current sort setting in case of error
            }
        }

        /// <summary>
        /// Refreshes the entire panel content by rebuilding tab pages and updating tag data.
        /// </summary>
        /// <param name="rebuildTabs">Optional. If true, rebuilds all tab pages. Default is true.</param>
        /// <remarks>
        /// This method performs a complete refresh of the UI, which may be expensive.
        /// For partial updates, consider using more specific refresh methods.
        /// </remarks>
        private void RefreshPanelContent(bool rebuildTabs = true)
        {
            try
            {
                _logger?.Debug("Beginning panel content refresh");

                if (rebuildTabs)
                {
                    RebuildTabPages();
                }

                InvokeRefreshTagTableData();

                _logger?.Debug("Panel content refresh completed");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error refreshing panel content: {ex.Message}", ex);

                // Ensure tag table data is still refreshed even if tab rebuild fails
                if (rebuildTabs)
                {
                    try
                    {
                        InvokeRefreshTagTableData();
                    }
                    catch (Exception innerEx)
                    {
                        _logger?.Error($"Failed to refresh tag table data after tab rebuild error: {innerEx.Message}", innerEx);
                    }
                }
            }
        }

        /// <summary>
        /// Rebuilds all tab pages by clearing existing pages and repopulating with fresh data.
        /// </summary>
        /// <param name="preserveSelectedTab">If true, attempts to restore the previously selected tab after rebuilding.</param>
        private void RebuildTabPages(bool preserveSelectedTab = true)
        {
            try
            {
                _logger?.Debug("Beginning tab pages rebuild...");

                // Store currently selected tab name if needed
                string selectedTabName = null;
                if (preserveSelectedTab && _tabControl != null && !_tabControl.IsDisposed &&
                    _tabControl.SelectedTab != null)
                {
                    selectedTabName = _tabControl.SelectedTab.Text;
                    _logger?.Debug($"Preserving selected tab: '{selectedTabName}'");
                }

                // Suspend layout to reduce flickering
                if (_tabControl != null && !_tabControl.IsDisposed)
                    _tabControl.SuspendLayout();

                // Clear and rebuild tabs
                ClearAllTagPages();
                PopulateTabPages();

                // Restore selected tab if possible
                if (preserveSelectedTab && selectedTabName != null &&
                    _tabControl != null && !_tabControl.IsDisposed)
                {
                    for (int i = 0; i < _tabControl.TabCount; i++)
                    {
                        if (_tabControl.TabPages[i].Text == selectedTabName)
                        {
                            _tabControl.SelectedIndex = i;
                            _logger?.Debug($"Restored selected tab: '{selectedTabName}'");
                            break;
                        }
                    }
                }

                // Resume layout
                if (_tabControl != null && !_tabControl.IsDisposed)
                    _tabControl.ResumeLayout();

                _logger?.Debug("Tab pages rebuild completed successfully");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error rebuilding tab pages: {ex.Message}", ex);

                // Make sure layout is resumed even if an exception occurs
                if (_tabControl != null && !_tabControl.IsDisposed)
                    _tabControl.ResumeLayout();
            }
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

                if (!_tabPageList.TryGetValue(tagName, out var tabPage) || tabPage == null || tabPage.IsDisposed)
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

        /// <summary>
        /// Applies the selected tag to all selected files with the specified check state.
        /// </summary>
        /// <param name="fileUrls">Array of file paths to process</param>
        /// <param name="selected">The check state to apply</param>
        /// <param name="selectedTag">The tag to apply or remove</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        private bool ApplyTagsToSelectedFiles(string[] fileUrls, CheckState selected, string selectedTag)
        {
            if (fileUrls == null || fileUrls.Length == 0 || string.IsNullOrWhiteSpace(selectedTag))
            {
                _logger?.Debug("ApplyTagsToSelectedFiles called with invalid parameters");
                return false;
            }

            try
            {
                var metaDataType = GetActiveTabMetaDataType();
                if (metaDataType == 0)
                {
                    _logger?.Error("ApplyTagsToSelectedFiles failed: Invalid metadata type");
                    return false;
                }

                _logger?.Info($"Applying tag '{selectedTag}' with state {selected} to {fileUrls.Length} files");
                _tagManager.SetTagsInFile(fileUrls, selected, selectedTag, metaDataType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error applying tags to files: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets the MetaDataType enum value for the currently active tab.
        /// </summary>
        /// <returns>
        /// The MetaDataType enum value corresponding to the active tab name, or 0 (None) if 
        /// the tab name is invalid or cannot be parsed as a MetaDataType.
        /// </returns>
        public MetaDataType GetActiveTabMetaDataType()
        {
            if (string.IsNullOrEmpty(_metaDataTypeName))
            {
                _logger?.Debug("GetActiveTabMetaDataType: _metaDataTypeName is null or empty");
                return 0;
            }

            if (Enum.TryParse(_metaDataTypeName, true, out MetaDataType result))
            {
                return result;
            }
            else
            {
                _logger?.Warn($"GetActiveTabMetaDataType: Failed to parse '{_metaDataTypeName}' as MetaDataType");
                return 0;
            }
        }

        /// <summary>
        /// Gets the current TagsStorage object associated with the active tab.
        /// </summary>
        /// <returns>
        /// The TagsStorage object for the currently active metadata type,
        /// or null if not available or if the metadata type is invalid.
        /// </returns>
        private TagsStorage GetCurrentTagsStorage()
        {
            if (_settingsManager == null)
            {
                _logger?.Error("GetCurrentTagsStorage failed: SettingsManager is null");
                return null;
            }

            MetaDataType metaDataType = GetActiveTabMetaDataType();
            if (metaDataType == 0)
            {
                _logger?.Debug("GetCurrentTagsStorage: No valid metadata type selected");
                return null;
            }

            var tagName = metaDataType.ToString();
            var tagsStorage = _settingsManager.RetrieveTagsStorageByTagName(tagName);

            // Only log at Info level if a tag storage was found
            if (tagsStorage != null)
            {
                _logger?.Debug($"Retrieved TagsStorage for metaDataType: {metaDataType}");
            }
            else
            {
                _logger?.Warn($"No TagsStorage found for metaDataType: {metaDataType}");
            }

            return tagsStorage;
        }

        private void ClearAllTagPages()
        {
            _logger?.Debug($"ClearAllTagPages: Starting cleanup of {_tabPageList.Count} tab pages");

            // Safely clean up and dispose all TagListPanel controls in each tab
            foreach (var tabPage in _tabPageList.Values.Where(tp => tp != null && !tp.IsDisposed))
            {
                try
                {
                    // Find and dispose all TagListPanel controls
                    var panels = tabPage.Controls.OfType<TagListPanel>().ToList();
                    foreach (var panel in panels)
                    {
                        // Unregister any event handlers to prevent memory leaks
                        panel.UnregisterItemCheckEventHandler(TagCheckStateChanged);
                        panel.Dispose();
                    }

                    tabPage.Controls.Clear();
                    _logger?.Debug($"Cleared controls from tab page: {tabPage.Text}");
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Error cleaning up tab page {tabPage.Text}: {ex.Message}");
                }
            }

            // Clear collections
            _tabPageList.Clear();

            // Clear TabControl if it exists
            if (_tabControl != null && !_tabControl.IsDisposed)
            {
                _tabControl.TabPages.Clear();
                _logger?.Debug("Cleared all tab pages from TabControl");
            }
            else
            {
                _logger?.Warn("TabControl was null or disposed during cleanup");
            }

            _logger?.Debug("ClearAllTagPages: Cleanup completed");
        }

        private Dictionary<string, CheckState> GetTagsFromStorage(TagsStorage currentTagsStorage)
        {
            if (currentTagsStorage == null)
            {
                _logger?.Error($"{nameof(GetTagsFromStorage)}: Received null TagsStorage");
                return new Dictionary<string, CheckState>();
            }
            
            return _tagManager.GetTagsFromStorage(currentTagsStorage, _tagsFromFiles);
        }

        private void UpdateTagsDisplayFromStorage()
        {
            try
            {
                // Get current tags storage and validate
                var currentTagsStorage = GetCurrentTagsStorage();
                if (currentTagsStorage == null)
                {
                    _logger?.Error($"{nameof(UpdateTagsDisplayFromStorage)}: Current TagsStorage is null");
                    return;
                }

                // Get tag name from storage
                var tagName = currentTagsStorage.GetTagName();
                if (string.IsNullOrEmpty(tagName))
                {
                    _logger?.Error($"{nameof(UpdateTagsDisplayFromStorage)}: Tag name from storage is empty or null");
                    return;
                }

                _logger?.Debug($"Updating tag display for '{tagName}'");

                // Get tags from storage and merge with tags from files
                var data = GetTagsFromStorage(currentTagsStorage);
                data = _tagManager.MergeTagsFromFiles(data, _tagsFromFiles);

                // Update the appropriate panel if it exists
                if (_checklistBoxList?.TryGetValue(tagName, out var tagListPanel) == true && tagListPanel != null)
                {
                    UpdateTagListPanel(tagListPanel, currentTagsStorage, data);
                }
                else
                {
                    _logger?.Error($"TagListPanel for tag '{tagName}' not found in checklist box dictionary");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in {nameof(UpdateTagsDisplayFromStorage)}: {ex.Message}", ex);
            }
        }

        private void UpdateTagListPanel(TagListPanel tagListPanel, TagsStorage tagsStorage, Dictionary<string, CheckState> data)
        {
            if (tagListPanel == null)
                return;

            if (tagListPanel.InvokeRequired)
            {
                try
                {
                    tagListPanel.Invoke(new Action(() => tagListPanel.UpdateTagsStorage(tagsStorage, data)));
                    _logger?.Debug($"Updated tags via Invoke for panel '{tagsStorage.GetTagName()}'");
                }
                catch (ObjectDisposedException)
                {
                    _logger?.Warn($"TagListPanel was disposed while attempting to invoke UpdateTagsStorage");
                }
                catch (InvalidOperationException ex)
                {
                    _logger?.Warn($"Failed to invoke UpdateTagsStorage: {ex.Message}");
                }
            }
            else
            {
                tagListPanel.UpdateTagsStorage(tagsStorage, data);
                _logger?.Debug($"Updated tags directly for panel '{tagsStorage.GetTagName()}'");
            }
        }

        /// <summary>
        /// Refreshes the tag table data in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method ensures that UI updates are performed on the UI thread
        /// and handles any exceptions that might occur during the update process.
        /// </remarks>
        private void InvokeRefreshTagTableData()
        {
            if (_tagsPanelControl == null || _tagsPanelControl.IsDisposed)
            {
                _logger?.Debug("RefreshTagTableData skipped: panel control is null or disposed");
                return;
            }

            try
            {
                if (_tagsPanelControl.InvokeRequired)
                {
                    _logger?.Debug("Dispatching UpdateTagsDisplayFromStorage to UI thread");
                    _tagsPanelControl.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            UpdateTagsDisplayFromStorage();
                        }
                        catch (Exception ex)
                        {
                            _logger?.Error($"Error in UI thread while updating tags display: {ex.Message}", ex);
                        }
                    }));
                }
                else
                {
                    _logger?.Debug("Directly updating tags display from storage");
                    UpdateTagsDisplayFromStorage();
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger?.Error($"Failed to invoke UpdateTagsDisplayFromStorage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Unexpected error in {nameof(InvokeRefreshTagTableData)}: {ex.Message}", ex);
            }
        }

        public void OnSettingsMenuClicked(object sender, EventArgs args)
        {
            try
            {
                _logger?.Info("Settings menu clicked - opening settings dialog");

                // Ensure settings manager is initialized before showing dialog
                if (_settingsManager == null)
                {
                    _logger?.Error("Unable to open settings: SettingsManager is not initialized");
                    MessageBox.Show("Cannot open settings at this time. Please try again later.",
                                   "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Use cursor to indicate processing
                using (new CursorScope(Cursors.WaitCursor))
                {
                    ShowSettingsDialog();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error opening settings dialog: {ex.Message}", ex);
                MessageBox.Show($"An error occurred while opening settings: {ex.Message}",
                               "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper class to temporarily change the cursor and restore it when done
        /// </summary>
        private class CursorScope : IDisposable
        {
            private readonly Cursor _previous;

            public CursorScope(Cursor cursor)
            {
                _previous = Cursor.Current;
                Cursor.Current = cursor;
            }

            public void Dispose()
            {
                Cursor.Current = _previous;
            }
        }

        /// <summary>
        /// Handles the check state change event for tag checkboxes.
        /// Updates the selected files with the appropriate tag state when a user checks or unchecks a tag.
        /// </summary>
        /// <param name="sender">The CheckedListBox that raised the event</param>
        /// <param name="e">Event arguments containing the item index and check state</param>
        private void TagCheckStateChanged(object sender, ItemCheckEventArgs e)
        {
            // Skip processing if batch operations are excluded or if we're already handling an event
            if (_excludeFromBatchSelection || _ignoreEventFromHandler)
                return;

            // Prevent recursive event handling by setting the ignore flag
            _ignoreEventFromHandler = true;
            try
            {
                // Ensure sender is a CheckedListBox and has valid items
                if (sender is CheckedListBox checkedListBox && e.Index >= 0 && e.Index < checkedListBox.Items.Count)
                {
                    var newState = e.NewValue;
                    var tagName = checkedListBox.Items[e.Index]?.ToString();

                    if (!string.IsNullOrEmpty(tagName) && _selectedFilesUrls != null && _selectedFilesUrls.Length > 0)
                    {
                        // Apply the tag change to all selected files
                        _logger?.Debug($"Applying tag '{tagName}' with state {newState} to {_selectedFilesUrls.Length} files");
                        ApplyTagsToSelectedFiles(_selectedFilesUrls, newState, tagName);

                        // Refresh UI to reflect changes
                        _mbApiInterface.MB_RefreshPanels();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error while changing tag check state: {ex.Message}", ex);
            }
            finally
            {
                // Always reset the ignore flag to prevent event handling from being permanently disabled
                _ignoreEventFromHandler = false;
            }
        }

        /// <summary>
        /// Handles the selection change event for the tab control.
        /// Updates the current metadata type and refreshes the panel contents.
        /// </summary>
        /// <param name="sender">The tab control that raised the event</param>
        /// <param="e">Event arguments</param>
        private void TabControlSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Validate the sender is a TabControl and has a valid selected tab
                if (!(sender is TabControl tabControl) || tabControl.SelectedTab == null || tabControl.SelectedTab.IsDisposed)
                {
                    _logger?.Debug("TabControlSelectionChanged: Invalid tab control or selected tab");
                    return;
                }

                string newMetaDataTypeName = tabControl.SelectedTab.Text;
                _logger?.Debug($"TabControlSelectionChanged: Tab changed to '{newMetaDataTypeName}'");

                // Only update if the metadata type has actually changed
                if (_metaDataTypeName != newMetaDataTypeName)
                {
                    _logger?.Info($"Switching metadata type from '{_metaDataTypeName}' to '{newMetaDataTypeName}'");
                    _metaDataTypeName = newMetaDataTypeName;

                    // Update the UI manager with the new visible tag panel
                    _uiManager.SwitchVisibleTagPanel(_metaDataTypeName);

                    // Refresh the tags from the selected files with the new metadata type
                    RefreshPanelTagsFromFiles(_selectedFilesUrls);

                    // Update the panel to reflect the current file selection
                    UpdateTagsInPanelOnFileSelection();
                }

                // Find and refresh the tag list panel in the selected tab
                var tagListPanel = tabControl.SelectedTab.Controls
                    .OfType<TagListPanel>()
                    .FirstOrDefault();

                if (tagListPanel != null)
                {
                    _logger?.Debug($"Refreshing tag list panel for tab '{newMetaDataTypeName}'");
                    tagListPanel.Refresh();
                }
                else
                {
                    _logger?.Debug($"No tag list panel found in tab '{newMetaDataTypeName}'");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in TabControlSelectionChanged: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enables or disables the tags panel control in a thread-safe manner.
        /// </summary>
        /// <param name="enabled">True to enable the panel, false to disable it.</param>
        /// <remarks>
        /// This method safely handles cross-thread UI updates by using Invoke when necessary.
        /// It also protects against operating on disposed controls.
        /// </remarks>
        private void SetPanelEnabled(bool enabled = true)
        {
            if (_tagsPanelControl == null || _tagsPanelControl.IsDisposed)
            {
                _logger?.Debug("SetPanelEnabled: Panel control is null or disposed");
                return;
            }

            try
            {
                if (_tagsPanelControl.InvokeRequired)
                {
                    _tagsPanelControl.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (!_tagsPanelControl.IsDisposed)
                            {
                                _tagsPanelControl.Enabled = enabled;
                                _logger?.Debug($"Panel {(enabled ? "enabled" : "disabled")} via UI thread");
                            }
                        }
                        catch (ObjectDisposedException ex)
                        {
                            _logger?.Debug($"Panel was disposed during Invoke: {ex.Message}");
                        }
                    }));
                }
                else
                {
                    _tagsPanelControl.Enabled = enabled;
                    _logger?.Debug($"Panel directly {(enabled ? "enabled" : "disabled")}");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger?.Warn($"Failed to set panel enabled state: {ex.Message}");
            }
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

        /// <summary>
        /// Refreshes the tags panel with tag information from the selected files.
        /// </summary>
        /// <param name="filenames">Array of file URLs to process</param>
        private void RefreshPanelTagsFromFiles(string[] filenames)
        {
            try
            {
                _logger?.Debug($"Refreshing panel tags from {(filenames?.Length ?? 0)} files");

                // Handle case where no files are selected
                if (filenames == null || filenames.Length == 0)
                {
                    _logger?.Debug("No files selected, clearing tags");
                    _tagsFromFiles.Clear();
                    UpdateTagsInPanelOnFileSelection();
                    SetPanelEnabled(true);
                    return;
                }

                // Get current tags storage
                var currentTagsStorage = GetCurrentTagsStorage();
                if (currentTagsStorage == null)
                {
                    _logger?.Error("RefreshPanelTagsFromFiles: Current TagsStorage is null");
                    SetPanelEnabled(false);
                    return;
                }

                // Update tags from selected files
                _logger?.Debug($"Combining tag lists for {filenames.Length} files with metadata type: {currentTagsStorage.GetTagName()}");
                _tagsFromFiles = _tagManager.CombineTagLists(filenames, currentTagsStorage);

                // Update UI with refreshed tag data
                UpdateTagsInPanelOnFileSelection();
                SetPanelEnabled(true);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error refreshing panel tags: {ex.Message}", ex);
                SetPanelEnabled(true); // Ensure panel is enabled even if an error occurs
            }
        }



        /// Creates and initializes the tab control panel for displaying tag categories.
        /// </summary>
        /// <returns>True if the tab panel was successfully created, false otherwise.</returns>
        private bool CreateTabPanel()
        {
            try
            {
                _logger?.Debug("Creating tab panel...");

                // Validate required dependencies
                if (_mbApiInterface.Equals(default(MusicBeeApiInterface)))
                {
                    _logger?.Error("Cannot create tab panel: MusicBee API interface is null");
                    return false;
                }
                // Remove this redundant check since we already checked above
                // if (_mbApiInterface == null)
                // {
                //     _logger?.Error("Cannot create tab panel: MusicBee API interface is null");
                //     return false;
                // }

                if (_tagsPanelControl == null || _tagsPanelControl.IsDisposed)
                {
                    _logger?.Error("Cannot create tab panel: Tags panel control is null or disposed");
                    return false;
                }

                // Create the tab control through MusicBee API
                _tabControl = (TabControl)_mbApiInterface.MB_AddPanel(_tagsPanelControl, (PluginPanelDock)6);
                if (_tabControl == null)
                {
                    _logger?.Error("Failed to create tab control: MB_AddPanel returned null");
                    return false;
                }

                // Configure the tab control
                _tabControl.Dock = DockStyle.Fill;
                _tabControl.SuspendLayout();

                // Apply styling if UIManager is available
                _uiManager?.ApplySkinStyleToControl(_tabControl);

                // Register event handler
                _tabControl.SelectedIndexChanged += TabControlSelectionChanged;

                // Populate the tab pages if needed
                if (_tabControl.TabPages.Count == 0)
                {
                    _logger?.Debug("Tab control created with no pages, populating pages...");
                    PopulateTabPages();
                }

                _tabControl.ResumeLayout();
                _logger?.Info($"Tab panel created successfully with {_tabControl.TabPages.Count} tabs");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error creating tab panel: {ex.Message}", ex);
                return false;
            }
        }

        private void AddControls()
        {
            try
            {
                // Check for null control before accessing InvokeRequired
                if (_tagsPanelControl == null)
                {
                    _logger?.Error("Cannot add controls: Tags panel control is null");
                    return;
                }

                // Handle cross-thread invocation
                if (_tagsPanelControl.InvokeRequired)
                {
                    _logger?.Debug("Dispatching AddControls to UI thread");
                    _tagsPanelControl.Invoke(new Action(AddControls));
                    return;
                }

                // Check if control is still valid (not disposed)
                if (_tagsPanelControl.IsDisposed)
                {
                    _logger?.Error("Cannot add controls: Tags panel control is disposed");
                    return;
                }

                _logger?.Debug("Adding controls to panel");
                _tagsPanelControl.SuspendLayout();

                // Create tab panel and check if successful before adding to controls
                if (CreateTabPanel())
                {
                    _tagsPanelControl.Controls.Add(_tabControl);
                    _tagsPanelControl.Enabled = false;
                    _logger?.Debug("Controls added successfully");
                }
                else
                {
                    _logger?.Error("Failed to create tab panel, controls not added");
                }

                _tagsPanelControl.ResumeLayout();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in AddControls: {ex.Message}", ex);
            }
        }

        public void Close(PluginCloseReason reason)
        {
            try
            {
                _logger?.Info($"Plugin closing with reason: {reason:G}");

                // Clean up event handlers
                if (_tabControl != null)
                {
                    _tabControl.SelectedIndexChanged -= TabControlSelectionChanged;
                }

                // Dispose of all TagListPanel controls
                foreach (var panel in _checklistBoxList.Values)
                {
                    try
                    {
                        panel?.UnregisterItemCheckEventHandler(TagCheckStateChanged);
                        panel?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error($"Error disposing TagListPanel: {ex.Message}");
                    }
                }

                // Clear collections
                _checklistBoxList.Clear();
                _tabPageList.Clear();
                _tagsFromFiles.Clear();

                // Dispose of remaining resources
                _tagManager = null;
                _uiManager?.Dispose();
                _uiManager = null;
                _settingsManager = null;

                // Finally dispose of the panel and logger
                _tagsPanelControl?.Dispose();
                _tagsPanelControl = null;

                _logger?.Dispose();
                _logger = null;
            }
            catch (Exception ex)
            {
                // Can't use logger here as it might be disposed
                System.Diagnostics.Debug.WriteLine($"Error during plugin shutdown: {ex}");
            }
        }

      
        public void Uninstall()
        {
            try
            {
                _logger?.Info("Beginning plugin uninstallation process");

                // Clean up settings file
                CleanUpSettingsFile();

                // Clean up log file
                CleanUpLogFile();

                // Final cleanup of resources
                DisposeRemainingResources();

                _logger?.Info("Uninstallation completed successfully");
            }
            catch (Exception ex)
            {
                // Can't use logger here as it might be disposed or in an error state
                System.Diagnostics.Debug.WriteLine($"Error during plugin uninstallation: {ex}");
            }
            finally
            {
                // Ensure logger is disposed even if an exception occurs
                _logger?.Dispose();
            }
        }

        private void CleanUpSettingsFile()
        {
            if (_settingsManager == null)
            {
                _logger?.Debug("Settings manager is null, skipping settings cleanup");
                return;
            }

            try
            {
                string settingsPath = _settingsManager.GetSettingsPath();
                if (string.IsNullOrEmpty(settingsPath))
                {
                    _logger?.Debug("Settings path is empty, skipping settings cleanup");
                    return;
                }

                DeleteFileWithRetry(settingsPath, "settings");
                CleanEmptyDirectory(Path.GetDirectoryName(settingsPath), "settings");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                _logger?.Error($"Failed to delete settings file: {ex.Message}", ex);
            }
        }

        private void CleanUpLogFile()
        {
            try
            {
                string logFilePath = _logger?.GetLogFilePath();
                if (string.IsNullOrEmpty(logFilePath))
                {
                    return;
                }

                DeleteFileWithRetry(logFilePath, "log");
                CleanEmptyDirectory(Path.GetDirectoryName(logFilePath), "log");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                _logger?.Error($"Failed to delete log file: {ex.Message}", ex);
            }
        }

        private bool DeleteFileWithRetry(string filePath, string fileType)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            const int maxRetries = 3;
            const int retryDelayMs = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    File.Delete(filePath);
                    _logger?.Info($"{fileType.ToUpperInvariant()} file deleted: {filePath}");
                    return true;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    // File might be locked, wait briefly and retry
                    System.Threading.Thread.Sleep(retryDelayMs * attempt);
                }
            }

            _logger?.Warn($"Failed to delete {fileType} file after {maxRetries} attempts: {filePath}");
            return false;
        }

        private bool CleanEmptyDirectory(string dirPath, string dirType)
        {
            if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
            {
                return false;
            }

            try
            {
                if (!Directory.EnumerateFileSystemEntries(dirPath).Any())
                {
                    Directory.Delete(dirPath);
                    _logger?.Info($"Empty {dirType} directory removed: {dirPath}");
                    return true;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger?.Warn($"Failed to remove empty {dirType} directory: {ex.Message}");
            }

            return false;
        }

        private void DisposeRemainingResources()
        {
            try
            {
                // Dispose UI manager if not already disposed
                if (_uiManager != null)
                {
                    _uiManager.Dispose();
                    _logger?.Debug("UI manager disposed");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error disposing remaining resources: {ex.Message}", ex);
            }
        }
        

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            try
            {
                // Skip processing with a single condition check for better readability
                if (ShouldSkipNotificationProcessing(type))
                {
                    return;
                }

                _logger?.Debug($"Processing notification: {type} for file: {sourceFileUrl ?? "(null)"}");

                switch (type)
                {
                    // Tag-related notifications
                    case NotificationType.TagsChanging:
                        HandleTagsChanging(sourceFileUrl);
                        break;
                    case NotificationType.TagsChanged:
                        HandleTagsChanged(sourceFileUrl);
                        break;

                    // File/track change notifications
                    case NotificationType.TrackChanged:
                        HandleTrackChanged(sourceFileUrl);
                        break;
                    case NotificationType.FileAddedToLibrary:
                        HandleFileAddedToLibrary(sourceFileUrl);
                        break;

                    // Player state notifications
                    case NotificationType.PlayStateChanged:
                        // Only log these high-frequency events at trace level
                        _logger?.Debug("Play state changed - no action needed");
                        break;

                    // Library notifications
                    case NotificationType.FileDeleted:
                        HandleFileDeleted(sourceFileUrl);
                        break;

                    // Other notifications we don't need to handle specifically
                    default:
                        // Only log unhandled notification types at debug level to avoid log spam
                        _logger?.Debug($"Unhandled notification type: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error handling notification {type}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determines if notification processing should be skipped.
        /// </summary>
        /// <param name="type">The notification type to check</param>
        /// <returns>True if the notification should be skipped, false otherwise</returns>
        private bool ShouldSkipNotificationProcessing(NotificationType type)
        {
            // Skip processing if any of these conditions are true
            if (_tagsPanelControl == null || _tagsPanelControl.IsDisposed)
            {
                _logger?.Debug("Skipping notification: panel control is null or disposed");
                return true;
            }

            if (type == NotificationType.ApplicationWindowChanged)
            {
                // This is a high-frequency event that we don't need to process
                return true;
            }

            if (GetActiveTabMetaDataType() == 0)
            {
                _logger?.Debug("Skipping notification: no active metadata type");
                return true;
            }

            if (_ignoreEventFromHandler)
            {
                _logger?.Debug("Skipping notification: events being ignored due to handler operation");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles tag change notifications from MusicBee.
        /// </summary>
        /// <param name="sourceFileUrl">The URL of the file whose tags have changed</param>
        private void HandleTagsChanged(string sourceFileUrl)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
            {
                _logger?.Warn("HandleTagsChanged: sourceFileUrl is null or empty");
                return;
            }

            // Refresh the panel contents for the affected file
            _logger?.Debug($"Tags changed for file: {sourceFileUrl}");

            MetaDataType metaDataType = GetActiveTabMetaDataType();
            _tagsFromFiles = _tagManager.UpdateTagsFromFile(sourceFileUrl, metaDataType);

            InvokeRefreshTagTableData();
        }

        /// <summary>
        /// Handles file deletion notifications from MusicBee.
        /// </summary>
        /// <param name="sourceFileUrl">The URL of the file that was deleted</param>
        private void HandleFileDeleted(string sourceFileUrl)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
            {
                _logger?.Warn("HandleFileDeleted: sourceFileUrl is null or empty");
                return;
            }

            _logger?.Debug($"File deleted: {sourceFileUrl}");

            // If this file was part of the current selection, refresh the panel
            if (_selectedFilesUrls != null && _selectedFilesUrls.Contains(sourceFileUrl))
            {
                var updatedSelection = _selectedFilesUrls.Where(url => url != sourceFileUrl).ToArray();
                OnSelectedFilesChanged(updatedSelection);
            }
        }

        /// <summary>
        /// Handles file added to library notifications from MusicBee.
        /// </summary>
        /// <param name="sourceFileUrl">The URL of the file that was added</param>
        private void HandleFileAddedToLibrary(string sourceFileUrl)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
            {
                _logger?.Warn("HandleFileAddedToLibrary: sourceFileUrl is null or empty");
                return;
            }

            _logger?.Debug($"File added to library: {sourceFileUrl}");

            // If no files are currently selected, we don't need to update anything
            if (_selectedFilesUrls == null || _selectedFilesUrls.Length == 0)
            {
                return;
            }

            // If exactly this file is selected (e.g., via pending files list), update the tags
            if (_selectedFilesUrls.Length == 1 && _selectedFilesUrls[0] == sourceFileUrl)
            {
                RefreshPanelTagsFromFiles(_selectedFilesUrls);
            }
        }

        private void HandleTagsChanging(string sourceFileUrl)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
            {
                _logger?.Warn("HandleTagsChanging: sourceFileUrl is null or empty");
                return;
            }

            _excludeFromBatchSelection = true;
            _mbApiInterface.Library_CommitTagsToFile(sourceFileUrl);
            _logger?.Debug($"Tags committed to file: {sourceFileUrl}");
        }

        private void HandleTrackChanged(string sourceFileUrl)
        {
            if (string.IsNullOrEmpty(sourceFileUrl))
            {
                _logger?.Debug("HandleTrackChanged: sourceFileUrl is null or empty, clearing tags");
                _tagsFromFiles.Clear();
            }
            else
            {
                MetaDataType metaDataType = GetActiveTabMetaDataType();
                _tagsFromFiles = _tagManager.UpdateTagsFromFile(sourceFileUrl, metaDataType);
                _logger?.Debug($"Updated tags from file: {sourceFileUrl} for metadata type: {metaDataType}");
            }

            InvokeRefreshTagTableData();
            _excludeFromBatchSelection = false;
        }

        /// <summary>
        /// Handles creation of the dockable panel by MusicBee and initializes the plugin UI components.
        /// </summary>
        /// <param name="panel">The control provided by MusicBee to host the plugin UI</param>
        /// <returns>Always returns 0 to indicate successful initialization</returns>
        public int OnDockablePanelCreated(Control panel)
        {
            try
            {
                _logger?.Debug("OnDockablePanelCreated: Initializing plugin panel");
                _tagsPanelControl = panel ?? throw new ArgumentNullException(nameof(panel));

                // Ensure the panel is created and ready for UI elements
                EnsureControlCreated();

                // Add controls to the panel
                AddControls();

                // Display prompt if no tags are configured
                if (_tabControl == null || _tabControl.TabCount == 0)
                {
                    _logger?.Info("No tag tabs available - showing settings prompt");
                    _uiManager?.DisplaySettingsPromptLabel(_tagsPanelControl, _tabControl, "No tags available. Please add tags in the settings.");
                }

                // Initialize tag data if panel is ready
                if (_tagsPanelControl.IsHandleCreated)
                {
                    _logger?.Debug("Panel handle created - refreshing tag data");
                    InvokeRefreshTagTableData();
                }

                _logger?.Info("Dockable panel initialization completed");
                return 0;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error in OnDockablePanelCreated: {ex.Message}", ex);
                return 0; // Return 0 even on error to prevent plugin loading failure
            }
        }

        /// <summary>
        /// Ensures the panel control is created and ready for UI elements.
        /// </summary>
        /// <remarks>
        /// This method safely creates the control handle if it doesn't already exist,
        /// allowing UI operations to be performed on the control.
        /// </remarks>
        private void EnsureControlCreated()
        {
            if (_tagsPanelControl == null)
            {
                _logger?.Error("Cannot ensure control created: _tagsPanelControl is null");
                return;
            }

            try
            {
                if (!_tagsPanelControl.IsDisposed && !_tagsPanelControl.Created)
                {
                    _logger?.Debug("Creating control handle for panel");
                    _tagsPanelControl.CreateControl();
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger?.Error($"Failed to create control: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles changes in file selection in MusicBee.
        /// Updates the tags panel to display tags for the newly selected files.
        /// </summary>
        /// <param name="filenames">Array of selected file paths, or null if no files are selected</param>
        public void OnSelectedFilesChanged(string[] filenames)
        {
            try
            {
                string[] newSelection = filenames ?? Array.Empty<string>();

                // Check if the selection actually changed to avoid unnecessary processing
                if (SequenceEqual(_selectedFilesUrls, newSelection))
                {
                    _logger?.Debug("File selection unchanged - skipping refresh");
                    return;
                }

                _logger?.Debug($"File selection changed: {newSelection.Length} files selected");
                _selectedFilesUrls = newSelection;

                if (_selectedFilesUrls.Any())
                {
                    // Files are selected - refresh the panel with tag data
                    RefreshPanelTagsFromFiles(_selectedFilesUrls);
                }
                else
                {
                    // No files selected - clear the tag display
                    _logger?.Debug("No files selected - clearing tag display");
                    _tagsFromFiles.Clear();
                    UpdateTagsInPanelOnFileSelection();
                }

                // Always ensure the panel is enabled
                SetPanelEnabled(true);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error handling file selection change: {ex.Message}", ex);

                // Ensure panel is enabled even if an error occurs
                SetPanelEnabled(true);
            }
        }

        /// <summary>
        /// Compares two string arrays for sequence equality.
        /// </summary>
        /// <returns>True if both arrays contain the same elements in the same order, or if both are null</returns>
        private bool SequenceEqual(string[] array1, string[] array2)
        {
            if (array1 == array2) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Provides a list of menu items that will be displayed in MusicBee's plugin menu.
        /// </summary>
        /// <returns>A list of ToolStripItems to be displayed in the plugins menu.</returns>
        public List<ToolStripItem> GetMenuItems()
        {
            return new List<ToolStripItem>
            {
                new ToolStripMenuItem("Settings", null, OnSettingsMenuClicked)
            };
        }
    }
}