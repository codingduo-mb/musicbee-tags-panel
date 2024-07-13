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
        private const string CsvDialogTitle = "Choose a CSV file";
        private const string CsvImportSuccessMessage = "CSV import successful";
        private const string CsvImportCancelMessage = "CSV import canceled";
        private const string CsvExportSuccessMessage = "Tags exported in CSV";
        private const string CsvConfirmationMessage = "Warning: This will replace all entries of this tag. Do you want to continue with the CSV import?";
        private const string CsvConfirmationTitle = "Confirmation";

        private const string EnterTagMessage = "Please enter a tag";
        private const string TagSortToolTip = "If enabled the Tags are always sorted alphabetically in the tag. Otherwise you can use the up and down buttons to reorder your tag lists.";
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
            MakeOwnModifications(); // this must be at the very end to suppress the events
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

        private void SetUpDownButtonsStateDisabled()
        {
            btnTagUp.Enabled = false;
            btnTagDown.Enabled = false;
        }

        private void SetUpDownButtonsStateEnabled()
        {
            btnTagUp.Enabled = true;
            btnTagDown.Enabled = true;
        }

        private void ShowMessageBox(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, buttons, icon);
        }

        private void TxtNewTagInput_Leave(object sender, EventArgs e)
        {
            if (TxtNewTagInput.Text.Length == 0)
            {
                TxtNewTagInput.Text = EnterTagMessage;
                TxtNewTagInput.ForeColor = SystemColors.GrayText;
            }
        }

        private void TxtNewTagInput_Enter(object sender, EventArgs e)
        {
            if (TxtNewTagInput.Text == EnterTagMessage)
            {
                TxtNewTagInput.Text = "";
                TxtNewTagInput.ForeColor = SystemColors.WindowText;
            }
        }

        public void SetUpPanelForFirstUse()
        {
            if (lstTags.Items.Count != 0)
            {
                lstTags.SelectedIndex = 0;
            }

            if (tagsStorage.Sorted)
            {
                SetUpDownButtonsStateDisabled();
            }
            else
            {
                SetUpDownButtonsStateEnabled();
            }
        }

        private void UpdateSortOption()
        {
            cbEnableAlphabeticalTagSort.Checked = tagsStorage.Sorted;
            lstTags.Sorted = tagsStorage.Sorted;
            if (tagsStorage.Sorted)
            {
                SetUpDownButtonsStateDisabled();
            }
            else
            {
                SetUpDownButtonsStateEnabled();
            }
        }

        private void MakeOwnModifications()
        {
            lstTags.KeyDown += KeyEventHandler;
            TxtNewTagInput.KeyDown += KeyEventHandler;

            cbEnableAlphabeticalTagSort.CheckedChanged += CbEnableTagSort_CheckedChanged;
        }

        private void KeyEventHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && sender == TxtNewTagInput)
            {
                e.SuppressKeyPress = true;
                AddNewTagToList();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete && sender == lstTags)
            {
                e.SuppressKeyPress = true;
                RemoveSelectedTagFromList();
                e.Handled = true;
            }
        }

        private void CbEnableTagSort_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Checked)
            {
                ShowConfirmationDialogToSort();
                SetUpDownButtonsStateDisabled();
            }
            else
            {
                SetUpDownButtonsStateEnabled();
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

            lstTags.Items.AddRange(tags.ToArray());
        }

        public void AddNewTagToList()
        {
            string newTag = TxtNewTagInput.Text.Trim();
            if (string.IsNullOrEmpty(newTag) || newTag == EnterTagMessage)
            {
                return;
            }

            if (lstTags.Items.Contains(newTag))
            {
                ShowMessageBox(DuplicateTagMessage, DuplicateTagTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            tagsStorage.TagList[newTag] = tagsStorage.TagList.Count;
            lstTags.Items.Add(newTag);
            TxtNewTagInput.Text = string.Empty;
        }

        public void RemoveSelectedTagFromList()
        {
            if (lstTags.SelectedIndex == -1 || lstTags.Items.Count == 0)
            {
                return;
            }

            int selectedIndex = lstTags.SelectedIndex; // Store the index of the selected item

            var selectedItems = new List<object>(lstTags.SelectedItems.Cast<object>());

            foreach (var selectedItem in selectedItems)
            {
                lstTags.Items.Remove(selectedItem);
                tagsStorage.TagList.Remove((string)selectedItem);
            }

            // Select the item above the removed item
            if (selectedIndex > 0)
            {
                lstTags.SelectedIndex = selectedIndex - 1;
            }
            else if (lstTags.Items.Count > 0)
            {
                lstTags.SelectedIndex = 0;
            }
        }

        public void ClearTagsListInSettings()
        {
            lstTags.Items.Clear();
            tagsStorage.Clear();
        }

        public void ImportCsv()
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;

                openFileDialog1.Title = CsvDialogTitle;
                openFileDialog1.Filter = CsvFileFilter;
                openFileDialog1.DefaultExt = CsvDefaultExt;
                openFileDialog1.Multiselect = false;

                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string importCsvFilename = openFileDialog1.FileName;
                    if (string.IsNullOrEmpty(importCsvFilename))
                    {
                        return;
                    }

                    DialogResult dialogResult = MessageBox.Show(CsvConfirmationMessage, CsvConfirmationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        string[] lines = File.ReadAllLines(importCsvFilename);
                        HashSet<string> importedTags = new HashSet<string>();

                        foreach (string line in lines)
                        {
                            string[] values = line.Split(';');
                            foreach (string value in values)
                            {
                                string importtag = value.Trim();
                                if (!string.IsNullOrEmpty(importtag) && importedTags.Add(importtag))
                                {
                                    tagsStorage.TagList[importtag] = tagsStorage.TagList.Count;
                                    lstTags.Items.Add(importtag);
                                }
                            }
                        }

                        ShowMessageBox(CsvImportSuccessMessage, CsvDialogTitle);
                    }
                    else
                    {
                        ShowMessageBox(CsvImportCancelMessage, CsvDialogTitle);
                    }
                }
            }
        }

        public void ExportCsv()
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.CheckFileExists = false;
                saveFileDialog1.Title = CsvDialogTitle;
                saveFileDialog1.Filter = CsvFileFilter;
                saveFileDialog1.DefaultExt = CsvDefaultExt;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string exportCSVFilename = saveFileDialog1.FileName;

                    using (StreamWriter csvWriter = new StreamWriter(exportCSVFilename))
                    {
                        foreach (string tag in lstTags.Items.Cast<string>())
                        {
                            csvWriter.WriteLine(tag);
                        }
                    }

                    ShowMessageBox(CsvExportSuccessMessage, CsvDialogTitle);
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
            ImportCsv();
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            ExportCsv();
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
            if (lstTags.Items.Count != 0)
            {
                MoveUp();
            }
        }

        private void BtnMoveTagDownSettings_Click(object sender, EventArgs e)
        {
            if (lstTags.Items.Count != 0)
            {
                MoveDown();
            }
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
            SetUpDownButtonsStateDisabled();
            tagsStorage.Sort();
            lstTags.BeginUpdate(); // Suspend drawing of the ListBox
            lstTags.Items.Clear();

            lstTags.Items.AddRange(tagsStorage.GetTags().Keys.ToArray());

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