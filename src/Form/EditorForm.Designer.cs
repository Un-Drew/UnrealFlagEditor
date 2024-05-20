namespace UnrealFlagEditor
{
    partial class EditorForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("No package loaded...");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorForm));
            this.workspaceSplit = new System.Windows.Forms.SplitContainer();
            this.packageTree = new System.Windows.Forms.TreeView();
            this.classesImageList = new System.Windows.Forms.ImageList(this.components);
            this.statusLabel = new System.Windows.Forms.Label();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.propertyPanel = new UnrealFlagEditor.PropertyPanel();
            this.workspacePanel = new System.Windows.Forms.Panel();
            this.statsSplit = new System.Windows.Forms.SplitContainer();
            this.statsBox = new System.Windows.Forms.GroupBox();
            this.statsLabel = new System.Windows.Forms.Label();
            this.formMenuStrip = new System.Windows.Forms.MenuStrip();
            this.itemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.takeOutTheTrashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iterationPerformanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceSplit)).BeginInit();
            this.workspaceSplit.Panel1.SuspendLayout();
            this.workspaceSplit.Panel2.SuspendLayout();
            this.workspaceSplit.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.workspacePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statsSplit)).BeginInit();
            this.statsSplit.Panel1.SuspendLayout();
            this.statsSplit.Panel2.SuspendLayout();
            this.statsSplit.SuspendLayout();
            this.statsBox.SuspendLayout();
            this.formMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // workspaceSplit
            // 
            this.workspaceSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workspaceSplit.Location = new System.Drawing.Point(0, 0);
            this.workspaceSplit.Name = "workspaceSplit";
            // 
            // workspaceSplit.Panel1
            // 
            this.workspaceSplit.Panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.workspaceSplit.Panel1.Controls.Add(this.packageTree);
            this.workspaceSplit.Panel1.Controls.Add(this.statusLabel);
            this.workspaceSplit.Panel1.Controls.Add(this.searchPanel);
            // 
            // workspaceSplit.Panel2
            // 
            this.workspaceSplit.Panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.workspaceSplit.Panel2.Controls.Add(this.propertyPanel);
            this.workspaceSplit.Size = new System.Drawing.Size(876, 390);
            this.workspaceSplit.SplitterDistance = 450;
            this.workspaceSplit.TabIndex = 1;
            // 
            // packageTree
            // 
            this.packageTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageTree.HideSelection = false;
            this.packageTree.ImageKey = "Root";
            this.packageTree.ImageList = this.classesImageList;
            this.packageTree.Location = new System.Drawing.Point(0, 23);
            this.packageTree.Name = "packageTree";
            treeNode1.ImageKey = "Unknown";
            treeNode1.Name = "noPackageLoaded";
            treeNode1.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            treeNode1.SelectedImageKey = "Unknown";
            treeNode1.Text = "No package loaded...";
            this.packageTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.packageTree.SelectedImageKey = "Root";
            this.packageTree.Size = new System.Drawing.Size(450, 367);
            this.packageTree.TabIndex = 1;
            this.packageTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.packageTree_BeforeExpand);
            this.packageTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.packageTree_AfterSelect);
            // 
            // classesImageList
            // 
            this.classesImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("classesImageList.ImageStream")));
            this.classesImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.classesImageList.Images.SetKeyName(0, "Unknown");
            this.classesImageList.Images.SetKeyName(1, "Object");
            this.classesImageList.Images.SetKeyName(2, "Package");
            this.classesImageList.Images.SetKeyName(3, "MetaData");
            this.classesImageList.Images.SetKeyName(4, "Class");
            this.classesImageList.Images.SetKeyName(5, "Enum");
            this.classesImageList.Images.SetKeyName(6, "Struct");
            this.classesImageList.Images.SetKeyName(7, "Function");
            this.classesImageList.Images.SetKeyName(8, "Event");
            this.classesImageList.Images.SetKeyName(9, "Operator");
            this.classesImageList.Images.SetKeyName(10, "Delegate");
            this.classesImageList.Images.SetKeyName(11, "Local");
            this.classesImageList.Images.SetKeyName(12, "Parameter");
            this.classesImageList.Images.SetKeyName(13, "Variable");
            this.classesImageList.Images.SetKeyName(14, "Return");
            this.classesImageList.Images.SetKeyName(15, "Template");
            this.classesImageList.Images.SetKeyName(16, "Const");
            this.classesImageList.Images.SetKeyName(17, "TextBuffer");
            this.classesImageList.Images.SetKeyName(18, "State");
            // 
            // statusLabel
            // 
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(0, 23);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(450, 367);
            this.statusLabel.TabIndex = 4;
            this.statusLabel.Text = "Searching...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statusLabel.Visible = false;
            // 
            // searchPanel
            // 
            this.searchPanel.AutoSize = true;
            this.searchPanel.Controls.Add(this.searchBox);
            this.searchPanel.Controls.Add(this.searchButton);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Location = new System.Drawing.Point(0, 0);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(450, 23);
            this.searchPanel.TabIndex = 3;
            // 
            // searchBox
            // 
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchBox.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchBox.Location = new System.Drawing.Point(0, 0);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(420, 23);
            this.searchBox.TabIndex = 2;
            this.searchBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyUp);
            // 
            // searchButton
            // 
            this.searchButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
            this.searchButton.Location = new System.Drawing.Point(420, 0);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(30, 23);
            this.searchButton.TabIndex = 3;
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // propertyPanel
            // 
            this.propertyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyPanel.Location = new System.Drawing.Point(0, 0);
            this.propertyPanel.Name = "propertyPanel";
            this.propertyPanel.Size = new System.Drawing.Size(422, 390);
            this.propertyPanel.TabIndex = 0;
            // 
            // workspacePanel
            // 
            this.workspacePanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.workspacePanel.Controls.Add(this.statsSplit);
            this.workspacePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workspacePanel.Location = new System.Drawing.Point(0, 24);
            this.workspacePanel.Name = "workspacePanel";
            this.workspacePanel.Padding = new System.Windows.Forms.Padding(4);
            this.workspacePanel.Size = new System.Drawing.Size(884, 437);
            this.workspacePanel.TabIndex = 0;
            // 
            // statsSplit
            // 
            this.statsSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.statsSplit.IsSplitterFixed = true;
            this.statsSplit.Location = new System.Drawing.Point(4, 4);
            this.statsSplit.Name = "statsSplit";
            this.statsSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // statsSplit.Panel1
            // 
            this.statsSplit.Panel1.Controls.Add(this.workspaceSplit);
            // 
            // statsSplit.Panel2
            // 
            this.statsSplit.Panel2.Controls.Add(this.statsBox);
            this.statsSplit.Size = new System.Drawing.Size(876, 429);
            this.statsSplit.SplitterDistance = 390;
            this.statsSplit.TabIndex = 2;
            // 
            // statsBox
            // 
            this.statsBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.statsBox.Controls.Add(this.statsLabel);
            this.statsBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsBox.Location = new System.Drawing.Point(0, 0);
            this.statsBox.Name = "statsBox";
            this.statsBox.Size = new System.Drawing.Size(876, 35);
            this.statsBox.TabIndex = 2;
            this.statsBox.TabStop = false;
            this.statsBox.Text = "Stats";
            // 
            // statsLabel
            // 
            this.statsLabel.AutoSize = true;
            this.statsLabel.Location = new System.Drawing.Point(10, 14);
            this.statsLabel.Name = "statsLabel";
            this.statsLabel.Size = new System.Drawing.Size(49, 13);
            this.statsLabel.TabIndex = 0;
            this.statsLabel.Text = "My stats.";
            // 
            // formMenuStrip
            // 
            this.formMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemFile,
            this.itemDebug});
            this.formMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.formMenuStrip.Name = "formMenuStrip";
            this.formMenuStrip.Size = new System.Drawing.Size(884, 24);
            this.formMenuStrip.TabIndex = 2;
            this.formMenuStrip.Text = "menuStrip1";
            // 
            // itemFile
            // 
            this.itemFile.AccessibleName = "";
            this.itemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.itemFile.Name = "itemFile";
            this.itemFile.Size = new System.Drawing.Size(37, 20);
            this.itemFile.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.saveToolStripMenuItem.Text = "Save (Overwrite)";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+S";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // itemDebug
            // 
            this.itemDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.takeOutTheTrashToolStripMenuItem,
            this.iterationPerformanceToolStripMenuItem});
            this.itemDebug.Name = "itemDebug";
            this.itemDebug.Size = new System.Drawing.Size(54, 20);
            this.itemDebug.Text = "Debug";
            this.itemDebug.Visible = false;
            // 
            // takeOutTheTrashToolStripMenuItem
            // 
            this.takeOutTheTrashToolStripMenuItem.Name = "takeOutTheTrashToolStripMenuItem";
            this.takeOutTheTrashToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.takeOutTheTrashToolStripMenuItem.Text = "Take Out The Trash";
            this.takeOutTheTrashToolStripMenuItem.Click += new System.EventHandler(this.takeOutTheTrashToolStripMenuItem_Click);
            // 
            // iterationPerformanceToolStripMenuItem
            // 
            this.iterationPerformanceToolStripMenuItem.Name = "iterationPerformanceToolStripMenuItem";
            this.iterationPerformanceToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.iterationPerformanceToolStripMenuItem.Text = "Iteration Performance";
            this.iterationPerformanceToolStripMenuItem.Click += new System.EventHandler(this.iterationPerformanceToolStripMenuItem_Click);
            // 
            // EditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 461);
            this.Controls.Add(this.workspacePanel);
            this.Controls.Add(this.formMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.formMenuStrip;
            this.Name = "EditorForm";
            this.Text = "Unreal Flag Editor";
            this.workspaceSplit.Panel1.ResumeLayout(false);
            this.workspaceSplit.Panel1.PerformLayout();
            this.workspaceSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.workspaceSplit)).EndInit();
            this.workspaceSplit.ResumeLayout(false);
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.workspacePanel.ResumeLayout(false);
            this.statsSplit.Panel1.ResumeLayout(false);
            this.statsSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statsSplit)).EndInit();
            this.statsSplit.ResumeLayout(false);
            this.statsBox.ResumeLayout(false);
            this.statsBox.PerformLayout();
            this.formMenuStrip.ResumeLayout(false);
            this.formMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer workspaceSplit;
        private System.Windows.Forms.Panel workspacePanel;
        private System.Windows.Forms.TreeView packageTree;
        private System.Windows.Forms.ImageList classesImageList;
        private System.Windows.Forms.MenuStrip formMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem itemFile;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemDebug;
        private System.Windows.Forms.ToolStripMenuItem takeOutTheTrashToolStripMenuItem;
        private System.Windows.Forms.GroupBox statsBox;
        private System.Windows.Forms.Label statsLabel;
        private System.Windows.Forms.SplitContainer statsSplit;
        private PropertyPanel propertyPanel;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.ToolStripMenuItem iterationPerformanceToolStripMenuItem;
        private System.Windows.Forms.Label statusLabel;
    }
}

