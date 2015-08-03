namespace DeepConfigEditor.FileExplorer
{
    partial class FileExplorerControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.explorerTree = new GongSolutions.Shell.ShellTreeView();
            this.explorer = new GongSolutions.Shell.ShellView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.explorerTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.explorer);
            this.splitContainer1.Size = new System.Drawing.Size(150, 150);
            this.splitContainer1.TabIndex = 0;
            // 
            // explorerTree
            // 
            this.explorerTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explorerTree.Location = new System.Drawing.Point(0, 0);
            this.explorerTree.Name = "explorerTree";
            this.explorerTree.ShellView = this.explorer;
            this.explorerTree.ShowHidden = GongSolutions.Shell.ShowHidden.False;
            this.explorerTree.Size = new System.Drawing.Size(50, 150);
            this.explorerTree.TabIndex = 0;
            // 
            // explorer
            // 
            this.explorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explorer.Location = new System.Drawing.Point(0, 0);
            this.explorer.MultiSelect = false;
            this.explorer.Name = "explorer";
            this.explorer.Size = new System.Drawing.Size(96, 150);
            this.explorer.StatusBar = null;
            this.explorer.TabIndex = 0;
            this.explorer.Text = "shellView1";
            this.explorer.View = GongSolutions.Shell.ShellViewStyle.Details;
            // 
            // FileExplorerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FileExplorerControl";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private GongSolutions.Shell.ShellTreeView explorerTree;
        private GongSolutions.Shell.ShellView explorer;
    }
}
