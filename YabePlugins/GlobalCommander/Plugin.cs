using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yabe;

namespace GlobalCommander
{
    public class Plugin : IYabePlugin
    {
        private YabeMainDialog _yabeFrm;

        public void Init(YabeMainDialog yabeFrm) // This is the unique mandatory method for a Yabe plugin 
        {
            this._yabeFrm = yabeFrm;

            // Creates the Menu Item
            ToolStripMenuItem MenuItem = new ToolStripMenuItem();
            MenuItem.Text = "Global Commander";
            MenuItem.Click += new EventHandler(MenuItem_Click);

            // Add It as a sub menu (pluginsToolStripMenuItem is the only public Menu member)
            yabeFrm.pluginsToolStripMenuItem.DropDownItems.Add(MenuItem);

        }

        public void MenuItem_Click(object sender, EventArgs e)
        {
            try // try catch all to avoid Yabe crach
            {
                Trace.WriteLine("Loading Global Commander window...");
                GlobalCommander frm = new GlobalCommander(this._yabeFrm);
                frm.Show();
            }
            catch
            {
                Cursor.Current = Cursors.Default;
                Trace.Fail("Failed to load the Global Commander window.");
            }
        }
    }
}
