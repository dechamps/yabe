
namespace GlobalCommander
{
    partial class PropertyValueListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PropertyView = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cmdClose = new System.Windows.Forms.Button();
            this.colDevice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPropID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.PropertyView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PropertyView
            // 
            this.PropertyView.AllowUserToAddRows = false;
            this.PropertyView.AllowUserToDeleteRows = false;
            this.PropertyView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.PropertyView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PropertyView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDevice,
            this.colObject,
            this.colPropID});
            this.PropertyView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyView.Location = new System.Drawing.Point(0, 0);
            this.PropertyView.Name = "PropertyView";
            this.PropertyView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.PropertyView.Size = new System.Drawing.Size(1045, 599);
            this.PropertyView.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.PropertyView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cmdClose);
            this.splitContainer1.Panel2MinSize = 46;
            this.splitContainer1.Size = new System.Drawing.Size(1045, 649);
            this.splitContainer1.SplitterDistance = 599;
            this.splitContainer1.TabIndex = 1;
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClose.Location = new System.Drawing.Point(898, 3);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(144, 40);
            this.cmdClose.TabIndex = 0;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // colDevice
            // 
            this.colDevice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDevice.DataPropertyName = "ParentDeviceName";
            this.colDevice.FillWeight = 25F;
            this.colDevice.HeaderText = "Device";
            this.colDevice.MinimumWidth = 20;
            this.colDevice.Name = "colDevice";
            this.colDevice.ReadOnly = true;
            // 
            // colObject
            // 
            this.colObject.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colObject.DataPropertyName = "ParentPointName";
            this.colObject.FillWeight = 35F;
            this.colObject.HeaderText = "Point";
            this.colObject.MinimumWidth = 20;
            this.colObject.Name = "colObject";
            this.colObject.ReadOnly = true;
            // 
            // colPropID
            // 
            this.colPropID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colPropID.DataPropertyName = "ValueObjectString";
            this.colPropID.FillWeight = 40F;
            this.colPropID.HeaderText = "Value of Selected Property";
            this.colPropID.MinimumWidth = 20;
            this.colPropID.Name = "colPropID";
            // 
            // PropertyValueListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 649);
            this.Controls.Add(this.splitContainer1);
            this.Name = "PropertyValueListForm";
            this.Text = "Property Value List";
            this.Load += new System.EventHandler(this.PropertyValueListForm_Load);
            this.Shown += new System.EventHandler(this.PropertyValueListForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.PropertyView)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView PropertyView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDevice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObject;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPropID;
    }
}