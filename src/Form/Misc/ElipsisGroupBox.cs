using System;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    // Based on https://stackoverflow.com/a/31344128
    // GroupBox that adds an elipsis (...) at the end of the title if it's too long.
    public class ElipsisGroupBox : GroupBox
    {
        private string _tempText;
        private bool _isElipsisOn;

        public ElipsisGroupBox() : base()
        {
            _tempText = string.Empty;
            _isElipsisOn = false;
        }

        protected void UpdateTextElipsis()
        {
            if (this.AutoElipsis)
            {
                _tempText = base.Text;
                if (_tempText.Length == 0 || base.Width == 0)
                    return;

                int i = _tempText.Length;

                string textToCheck = _isElipsisOn ? _tempText : _tempText + "...";

                _isElipsisOn = false;
                while (TextRenderer.MeasureText(textToCheck, base.Font).Width > (base.Width - 14))
                {
                    _isElipsisOn = true;
                    textToCheck = base.Text.Substring(0, --i) + "...";
                    if (i == 0)
                        break;
                }

                if (_isElipsisOn)
                    _tempText = textToCheck;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateTextElipsis();
        }

        public override string Text
        {
            get
            {
                return this.AutoElipsis && _isElipsisOn ? _tempText : base.Text;
            }
            set
            {
                base.Text = value;
                UpdateTextElipsis();
            }
        }

        public bool AutoElipsis { get; set; }
    }
}
