using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UELib;

namespace UnrealFlagEditor
{
    public partial class EditorForm : Form
    {
        public EditorEngine EdEngine;

        public TreeNode[] NoPackageLoadedNode;

        // Null if the search box is empty.
        // Only set when pressing the enter key/search button, so if the search box is edited but not confirmed,
        // it doesn't cause a discrepancy between what's already attached, and what WILL be lazy-attached.
        public string SearchText;

        //public ToolTip PropsToolTip => propsToolTip;
        public PropertyPanel PropPanel => propertyPanel;

        public string PendingPackagePath;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public EditorForm()
        {
            InitializeComponent();

            InitForm();
        }

        public EditorForm(string package)
        {
            InitializeComponent();

            InitForm();

            if (package != null)
            {
                PendingPackagePath = package;
            }
        }

        public void InitForm()
        {
            Disposed += OnDispose;

            EdEngine = new EditorEngine(EditorEngine.EditorLoadFlags.Form);
            EdEngine.StatsUpdated += OnStatsUpdated;

#if DEBUG
            itemDebug.Visible = true;
#endif
            NoPackageLoadedNode = packageTree.Nodes.Find("noPackageLoaded", false);
            SetSearchPlaceholder();

            UpdateStatText();
        }

        private void EditorForm_Shown(object sender, EventArgs e)
        {
            if (PendingPackagePath != null)
            {
                OpenPackage(PendingPackagePath);
                PendingPackagePath = null;
            }
        }

        static public void Print(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnClicked_Open();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnClicked_Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnClicked_SaveAs();
        }

        // Using this event since it doesn't get spammed when you hold a key.
        private void searchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                PerformSearch();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void packageTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdatePropertySelection();
        }

