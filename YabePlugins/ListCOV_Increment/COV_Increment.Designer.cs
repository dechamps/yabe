namespace ListCOV_Increment
{
    partial class COV_Increment
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
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.Devicename = new System.Windows.Forms.Label();
            this.EmptyList = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView1.Location = new System.Drawing.Point(16, 32);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(782, 689);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // Devicename
            // 
            this.Devicename.AutoSize = true;
            this.Devicename.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Devicename.Location = new System.Drawing.Point(16, 13);
            this.Devicename.Name = "Devicename";
            this.Devicename.Size = new System.Drawing.Size(35, 13);
            this.Devicename.TabIndex = 1;
            this.Devicename.Text = "label1";
            // 
            // EmptyList
            // 
            this.EmptyList.AutoSize = true;
            this.EmptyList.BackColor = System.Drawing.SystemColors.Window;
            this.EmptyList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmptyList.Location = new System.Drawing.Point(280, 61);
            this.EmptyList.Name = "EmptyList";
            this.EmptyList.Size = new System.Drawing.Size(163, 13);
            this.EmptyList.TabIndex = 2;
            this.EmptyList.Text = "No COV_Increments were found.";
            this.EmptyList.Visible = false;
            // 
            // COV_Increment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 738);
            this.Controls.Add(this.EmptyList);
            this.Controls.Add(this.Devicename);
            this.Controls.Add(this.treeView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "COV_Increment";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "List of COV_Increment";
            this.Load += new System.EventHandler(this.COV_Increment_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label Devicename;
        private System.Windows.Forms.Label EmptyList;
    }
}