namespace UnrealFlagEditor
{
    partial class PropertyPanel
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
            this.components = new System.ComponentModel.Container();
            this.groupBox = new UnrealFlagEditor.ElipsisGroupBox();
            this.flowPanel = new UnrealFlagEditor.FlowPanel_CustomizableScrollSpeed();
            this.notifPanel = new System.Windows.Forms.Panel();
            this.propsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.AutoElipsis = true;
            this.groupBox.Controls.Add(this.flowPanel);
            this.groupBox.Controls.Add(this.notifPanel);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(400, 500);
            this.groupBox.TabIndex = 1;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "No node selected.";
            // 
            // flowPanel
            // 
            this.flowPanel.AutoScroll = true;
            this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanel.Location = new System.Drawing.Point(3, 17);
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.ScrollMultiplier = 0.4F;
            this.flowPanel.Size = new System.Drawing.Size(394, 479);
            this.flowPanel.TabIndex = 0;
            // 
            // notifPanel
            // 
            this.notifPanel.AutoSize = true;
            this.notifPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.notifPanel.Location = new System.Drawing.Point(3, 496);
            this.notifPanel.MinimumSize = new System.Drawing.Size(0, 1);
            this.notifPanel.Name = "notifPanel";
            this.notifPanel.Size = new System.Drawing.Size(394, 1);
            this.notifPanel.TabIndex = 0;
            // 
            // propsToolTip
            // 
            this.propsToolTip.AutoPopDelay = 20000;
            this.propsToolTip.InitialDelay = 500;
            this.propsToolTip.ReshowDelay = 100;
            // 
            // PropertyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "PropertyPanel";
            this.Size = new System.Drawing.Size(400, 500);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ElipsisGroupBox groupBox;
        private FlowPanel_CustomizableScrollSpeed flowPanel;
        private System.Windows.Forms.Panel notifPanel;
        private System.Windows.Forms.ToolTip propsToolTip;
    }
}
