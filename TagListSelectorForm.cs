using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin

{
    public class MetaDataTypeItem
    {
        public MetaDataType MetaDataType { get; set; }
        public string DisplayValue { get; set; }
    }

    public partial class TagListSelectorForm : Form
    {
        private readonly List<MetaDataTypeItem> metaDataTypeItems;

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

            metaDataTypeItems = GetAvailableMetaDataTypes(usedTags);
            ComboBoxTagSelect.DataSource = metaDataTypeItems;
            ComboBoxTagSelect.DisplayMember = "DisplayValue";
            ComboBoxTagSelect.ValueMember = "MetaDataType";
            ComboBoxTagSelect.Validating += ComboBoxTagSelect_Validating;

            KeyDown += TagListSelectorForm_KeyDown;
        }

        private void ComboBoxTagSelect_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ComboBoxTagSelect.SelectedItem == null)
            {
                MessageBox.Show(
                    Messages.ComboBoxTagSelectNotValidMessage,
                    Messages.ComboBoxTagSelectNotValidTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                e.Cancel = true; // Prevents the focus from leaving the ComboBox if the validation fails
            }
        }

        private static List<MetaDataTypeItem> GetAvailableMetaDataTypes(IEnumerable<string> usedTags)
        {
            var allMetaDataTypes = Enum.GetValues(typeof(MetaDataType))
                .Cast<MetaDataType>()
                .Except(_blacklistedMetaDataTypes);

            var usedMetaDataTypeEnums = usedTags
                .Select(tag => Enum.TryParse(tag, out MetaDataType type) ? type : (MetaDataType?)null)
                .Where(t => t.HasValue)
                .Select(t => t.Value);

            return allMetaDataTypes
                .Except(usedMetaDataTypeEnums)
                .Select(t => new MetaDataTypeItem
                {
                    MetaDataType = t,
                    DisplayValue = t.ToString()
                })
                .ToList();
        }

        public string GetSelectedMetaDataType()
        {
            if (ComboBoxTagSelect.SelectedValue is MetaDataType selectedMetaDataType)
            {
                return selectedMetaDataType.ToString();
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