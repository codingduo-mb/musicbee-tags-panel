using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    /// <summary>
    /// Helper class for importing and exporting tags from CSV files.
    /// </summary>
    public class TagsCsvHelper
    {
        private readonly Action<string, string, MessageBoxButtons, MessageBoxIcon> _showMessageBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsCsvHelper"/> class.
        /// </summary>
        /// <param name="showMessageBoxAction">Action to show message boxes.</param>
        public TagsCsvHelper(Action<string, string, MessageBoxButtons, MessageBoxIcon> showMessageBoxAction)
        {
            _showMessageBox = showMessageBoxAction ?? throw new ArgumentNullException(nameof(showMessageBoxAction));
        }

        /// <summary>
        /// Imports tags from a CSV file and adds them to the provided TagsStorage.
        /// </summary>
        /// <param name="tagsStorage">The tags storage to add imported tags to.</param>
        /// <param name="updateControl">Optional action to update UI control after tag import.</param>
        public void ImportTagsFromCsv(TagsStorage tagsStorage, Action<string> addItemToListAction = null)
        {
            if (tagsStorage == null)
                throw new ArgumentNullException(nameof(tagsStorage));

            using (var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = Messages.CsvDialogTitle,
                Filter = Messages.CsvFileFilter,
                DefaultExt = Messages.CsvDefaultExt,
                Multiselect = false,
                RestoreDirectory = true
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var importCsvFilename = openFileDialog.FileName;
                    if (string.IsNullOrEmpty(importCsvFilename))
                    {
                        return;
                    }

                    if (MessageBox.Show(Messages.CsvImportWarningReplaceMessage, Messages.WarningTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
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
                                        addItemToListAction?.Invoke(tag);
                                    }
                                }
                                _showMessageBox($"{importedTags.Count} {Messages.CsvImportTagImportSuccessfulMessage}", Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                _showMessageBox(Messages.CsvImportNoTagsFoundMessage, Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            _showMessageBox($"Error importing tags: {ex.Message}", Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        _showMessageBox(Messages.CsvImportCancelMessage, Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Exports tags to a CSV file.
        /// </summary>
        /// <param name="tags">The tags to export.</param>
        public void ExportTagsToCsv(IEnumerable<string> tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            using (var saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                Title = Messages.CsvDialogTitle,
                Filter = Messages.CsvFileFilter,
                DefaultExt = Messages.CsvDefaultExt,
                RestoreDirectory = true
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var exportCSVFilename = saveFileDialog.FileName;

                    try
                    {
                        using (var csvWriter = new StreamWriter(exportCSVFilename))
                        {
                            foreach (var tag in tags)
                            {
                                csvWriter.WriteLine(tag);
                            }
                        }

                        _showMessageBox($"{Messages.CsvExportSuccessMessage} {exportCSVFilename}", Messages.CsvDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        _showMessageBox($"Error exporting tags: {ex.Message}", Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
