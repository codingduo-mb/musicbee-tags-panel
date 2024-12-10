﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class UIManager : IDisposable
    {
        private const SkinElement DefaultSkinElement = SkinElement.SkinTrackAndArtistPanel;
        private const ElementState DefaultElementState = ElementState.ElementStateDefault;

        private readonly MusicBeeApiInterface _mbApiInterface;
        private readonly ConcurrentDictionary<int, Color> _colorCache = new ConcurrentDictionary<int, Color>();
        private Font _defaultFont;

        private readonly Dictionary<string, TagListPanel> _checklistBoxList;
        private readonly string[] _selectedFileUrls;
        private readonly Action<string[]> _refreshPanelTagsFromFiles;

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
                Text = message
            };

            panel.SuspendLayout();
            panel.Controls.Add(emptyPanelText);
            panel.Controls.SetChildIndex(emptyPanelText, 1);
            panel.Controls.SetChildIndex(tabControl, 0);

            tabControl.Visible = tabControl.TabPages.Count > 0;

            panel.ResumeLayout();
        }

        public void AddTagsToChecklistBoxPanel(string tagName, Dictionary<string, CheckState> tags)
        {
            if (_checklistBoxList.TryGetValue(tagName, out var checklistBoxPanel) &&
                checklistBoxPanel?.IsDisposed == false &&
                checklistBoxPanel.IsHandleCreated)
            {
                checklistBoxPanel.PopulateChecklistBoxesFromData(tags);
            }
        }

        public void SwitchVisibleTagPanel(string visibleTag)
        {
            if (_checklistBoxList == null || _selectedFileUrls == null)
            {
                return;
            }

            foreach (var checklistBoxPanel in _checklistBoxList.Values)
            {
                if (checklistBoxPanel?.Tag == null)
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

            return _colorCache.GetOrAdd(key, k =>
            {
                var colorValue = _mbApiInterface.Setting_GetSkinElementColour(skinElement, elementState, elementComponent);
                return Color.FromArgb(colorValue);
            });
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _defaultFont?.Dispose();
                _colorCache.Clear();
            }
        }
    }
}