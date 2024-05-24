using System;
using System.Drawing;
using System.IO;
using UELib;
using UELib.Flags;

namespace UnrealFlagEditor
{
    // TreeNode for the root package (the file itself). Not to be confused with sub-packages (groups), see EdNode_UPackage for that.
    public class EdNode_RootPackage : EdNode_Base
    {
        public UnrealPackage OwnerPackage;
        public uint OldPackageFlags, PackageFlags;
        public long PackageFlagsPos;
        public bool ArePackageFlagsSupported;

        static public ControlDef_Base ControlDef_Pack = new ControlDef_RootPackage();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_Pack.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public uint GetPackageFlagsDel()
        {
            return PackageFlags;
        }

        public void SetPackageFlagsDel(uint inFlags)
        {
            PackageFlags = inFlags;
        }

        public EdNode_RootPackage(EditorEngine eng, UnrealPackage inObject) : base(eng, inObject)
        {
            OwnerPackage = inObject;

            Text = OwnerPackage.PackageName;
            Name = Text.ToUpperInvariant();  // For searches.

            ImageKey = "Package";
            SelectedImageKey = "Package";
        }

        public override void InitNode()
        {
            base.InitNode();
            PackageFlags = (uint)OwnerPackage.Summary.PackageFlags;
            OldPackageFlags = PackageFlags;
        }

        public override string GetPropertyPanelName()
        {
            return "Root Package - " + OwnerPackage.PackageName;
        }

        public override string GetReferencePath()
        {
            return $"Package'{OwnerPackage.PackageName}'";
        }

        // Returned string should be in invariant-uppercase so it's easier to compare.
        public override string GetUnrealTypeString()
        {
            return "PACKAGE";
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Free any other managed objects here
            }

            // Free any unmanaged objects here. 
            OwnerPackage = null;
        }

        // As of 1.5.0, UELib never records the position of anything in the package header, so my hands are tied here.
        static public long FindPackageFlagsBytePosition(UnrealPackage package)
        {
            package.Stream.Position = 4;
            
            package.Stream.Skip(4);  // Skip over the package version and licensee version.
            
            // Some build-specific stuff. Probably won't be able to update this forever though...
            if (package.Build == UnrealPackage.GameBuild.BuildName.Bioshock_Infinite)
                package.Stream.Skip(4);
            if (package.Build == UnrealPackage.GameBuild.BuildName.MKKE)
                package.Stream.Skip(8);
            if (package.Build == BuildGeneration.HMS &&
                package.LicenseeVersion >= 55)
            {
                if (package.LicenseeVersion >= 181) package.Stream.Skip(16);
                package.Stream.Skip(4);
            }

            if (package.Version >= 249/*UnrealPackage.PackageFileSummary.VHeaderSize*/)
                package.Stream.Skip(4);  // Skip over the header size.

            if (package.Version >= 269/*UnrealPackage.PackageFileSummary.VFolderName*/)
                package.Stream.ReadString();  // Skip over the folder name.

            var packageFlagsPos = package.Stream.Position;
            var allegedlyPackageFlags = ReadPackageFlags(package);
            if (allegedlyPackageFlags != (uint)package.Summary.PackageFlags)
            {
                throw new DeserializationException("Could not find PackageFlags at expected position in the header.");
            }
            return packageFlagsPos;
        }

        static public uint ReadPackageFlags(UnrealPackage package)
        {
            return package.Stream.ReadUInt32();
        }

        static public void WritePackageFlags(UnrealPackage package, uint value)
        {
            Write(package.Stream, value);
        }

        public bool IsSavingHeaderSupported()
        {
            // UE4 adds a bunch of stuff, and I'm not even sure if UELib's handling of it is all finished yet.
            // This whole tool isn't really meant for UE4^ anyway.
            return OwnerPackage.Summary.UE4Version == 0;
        }

