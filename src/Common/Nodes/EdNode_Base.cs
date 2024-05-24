using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UELib;

namespace UnrealFlagEditor
{
    // Node that appears in the editor's package tree.
    public abstract class EdNode_Base : TreeNode, IDisposable
    {
        public EdNode_Base ParentEdNode;
        public List<EdNode_Base> ChildrenEdNodes;
        // The TreeView and Parent properties all try to search through the parent chain for some reason.
        // This is just a little faster.
        public bool IsAttached;
        // For optimization reasons, a node should not attach all its children if it's not expanded.
        public bool ChildrenFullyAttached;
        // Keeps track of whether this node was already initialized. A node will not be initialized
        // until it's visible (all parents in the chain are exposed).
        public bool WasInitialized = false;

        public EditorEngine EdEngine;
        // Initialized on EnsureControls so properties can cache a reference to their controls (if not in headless mode).
        // TODO: Kinda hacky though...
        public PropertyPanel PropPanel;

        public List<EdProp_Base> Properties;
        public List<PropNotification_Base> Notifications;
        public bool HasInitializedProperties = false;
        public bool HasInitializedNotifications = false;
        public string ErrorMessage = null, ErrorLongerMessage = null;

        // Sorting priorities.
        public const int EDSRT_UMETADATA = 10;
        public const int EDSRT_UCLASS = 20;
        public const int EDSRT_UTEXTBUFFER = 30;
        public const int EDSRT_UENUM = 40;
        public const int EDSRT_USTRUCT = 50;
        public const int EDSRT_UCONST = 60;
        public const int EDSRT_UPROPERTY = 70;
        public const int EDSRT_UFUNCTION = 80;
        public const int EDSRT_USTATE = 90;
        public const int EDSRT_UOBJECT = 100;
        public const int EDSRT_DEFAULTS = 110;
        public const int EDSRT_DUMMIES = 500;

        public virtual int SortPriority => EDSRT_UOBJECT;
        public virtual bool SortAlphabetically => false;

        // Should call EnsureNodes on the static instance of ControlDef_Base meant for defining this class's property controls.
        // No such call exists here because EdNode_Base doesn't have any properties by default.
        public virtual void EnsureControls(PropertyPanel propPanel)
        {
            return;
        }

        [Flags]
        // Flags for the types of errors that this node encountered.
        public enum NodeErrorFlags : byte
        {
            // NOTE: As of 1.5.0, if any of the export table entires fail, that results in a package load exception.
            // That can't really be inditidually caught, so no reason to list it here.

            ObjectDeserializationException = 0x02,
            NodeInitException = 0x04,
            Other = 0x08
        }

        public NodeErrorFlags ErrorFlags;

        public EdNode_Base(EditorEngine eng, object inObject)
        {
            EdEngine = eng;
        }

        public void PreInitNode()
        {
            if (WasInitialized) return;
            WasInitialized = true;
            try
            {
                InitNode();
            }
            catch (Exception e)
            {
                ErrorFlags |= NodeErrorFlags.NodeInitException;
                EnsureNotificationInit();
                Notifications.Add(new PropNotification_Error("Node init exception:", e.ToString()));
            }
        }

        public virtual void InitNode()
        {

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free any other managed objects here
                if (HasInitializedProperties)
                {
                    foreach (var prop in Properties)
                    {
                        ((IDisposable)prop).Dispose();
                    }
                    Properties.Clear();
                }
                if (HasInitializedNotifications)
                {
                    foreach (var notif in Notifications)
                    {
                        ((IDisposable)notif).Dispose();
                    }
                    Notifications.Clear();
                }
            }

            // Free any unmanaged objects here. 
            Properties = null;
            Notifications = null;
            HasInitializedProperties = false;
            HasInitializedNotifications = false;

            ParentEdNode = null;
            ChildrenEdNodes = null;
        }

        ~EdNode_Base()
        {
            Dispose(false);
        }

        public void AddEdNodeChild(EdNode_Base node)
        {
            if (ChildrenEdNodes == null)
                ChildrenEdNodes = new List<EdNode_Base>();
            ChildrenEdNodes.Add(node);
            node.ParentEdNode = this;
        }

        public void SortEdNodeChildren(System.Collections.Generic.IComparer<EdNode_Base> comparer, bool recursive = false)
        {
            if (ChildrenEdNodes == null) return;
            ChildrenEdNodes.Sort(comparer);
            if (recursive)
            {
                foreach (var node in ChildrenEdNodes)
                {
                    node.SortEdNodeChildren(comparer, recursive: true);
                }
            }
        }

        public void PreInitProperties()
        {
            Properties = new List<EdProp_Base>();

            InitializeOtherProperties();

            if ((ErrorFlags & NodeErrorFlags.NodeInitException) == 0)
            {
                if ((ErrorFlags & NodeErrorFlags.ObjectDeserializationException) == 0)
                    InitializeObjectProperties();

                InitializeExportTableProperties();
            }
        }

