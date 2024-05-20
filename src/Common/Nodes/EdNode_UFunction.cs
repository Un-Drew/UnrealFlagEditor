using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UELib;
using UELib.Core;
using Flags = UELib.Flags;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UFunction))]
    public class EdNode_UFunction : EdNode_UStruct
    {
        public override bool RequiresObjectDeserialization => true;

        public enum FunctionType
        {
            Function = 0,
            Event = 1,
            Operator = 2,
            Delegate = 3
        }

        public FunctionType FuncType;

        public BinaryMetaData.BinaryField FuncFlagsBinaryField;
        public ulong FuncFlags;

        public override int SortPriority => EDSRT_UFUNCTION;

        static public ControlDef_Base ControlDef_Func = new ControlDef_UFunction();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_Func.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public ulong GetFunctionFlagsDel()
        {
            return FuncFlags;
        }

        public void SetFunctionFlagsDel(ulong inFlags)
        {
            FuncFlags = inFlags;
        }

        public EdNode_UFunction(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Function";
            SelectedImageKey = "Function";
        }

        public override void InitNode()
        {
            base.InitNode();
            FuncFlagsBinaryField = FindBinaryFieldByName("FunctionFlags");
            if (FuncFlagsBinaryField.Size != 0)
                FuncFlags = (ulong)FuncFlagsBinaryField.Value;

            UpdateFunctionType();
        }

        public override void OnChangeMade(EdProp_Base property)
        {
            base.OnChangeMade(property);
            UpdateFunctionType();
        }

        public FunctionType DetermineFunctionType()
        {
            if ((FuncFlags & (ulong)Flags.FunctionFlags.Delegate) != 0)
            {
                return FunctionType.Delegate;
            }
            else if ((FuncFlags & (ulong)Flags.FunctionFlags.Operator) != 0)
            {
                return FunctionType.Operator;
            }
            else if ((FuncFlags & (ulong)Flags.FunctionFlags.Event) != 0)
            {
                return FunctionType.Event;
            }
            else
            {
                return FunctionType.Function;
            }
        }

        public void UpdateFunctionType()
        {
            FuncType = DetermineFunctionType();

            switch (FuncType)
            {
                case FunctionType.Delegate:
                    ImageKey = "Delegate";
                    break;
                case FunctionType.Operator:
                    ImageKey = "Operator";
                    break;
                case FunctionType.Event:
                    ImageKey = "Event";
                    break;
                case FunctionType.Function:
                    ImageKey = "Function";
                    break;
            }
            SelectedImageKey = ImageKey;
        }

        public override bool SaveChanges()
        {
            if (!base.SaveChanges()) return false;
            WriteBinaryFieldValue_Int(FuncFlagsBinaryField, FuncFlags);
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            ApplyValueToBinaryField(ref FuncFlagsBinaryField, FuncFlags);
        }

        public void AddFunctionFlag(string inIdentifier, ulong flag)
        {
            Properties.Add(new EdProp_FlagUInt64(this, inIdentifier, GetFunctionFlagsDel, SetFunctionFlagsDel, flag));
        }

        public void AddFunctionFlag(string inIdentifier, Flags.FunctionFlags flag)
        {
            AddFunctionFlag(inIdentifier, (ulong)flag);
        }

        public override void InitializeObjectProperties()
        {
            if (FuncFlagsBinaryField.Size != 0)
            {
                ////////////////////////////////////////////
                // General
                ////////////////////////////////////////////
                AddHeaderProperty("Function.FunctionFlags.Header.General");

                AddFunctionFlag("Function.FunctionFlags.Final", Flags.FunctionFlags.Final);
                AddFunctionFlag("Function.FunctionFlags.Defined", Flags.FunctionFlags.Defined);
                AddFunctionFlag("Function.FunctionFlags.Singular", Flags.FunctionFlags.Singular);
                AddFunctionFlag("Function.FunctionFlags.Exec", Flags.FunctionFlags.Exec);
                AddFunctionFlag("Function.FunctionFlags.Static", Flags.FunctionFlags.Static);
                AddFunctionFlag("Function.FunctionFlags.Invariant", Flags.FunctionFlags.Invariant);
                AddFunctionFlag("Function.FunctionFlags.Public", Flags.FunctionFlags.Public);
                AddFunctionFlag("Function.FunctionFlags.Private", Flags.FunctionFlags.Private);
                AddFunctionFlag("Function.FunctionFlags.Protected", Flags.FunctionFlags.Protected);
                AddFunctionFlag("Function.FunctionFlags.Delegate", Flags.FunctionFlags.Delegate);
                // TODO: What does this flag mean in UE2? I don't currently have a game where this is ever present,
                //       but UELib.UnrealFlags.cs alludes to something being there, in SOME game...
                if (OwnerObject.Package.Version >= 655 /* UnrealPackage.VDLLBIND */)
                    AddFunctionFlag("Function.FunctionFlags.DLLImport", Flags.FunctionFlags.DLLImport);
                if (OwnerObject.Package.Build != UnrealPackage.GameBuild.BuildName.AHIT)  // These K2 flags overlap with AHIT's flags.
                {
                    AddFunctionFlag("Function.FunctionFlags.K2Call",     Flags.FunctionFlags.K2Call);
                    AddFunctionFlag("Function.FunctionFlags.K2Override", Flags.FunctionFlags.K2Override);
                    AddFunctionFlag("Function.FunctionFlags.K2Pure",     Flags.FunctionFlags.K2Pure);
                }

                ////////////////////////////////////////////
                // Replication
                ////////////////////////////////////////////
                AddHeaderProperty("Function.FunctionFlags.Header.Replication");

                AddFunctionFlag("Function.FunctionFlags.Net", Flags.FunctionFlags.Net);
                AddFunctionFlag("Function.FunctionFlags.NetReliable", Flags.FunctionFlags.NetReliable);
                AddFunctionFlag("Function.FunctionFlags.Simulated", Flags.FunctionFlags.Simulated);
                AddFunctionFlag("Function.FunctionFlags.NetServer", Flags.FunctionFlags.NetServer);
                AddFunctionFlag("Function.FunctionFlags.NetClient", Flags.FunctionFlags.NetClient);

                ////////////////////////////////////////////
                // Native
                ////////////////////////////////////////////
                AddHeaderProperty("Function.FunctionFlags.Header.Native");

                AddFunctionFlag("Function.FunctionFlags.Native", Flags.FunctionFlags.Native);
                AddFunctionFlag("Function.FunctionFlags.Iterator", Flags.FunctionFlags.Iterator);
                AddFunctionFlag("Function.FunctionFlags.Latent", Flags.FunctionFlags.Latent);
                AddFunctionFlag("Function.FunctionFlags.Event", Flags.FunctionFlags.Event);
                if (OwnerObject.Package.Version <= 300)  // Overlaps with OptionalParams, allegedly added in v300.
                    AddFunctionFlag("Function.FunctionFlags.NoExport", Flags.FunctionFlags.NoExport);
                AddFunctionFlag("Function.FunctionFlags.Const", Flags.FunctionFlags.Const);

                ////////////////////////////////////////////
                // Operators
                ////////////////////////////////////////////
                AddHeaderProperty("Function.FunctionFlags.Header.Operators");

                AddFunctionFlag("Function.FunctionFlags.Operator", Flags.FunctionFlags.Operator);
                AddFunctionFlag("Function.FunctionFlags.PreOperator", Flags.FunctionFlags.PreOperator);

                ////////////////////////////////////////////
                // Auto-generated
                ////////////////////////////////////////////
                AddHeaderProperty("Function.FunctionFlags.Header.AutoGen");

                if (OwnerObject.Package.Version > 300)
                    AddFunctionFlag("Function.FunctionFlags.OptionalParams", Flags.FunctionFlags.OptionalParameters);
                // TODO: Where on earth does this mean "Interface" ??
                AddFunctionFlag("Function.FunctionFlags.OutParams", 0x00400000U);
                AddFunctionFlag("Function.FunctionFlags.StructDefaults", 0x00800000U);

                ////////////////////////////////////////////
                // Vengeance
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == BuildGeneration.Vengeance)
                {
                    AddHeaderProperty("Function.FunctionFlags.Header.Vengeance");

                    AddFunctionFlag("Function.FunctionFlags.VG_Unk1", Flags.FunctionFlags.VG_Unk1);
                    AddFunctionFlag("Function.FunctionFlags.VG_Overloaded", Flags.FunctionFlags.VG_Overloaded);
                }

                ////////////////////////////////////////////
                // AHIT
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build != UnrealPackage.GameBuild.BuildName.AHIT)
                {
                    AddHeaderProperty("Function.FunctionFlags.Header.AHIT");

                    AddFunctionFlag("Function.FunctionFlags.AHIT_Multicast",   Flags.FunctionFlags.AHIT_Multicast);
                    AddFunctionFlag("Function.FunctionFlags.AHIT_NoOwnerRepl", Flags.FunctionFlags.AHIT_NoOwnerRepl);
                    AddFunctionFlag("Function.FunctionFlags.AHIT_Optional",    Flags.FunctionFlags.AHIT_Optional);
                    AddFunctionFlag("Function.FunctionFlags.AHIT_EditorOnly",  Flags.FunctionFlags.AHIT_EditorOnly);
                }
            }

            base.InitializeObjectProperties();
        }
    }

    public class ControlDef_UFunction : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("Function Flags", Color.FromArgb(220, 200, 255),
                "A set of flags specific to functions - this includes events, operators and delegates.\n" +
                "Mainly consists of the keywords added in the function declaration."
            );

            ////////////////////////////////////////////
            // General
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.General", "General:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Final", "Final",
                "(UE1-3)\n" +
                "This function cannot be overridden by sub-classes.\n"
                + "This is only enforced at compile-time and can be safely modified."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Defined", "Defined",
                "(UE1-3)\n" +
                "This function has a body, so it's not just a declaration."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Singular", "Singular",
                "(UE1-3)\n" +
                "This function will not be called if a singular function is already present on the script stack.\n"
                + "Used for preventing infinite-recursion bugs."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Exec", "Exec",
                "(UE1-3)\n" +
                "This function can be called from the dev console, or via the input system."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Static", "Static",
                "(UE1-3)\n" +
                "This function is static and isn't tied to any specific instance. As such, it doesn't require an instance to call."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Invariant", "Invariant",
                "(UE1?)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Public", "Public",
                "(UE2-3)\n" +
                "This function is public and can be accessed by any other classes."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Private", "Private",
                "(UE2-3)\n" +
                "This function is private and can only be accessed by its owning class.\n"
                + "This is only enforced at compile-time and can be safely modified."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Protected", "Protected",
                "(UE2-3)\n" +
                "This function is protected and can only be accessed by its owning class and its sub-classes.\n"
                + "This is only enforced at compile-time and can be safely modified."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Delegate", "Delegate",
                "(UE2-3)\n" +
                "This function is a delegate, and can be assigned functions to it. Note that, at compile-time, delegates get an\n"
                + "auto-generated property, so editing this after the fact isn't safe, and will probably crash the engine."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.DLLImport", "DLLImport",
                "(UDK 2009)\n" +
                "This function should be linked to the DLL specified in the class's DLLBind."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.K2Call", "K2Call",
                "(UDK 2010)\n" +
                "Allows the function to be referenced and called via Kismet 2 (early deprecated implementation of blueprints)."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.K2Override", "K2Override",
                "(UDK 2010)\n" +
                "Related to Kismet 2. Effect unknown."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.K2Pure", "K2Pure",
                "(UDK 2010)\n" +
                "Related to Kismet 2. Effect unknown."
            ));

            ////////////////////////////////////////////
            // Replication
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.Replication", "Replication:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Net", "Net",
                "(UE1-3)\n" +
                "This function deals with network replication in some way. In earlier engine versions, this is because this function\n"
                + "is present in the replication block. In later engine versions, this is implied from the function specifiers."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.NetReliable", "NetReliable",
                "(UE1-3)\n" +
                "This function is replicated reliably and is guaranteed to reach the other side,\n"
                + "in the correct order relative to the rest of this actor's replication."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Simulated", "Simulated",
                "(UE1-3)\n" +
                "This function may execute on the client-side when an actor is either a simulated proxy or an autonomous proxy."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.NetServer", "NetServer",
                "(UE3)\n" +
                "This function should be replicated to the server if it was called on a replicated actor that is owned by the local client."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.NetClient", "NetClient",
                "(UE3)\n" +
                "This function should be replicated to the client owning the actor if it is called on the server."
            ));

            ////////////////////////////////////////////
            // Native
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.Native", "Native:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Native", "Native",
                "(UE1-3)\n" +
                "This function is defined in C++."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Iterator", "Iterator",
                "(UE1-3)\n" +
                "Only relevant for native functions. This function is an iterator and must be used in a foreach block."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Latent", "Latent",
                "(UE1-3)\n" +
                "Only relevant for native functions. This function is meant to delay state execution and may only be used in state code."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Event", "Event",
                "(UE1-3)\n" +
                "(Only used during compiling) This function is an event, which means the auto-generated C++ headers will include a function\n"
                + "to more easily fire this event from C++."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.NoExport", "NoExport",
                "(???)\n" +
                "(Only used during compiling) This function will not be exported to the C++ header."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Const", "Const",
                "(UE3)\n" +
                "(Only used during compiling) This function will have the const modifier in the C++ header. Seemingly unused in UDK 2010?"
            ));

            ////////////////////////////////////////////
            // Operators
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.Operators", "Operators:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.Operator", "Operator",
                "(UE1-3)\n" +
                "This function is an operator, which means that it should have 1 parameter at least, 2 parameters at most."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.PreOperator", "PreOperator",
                "(UE1-3)\n" +
                "Only relevant for operators. This single-parameter operator is a pre-operator, not a post-operator."
            ));

            ////////////////////////////////////////////
            // Auto-generated
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.AutoGen", "Auto-generated:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.OptionalParams", "OptionalParameters",
                "(UE3)\n" +
                "This function contains optional parameters."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.OutParams", "OutParameters",
                "(UE3)\n" +
                "This function contains out parameters."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.StructDefaults", "StructDefaults",
                "(UE3)\n" +
                "Contains struct properties (e.g. local variables) that need to be initialized to their default value."
            ));

            ////////////////////////////////////////////
            // Vengeance
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.Vengeance", "Vengeance:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.VG_Unk1", "Unknown 1",
                "(Vengeance)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.VG_Overloaded", "Overloaded",
                "(Vengeance)\n" +
                "???"
            ));

            ////////////////////////////////////////////
            // AHIT
            ////////////////////////////////////////////
            AddHeaderControl("Function.FunctionFlags.Header.AHIT", "A Hat in Time:");

            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.AHIT_Multicast", "Multicast",
                "(AHIT)\n" +
                "This function is executed on all clients."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.AHIT_NoOwnerRepl", "NoOwnerReplication",
                "(AHIT)\n" +
                "This function is never replicated on owning client."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.AHIT_Optional", "Optional",
                "(AHIT)\n" +
                "This interface function is optional and can have no implementation."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Function.FunctionFlags.AHIT_EditorOnly", "EditorOnly",
                "(AHIT)\n" +
                "This function must only be called in the editor."
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