        public override bool SaveChanges()
        {
            if (ArePackageFlagsSupported)
            {
                OwnerPackage.Stream.Seek(PackageFlagsPos, SeekOrigin.Begin);
                WritePackageFlags(OwnerPackage, PackageFlags);
            }
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            OldPackageFlags = PackageFlags;
        }

        public void GetPackageFlagsEnumMap(out ulong[] enumMap)
        {
            OwnerPackage.Branch.EnumFlagsMap.TryGetValue(typeof(PackageFlag), out enumMap);
        }

        // Note: Arrays are already passed by reference, so no need for the ref keyword here.
        public void ConditionalInsertPackageFlagProp(ulong[] enumMap, PackageFlag flag, string identifier)
        {
            
            if (enumMap != null)
            {
                uint flagMask = (uint)enumMap[(int)flag];
                if (flagMask != 0)
                    Properties.Add(new EdProp_FlagUInt32(this, identifier, GetPackageFlagsDel, SetPackageFlagsDel, flagMask, !ArePackageFlagsSupported));
            }
        }

        

        public override void InitializeOtherProperties()
        {
            ArePackageFlagsSupported = IsSavingHeaderSupported();
            if (ArePackageFlagsSupported)
            {
                try
                {
                    PackageFlagsPos = FindPackageFlagsBytePosition(OwnerPackage);
                }
                catch (Exception e)
                {
                    ArePackageFlagsSupported = false;
                    EnsureNotificationInit();
                    Notifications.Add(new PropNotification_Error("Position lookup exception - Changes to PackageFlags have been disabled.", e.ToString()));
                }
            }
            else
            {
                Notifications.Add(new PropNotification_Warning("Changes to PackageFlags aren't supported for UE4."));
            }

            GetPackageFlagsEnumMap(out ulong[] enumMap);
            if (enumMap != null)
            {
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.AllowDownload,     "Package.PackageFlags.AllowDownload");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.ClientOptional,    "Package.PackageFlags.ClientOptional");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.ServerSideOnly,    "Package.PackageFlags.ServerSideOnly");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.Encrypted,         "Package.PackageFlags.Encrypted");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.Official,          "Package.PackageFlags.Official");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.Cooked,            "Package.PackageFlags.Cooked");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.ContainsMap,       "Package.PackageFlags.ContainsMap");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.ContainsScript,    "Package.PackageFlags.ContainsScript");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.ContainsDebugData, "Package.PackageFlags.ContainsDebugData");
                ConditionalInsertPackageFlagProp(enumMap, PackageFlag.StrippedSource,    "Package.PackageFlags.StrippedSource");
            }

            base.InitializeOtherProperties();
        }
    }

    public class ControlDef_RootPackage : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("Package Flags", Color.LightSalmon,
                "Flags that belong exclusively to the package file. Sub-packages (content groups) won't have this."
            );

            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.AllowDownload", "AllowDownload",
                "(UE1-3)\n" +
                "Whether clients are allowed to download the package from the server."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.ClientOptional", "ClientOptional",
                "(UE1-3)\n" +
                "Whether clients can skip downloading the package but still be able to join the server."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.ServerSideOnly", "ServerSideOnly",
                "(UE1-3)\n" +
                "Only necessary to load on the server."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.Encrypted", "Encrypted",
                "(UE1)\n" +
                "The package is encrypted."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.Official", "Official",
                "(UE2)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.Cooked", "Cooked",
                "(UE3)\n" +
                "Whether the package has been cooked."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.ContainsMap", "ContainsMap",
                "(UE3)\n" +
                "Whether the package contains map data."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.ContainsScript", "ContainsScript",
                "(UE3)\n" +
                "Whether the package contains classes."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.ContainsDebugData", "ContainsDebugData",
                "(UE3)\n" +
                "Whether the package contains debug info i.e. it was built with -Debug."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Package.PackageFlags.StrippedSource", "StrippedSource",
                "(UE3)\n" +
               "Whether the package's TextBuffers have been stripped of their content."
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