        public void EnsureNotificationInit()
        {
            if (HasInitializedNotifications) return;
            Notifications = new List<PropNotification_Base>();
            HasInitializedNotifications = true;
        }

        public virtual void InitializeExportTableProperties()
        {

        }

        public virtual void InitializeObjectProperties()
        {
            
        }

        public virtual void InitializeOtherProperties()
        {

        }

        public virtual bool SaveChanges()
        {
            return true;
        }

        public virtual void ApplyToDefault()
        {
            if (HasInitializedProperties)
            {
                foreach (EdProp_Base property in Properties)
                {
                    property.ApplyToDefault();
                }
            }
        }

        public virtual void OnChangeMade(EdProp_Base property)
        {
            EdEngine.OnPropertyChangeMade(property);
        }

        public abstract string GetPropertyPanelName();

        public abstract string GetReferencePath();

        // Returned string should be in invariant-uppercase so it's easier to compare.
        public abstract string GetUnrealTypeString();

        // type should be in invariant-uppercase so it's easier to compare.
        public bool IsOfUnrealType(string type)
        {
            return (string.Compare(GetUnrealTypeString(), type) == 0);
        }

        public bool HasAnyChanges()
        {
            return CountChanges() > 0;
        }

        public virtual uint CountChanges()
        {
            if (!HasInitializedProperties) return 0;
            uint sum = 0;
            foreach (EdProp_Base property in Properties)
            {
                if (property.IsChanged())
                    sum++;
            }
            return sum;
        }

        public void AddHeaderProperty(string identifier)
        {
            Properties.Add(new EdProp_Header(this, identifier));
        }

        static public void Write(IUnrealStream stream, byte value)
        {
            stream.Write(value);
        }

        static public void Write(IUnrealStream stream, ushort value)
        {
            if (stream.BigEndianCode) ReverseBytes(value);
            stream.Write(value);
        }

        static public void Write(IUnrealStream stream, uint value)
        {
            if (stream.BigEndianCode) ReverseBytes(value);
            stream.Write(value);
        }

        static public void Write(IUnrealStream stream, ulong value)
        {
            if (stream.BigEndianCode) ReverseBytes(value);
            stream.Write(value);
        }

        // Yuck. I need to do this because apparently unreal packages can be big-endian?? Since when???
        // And that's not being handled by the stream when writing... (as of 1.5.0)
        static public ulong ReverseBytes(ulong value)
        {
            return ((value & 0x00000000000000FFUL) << 56) | ((value & 0x000000000000FF00UL) << 40)
                 | ((value & 0x0000000000FF0000UL) << 24) | ((value & 0x00000000FF000000UL) << 8)
                 | ((value & 0x0000FF0000000000UL) >> 8)  | ((value & 0x0000FF0000000000UL) >> 24)
                 | ((value & 0x00FF000000000000UL) >> 40) | ((value & 0xFF00000000000000UL) >> 56);
        }

        static public uint ReverseBytes(uint value)
        {
            return ((value & 0x000000FFU) << 24) | ((value & 0x0000FF00U) << 8)
                 | ((value & 0x00FF0000U) >> 8)  | ((value & 0xFF000000U) >> 24);
        }

        static public ushort ReverseBytes(ushort value)
        {
            return (ushort)(((value & 0x00FFU) << 8) | ((value & 0xFF00U) >> 8));
        }
    }

    // Defines the property controls for an EdNode class. One of these should exist for each EdNode CLASS - not each EdNode instance!
    public class ControlDef_Base
    {
        public PropertyPanel PropPanel;
        public bool HasInitialized;

        // Temporary var used during the initialization of controls. Controlled by PushNestControl and PopNestControl.
        // Used for nesting any new controls inside of something like a category.
        public IControlContainer CurrentNestControl = null;

        public bool EnsureControls(PropertyPanel propPanel)
        {
            if (HasInitialized) return false;

            PropPanel = propPanel;
            CurrentNestControl = PropPanel;

            InitializeControls();
            HasInitialized = true;

            PropPanel = null;
            CurrentNestControl = null;

            return true;
        }

        public virtual void InitializeControls()
        {

        }
        
        public void InsertControl(IPropertyControl c)
        {
            PropPanel.InsertControl(c);
            c.ShouldNestInContainer = CurrentNestControl;
        }

        public void AddHeaderControl(string identifier, string name)
        {
            InsertControl(new PropControl_Header(PropPanel, identifier, name));
        }

        public void PushNestControl(IControlContainer c)
        {
            c.ShouldNestInContainer = CurrentNestControl;
            CurrentNestControl = c;
        }

        public void PopNestControl()
        {
            CurrentNestControl = CurrentNestControl.ShouldNestInContainer;
        }

        public void OpenCategory(string name, System.Drawing.Color bgColor, string inToolTip = null)
        {
            PushNestControl(new PropContainer_Category(PropPanel, name, bgColor, inToolTip));
        }

        public void CloseCategory()
        {
            PopNestControl();
        }
    };
}
