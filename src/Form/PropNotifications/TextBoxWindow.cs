using System.Windows.Forms;

namespace UnrealFlagEditor
{
    public partial class TextBoxWindow : Form
    {
        public TextBoxWindow() : this("Message", "Text :)", inEditable: false) { }

        public TextBoxWindow(string inTitle, string inText, bool inEditable = false)
        {
            InitializeComponent();

            textBox.Text = inText;
            textBox.ReadOnly = !inEditable;
        }
    }
}
