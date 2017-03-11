using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.BACnet;

namespace Yabe
{
    public partial class CreateObject : Form
    {
        public CreateObject()
        {
            InitializeComponent();
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void CreateObject_Load(object sender, EventArgs e)
        {
            for (int i=0;i<=(int)BacnetObjectTypes.OBJECT_BINARY_LIGHTING_OUTPUT;i++)
                ObjectType.Items.Add(Enum.GetName(typeof(BacnetObjectTypes),i));

            ObjectType.SelectedIndex = 0;
            //ObjectType.Text = ObjectType.Items[0].ToString();

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Create_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
