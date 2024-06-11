using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class TabPageSelectorForm : Form
    {
        private static readonly HashSet<MetaDataType> Blacklist = new HashSet<MetaDataType>
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

        public TabPageSelectorForm(IEnumerable<string> usedTags)
        {
            InitializeComponent();
            Btn_ComboBoxAddTag.DialogResult = DialogResult.OK;
            Btn_ComboBoxCancel.DialogResult = DialogResult.Cancel;
            comboBoxTagSelect.DataSource = GetUnusedMetaDataTypes(usedTags);
        }

        private static IEnumerable<string> GetUnusedMetaDataTypes(IEnumerable<string> usedTags)
        {
            return Enum.GetValues(typeof(MetaDataType))
                .Cast<MetaDataType>()
                .Except(Blacklist)
                .Select(dataType => dataType.ToString("g"))
                .Except(usedTags)
                .OrderBy(dataType => dataType)
                .ToList();
        }

        public string GetMetaDataType()
        {
            return comboBoxTagSelect.SelectedItem?.ToString();
        }
    }
}
