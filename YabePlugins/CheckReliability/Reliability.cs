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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yabe;
using System.IO.BACnet;
using System.Diagnostics;

namespace CheckReliability
{
    public partial class Reliability : Form
    {
        YabeMainDialog yabeFrm;
        BacnetClient client; BacnetAddress adr; BacnetObjectId objId;

        public Reliability(YabeMainDialog yabeFrm)
        {
            this.yabeFrm = yabeFrm;
            Icon = yabeFrm.Icon; // gets Yabe Icon
            InitializeComponent();
        }

        private void Reliability_Load(object sender, EventArgs e)
        {

            // Gets all elements concerning the selected device into the DeviceTree
            // and optionnaly the object into the AddressSpaceTree treeview
            // return false if objId is not OK (but got the value ANALOG:0 !)
            // BacnetClient & BacnetAddress could be null if nothing is selected into the DeviceTree

            // a lot of Error in the Trace due to Read property not existing, remove listerner, then add it back
            TraceListener trace = Trace.Listeners[1];
            Trace.Listeners.Remove(Trace.Listeners[1].Name);

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();

                yabeFrm.GetObjectLink(out client, out adr, out objId, BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE);
                Devicename.Text = adr.ToString();

                CheckAllObjects(yabeFrm.m_AddressSpaceTree.Nodes);

            }
            catch
            { }

            Trace.Listeners.Add(trace);

            treeView1.ExpandAll();

            Cursor.Current = Cursors.Default;
        }

        void CheckAllObjects(TreeNodeCollection tncol)
        {

            foreach (TreeNode tn in tncol) // gets all nodes into the AddressSpaceTree
            {
                BacnetObjectId object_id = (BacnetObjectId)tn.Tag;

                String Identifier = "";

                lock (yabeFrm.DevicesObjectsName) // translate to it's name if already known
                    yabeFrm.DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), object_id), out Identifier);

                try
                {

                    // read RELIABILITY property on all objects (maybe a test could be done to avoid call without interest)
                    IList<BacnetValue> value;
                    bool ret = client.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_RELIABILITY, out value);

                    if (ret)
                        if ((uint)value[0].Value != 0) // different than RELIABILITY_NO_FAULT_DETECTED
                        {
                            string name = object_id.ToString();
                            if (name.StartsWith("OBJECT_"))
                                name=name.Substring(7);

                            TreeNode N = treeView1.Nodes.Add(name + " / " + Identifier);

                            name = ((BacnetReliability)value[0].Value).ToString();
                            name = name.Replace('_', ' ');
                            name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());

                            N.Nodes.Add(name);
                        }                            
                }
                catch
                {
                }

                if (tn.Nodes != null)   // go deap into the tree
                    CheckAllObjects(tn.Nodes);
            }
        }
    }
}
