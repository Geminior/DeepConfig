namespace DeepConfigEditor.FileExplorer
{
    partial class FileSelection
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
            System.Windows.Forms.Label filenameLabel;
            System.Windows.Forms.Label filetypeLabel;
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.shellView = new GongSolutions.Shell.ShellView();
            this.toolbar = new GongSolutions.Shell.FileDialogToolbar();
            this.filterCombo = new GongSolutions.Shell.FileFilterComboBox();
            this.fileNameCombo = new GongSolutions.Shell.FileNameComboBox();
            this.placesToolbar1 = new GongSolutions.Shell.PlacesToolbar();
            filenameLabel = new System.Windows.Forms.Label();
            filetypeLabel = new System.Windows.Forms.Label();
            this.mainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // filenameLabel
            // 
            filenameLabel.AutoSize = true;
            filenameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            filenameLabel.Location = new System.Drawing.Point(99, 366);
            filenameLabel.Name = "filenameLabel";
            filenameLabel.Size = new System.Drawing.Size(66, 27);
            filenameLabel.TabIndex = 0;
            filenameLabel.Text = "File &name:";
            filenameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // filetypeLabel
            // 
            filetypeLabel.AutoSize = true;
            filetypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            filetypeLabel.Location = new System.Drawing.Point(99, 393);
            filetypeLabel.Name = "filetypeLabel";
            filetypeLabel.Size = new System.Drawing.Size(66, 27);
            filetypeLabel.TabIndex = 2;
            filetypeLabel.Text = "Files of &type:";
            filetypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mainLayout
            // 
            this.mainLayout.BackColor = System.Drawing.Color.Transparent;
            this.mainLayout.ColumnCount = 3;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayout.Controls.Add(this.shellView, 1, 1);
            this.mainLayout.Controls.Add(filenameLabel, 1, 2);
            this.mainLayout.Controls.Add(filetypeLabel, 1, 3);
            this.mainLayout.Controls.Add(this.toolbar, 0, 0);
            this.mainLayout.Controls.Add(this.filterCombo, 2, 3);
            this.mainLayout.Controls.Add(this.fileNameCombo, 2, 2);
            this.mainLayout.Controls.Add(this.placesToolbar1, 0, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(4);
            this.mainLayout.RowCount = 4;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.Size = new System.Drawing.Size(647, 424);
            this.mainLayout.TabIndex = 2;
            // 
            // shellView
            // 
            this.mainLayout.SetColumnSpan(this.shellView, 2);
            this.shellView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shellView.Location = new System.Drawing.Point(99, 44);
            this.shellView.MultiSelect = false;
            this.shellView.Name = "shellView";
            this.shellView.Size = new System.Drawing.Size(541, 319);
            this.shellView.StatusBar = null;
            this.shellView.TabIndex = 6;
            this.shellView.Text = "shellView1";
            this.shellView.View = GongSolutions.Shell.ShellViewStyle.List;
            this.shellView.SelectionChanged += new System.EventHandler(this.shellView_SelectionChanged);
            this.shellView.DoubleClick += new System.EventHandler(this.shellView_DoubleClick);
            // 
            // toolbar
            // 
            this.toolbar.AutoSize = true;
            this.mainLayout.SetColumnSpan(this.toolbar, 3);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolbar.Location = new System.Drawing.Point(7, 7);
            this.toolbar.Name = "toolbar";
            this.toolbar.ShellView = this.shellView;
            this.toolbar.Size = new System.Drawing.Size(633, 31);
            this.toolbar.TabIndex = 7;
            // 
            // filterCombo
            // 
            this.filterCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterCombo.Filter = "*.config";
            this.filterCombo.FilterItems = "Config Files (*.config)|*.config|All Files (*.*)|*.*";
            this.filterCombo.FormattingEnabled = true;
            this.filterCombo.Location = new System.Drawing.Point(171, 396);
            this.filterCombo.Name = "filterCombo";
            this.filterCombo.ShellView = this.shellView;
            this.filterCombo.Size = new System.Drawing.Size(469, 21);
            this.filterCombo.TabIndex = 3;
            // 
            // fileNameCombo
            // 
            this.fileNameCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileNameCombo.FilterControl = this.filterCombo;
            this.fileNameCombo.FormattingEnabled = true;
            this.fileNameCombo.Location = new System.Drawing.Point(171, 369);
            this.fileNameCombo.Name = "fileNameCombo";
            this.fileNameCombo.ShellView = this.shellView;
            this.fileNameCombo.Size = new System.Drawing.Size(469, 21);
            this.fileNameCombo.TabIndex = 1;
            this.fileNameCombo.FileNameEntered += new System.EventHandler(this.fileNameCombo_FilenameEntered);
            this.fileNameCombo.TextChanged += new System.EventHandler(this.fileNameCombo_TextChanged);
            // 
            // placesToolbar1
            // 
            this.placesToolbar1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.placesToolbar1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.placesToolbar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.placesToolbar1.Location = new System.Drawing.Point(7, 44);
            this.placesToolbar1.Name = "placesToolbar1";
            this.mainLayout.SetRowSpan(this.placesToolbar1, 3);
            this.placesToolbar1.ShellView = this.shellView;
            this.placesToolbar1.Size = new System.Drawing.Size(86, 373);
            this.placesToolbar1.TabIndex = 8;
            // 
            // FileSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayout);
            this.Name = "FileSelection";
            this.Size = new System.Drawing.Size(647, 424);
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private GongSolutions.Shell.ShellView shellView;
        private GongSolutions.Shell.FileDialogToolbar toolbar;
        private GongSolutions.Shell.FileFilterComboBox filterCombo;
        private GongSolutions.Shell.FileNameComboBox fileNameCombo;
        private GongSolutions.Shell.PlacesToolbar placesToolbar1;
    }
}
