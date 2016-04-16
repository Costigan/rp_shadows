namespace Shadows2
{
    partial class Form1
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
            this.open1000X1000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderHeightFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoUpdateAfterAzElChangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnUpdateFromAzEl = new System.Windows.Forms.Button();
            this.trackElevation = new System.Windows.Forms.TrackBar();
            this.tbElevation = new System.Windows.Forms.TextBox();
            this.trackAzimuth = new System.Windows.Forms.TrackBar();
            this.tbAzimuth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.open20X20ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open100X100ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackElevation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAzimuth)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(809, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.open20X20ToolStripMenuItem,
            this.open100X100ToolStripMenuItem,
            this.open500X500ToolStripMenuItem,
            this.open1000X1000ToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // open500X500ToolStripMenuItem
            // 
            this.open500X500ToolStripMenuItem.Name = "open500X500ToolStripMenuItem";
            this.open500X500ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open500X500ToolStripMenuItem.Text = "Open 500 x 500";
            this.open500X500ToolStripMenuItem.Click += new System.EventHandler(this.open500X500ToolStripMenuItem_Click);
            // 
            // open1000X1000ToolStripMenuItem
            // 
            this.open1000X1000ToolStripMenuItem.Name = "open1000X1000ToolStripMenuItem";
            this.open1000X1000ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open1000X1000ToolStripMenuItem.Text = "Open 1000 x 1000";
            this.open1000X1000ToolStripMenuItem.Click += new System.EventHandler(this.open1000X1000ToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renderHeightFieldToolStripMenuItem,
            this.autoUpdateAfterAzElChangeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // renderHeightFieldToolStripMenuItem
            // 
            this.renderHeightFieldToolStripMenuItem.Name = "renderHeightFieldToolStripMenuItem";
            this.renderHeightFieldToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.renderHeightFieldToolStripMenuItem.Text = "Render Height Field";
            this.renderHeightFieldToolStripMenuItem.Click += new System.EventHandler(this.renderHeightFieldToolStripMenuItem_Click);
            // 
            // autoUpdateAfterAzElChangeToolStripMenuItem
            // 
            this.autoUpdateAfterAzElChangeToolStripMenuItem.Name = "autoUpdateAfterAzElChangeToolStripMenuItem";
            this.autoUpdateAfterAzElChangeToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.autoUpdateAfterAzElChangeToolStripMenuItem.Text = "Auto Update after Az/El change";
            this.autoUpdateAfterAzElChangeToolStripMenuItem.Click += new System.EventHandler(this.autoUpdateAfterAzElChangeToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(154, 121);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(655, 352);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 121);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(154, 352);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(809, 97);
            this.panel2.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnUpdateFromAzEl);
            this.panel3.Controls.Add(this.trackElevation);
            this.panel3.Controls.Add(this.tbElevation);
            this.panel3.Controls.Add(this.trackAzimuth);
            this.panel3.Controls.Add(this.tbAzimuth);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(809, 22);
            this.panel3.TabIndex = 2;
            // 
            // btnUpdateFromAzEl
            // 
            this.btnUpdateFromAzEl.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnUpdateFromAzEl.Location = new System.Drawing.Point(734, 0);
            this.btnUpdateFromAzEl.Name = "btnUpdateFromAzEl";
            this.btnUpdateFromAzEl.Size = new System.Drawing.Size(75, 22);
            this.btnUpdateFromAzEl.TabIndex = 7;
            this.btnUpdateFromAzEl.Text = "Update";
            this.btnUpdateFromAzEl.UseVisualStyleBackColor = true;
            this.btnUpdateFromAzEl.Click += new System.EventHandler(this.btnUpdateFromAzEl_Click);
            // 
            // trackElevation
            // 
            this.trackElevation.Dock = System.Windows.Forms.DockStyle.Left;
            this.trackElevation.Location = new System.Drawing.Point(354, 0);
            this.trackElevation.Maximum = 800;
            this.trackElevation.Minimum = -800;
            this.trackElevation.Name = "trackElevation";
            this.trackElevation.Size = new System.Drawing.Size(182, 22);
            this.trackElevation.TabIndex = 6;
            this.trackElevation.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackElevation.Scroll += new System.EventHandler(this.trackElevation_Scroll);
            // 
            // tbElevation
            // 
            this.tbElevation.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbElevation.Location = new System.Drawing.Point(303, 0);
            this.tbElevation.Name = "tbElevation";
            this.tbElevation.Size = new System.Drawing.Size(51, 20);
            this.tbElevation.TabIndex = 5;
            // 
            // trackAzimuth
            // 
            this.trackAzimuth.Dock = System.Windows.Forms.DockStyle.Left;
            this.trackAzimuth.Location = new System.Drawing.Point(121, 0);
            this.trackAzimuth.Maximum = 359;
            this.trackAzimuth.Name = "trackAzimuth";
            this.trackAzimuth.Size = new System.Drawing.Size(182, 22);
            this.trackAzimuth.TabIndex = 4;
            this.trackAzimuth.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackAzimuth.Scroll += new System.EventHandler(this.trackAzimuth_Scroll);
            // 
            // tbAzimuth
            // 
            this.tbAzimuth.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbAzimuth.Location = new System.Drawing.Point(70, 0);
            this.tbAzimuth.Name = "tbAzimuth";
            this.tbAzimuth.Size = new System.Drawing.Size(51, 20);
            this.tbAzimuth.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 22);
            this.label2.TabIndex = 1;
            this.label2.Text = "Az / El";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // open20X20ToolStripMenuItem
            // 
            this.open20X20ToolStripMenuItem.Name = "open20X20ToolStripMenuItem";
            this.open20X20ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open20X20ToolStripMenuItem.Text = "Open 20 x 20";
            this.open20X20ToolStripMenuItem.Click += new System.EventHandler(this.open20X20ToolStripMenuItem_Click);
            // 
            // open100X100ToolStripMenuItem
            // 
            this.open100X100ToolStripMenuItem.Name = "open100X100ToolStripMenuItem";
            this.open100X100ToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.open100X100ToolStripMenuItem.Text = "Open 100 x 100";
            this.open100X100ToolStripMenuItem.Click += new System.EventHandler(this.open100X100ToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 473);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Shadows2";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackElevation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAzimuth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnUpdateFromAzEl;
        private System.Windows.Forms.TrackBar trackElevation;
        private System.Windows.Forms.TextBox tbElevation;
        private System.Windows.Forms.TrackBar trackAzimuth;
        private System.Windows.Forms.TextBox tbAzimuth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderHeightFieldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open500X500ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open1000X1000ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoUpdateAfterAzElChangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open20X20ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open100X100ToolStripMenuItem;
    }
}

