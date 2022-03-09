using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Yabe;
using System.IO.BACnet;

namespace GlobalCommander
{
    public partial class PropertyValueListForm : Form
    {
        public List<GlobalCommander.BacnetPropertyExport> SelectedProperties { get; set; }
        private BindingSource _bindingSource;
        private string _propertyNiceName;

        public PropertyValueListForm(List<GlobalCommander.BacnetPropertyExport> selectedProperties, string propertyNiceName = null)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.SelectedProperties = selectedProperties;
            _propertyNiceName = propertyNiceName;
            _bindingSource = new BindingSource(this, "SelectedProperties");
            _bindingSource.DataSource = SelectedProperties;
            InitializeComponent();
        }

        private void UpdateBinding()
        {
            _bindingSource.DataSource = SelectedProperties;
            _bindingSource.ResetBindings(false);
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void PropertyValueListForm_Shown(object sender, EventArgs e)
        {
            UpdateBinding();
            Cursor.Current = Cursors.Default;
        }

        private void PropertyValueListForm_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            PropertyView.AutoGenerateColumns = false;
            PropertyView.DataSource = _bindingSource;
            colPropID.HeaderText = ((_propertyNiceName == null) ? (colPropID.HeaderText) : (colPropID.HeaderText + " - " + _propertyNiceName));
        }
    }
}
