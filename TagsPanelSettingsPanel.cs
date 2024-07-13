using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class TagsPanelSettingsPanel : UserControl
    {
        private TagsStorage tagsStorage;

        private const int EM_SETCUEBANNER = 0x1501;

        private const string CsvFileFilter = "csv files (*.csv)|*.csv";
        private const string CsvDefaultExt = "csv";
        // private const string CsvDialogTitle = Messages.CsvDialogTitle;
        private const string CsvImportSuccessMessage = "CSV import successful";
        private const string CsvImportCancelMessage = "CSV import canceled";
        private const string CsvExportSuccessMessage = "Tags exported in CSV";
        private const string CsvConfirmationMessage = "Warning: This will replace all entries of this tag. Do you want to continue with the CSV import?";
        private const string CsvConfirmationTitle = "Confirmation";

        private const string EnterTagMessage = "Please enter a tag";
        private const string TagSortToolTip = "If enabled, the tags will always be sorted alphabetically in the tag. Otherwise, you can use the up and down buttons to reorder your tag lists.";
        private const string DuplicateTagMessage = "Tag is already in the list!";
        private const string DuplicateTagTitle = "Duplicate found!";
        private const string ClearListMessage = "This will clear your current tag list. Continue?";
        private const string ClearListTitle = "Warning";
        private const string SortConfirmationMessage = "Do you really want to sort the tags alphabetically? Your current order will be lost.";
        private const string SortConfirmationTitle = "Warning";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public TagsPanelSettingsPanel(string tagName)
        {
            InitializeComponent();
            InitializeToolTip();
            SendMessage(TxtNewTagInput.Handle, EM_SETCUEBANNER, 0, EnterTagMessage);
            tagsStorage = SettingsStorage.GetTagsStorage(tagName);
            UpdateTags();
            UpdateSortOption();
            AttachEventHandlers(); // this must be at the very end to suppress the events
            TxtNewTagInput.Focus(); // Set focus to the textbox
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
            toolTip.SetToolTip(cbEnableAlphabeticalTagSort, TagSortToolTip);
        }

        private void SetUpDownButtonsState(bool enabled)
        {
            btnTagUp.Enabled = enabled;
            btnTagDown.Enabled = enabled;
        }

        private void ShowMessageBox(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, buttons, icon);
        }

        private void TxtNewTagInput_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtNewTagInput.Text))
            {
                TxtNewTagInput.Text = EnterTagMessage;
                TxtNewTagInput.ForeColor = SystemColors.GrayText;
            }
        }

        private void TxtNewTagInput_Enter(object sender, EventArgs e)
        {
            if (TxtNewTagInput.Text == EnterTagMessage)
            {
                TxtNewTagInput.Text = string.Empty;
                TxtNewTagInput.ForeColor = SystemColors.WindowText;
            }
        }

        public void SetUpPanelForFirstUse()
        {
            lstTags.SelectedIndex = lstTags.Items.Count > 0 ? 0 : -1;
            SetUpDownButtonsState(!tagsStorage.Sorted);
        }

        private void UpdateSortOption()
        {
            bool isSorted = tagsStorage.Sorted;
            cbEnableAlphabeticalTagSort.Checked = isSorted;
            lstTags.Sorted = isSorted;
            SetUpDownButtonsState(!isSorted);
        }

        private void AttachEventHandlers()
        {
            lstTags.KeyDown += KeyEventHandler;
            TxtNewTagInput.KeyDown += KeyEventHandler;

            cbEnableAlphabeticalTagSort.CheckedChanged += CbEnableTagSort_CheckedChanged;
        }

        private void KeyEventHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender == TxtNewTagInput)
                {
                    e.SuppressKeyPress = true;
                    AddNewTagToList();
                    e.Handled = true;
                }
                else if (sender == lstTags)
                {
                    e.SuppressKeyPress = true;
                    RemoveSelectedTagFromList();
                    e.Handled = true;
                }
            }
        }

        private void CbEnableTagSort_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Checked)
            {
                ShowConfirmationDialogToSort();
                SetUpDownButtonsState(false);
            }
            else
            {
                SetUpDownButtonsState(true);
                tagsStorage.Sorted = false;
                lstTags.Sorted = false;
            }
        }

        public bool IsSortEnabled() => cbEnableAlphabeticalTagSort.Checked;

        public void UpdateTags()
        {
            var tagsDict = tagsStorage.GetTags();
            var tags = tagsDict.Keys.ToList();

            if (IsSortEnabled())
            {
                tags = tags.OrderBy(tag => tag).ToList();
            }

            lstTags.Items.Clear(); // Clear the existing items

            // Add the tags in the user-defined order
            foreach (var tag in tags)
            {
                lstTags.Items.Add(tag);
            }
        }

        public void AddNewTagToList()
        {
            var newTag = TxtNewTagInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(newTag) || tagsStorage.TagList.ContainsKey(newTag))
            {
                ShowMessageBox(string.IsNullOrWhiteSpace(newTag) ? "Tag cannot be empty." : DuplicateTagMessage, DuplicateTagTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            tagsStorage.TagList.Add(newTag, tagsStorage.TagList.Count);
            lstTags.Items.Add(newTag);
            TxtNewTagInput.Text = string.Empty;
        }

        public void RemoveSelectedTagFromList()
        {
            if (lstTags.SelectedIndex == -1 || lstTags.Items.Count == 0)
            {
                return;
            }

            var selectedItems = lstTags.SelectedItems.Cast<string>().ToList();

            int newIndex = lstTags.SelectedIndex - selectedItems.Count;

            foreach (var selectedItem in selectedItems)
            {
                int itemIndex = lstTags.Items.IndexOf(selectedItem);
                lstTags.Items.Remove(selectedItem);
                tagsStorage.TagList.Remove(selectedItem);

                if (itemIndex < newIndex)
                {
                    newIndex--;
                }
            }

            if (lstTags.Items.Count > 0)
            {
                newIndex = Math.Max(0, newIndex);
                lstTags.SelectedIndex = newIndex;
            }
        }

        public void ClearTagsListInSettings()
        {
            lstTags.Items.Clear();
            tagsStorage.Clear();
        }

        public void ImportTagsFromCsv()
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;

                openFileDialog1.Title = Messages.CsvDialogTitle;
                openFileDialog1.Filter = CsvFileFilter;
                openFileDialog1.DefaultExt = CsvDefaultExt;
                openFileDialog1.Multiselect = false;

                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var importCsvFilename = openFileDialog1.FileName;
                    if (string.IsNullOrEmpty(importCsvFilename))
                    {
                        return;
                    }

                    var dialogResult = MessageBox.Show(CsvConfirmationMessage, CsvConfirmationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        var lines = File.ReadAllLines(importCsvFilename);
                        var importedTags = new HashSet<string>();

                        foreach (var line in lines)
                        {
                            var values = line.Split(';');
                            foreach (var value in values)
                            {
                                var importTag = value.Trim();
                                if (!string.IsNullOrEmpty(importTag))
                                {
                                    importedTags.Add(importTag);
                                }
                            }
                        }

                        if (importedTags.Count > 0)
                        {
                            foreach (var tag in importedTags)
                            {
                                if (!tagsStorage.TagList.ContainsKey(tag))
                                {
                                    tagsStorage.TagList.Add(tag, tagsStorage.TagList.Count);
                                    lstTags.Items.Add(tag);
                                }
                            }
                            ShowMessageBox($"{importedTags.Count} Tags imported successfully.", Messages.CsvDialogTitle);

                        }
                        else
                        {
                            ShowMessageBox("Not tags found to import.", Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        ShowMessageBox(CsvImportCancelMessage, Messages.CsvDialogTitle);
                    }
                }
            }
        }

        public void ExportTagsToCsv()
        {
            using (var saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.CheckFileExists = false;
                saveFileDialog1.Title = Messages.CsvDialogTitle;
                saveFileDialog1.Filter = CsvFileFilter;
                saveFileDialog1.DefaultExt = CsvDefaultExt;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var exportCSVFilename = saveFileDialog1.FileName;

                    using (var csvWriter = new StreamWriter(exportCSVFilename))
                    {
                        foreach (var tag in lstTags.Items.Cast<string>())
                        {
                            csvWriter.WriteLine(tag);
                        }
                    }

                    ShowMessageBox(CsvExportSuccessMessage, Messages.CsvDialogTitle);
                }
            }
        }

        private void BtnAddTag_Click(object sender, EventArgs e)
        {
            AddNewTagToList();
        }

        private void BtnRemTag_Click(object sender, EventArgs e)
        {
            RemoveSelectedTagFromList();
        }

        private void BtnImportCsv_Click(object sender, EventArgs e)
        {
            ImportTagsFromCsv();
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            ExportTagsToCsv();
        }

        private void BtnClearTagSettings_Click(object sender, EventArgs e)
        {
            if (lstTags.Items.Count != 0)
            {
                ShowDialogToClearList();
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

        public void MoveUp()
        {
            MoveItem(-1);
        }

        public void MoveDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            // Checking selected item
            if (lstTags.SelectedItem == null || lstTags.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = lstTags.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= lstTags.Items.Count)
                return; // Index out of range - nothing to do

            // Removing removable element
            object selected = lstTags.SelectedItem;
            lstTags.Items.RemoveAt(lstTags.SelectedIndex);

            // Insert it in new position
            lstTags.Items.Insert(newIndex, selected);

            // Restore selection
            lstTags.SetSelected(newIndex, true);

            // Put the selected item to a new position
            tagsStorage.SwapElement(selected.ToString(), newIndex);
        }

        public void SortAlphabetically()
        {
            SetUpDownButtonsState(false);
            tagsStorage.Sort();
            var tags = tagsStorage.GetTags().Keys.ToList();
            tags.Sort();
            lstTags.BeginUpdate(); // Suspend drawing of the ListBox
            lstTags.Items.Clear();
            lstTags.Items.AddRange(tags.ToArray());
            lstTags.EndUpdate(); // Resume drawing of the ListBox
        }

        private void ShowConfirmationDialogToSort()
        {
            DialogResult dialogResult = MessageBox.Show(SortConfirmationMessage, SortConfirmationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            bool sort = dialogResult == DialogResult.Yes;
            SortAlphabetically();
            tagsStorage.Sorted = sort;
            lstTags.Sorted = sort;
            cbEnableAlphabeticalTagSort.Checked = sort;
        }

        private void ShowDialogForDuplicate()
        {
            MessageBox.Show(DuplicateTagMessage, DuplicateTagTitle, MessageBoxButtons.OK);
        }

        private void ShowDialogToClearList()
        {
            DialogResult dialogResult = MessageBox.Show(ClearListMessage, ClearListTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                ClearTagsListInSettings();
            }
        }
    }
}