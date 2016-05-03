namespace Shadows
{
    partial class TerrainViz
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open500X500ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open40X40ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackBallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joystickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subtractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open2000X2000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.actionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(723, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.open2000X2000ToolStripMenuItem,
            this.open500X500ToolStripMenuItem,
            this.open40X40ToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.openToolStripMenuItem.Text = "&Open GeoTIFF";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // open500X500ToolStripMenuItem
            // 
            this.open500X500ToolStripMenuItem.Name = "open500X500ToolStripMenuItem";
            this.open500X500ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open500X500ToolStripMenuItem.Text = "Open 400 x 400";
            this.open500X500ToolStripMenuItem.Click += new System.EventHandler(this.open400X400ToolStripMenuItem_Click);
            // 
            // open40X40ToolStripMenuItem
            // 
            this.open40X40ToolStripMenuItem.Name = "open40X40ToolStripMenuItem";
            this.open40X40ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open40X40ToolStripMenuItem.Text = "Open 8 x 8";
            this.open40X40ToolStripMenuItem.Click += new System.EventHandler(this.open8X8ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(162, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cameraModeToolStripMenuItem,
            this.showTextureToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // cameraModeToolStripMenuItem
            // 
            this.cameraModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackBallToolStripMenuItem,
            this.joystickToolStripMenuItem});
            this.cameraModeToolStripMenuItem.Name = "cameraModeToolStripMenuItem";
            this.cameraModeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.cameraModeToolStripMenuItem.Text = "Camera Mode";
            // 
            // trackBallToolStripMenuItem
            // 
            this.trackBallToolStripMenuItem.Name = "trackBallToolStripMenuItem";
            this.trackBallToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.trackBallToolStripMenuItem.Text = "Track Ball";
            this.trackBallToolStripMenuItem.Click += new System.EventHandler(this.trackBallToolStripMenuItem_Click);
            // 
            // joystickToolStripMenuItem
            // 
            this.joystickToolStripMenuItem.Name = "joystickToolStripMenuItem";
            this.joystickToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.joystickToolStripMenuItem.Text = "Joystick";
            this.joystickToolStripMenuItem.Click += new System.EventHandler(this.joystickToolStripMenuItem_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.subtractToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.actionsToolStripMenuItem.Text = "&Actions";
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // subtractToolStripMenuItem
            // 
            this.subtractToolStripMenuItem.Name = "subtractToolStripMenuItem";
            this.subtractToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.subtractToolStripMenuItem.Text = "Subtract";
            this.subtractToolStripMenuItem.Click += new System.EventHandler(this.subtractToolStripMenuItem_Click);
            // 
            // open2000X2000ToolStripMenuItem
            // 
            this.open2000X2000ToolStripMenuItem.Name = "open2000X2000ToolStripMenuItem";
            this.open2000X2000ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open2000X2000ToolStripMenuItem.Text = "Open 2000 x 2000";
            this.open2000X2000ToolStripMenuItem.Click += new System.EventHandler(this.open2000X2000ToolStripMenuItem_Click);
            // 
            // showTextureToolStripMenuItem
            // 
            this.showTextureToolStripMenuItem.Checked = true;
            this.showTextureToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showTextureToolStripMenuItem.Name = "showTextureToolStripMenuItem";
            this.showTextureToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.showTextureToolStripMenuItem.Text = "Show Texture";
            this.showTextureToolStripMenuItem.Click += new System.EventHandler(this.showTextureToolStripMenuItem_Click);
            // 
            // TerrainViz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 498);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TerrainViz";
            this.Text = "Terrain Visualization";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open500X500ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open40X40ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subtractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trackBallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem joystickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open2000X2000ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTextureToolStripMenuItem;
    }
}

