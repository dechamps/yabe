namespace LISTAnalog_Values
{
    partial class AnalogValues
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.Devicename = new System.Windows.Forms.Label();
            this.ListAnalogValues = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnExport = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ListAnalogValues)).BeginInit();
            this.SuspendLayout();
            // 
            // Devicename
            // 
            this.Devicename.AutoSize = true;
            this.Devicename.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.Devicename.Location = new System.Drawing.Point(12, 10);
            this.Devicename.Name = "Devicename";
            this.Devicename.Size = new System.Drawing.Size(46, 17);
            this.Devicename.TabIndex = 1;
            this.Devicename.Text = "label1";
            // 
            // ListAnalogValues
            // 
            this.ListAnalogValues.AllowUserToAddRows = false;
            this.ListAnalogValues.AllowUserToDeleteRows = false;
            this.ListAnalogValues.AllowUserToResizeRows = false;
            this.ListAnalogValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListAnalogValues.BackgroundColor = System.Drawing.SystemColors.Control;
            this.ListAnalogValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ListAnalogValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6});
            this.ListAnalogValues.EnableHeadersVisualStyles = false;
            this.ListAnalogValues.Location = new System.Drawing.Point(12, 35);
            this.ListAnalogValues.Name = "ListAnalogValues";
            this.ListAnalogValues.ReadOnly = true;
            this.ListAnalogValues.RowTemplate.Height = 24;
            this.ListAnalogValues.Size = new System.Drawing.Size(1319, 582);
            this.ListAnalogValues.TabIndex = 6;
            this.ListAnalogValues.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ListAnalogValues_CellContentClick);
            this.ListAnalogValues.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ListAnalogValues_CellFormatting);
            this.ListAnalogValues.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.ListAnalogValues_RowPrePaint);
            this.ListAnalogValues.SelectionChanged += new System.EventHandler(this.ListAnalogValues_SectionChanged);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column1.HeaderText = "Object Name";
            this.Column1.MinimumWidth = 125;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 125;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column2.HeaderText = "Object Type";
            this.Column2.MinimumWidth = 125;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 125;
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column3.HeaderText = "Instance Number";
            this.Column3.MinimumWidth = 158;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 158;
            // 
            // Column4
            // 
            this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column4.HeaderText = "Description";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 125;
            // 
            // Column5
            // 
            this.Column5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column5.HeaderText = "Present Value";
            this.Column5.MinimumWidth = 145;
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Width = 145;
            // 
            // Column6
            // 
            this.Column6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column6.HeaderText = "Relinquish Default";
            this.Column6.MinimumWidth = 190;
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            this.Column6.Width = 190;
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.btnExport.Location = new System.Drawing.Point(286, 5);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(100, 27);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "Export csv";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // AnalogValues
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1343, 630);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.ListAnalogValues);
            this.Controls.Add(this.Devicename);
            this.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.Name = "AnalogValues";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "List of Analog Values";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Analog_Values_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ListAnalogValues)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label Devicename;
        private System.Windows.Forms.DataGridView ListAnalogValues;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
    }
}