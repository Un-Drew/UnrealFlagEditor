namespace UnrealFlagEditor
{
    public partial class PropNotification_Info : PropNotification_Base
    {
        public PropNotification_Info() : base()
        {
            InitializeComponent();
        }

        public PropNotification_Info(string inMessage, string inLongerMessage = null) : this()
        {
            InitParams(inMessage, inLongerMessage);
        }

        public override string GetFriendlyTypeName()
        {
            return "Message";
        }
    }
}
