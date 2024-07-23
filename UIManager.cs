using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class UIManager
    {
        private const SkinElement DefaultSkinElement = SkinElement.SkinTrackAndArtistPanel;
        private const ElementState DefaultElementState = ElementState.ElementStateDefault;

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly Dictionary<int, Color> _colorCache = new Dictionary<int, Color>();
        private Font _defaultFont;

        private Dictionary<string, TagListPanel> _checklistBoxList;
        private string[] _selectedFileUrls;
        private Action<string[]> _refreshPanelTagsFromFiles;

        public UIManager(MusicBeeApiInterface mbApiInterface, Dictionary<string, TagListPanel> checklistBoxList, string[] selectedFileUrls, Action<string[]> refreshPanelTagsFromFiles)
        {
            _mbApiInterface = mbApiInterface;
            _checklistBoxList = checklistBoxList;
            _selectedFileUrls = selectedFileUrls;
            _refreshPanelTagsFromFiles = refreshPanelTagsFromFiles;
        }

        public void DisplaySettingsPromptLabel(Control panel, TabControl tabControl, string message)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() => DisplaySettingsPromptLabel(panel, tabControl, message)));
                return;
            }

            var emptyPanelText = new Label
            {
                AutoSize = true,
                Location = new Point(14, 30),
                Size = new Size(38, 13),
                TabIndex = 2,
                Text = message
            };

            panel.SuspendLayout();
            panel.Controls.Add(emptyPanelText);
            panel.Controls.SetChildIndex(emptyPanelText, 1);
            panel.Controls.SetChildIndex(tabControl, 0);

            if (tabControl.TabPages.Count == 0)
            {
                tabControl.Visible = false;
            }

            panel.ResumeLayout();
        }

        public void SwitchVisibleTagPanel(string visibleTag)
        {
            if (_checklistBoxList == null || _selectedFileUrls == null)
            {
                return;
            }

            foreach (var checklistBoxPanel in _checklistBoxList.Values)
            {
                if (checklistBoxPanel == null || checklistBoxPanel.Tag == null)
                {
                    continue;
                }

                checklistBoxPanel.Visible = checklistBoxPanel.Tag.ToString() == visibleTag;
            }

            _refreshPanelTagsFromFiles?.Invoke(_selectedFileUrls);
        }

        

        private int GetKeyFromArgs(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            return ((int)skinElement & 0x7F) << 24 | ((int)elementState & 0xFF) << 16 | (int)elementComponent & 0xFFFF;
        }

        public Color GetElementColor(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            var key = GetKeyFromArgs(skinElement, elementState, elementComponent);

            if (!_colorCache.TryGetValue(key, out var color))
            {
                var colorValue = _mbApiInterface.Setting_GetSkinElementColour(skinElement, elementState, elementComponent);
                color = Color.FromArgb(colorValue);
                _colorCache.Add(key, color);
            }

            return color;
        }

        public void ApplySkinStyleToControl(Control formControl)
        {
            if (_defaultFont == null)
            {
                _defaultFont = _mbApiInterface.Setting_GetDefaultFont();
            }

            formControl.Font = _defaultFont;
            formControl.BackColor = GetElementColor(DefaultSkinElement, DefaultElementState, ElementComponent.ComponentBackground);
            formControl.ForeColor = GetElementColor(DefaultSkinElement, DefaultElementState, ElementComponent.ComponentForeground);
        }
    }
}
