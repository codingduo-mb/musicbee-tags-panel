using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class TagListSelectorForm : Form
    {
        private readonly IDictionary<MetaDataType, string> availableMetaDataTypes;

        private static readonly HashSet<MetaDataType> _blacklistedMetaDataTypes = new HashSet<MetaDataType>
            {
                MetaDataType.Artwork,
                MetaDataType.DiscNo,
                MetaDataType.DiscCount,
                MetaDataType.Encoder,
                MetaDataType.GenreCategory,
                MetaDataType.HasLyrics,
                MetaDataType.Lyrics,
                MetaDataType.TrackCount,
                MetaDataType.Rating,
                MetaDataType.RatingAlbum,
                MetaDataType.RatingLove
            };

        public TagListSelectorForm(IEnumerable<string> usedTags)
        {
            InitializeComponent();
            KeyPreview = true; // Enable form to receive key events first

            BtnComboBoxAddMetaDataType.DialogResult = DialogResult.OK;
            BtnComboBoxMetaDataTypCancel.DialogResult = DialogResult.Cancel;
            availableMetaDataTypes = GetAvailableMetaDataTypes(usedTags);
            ComboBoxTagSelect.DataSource = new BindingSource(availableMetaDataTypes.Values, null);
            ComboBoxTagSelect.Validating += ComboBoxTagSelect_Validating;

            KeyDown += TagListSelectorForm_KeyDown;
        }

        private void ComboBoxTagSelect_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is ComboBox comboBox && !availableMetaDataTypes.Values.Contains(comboBox.Text))
            {
                MessageBox.Show($" { Messages.ComboBoxTagSelectNotValidMessage}, { Messages.ComboBoxTagSelectNotValidTitle}, MessageBoxButtons.OK, MessageBoxIcon.Warning");
                e.Cancel = true; // Prevents the focus from leaving the ComboBox if the validation fails
            }
        }

        private static IDictionary<MetaDataType, string> GetAvailableMetaDataTypes(IEnumerable<string> usedTags)
        {
            var allMetaDataTypes = Enum.GetValues(typeof(MetaDataType))
                .Cast<MetaDataType>()
                .Except(_blacklistedMetaDataTypes)
                .ToDictionary(t => t, t => t.ToString("g"));

            var usedMetaDataTypeEnums = usedTags
                .Select(tag => Enum.TryParse(tag, out MetaDataType type) ? type : (MetaDataType?)null)
                .Where(t => t.HasValue)
                .Select(t => t.Value);

            return allMetaDataTypes
                .Where(kvp => !usedMetaDataTypeEnums.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public string GetSelectedMetaDataType()
        {
            if (ComboBoxTagSelect.SelectedItem is string selectedValue)
            {
                var selectedKey = availableMetaDataTypes.FirstOrDefault(kvp => kvp.Value == selectedValue).Key;
                return selectedKey.ToString();
            }
            return null;
        }

        private void TagListSelectorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnComboBoxAddMetaDataType.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnComboBoxMetaDataTypCancel.PerformClick();
            }
        }
    }
}