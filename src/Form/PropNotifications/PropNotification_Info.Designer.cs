namespace UnrealFlagEditor
{
    partial class PropNotification_Info
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropNotification_Info));
            ((System.ComponentModel.ISupportInitialize)(this.icon)).BeginInit();
            this.SuspendLayout();
            // 
            // innerText
            // 
            this.innerText.Text = "Placeholder message...";
            // 
            // icon
            // 
            this.icon.Image = ((System.Drawing.Image)(resources.GetObject("icon.Image")));
            // 
            // PropNotification_Info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.Name = "PropNotification_Info";
            ((System.ComponentModel.ISupportInitialize)(this.icon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
