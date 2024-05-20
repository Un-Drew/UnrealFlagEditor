namespace UnrealFlagEditor
{
    partial class TextBoxWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox.Location = new System.Drawing.Point(0, 0);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(584, 461);
            this.textBox.TabIndex = 0;
            this.textBox.Text = "The fitness gram pacer test-";
            // 
            // TextBoxWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.textBox);
            this.Name = "TextBoxWindow";
            this.ShowIcon = false;
            this.Text = "Message";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox;
    }
}