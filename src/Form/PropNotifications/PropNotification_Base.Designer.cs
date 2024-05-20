namespace UnrealFlagEditor
{
    partial class PropNotification_Base
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropNotification_Base));
            this.viewLink = new System.Windows.Forms.LinkLabel();
            this.innerText = new System.Windows.Forms.Label();
            this.icon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.icon)).BeginInit();
            this.SuspendLayout();
            // 
            // viewLink
            // 
            this.viewLink.Dock = System.Windows.Forms.DockStyle.Right;
            this.viewLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.viewLink.Location = new System.Drawing.Point(460, 0);
            this.viewLink.Name = "viewLink";
            this.viewLink.Size = new System.Drawing.Size(40, 30);
            this.viewLink.TabIndex = 0;
            this.viewLink.TabStop = true;
            this.viewLink.Text = "View";
            this.viewLink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.viewLink.Visible = false;
            this.viewLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.viewLink_LinkClicked);
            // 
            // innerText
            // 
            this.innerText.AutoEllipsis = true;
            this.innerText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerText.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.innerText.Location = new System.Drawing.Point(24, 0);
            this.innerText.Name = "innerText";
            this.innerText.Size = new System.Drawing.Size(436, 30);
            this.innerText.TabIndex = 1;
            this.innerText.Text = resources.GetString("innerText.Text");
            this.innerText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // icon
            // 
            this.icon.Dock = System.Windows.Forms.DockStyle.Left;
            this.icon.Location = new System.Drawing.Point(0, 0);
            this.icon.Name = "icon";
            this.icon.Size = new System.Drawing.Size(24, 30);
            this.icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.icon.TabIndex = 2;
            this.icon.TabStop = false;
            // 
            // PropNotification_Base
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.innerText);
            this.Controls.Add(this.icon);
            this.Controls.Add(this.viewLink);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(30, 30);
            this.Name = "PropNotification_Base";
            this.Size = new System.Drawing.Size(500, 30);
            ((System.ComponentModel.ISupportInitialize)(this.icon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel viewLink;
        protected System.Windows.Forms.Label innerText;
        protected System.Windows.Forms.PictureBox icon;
    }
}
