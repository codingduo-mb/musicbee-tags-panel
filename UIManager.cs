using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class UIManager
    {
        private const SkinElement _defaultSkinElement = SkinElement.SkinTrackAndArtistPanel;
        private const ElementState _defaultElementState = ElementState.ElementStateDefault;
        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly Dictionary<int, Color> _colorCache = new Dictionary<int, Color>();
        private Font _defaultFont;

        public UIManager(MusicBeeApiInterface mbApiInterface)
        {
            _mbApiInterface = mbApiInterface;
        }

        public void DisplaySettingsPromptLabel(Control panel, TabControl tabControl, string message)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() => DisplaySettingsPromptLabel(panel, tabControl, message)));
                return;
            }

            Label emptyPanelText = new Label
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



        private int GetKeyFromArgs(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            return ((int)skinElement & 0x7F) << 24 | ((int)elementState & 0xFF) << 16 | (int)elementComponent & 0xFFFF;
        }

        public Color GetElementColor(SkinElement skinElement, ElementState elementState, ElementComponent elementComponent)
        {
            int key = GetKeyFromArgs(skinElement, elementState, elementComponent);

            if (!_colorCache.TryGetValue(key, out var color))
            {
                int colorValue = _mbApiInterface.Setting_GetSkinElementColour(skinElement, elementState, elementComponent);
                color = Color.FromArgb(colorValue);
                _colorCache.Add(key, color);
            }

            return color;
        }

        public void StyleControl(Control formControl)
        {
            if (_defaultFont == null)
            {
                _defaultFont = _mbApiInterface.Setting_GetDefaultFont();
            }

            formControl.Font = _defaultFont;
            formControl.BackColor = GetElementColor(_defaultSkinElement, _defaultElementState, ElementComponent.ComponentBackground);
            formControl.ForeColor = GetElementColor(_defaultSkinElement, _defaultElementState, ElementComponent.ComponentForeground);
        }
    }
}