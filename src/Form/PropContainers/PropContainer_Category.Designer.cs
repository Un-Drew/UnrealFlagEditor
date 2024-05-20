namespace UnrealFlagEditor
{
    partial class PropContainer_Category
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
            this.groupBox = new UnrealFlagEditor.ElipsisGroupBox();
            this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.AutoElipsis = true;
            this.groupBox.AutoSize = true;
            this.groupBox.Controls.Add(this.flowPanel);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox.Location = new System.Drawing.Point(4, 4);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(142, 142);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "This is a category!";
            // 
            // flowPanel
            // 
            this.flowPanel.AutoSize = true;
            this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanel.Location = new System.Drawing.Point(3, 17);
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.Size = new System.Drawing.Size(136, 122);
            this.flowPanel.TabIndex = 0;
            this.flowPanel.WrapContents = false;
            // 
            // PropContainer_Category
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.groupBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(100, 0);
            this.Name = "PropContainer_Category";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowPanel;
        private ElipsisGroupBox groupBox;
    }
}
