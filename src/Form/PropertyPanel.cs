using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    public partial class PropertyPanel : UserControl, IControlContainer
    {
        IControlContainer IControlContained.ShouldNestInContainer { get; set; }
        ControlCollection IControlContainer.ContainedControls => flowPanel.Controls;

        public Dictionary<string, IPropertyControl> AllPropertyControls = new Dictionary<string, IPropertyControl>();
        public bool HasInitializedPropertyControls_Global;

        public List<EdNode_Base> SelectedNodes = new List<EdNode_Base>();
        public List<PropNotification_Base> CurrentNotifs = new List<PropNotification_Base>();
        public List<IPropertyControl> CurrentPropertyControls = new List<IPropertyControl>();

        public ToolTip PropsToolTip => propsToolTip;

        public PropertyPanel()
        {
            InitializeComponent();

            InitGlobalControls();
        }

        public void InitGlobalControls()
        {
            if (HasInitializedPropertyControls_Global) return;

            // ...

            HasInitializedPropertyControls_Global = true;
        }

        public void InsertControl(IPropertyControl c)
        {
            try
            {
                AllPropertyControls.Add(c.Identifier, c);
                c.ShouldNestInContainer = this;  // May get overridden afterwards, but for now, this is the default.
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Attempted to add control with identifier {c.Identifier}, but that identifier was already used!");
            }
        }

        public void DropSelection()
        {
            if (SelectedNodes.Count == 0) return;
            SelectedNodes.Clear();
            UpdatePropertiesPanel();
        }

        public void SetSelectedNode(EdNode_Base node)
        {
            if (node == null)
            {
                DropSelection();
                return;
            }
            if (SelectedNodes.Count == 1 && SelectedNodes[0] == node) return;
            SelectedNodes.Clear();
            SelectedNodes.Add(node);
            UpdatePropertiesPanel();
        }

        public void ForceUpdateProperties()
        {
            UpdatePropertiesPanel();
        }

        public void ClearPropertiesPanel()
        {
            notifPanel.Controls.Clear();
            CurrentNotifs.Clear();

            foreach (IPropertyControl control in CurrentPropertyControls)
            {
                if (control.Prop == null) continue;
                control.Prop.Clear();
            }
            ((IControlContainer)this).DetachAll(recursive: true);
            CurrentPropertyControls.Clear();
        }

        public void UpdatePropertiesPanel()
        {
            flowPanel.SuspendLayout();
            notifPanel.SuspendLayout();
            try
            {
                ClearPropertiesPanel();
            }
            finally
            {
                /*
                 * Yes. Apparently the FlowLayoutPanel needs an update after the previous items were removed.
                 * Without this, the positions at which it tries to place the controls will be seemingly random,
                 * depending on what was previously selected. How comical!
                 */
                flowPanel.ResumeLayout();
                notifPanel.ResumeLayout();
            }

            flowPanel.SuspendLayout();
            notifPanel.SuspendLayout();
            try
            {
                UpdateSelectionText();

                if (SelectedNodes.Count != 0)
                {
                    PopulatePropertyControlList();
                    PopulateNotificationList();

                    AttachAllNotifications();
                    AttachAllPropertyControls();
                }
            }
            finally
            {
                flowPanel.ResumeLayout();
                notifPanel.ResumeLayout();
                // Gotta perform the layout again, since for some reason horizontal scrollbars can appear
                // despite everything fitting horizontally.
                flowPanel.PerformLayout();
            }
        }

        public void UpdateSelectionText()
        {
            if (SelectedNodes.Count == 0)
            {
                groupBox.Text = "No node selected.";
                return;
            }

            if (SelectedNodes.Count == 1)
            {
                groupBox.Text = $"Selected: {SelectedNodes[0].GetPropertyPanelName()}";
                return;
            }

            // TODO: This shows the editor node type when it should be the unreal type. Luckly multiple selection isn't implemented yet.
            groupBox.Text = $"Selected {SelectedNodes.Count} {GetCommonParentType(SelectedNodes)}s.";
        }

        public Type GetCommonParentType(List<EdNode_Base> list)
        {
            if (list.Count == 0) return typeof(object);
            Type commonType = list[0].GetType();
            bool ok;
            do
            {
                ok = true;
                foreach (EdNode_Base node in list)
                {
                    if (!node.GetType().IsSubclassOf(commonType))
                    {
                        ok = false;
                        commonType = commonType.BaseType;
                        break;
                    }
                }
            }
            while (!ok && commonType != null);
            return commonType;
        }

        public void PopulateNotificationList()
        {
            if (SelectedNodes.Count == 0) return;

            EdNode_Base node = SelectedNodes.Last();
            if (!node.HasInitializedNotifications) return;

            foreach (PropNotification_Base notif in node.Notifications)
            {
                CurrentNotifs.Add(notif);
            }
        }

        public void AttachAllNotifications()
        {
            foreach (PropNotification_Base notif in CurrentNotifs)
            {
                notif.Dock = DockStyle.Bottom;
                notifPanel.Controls.Add(notif);
            }
        }

        public void PopulatePropertyControlList()
        {
            foreach (EdNode_Base node in SelectedNodes)
            {
                node.EnsureControls(this);
                // Save this in the node so properties can access it :/
                node.PropPanel = this;
                if (!node.HasInitializedProperties)
                {
                    node.PreInitProperties();
                    node.HasInitializedProperties = true;
                }
            }

            if (SelectedNodes.Count == 1)
            {
                foreach (EdProp_Base prop in SelectedNodes[0].Properties)
                {
                    if (!(prop.Control is Control)) continue;
                    CurrentPropertyControls.Add(prop.Control);
                    if (prop.Control.Prop != null) prop.Control.Prop.Add(prop);
                    prop.Control.UpdateStateFromProps();
                }
                return;
            }

            // Only show property controls that are present in ALL selected nodes.
            Dictionary<IPropertyControl, uint> controlHits = new Dictionary<IPropertyControl, uint>();

            foreach (EdProp_Base prop in SelectedNodes[0].Properties)
            {
                if (!(prop.Control is Control)) continue;
                controlHits.Add(prop.Control, 1);
                if (prop.Control.Prop != null) prop.Control.Prop.Add(prop);
            }

            foreach (EdNode_Base node in SelectedNodes.Skip(1))
            {
                foreach (EdProp_Base prop in node.Properties)
                {
                    if (!(prop.Control is Control)) continue;
                    if (controlHits.ContainsKey(prop.Control))
                    {
                        controlHits[prop.Control]++;
                        if (prop.Control.Prop != null) prop.Control.Prop.Add(prop);
                    }
                }
            }

            foreach (KeyValuePair<IPropertyControl, uint> controlHit in controlHits)
            {
                if (controlHit.Value == SelectedNodes.Count)
                {
                    CurrentPropertyControls.Add(controlHit.Key);
                    controlHit.Key.UpdateStateFromProps();
                }
                else if (controlHit.Key.Prop != null)
                    // Add the properties to the controls as you go, and clear them later on if they
                    // don't turn out to show up in every node. Kinda inefficient with memory, but eh.
                    controlHit.Key.Prop.Clear();
            }
        }

        public void AttachAllPropertyControls()
        {
#if DEBUG_STOPWATCH
            System.Diagnostics.Stopwatch watch = null;
            try
            {
                watch = System.Diagnostics.Stopwatch.StartNew();
#endif
                List<Control> suspendedLayoutControls = new List<Control>();

                foreach (IControlContained c in CurrentPropertyControls)
                {
                    c.AttachRecursive(suspendedLayoutControls);
                }

                foreach (Control control in suspendedLayoutControls)
                {
                    control.ResumeLayout();
                }
#if DEBUG_STOPWATCH
                watch.Stop();
                EditorForm.Print($"Property attaching elapsed (ticks) {watch.ElapsedTicks}");

                watch = null;
            }
            finally
            {
                if (watch != null) watch.Stop();
            }
#endif
        }
    }
}
