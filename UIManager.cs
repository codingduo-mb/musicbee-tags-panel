using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public class UiManager
    {
        // Beispiel für eine Methode, die eine neue TabPage hinzufügt
        public void AddTabPages(TabControl tabControl, string title)
        {
            var tabPage = new TabPage(title);
            tabControl.TabPages.Add(tabPage);
        }

        // Beispiel für eine Methode, die eine TagPage erstellt oder abruft, falls sie bereits existiert
        public TabPage GetOrCreateTagPage(TabControl tabControl, string title)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Text == title)
                {
                    return page;
                }
            }

            var newPage = new TabPage(title);
            tabControl.TabPages.Add(newPage);
            return newPage;
        }

        // Beispiel für eine Methode, die ein TagPanel hinzufügt, wenn es sichtbar sein soll
        public void AddTagPanelIfVisible(TabPage tabPage, Control tagPanel)
        {
            // Logik zur Bestimmung, ob das Panel sichtbar sein soll
            bool shouldBeVisible = true; // Beispielbedingung

            if (shouldBeVisible)
            {
                tabPage.Controls.Add(tagPanel);
            }
        }

        // Weitere Methoden zur Verwaltung der UI-Elemente...
    }
}
