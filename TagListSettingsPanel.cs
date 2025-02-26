using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    /// <summary>
    /// Represents the settings panel for managing tag lists.
    /// </summary>
    public partial class TagListSettingsPanel : UserControl
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private readonly SettingsManager _settingsManager;
        private readonly TagsStorage _tagsStorage;
        private readonly TagsCsvHelper _tagsCsvHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagListSettingsPanel"/> class.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="settingsManager">The settings manager.</param>
        public TagListSettingsPanel(string tagName, SettingsManager settingsManager)
        {
            InitializeComponent();
            InitializeToolTip();
            SendMessage(TxtNewTagInput.Handle, EM_SETCUEBANNER, 0, Messages.EnterTagMessagePlaceholder);

            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _tagsStorage = _settingsManager.RetrieveTagsStorageByTagName(tagName) ?? throw new ArgumentNullException(nameof(tagName));
            _tagsCsvHelper = new TagsCsvHelper(ShowMessageBox);

            AttachEventHandlers();
            UpdateSortOption();
            UpdateTags();
            TxtNewTagInput.Focus();
        }
        private void InitializeToolTip()
        {
            var toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip.SetToolTip(CbEnableAlphabeticalTagListSorting, Messages.TagSortTooltip);
        }

        private void SetUpDownButtonsState(bool enabled)
        {
            BtnMoveTagUp.Enabled = enabled;
            BtnMoveTagDown.Enabled = enabled;
        }

        private void ShowMessageBox(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, buttons, icon);
        }

        private void TxtNewTagInput_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtNewTagInput.Text))
            {
                TxtNewTagInput.Text = Messages.EnterTagMessagePlaceholder;
                TxtNewTagInput.ForeColor = SystemColors.GrayText;
            }
        }

        private void TxtNewTagInput_Enter(object sender, EventArgs e)
        {
            if (TxtNewTagInput.Text == Messages.EnterTagMessagePlaceholder)
            {
                TxtNewTagInput.Text = string.Empty;
                TxtNewTagInput.ForeColor = SystemColors.WindowText;
            }
        }

        /// <summary>
        /// Sets up the panel for first use.
        /// </summary>
        public void SetUpPanelForFirstUse()
        {
            LstTags.SelectedIndex = LstTags.Items.Count > 0 ? 0 : -1;
            SetUpDownButtonsState(!_tagsStorage.Sorted);
            UpdateTags();
        }

        private void UpdateSortOption()
        {
            CbEnableAlphabeticalTagListSorting.CheckedChanged -= CbEnableTagSort_CheckedChanged;
            CbEnableAlphabeticalTagListSorting.Checked = _tagsStorage.Sorted;
            SetUpDownButtonsState(!_tagsStorage.Sorted);
            CbEnableAlphabeticalTagListSorting.CheckedChanged += CbEnableTagSort_CheckedChanged;
        }

        private void AttachEventHandlers()
        {
            LstTags.KeyDown += KeyEventHandler;
            TxtNewTagInput.KeyDown += KeyEventHandler;
            CbEnableAlphabeticalTagListSorting.CheckedChanged += CbEnableTagSort_CheckedChanged;
        }

        private void KeyEventHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (sender == TxtNewTagInput)
                {
                    AddNewTagToList();
                }
                else if (sender == LstTags)
                {
                    RemoveSelectedTagFromList();
                }
                e.Handled = true;
            }
        }

        private void CbEnableTagSort_CheckedChanged(object sender, EventArgs e)
        {
            _tagsStorage.Sorted = CbEnableAlphabeticalTagListSorting.Checked;
            SetUpDownButtonsState(!_tagsStorage.Sorted);
            UpdateTags();
        }

        /// <summary>
        /// Updates the tags displayed in the list.
        /// </summary>
        public void UpdateTags()
        {
            var tags = _tagsStorage.GetTags().Keys.ToList();

            LstTags.BeginUpdate();
            LstTags.Items.Clear();
            LstTags.Items.AddRange(tags.ToArray());
            LstTags.EndUpdate();
        }

        /// <summary>
        /// Adds a new tag to the list.
        /// </summary>
        public void AddNewTagToList()
        {
            var newTag = TxtNewTagInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(newTag) || _tagsStorage.TagList.ContainsKey(newTag))
            {
                ShowMessageBox(string.IsNullOrWhiteSpace(newTag) ? Messages.TagInputBoxEmptyMessage : Messages.TagListAddDuplicateTagMessage, Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Assign index based on count
            int newIndex = _tagsStorage.TagList.Count;
            _tagsStorage.TagList.Add(newTag, newIndex);
            UpdateTags();
            TxtNewTagInput.Text = string.Empty;
        }

        /// <summary>
        /// Removes the selected tag from the list.
        /// </summary>
        public void RemoveSelectedTagFromList()
        {
            if (LstTags.SelectedIndex == -1 || LstTags.Items.Count == 0)
            {
                return;
            }

            var selectedItems = LstTags.SelectedItems.Cast<string>().ToList();

            foreach (var selectedItem in selectedItems)
            {
                _tagsStorage.TagList.Remove(selectedItem);
            }

            // Reindex remaining tags
            int index = 0;
            foreach (var key in _tagsStorage.TagList.Keys.ToList())
            {
                _tagsStorage.TagList[key] = index;
                index++;
            }

            UpdateTags();

            // Update selection
            if (LstTags.Items.Count > 0)
            {
                LstTags.SelectedIndex = Math.Min(LstTags.SelectedIndex, LstTags.Items.Count - 1);
            }
        }

        /// <summary>
        /// Clears the tags list in the settings.
        /// </summary>
        public void ClearTagsListInSettings()
        {
            _tagsStorage.Clear();
        }

        /// <summary>
        /// Imports tags from a CSV file.
        /// </summary>

        /// <summary>
        /// Imports tags from a CSV file.
        /// </summary>
        public void ImportTagsFromCsv()
        {
            _tagsCsvHelper.ImportTagsFromCsv(_tagsStorage, tag => LstTags.Items.Add(tag));
            UpdateTags(); // Ensure the list is fully updated
        }

        /// <summary>
        /// Exports the tags to a CSV file.
        /// </summary>
        public void ExportTagsToCsv()
        {
            _tagsCsvHelper.ExportTagsToCsv(LstTags.Items.Cast<string>());
        }


        private void BtnAddTag_Click(object sender, EventArgs e)
        {
            AddNewTagToList();
        }

        private void BtnRemTag_Click(object sender, EventArgs e)
        {
            RemoveSelectedTagFromList();
        }

        private void BtnImportCSVToList_Click(object sender, EventArgs e)
        {
            ImportTagsFromCsv();
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            ExportTagsToCsv();
        }

        private void BtnClearTagSettings_Click(object sender, EventArgs e)
        {
            if (LstTags.Items.Count != 0)
            {
                PromptClearTagsConfirmation();
            }
        }

        private void BtnMoveTagUpSettings_Click(object sender, EventArgs e)
        {
            MoveItem(-1);
        }

        private void BtnMoveTagDownSettings_Click(object sender, EventArgs e)
        {
            MoveItem(1);
        }

        /// <summary>
        /// Moves the selected tag up in the list.
        /// </summary>
        public void MoveUp()
        {
            MoveItem(-1);
        }

        /// <summary>
        /// Moves the selected tag down in the list.
        /// </summary>
        public void MoveDown()
        {
            MoveItem(1);
        }

        /// <summary>
        /// Moves the selected item in the specified direction.
        /// </summary>
        /// <param name="direction">The direction to move the item.</param>
        public void MoveItem(int direction)
        {
            if (LstTags.SelectedItem == null || LstTags.SelectedIndex < 0 || _tagsStorage.Sorted)
                return;

            int oldIndex = LstTags.SelectedIndex;
            int newIndex = oldIndex + direction;

            if (newIndex < 0 || newIndex >= LstTags.Items.Count)
                return;

            var tags = _tagsStorage.GetTags().ToList();
            var selectedTag = tags[oldIndex];

            // Swap indices in TagList
            var otherTag = tags[newIndex];

            int tempIndex = _tagsStorage.TagList[selectedTag.Key];
            _tagsStorage.TagList[selectedTag.Key] = _tagsStorage.TagList[otherTag.Key];
            _tagsStorage.TagList[otherTag.Key] = tempIndex;

            UpdateTags();
            LstTags.SetSelected(newIndex, true);
        }

        private void PromptClearTagsConfirmation()
        {
            if (MessageBox.Show(Messages.ClearAllCurrentTagsInLIstMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ClearTagsListInSettings();
            }
        }
    }
}