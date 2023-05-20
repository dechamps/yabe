/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2022 Frederic Chaxel <fchaxel@free.fr>
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

namespace GetDevicesNames // namespace should have the same name as the dll file
{
    public class Plugin : IYabePlugin // class should be named Plugin and implementation of IYabePlugin is required
    {
        YabeMainDialog yabeFrm;
	    // yabeFrm is also declared into Yabe Main class
	    // This is usefull for plugin developpement inside Yabe project, before exporting it

        public void Init(YabeMainDialog yabeFrm) // This is the unique mandatory method for a Yabe plugin 
        {
            this.yabeFrm = yabeFrm;

            // Creates the Menu Item
            ToolStripMenuItem MenuItem=new ToolStripMenuItem();
            MenuItem.Text = "Get All Devices Names";
            MenuItem.Click += new EventHandler(MenuItem_Click);

            // Add It as a sub menu (pluginsToolStripMenuItem is the only public Menu member)
            yabeFrm.pluginsToolStripMenuItem.DropDownItems.Add(MenuItem);           
                        
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
                Trace.WriteLine("call to the GetDevicesNames plugin");

                foreach (TreeNode tn in yabeFrm.m_DeviceTree.Nodes[0].Nodes)
                    FullFillDeviceTreeWithNames((BacnetClient)tn.Tag, tn.Nodes);

            }
            catch { }
        }

        void FullFillDeviceTreeWithNames(BacnetClient client, TreeNodeCollection tnc)
        {

            foreach (TreeNode tn in tnc)
                if (tn.ToolTipText == "")
                {
                    try
                    {
                        KeyValuePair<BacnetAddress, uint>? entry = tn.Tag as KeyValuePair<BacnetAddress, uint>?;
                        BacnetAddress adr = entry.Value.Key;
                        uint device_id = entry.Value.Value;

                        IList<BacnetValue> values;
                        BacnetObjectId bobj_id = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id);
                        if (client.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), BacnetPropertyIds.PROP_OBJECT_NAME, out values))
                        {
                            tn.ToolTipText = tn.Text;   // IP or MSTP node id -> in the Tooltip
                            tn.Text = values[0].ToString() + " [" + device_id.ToString() + "] ";  // change @ by the Name  

                            lock (yabeFrm.DevicesObjectsName)
                            {
                                Tuple<String, BacnetObjectId> t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), bobj_id);
                                yabeFrm.DevicesObjectsName.Remove(t);
                                yabeFrm.DevicesObjectsName.Add(t, values[0].ToString());
                            }
                        }
                    }
                    catch { }

                    if (tn.Nodes.Count != 0)
                        FullFillDeviceTreeWithNames(client, tn.Nodes);

                }
        }
        
    }
}
