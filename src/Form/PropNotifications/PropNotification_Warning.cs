namespace UnrealFlagEditor
{
    public partial class PropNotification_Warning : PropNotification_Base
    {
        public PropNotification_Warning() : base()
        {
            InitializeComponent();
        }

        public PropNotification_Warning(string inMessage, string inLongerMessage = null) : this()
        {
            InitParams(inMessage, inLongerMessage);
        }

        public override string GetFriendlyTypeName()
        {
            return "Warning";
        }
    }
}
