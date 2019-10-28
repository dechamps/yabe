/**************************************************************************
*                           MIT License
* 
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
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yabe;
using System.IO.BACnet;
using System.Diagnostics;
using System.IO.BACnet.Serialize;
using System.IO;



namespace LISTAnalog_Values
{
    public partial class AnalogValues : Form
    {
        public BacnetObjectTypes[] Filter; // Filtering list
        YabeMainDialog yabeFrm;
        BacnetClient client; BacnetAddress adr; BacnetObjectId objId;
        public AnalogValues(YabeMainDialog yabeFrm)
        {
            this.yabeFrm = yabeFrm;
            Icon = yabeFrm.Icon; // gets Yabe Icon
            InitializeComponent();
        }
        private void Analog_Values_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(RunReadAll)); // Leave Windows displaying the form before processing
        }
       // bool IsEmpty = true;
        void RunReadAll()
        {
            Application.UseWaitCursor = true;

            // Gets all elements concerning the selected device into the DeviceTree
            // and optionnaly the object into the AddressSpaceTree treeview
            // return false if objId is not OK (but got the value ANALOG:0 !)
            // BacnetClient & BacnetAddress could be null if nothing is selected into the DeviceTree
            // a lot of Error in the Trace due to Read property not existing, remove listerner, then add it back
            TraceListener trace = Trace.Listeners[1];
            Trace.Listeners.Remove(Trace.Listeners[1].Name);
            try
            {
                yabeFrm.GetObjectLink(out client, out adr, out objId, BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE);
                Devicename.Text = adr.ToString();
                CheckAllObjects(yabeFrm.m_AddressSpaceTree.Nodes);
                //    EmptyList.Visible = IsEmpty;
            }
            catch
            { }
            Trace.Listeners.Add(trace);
            Application.UseWaitCursor = false;
        }
        void CheckAllObjects(TreeNodeCollection tncol)
        {
            foreach (TreeNode tn in tncol) // gets all nodes into the AddressSpaceTree
            {
                Application.DoEvents();
                BacnetObjectId object_id = (BacnetObjectId)tn.Tag;
                if (Filter.Contains(object_id.type)) // Only for some objects
                {
                    String Identifier = null;
                    lock (yabeFrm.DevicesObjectsName) // translate to it's name if already known
                        yabeFrm.DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), object_id), out Identifier);
                    try
                    {
                        IList<BacnetValue> value;
                        bool ret = client.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_PRESENT_VALUE, out value); // with PRESENT_VALUE
                        string Present_Value = value[0].Value.ToString();
                        float PresentValue = float.Parse(Present_Value);
                        ret = client.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_DESCRIPTION, out value); // with Description
                        string Description = value[0].Value.ToString();
                        ret = client.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_RELINQUISH_DEFAULT, out value); // with Relinquish_Default
                        string Relinquish_Default = value[0].Value.ToString();
                        float RelinquishDefault = float.Parse(Relinquish_Default);

                        if (ret)
                        {
                         //   IsEmpty = false;
                            string name = object_id.ToString();
                            if (name.StartsWith("OBJECT_"))
                                name = name.Substring(7);
                            string[] name1 = name.Split(new Char[] { ':' });
                            int InstanceNumber = Int32.Parse(name1[1]);


                            {
                                ListAnalogValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                                ListAnalogValues.EnableHeadersVisualStyles = false;
                                ListAnalogValues.EnableHeadersVisualStyles = false;
                                ListAnalogValues.ColumnHeadersDefaultCellStyle.BackColor = Color.AliceBlue;
                                ListAnalogValues.DefaultCellStyle.BackColor = Color.White;
                                // Create a new row first as it will include the columns you've created at design-time.
                                int rowId = ListAnalogValues.Rows.Add();

                                // Grab the new row!
                                {    DataGridViewRow row = ListAnalogValues.Rows[rowId];
                                    if (Identifier != null)
                                    // Add the data
                                    {
                                        row.Cells["Column1"].Value = Identifier;
                                        row.Cells["Column2"].Value = name1[0];
                                        row.Cells["Column3"].Value = InstanceNumber;
                                        row.Cells["Column4"].Value = Description;
                                        row.Cells["Column5"].Value = PresentValue;
                                        row.Cells["Column6"].Value = RelinquishDefault;
                                    }
                                    else
                                        row.Cells["Column1"].Value = "";
                                    row.Cells["Column2"].Value = name1[0];
                                    row.Cells["Column3"].Value = InstanceNumber;
                                    row.Cells["Column4"].Value = Description;
                                    row.Cells["Column5"].Value = PresentValue;
                                    row.Cells["Column6"].Value = RelinquishDefault;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                if (tn.Nodes != null)   // go deap into the tree
                    CheckAllObjects(tn.Nodes);
            }
        }

        private void EmptyList_Click(object sender, EventArgs e)
        {
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            string filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog1.Filter = filter;
            //Build the CSV file data as a Comma separated string.
            string csv = string.Empty;
            //Add the Header row for CSV file.
            foreach (DataGridViewColumn column in ListAnalogValues.Columns)
            {
                csv += column.HeaderText + ';';
            }
            //Add new line.
            csv += "\r\n";
            //Adding the Rows
            foreach (DataGridViewRow row in ListAnalogValues.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    //Add the Data rows.
                    csv += cell.Value.ToString().Replace(";", ";") + ';';
                }
                //Add new line.
                csv += "\r\n";
            }
            StreamWriter writer = null;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filter = saveFileDialog1.FileName;
                writer = new StreamWriter(filter);
                writer.WriteLine(csv);
                writer.Close();
                MessageBox.Show("Data are exported!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ListAnalogValues_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ListAnalogValues_SectionChanged(object sender, EventArgs e)
        {
            ListAnalogValues.ClearSelection();
        }
        
        private void ListAnalogValues_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
                if (Convert.ToInt32(ListAnalogValues.Rows[e.RowIndex].Cells["Column5"].Value) != Convert.ToInt32(ListAnalogValues.Rows[e.RowIndex].Cells["Column6"].Value))
                {
                    ListAnalogValues.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Beige;
            }
        }

        private void ListAnalogValues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }
    }
}


    // Ende
