/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2018 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yabe;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.BACnet;

//
// For simplicity all this code can be tested directly into Yabe project before exporting it
//

namespace ListCOV_Increment // namespace should have the same name as the dll file
{
    public class Plugin : IYabePlugin // class should be named Plugin and implementation of IYabePlugin is required
    {
        YabeMainDialog yabeFrm;
	    // yabeFrm is also declared into Yabe Main class
	    // This is usefull for plugin developpement inside Yabe project, before exporting it

        public void Init(YabeMainDialog yabeFrm) // This is the unique mandatory method for a Yabe plugin 
        {
            this.yabeFrm = yabeFrm;

            // Create the Menu Item
            ToolStripMenuItem MenuItem=new ToolStripMenuItem();
            MenuItem.Text="List COV_Increment";
            // Add It as a sub menu (pluginsToolStripMenuItem is the only public Menu member)
            yabeFrm.pluginsToolStripMenuItem.DropDownItems.Add(MenuItem);

            // Create SubMenu Item
            ToolStripMenuItem  SubMenuItem = new ToolStripMenuItem();
            SubMenuItem.Text = "on all Objects";
            SubMenuItem.Tag = new BacnetObjectTypes[] 
                {BacnetObjectTypes.OBJECT_ANALOG_INPUT,
                 BacnetObjectTypes.OBJECT_ANALOG_VALUE,
                 BacnetObjectTypes.OBJECT_ANALOG_OUTPUT,
                 BacnetObjectTypes.OBJECT_LOOP,
                 BacnetObjectTypes.OBJECT_PULSE_CONVERTER }; // all objects types with COV_Increment property

            SubMenuItem.Click += new EventHandler(MenuItem_Click);
            MenuItem.DropDownItems.Add(SubMenuItem);

            SubMenuItem = new ToolStripMenuItem();
            SubMenuItem.Text = "on Analog Input Objects";
            SubMenuItem.Tag = new BacnetObjectTypes[] {BacnetObjectTypes.OBJECT_ANALOG_INPUT};
            SubMenuItem.Click += new EventHandler(MenuItem_Click);
            MenuItem.DropDownItems.Add(SubMenuItem);

            SubMenuItem = new ToolStripMenuItem();
            SubMenuItem.Text = "on Analog Output Objects";
            SubMenuItem.Tag = new BacnetObjectTypes[] { BacnetObjectTypes.OBJECT_ANALOG_OUTPUT };
            SubMenuItem.Click += new EventHandler(MenuItem_Click);
            MenuItem.DropDownItems.Add(SubMenuItem);

            SubMenuItem = new ToolStripMenuItem();
            SubMenuItem.Text = "on Analog Value Objects";
            SubMenuItem.Tag = new BacnetObjectTypes[] { BacnetObjectTypes.OBJECT_ANALOG_VALUE };
            SubMenuItem.Click += new EventHandler(MenuItem_Click);
            MenuItem.DropDownItems.Add(SubMenuItem);
                        
        }

        // Here only uses the content of the two Treeview into YabeMainDialog object
        // yabeFrm.m_AddressSpaceTree
        // yabeFrm.m_DeviceTree
        // DevicesObjectsName
        //
        // Also Trace.WriteLine can be used

        public void MenuItem_Click(object sender, EventArgs e)
        {
            try // try catch all to avoid Yabe crach
            {
                Trace.WriteLine("call to the ListCOV_Inrement plugin");

                COV_Increment frm = new COV_Increment(yabeFrm);
                frm.Filter = (BacnetObjectTypes[])((ToolStripMenuItem)sender).Tag;

                frm.ShowDialog();
            }
            catch { }
        }

        
    }
}
