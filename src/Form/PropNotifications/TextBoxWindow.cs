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