        private void packageTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (!(e.Node is EdNode_Base)) return;
            EdNode_Base node = (EdNode_Base)e.Node;
            if (!node.ChildrenFullyAttached)
            {
#if DEBUG_STOPWATCH
                System.Diagnostics.Stopwatch watch = null;
                try
                {
                    watch = System.Diagnostics.Stopwatch.StartNew();
#endif
                    ConditionalAttachChildren(node, SearchText, isCollapsed: false, recursive: true);
#if DEBUG_STOPWATCH
                    watch.Stop();
                    Print($"Lazy attach for {node.Text} elapsed (milliseconds) {watch.ElapsedMilliseconds}");

                    watch = null;
                }
                finally
                {
                    if (watch != null) watch.Stop();
                }
#endif
            }
        }

        private void takeOutTheTrashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void iterationPerformanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EdEngine.LoadedPackage == null) return;

            System.Diagnostics.Stopwatch watch = null;

            int i;

            try
            {
                watch = System.Diagnostics.Stopwatch.StartNew();

                i = 0;
                IterTest_Recursive(EdEngine.PackageRoot, ref i);

                watch.Stop();
                Print($"Recursive iteration elapsed (ticks) {watch.ElapsedTicks}");

                watch = System.Diagnostics.Stopwatch.StartNew();

                i = 0;
                IterTest_Linear(ref i);

                watch.Stop();
                Print($"Linear iteration elapsed (ticks) {watch.ElapsedTicks}");

                watch = null;
            }
            finally
            {
                if (watch != null) watch.Stop();
            }
        }

        public void IterTest_Recursive(EdNode_Base node, ref int i)
        {
            if (node.ChildrenEdNodes == null) return;
            foreach (EdNode_Base child in node.ChildrenEdNodes)
            {
                i = (i + 10) % 7;
                i = (i + 10) % 7;
                i = (i + 10) % 7;
                i = (i + 10) % 7;
                IterTest_Recursive(child, ref i);
            }
        }

        public void IterTest_Linear(ref int i)
        {
            foreach (EdNode_Base node in EdEngine.AllNodes)
            {
                i = (i + 10) % 7;
                i = (i + 10) % 7;
                i = (i + 10) % 7;
                i = (i + 10) % 7;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                OnClicked_Open();
                return true;
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                OnClicked_Save();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.S))
            {
                OnClicked_SaveAs();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void OnClicked_Open()
        {
            if (EdEngine.CachedChangeCount != 0)
            {
                if (!AskSaveUnsavedChanges()) return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = FormatUnrealExtensionsAsFilter();
            fileDialog.Title = "Open an Unreal package";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenPackage(fileDialog.FileName);
            }
        }

        public void OnClicked_Save()
        {
            if (EdEngine.LoadedPackage == null)
            {
                ShowNoPackageLoadedBox();
                return;
            }
            SaveOverwriteWithQuestion();
        }

        public void OnClicked_SaveAs()
        {
            if (EdEngine.LoadedPackage == null)
            {
                ShowNoPackageLoadedBox();
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All Files (*.*)|*.*";
            saveFileDialog.Title = "Save package as";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveAs(saveFileDialog.FileName);
            }
        }

        public void SetSearchPlaceholder()
        {
            const int EM_SETCUEBANNER = 5377;

            // I don't know why this isn't a feature in the TextBox class, but whatever.
            SendMessage(searchBox.Handle, EM_SETCUEBANNER, new IntPtr(0), "Search...");
        }

        public string FormatUnrealExtensionsAsFilter()
        {
            List<string> extensionList = UnrealExtensions.FormatUnrealExtensionsAsList();
            return extensionList.Aggregate("All Package Files|", (current, ext) => current + "*" + ext + ";") + "|All Files (*.*)|*.*";
        }

        public void ShowFileErrorBox(string error)
        {
            MessageBox.Show(error, "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowNoPackageLoadedBox()
        {
            MessageBox.Show("Please load an Unreal package first.", "No package loaded!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void CleanupPackageReferences()
        {
            propertyPanel.DropSelection();
            packageTree.Nodes.Clear();
        }

        public void OpenPackage(string path)
        {
            statusLabel.Text = "Loading package...";
            statusLabel.Visible = true;
            packageTree.Visible = false;
            workspaceSplit.Panel1.Update();

            try
            {
                CleanupPackageReferences();
                EdEngine.DisposeLoadedPackage();

                EdEngine.LoadPackage(path);
            }
            catch (Exception e)
            {
                string error = EditorEngine.GetUserFriendlyFileError(path, "package", e);
                if (error != null)
                    ShowFileErrorBox(error);
                else
                    ShowFileErrorBox($"Unexpected exception while trying to open package {path}: {e}");
            }

            if (EdEngine.LoadedPackage != null)
            {
                searchBox.Text = "";
                SearchText = null;
                RefreshNodeDisplay();

                UpdatePropertySelection();
            }
            else
            {
                // Restore the "no package loaded" initial node.
                if (NoPackageLoadedNode.Length != 0 && NoPackageLoadedNode[0].TreeView == null)
                {
                    packageTree.Nodes.Add(NoPackageLoadedNode[0]);
                }
            }

            packageTree.Visible = true;
            statusLabel.Visible = false;
        }

        public void PerformSearch()
        {
            if (EdEngine.LoadedPackage == null) return;

            statusLabel.Text = "Searching...";
            statusLabel.Visible = true;
            packageTree.Visible = false;
            workspaceSplit.Panel1.Update();

            SearchText = (searchBox.Text == "") ? null : searchBox.Text.ToUpperInvariant();
            RefreshNodeDisplay();

            packageTree.Visible = true;
            statusLabel.Visible = false;
        }

        public void RefreshNodeDisplay()
        {
            // TODO: Is this necessary?
            packageTree.SuspendLayout();

#if DEBUG_STOPWATCH
            System.Diagnostics.Stopwatch watch = null;
#endif
            try
            {
                TreeNode previouslySelected = packageTree.SelectedNode;
#if DEBUG_STOPWATCH
                watch = System.Diagnostics.Stopwatch.StartNew();
#endif

                ClearPackageTree();

#if DEBUG_STOPWATCH
                watch.Stop();
                Print($"Clearing elapsed {watch.ElapsedMilliseconds}");

                watch = System.Diagnostics.Stopwatch.StartNew();
#endif

                // isCollapsed: false, because the package root should never be considered collapsed.
                ConditionalAttachChildren(EdEngine.PackageRoot, SearchText, isCollapsed: false, recursive: true);

#if DEBUG_STOPWATCH
                watch.Stop();
                Print($"Attaching elapsed {watch.ElapsedMilliseconds}");

                watch = System.Diagnostics.Stopwatch.StartNew();
#endif

                // After attaching the children nodes, attach the root to display it!
                packageTree.Nodes.Add(EdEngine.PackageRoot);
                EdEngine.PackageRoot.Expand();

#if DEBUG_STOPWATCH
                watch.Stop();
                Print($"Rendering elapsed {watch.ElapsedMilliseconds}");

                watch = null;
#endif

                // Take what was selected and ensure its visibility (IF it's still attached).
                if (previouslySelected != null && previouslySelected.TreeView == packageTree)
                {
                    // Note: This automatically scrolls to the selected node too, which is nice.
                    packageTree.SelectedNode = previouslySelected;
                }
            }
            finally
            {
                packageTree.ResumeLayout();

#if DEBUG_STOPWATCH
                if (watch != null) watch.Stop();
#endif
            }
        }

        public void ClearPackageTree()
        {
            // Ensure the handle and treeview is gone from the entire tree before triggering additional clears.
            // NOTE: This is twice as slow as adding the items to the tree. I tried all sorts of combinations to
            // get around it, to no avail. I'd like to blame Windows for this, but I can't know for sure.
            packageTree.Nodes.Clear();

            ClearRecursive(EdEngine.PackageRoot);
            EdEngine.PackageRoot.IsAttached = false;
        }

        public void ClearRecursive(EdNode_Base inNode)
        {
            EdNode_Base childEdNode;
            foreach (TreeNode child in inNode.Nodes)
            {
                if (!(child is EdNode_Base)) continue;
                childEdNode = (EdNode_Base)child;
                ClearRecursive(childEdNode);
                childEdNode.IsAttached = false;
            }
            inNode.Nodes.Clear();
            inNode.ChildrenFullyAttached = false;
        }

        public bool ConditionalAttachChildren(EdNode_Base node, string search, bool isCollapsed, bool recursive = false)
        {
            if (node.ChildrenEdNodes == null) return false;

            bool attachedAny = false;
            bool matchesSearch;

            foreach (var child in node.ChildrenEdNodes)
            {
                if (child.IsAttached)
                {
                    attachedAny = true;
                    // Also do this for the one (1) node that was attached during the collapsed state.
                    if (!isCollapsed)
                        child.PreInitNode();
                    continue;
                }

                matchesSearch = (search == null || child.Name.Contains(search));

                // If one of its children gets attached, then so should it (regardless of search).
                // If the parent is collapsed, then the children should also be considered as such.
                if ((recursive && ConditionalAttachChildren(child, search, isCollapsed: (isCollapsed || !child.IsExpanded), recursive: true)) || matchesSearch)
                {
                    // Initialize the node in case it wasn't already (a check in EdNode_Base exists to prevent dupe inits).
                    if (!isCollapsed)
                        child.PreInitNode();

                    if (matchesSearch)
                        child.ForeColor = Color.Empty;
                    else
                        child.ForeColor = Color.DarkGray;

                    node.Nodes.Add(child);
                    child.IsAttached = true;
                    attachedAny = true;

                    if (isCollapsed)
                        // Stop here. Only add a single child for collapsed nodes - enough to show the + icon.
                        break;
                }
                else
                {
                    // This will not get attached, so make sure it's not accidently flagged as fully expanded.
                    child.ChildrenFullyAttached = false;
                }
            }

            if (!isCollapsed)
                node.ChildrenFullyAttached = true;

            return attachedAny;
        }

        public void UpdatePropertySelection()
        {
            propertyPanel.SetSelectedNode((packageTree.SelectedNode is EdNode_Base) ? (EdNode_Base)packageTree.SelectedNode : null);
        }

        public void OnStatsUpdated(object sender, EventArgs e)
        {
            UpdateStatText();
        }

        public void UpdateStatText()
        {
            statsLabel.Text = GetCurrentStatText();
        }

        public string GetCurrentStatText()
        {
            if (EdEngine.LoadedPackage == null) return "No package loaded...";
            string output = $"Changes: {EdEngine.CachedChangeCount}";
            return output;
        }

        public string OverwriteNote()
        {
            return "NOTE: This will OVERWRITE the existing file - please make a backup first!";
        }

        public bool AskSaveUnsavedChanges()
        {
            DialogResult dialogResult = MessageBox.Show(
                $"You have unsaved changes for this file:\n{Path.GetFullPath(EdEngine.LoadedPackage.Stream.Name)}\nDo you wish to save them?\n\n{OverwriteNote()}",
                "You have unsaved changes!",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );
            if (dialogResult == DialogResult.Yes)
            {
                SaveOverwrite();
            }
            else if (dialogResult == DialogResult.Cancel)
            {
                return false;
            }
            return true;
        }

        // Returns false if an error occured.
        public bool SaveOverwriteWithQuestion()
        {
            if (EdEngine.CachedChangeCount == 0)
            {
                MessageBox.Show("No change has been made to require saving.", "No changes to save!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            DialogResult dialogResult = MessageBox.Show(
                $"Are you sure you want to save this file?\n{Path.GetFullPath(EdEngine.LoadedPackage.Stream.Name)}\n\n{OverwriteNote()}",
                "Overwrite this file?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (dialogResult == DialogResult.Yes)
            {
                return SaveOverwrite();
            }
            return true;
        }

        // Returns false if an error occured.
        public bool SaveOverwrite()
        {
            try
            {
                EdEngine.SaveOverwrite();
                propertyPanel.ForceUpdateProperties();
                return true;
            }
            catch (Exception e)
            {
                ShowFileErrorBox($"Error trying to save file; exception thrown:\n{e}\n\nSome changes may have already been written, and may be incomplete.");
                return false;
            }
        }

        // Returns false if an error occured.
        public bool SaveAs(string path)
        {
            try
            {
                EdEngine.MigrateLoadedPackageToNewFile(path);
            }
            catch (Exception e)
            {
                ShowFileErrorBox($"Error trying to migrate file; exception thrown:\n{e}");
                return false;
            }
            // NOTE: Even if the migrate method doesn't end up doing anything, due to the path being the same,
            // the OS will probably ask them whether they want to replace the file, so no need to ask again.
            return SaveOverwrite();
        }

        protected void OnDispose(object sender, EventArgs e)
        {
            CleanupPackageReferences();
            EdEngine?.Dispose();
            EdEngine = null;
        }
    }
}
