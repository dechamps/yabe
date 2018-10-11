namespace CheckStatusFlags
{
    partial class StatusFlags
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatusFlags));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.Flags_imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.Devicename = new System.Windows.Forms.Label();
            this.EmptyList = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.Flags_imageList1;
            this.treeView1.Location = new System.Drawing.Point(16, 32);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(626, 882);
            this.treeView1.TabIndex = 0;
            // 
            // Flags_imageList1
            // 
            this.Flags_imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Flags_imageList1.ImageStream")));
            this.Flags_imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.Flags_imageList1.Images.SetKeyName(0, "Alarm.png");
            this.Flags_imageList1.Images.SetKeyName(1, "Fault.png");
            this.Flags_imageList1.Images.SetKeyName(2, "Overridden.png");
            this.Flags_imageList1.Images.SetKeyName(3, "Out Of Service.png");
            this.Flags_imageList1.Images.SetKeyName(4, "Info.png");
            this.Flags_imageList1.Images.SetKeyName(5, "DP.png");
            // 
            // Devicename
            // 
            this.Devicename.AutoSize = true;
            this.Devicename.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Devicename.Location = new System.Drawing.Point(16, 13);
            this.Devicename.Name = "Devicename";
            this.Devicename.Size = new System.Drawing.Size(49, 14);
            this.Devicename.TabIndex = 1;
            this.Devicename.Text = "label1";
            // 
            // EmptyList
            // 
            this.EmptyList.AutoSize = true;
            this.EmptyList.BackColor = System.Drawing.SystemColors.Window;
            this.EmptyList.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmptyList.Location = new System.Drawing.Point(256, 139);
            this.EmptyList.Name = "EmptyList";
            this.EmptyList.Size = new System.Drawing.Size(140, 14);
            this.EmptyList.TabIndex = 2;
            this.EmptyList.Text = "All Flags are clean";
            this.EmptyList.Visible = false;
            // 
            // StatusFlags
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 930);
            this.Controls.Add(this.EmptyList);
            this.Controls.Add(this.Devicename);
            this.Controls.Add(this.treeView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StatusFlags";
            this.Text = "StatusFlags";
            this.Load += new System.EventHandler(this.StatusFlags_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label Devicename;
        private System.Windows.Forms.Label EmptyList;
        private System.Windows.Forms.ImageList Flags_imageList1;
    }
}