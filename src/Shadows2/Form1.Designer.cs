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
            this.open20X20ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open100X100ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open400X400ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open500X500ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open1000X1000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.synthesize10X10ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synthesize400X400ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synthesize500X500ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synthesize8000X8000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderHeightFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoUpdateAfterAzElChangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleRayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.takeTimingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printTimeEstimatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button5 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tbSunRadius = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbScale = new System.Windows.Forms.TrackBar();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnUpdateFromAzEl = new System.Windows.Forms.Button();
            this.trackElevation = new System.Windows.Forms.TrackBar();
            this.tbElevation = new System.Windows.Forms.TextBox();
            this.trackAzimuth = new System.Windows.Forms.TrackBar();
            this.tbAzimuth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbScale)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackElevation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAzimuth)).BeginInit();
            this.panel4.SuspendLayout();
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
            this.menuStrip1.Size = new System.Drawing.Size(882, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.open20X20ToolStripMenuItem,
            this.open100X100ToolStripMenuItem,
            this.open400X400ToolStripMenuItem,
            this.open500X500ToolStripMenuItem,
            this.open1000X1000ToolStripMenuItem,
            this.toolStripSeparator1,
            this.synthesize10X10ToolStripMenuItem,
            this.synthesize400X400ToolStripMenuItem,
            this.synthesize500X500ToolStripMenuItem,
            this.synthesize8000X8000ToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // open20X20ToolStripMenuItem
            // 
            this.open20X20ToolStripMenuItem.Name = "open20X20ToolStripMenuItem";
            this.open20X20ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.open20X20ToolStripMenuItem.Text = "Open 20 x 20";
            this.open20X20ToolStripMenuItem.Click += new System.EventHandler(this.open20X20ToolStripMenuItem_Click);
            // 
            // open100X100ToolStripMenuItem
            // 
            this.open100X100ToolStripMenuItem.Name = "open100X100ToolStripMenuItem";
            this.open100X100ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.open100X100ToolStripMenuItem.Text = "Open 100 x 100";
            this.open100X100ToolStripMenuItem.Click += new System.EventHandler(this.open100X100ToolStripMenuItem_Click);
            // 
            // open400X400ToolStripMenuItem
            // 
            this.open400X400ToolStripMenuItem.Name = "open400X400ToolStripMenuItem";
            this.open400X400ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.open400X400ToolStripMenuItem.Text = "Open 400 x 400";
            this.open400X400ToolStripMenuItem.Click += new System.EventHandler(this.open400X400ToolStripMenuItem_Click);
            // 
            // open500X500ToolStripMenuItem
            // 
            this.open500X500ToolStripMenuItem.Name = "open500X500ToolStripMenuItem";
            this.open500X500ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.open500X500ToolStripMenuItem.Text = "Open 500 x 500";
            this.open500X500ToolStripMenuItem.Click += new System.EventHandler(this.open500X500ToolStripMenuItem_Click);
            // 
            // open1000X1000ToolStripMenuItem
            // 
            this.open1000X1000ToolStripMenuItem.Name = "open1000X1000ToolStripMenuItem";
            this.open1000X1000ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.open1000X1000ToolStripMenuItem.Text = "Open 1000 x 1000";
            this.open1000X1000ToolStripMenuItem.Click += new System.EventHandler(this.open1000X1000ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // synthesize10X10ToolStripMenuItem
            // 
            this.synthesize10X10ToolStripMenuItem.Name = "synthesize10X10ToolStripMenuItem";
            this.synthesize10X10ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.synthesize10X10ToolStripMenuItem.Text = "Synthesize 10 x 10";
            this.synthesize10X10ToolStripMenuItem.Click += new System.EventHandler(this.synthesize10X10ToolStripMenuItem_Click);
            // 
            // synthesize400X400ToolStripMenuItem
            // 
            this.synthesize400X400ToolStripMenuItem.Name = "synthesize400X400ToolStripMenuItem";
            this.synthesize400X400ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.synthesize400X400ToolStripMenuItem.Text = "Synthesize 400 x 400";
            this.synthesize400X400ToolStripMenuItem.Click += new System.EventHandler(this.synthesize400X400ToolStripMenuItem_Click);
            // 
            // synthesize500X500ToolStripMenuItem
            // 
            this.synthesize500X500ToolStripMenuItem.Name = "synthesize500X500ToolStripMenuItem";
            this.synthesize500X500ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.synthesize500X500ToolStripMenuItem.Text = "Synthesize 500 x 500";
            this.synthesize500X500ToolStripMenuItem.Click += new System.EventHandler(this.synthesize500X500ToolStripMenuItem_Click);
            // 
            // synthesize8000X8000ToolStripMenuItem
            // 
            this.synthesize8000X8000ToolStripMenuItem.Name = "synthesize8000X8000ToolStripMenuItem";
            this.synthesize8000X8000ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.synthesize8000X8000ToolStripMenuItem.Text = "Synthesize 8000 x 8000";
            this.synthesize8000X8000ToolStripMenuItem.Click += new System.EventHandler(this.synthesize8000X8000ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(188, 6);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.saveAsToolStripMenuItem.Text = "&Save as ...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renderHeightFieldToolStripMenuItem,
            this.autoUpdateAfterAzElChangeToolStripMenuItem,
            this.singleRayToolStripMenuItem});
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
            // singleRayToolStripMenuItem
            // 
            this.singleRayToolStripMenuItem.Name = "singleRayToolStripMenuItem";
            this.singleRayToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.singleRayToolStripMenuItem.Text = "Single Ray";
            this.singleRayToolStripMenuItem.Click += new System.EventHandler(this.singleRayToolStripMenuItem_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.takeTimingsToolStripMenuItem,
            this.printTimeEstimatesToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.actionsToolStripMenuItem.Text = "&Actions";
            // 
            // takeTimingsToolStripMenuItem
            // 
            this.takeTimingsToolStripMenuItem.Name = "takeTimingsToolStripMenuItem";
            this.takeTimingsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.takeTimingsToolStripMenuItem.Text = "Print Timings";
            this.takeTimingsToolStripMenuItem.Click += new System.EventHandler(this.takeTimingsToolStripMenuItem_Click);
            // 
            // printTimeEstimatesToolStripMenuItem
            // 
            this.printTimeEstimatesToolStripMenuItem.Name = "printTimeEstimatesToolStripMenuItem";
            this.printTimeEstimatesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.printTimeEstimatesToolStripMenuItem.Text = "Print Time Estimates";
            this.printTimeEstimatesToolStripMenuItem.Click += new System.EventHandler(this.printTimeEstimatesToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(500, 500);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 109);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(154, 499);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button5);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.button4);
            this.panel2.Controls.Add(this.button9);
            this.panel2.Controls.Add(this.button8);
            this.panel2.Controls.Add(this.button7);
            this.panel2.Controls.Add(this.button6);
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.tbSunRadius);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.tbScale);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(882, 85);
            this.panel2.TabIndex = 3;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(785, 56);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(90, 23);
            this.button5.TabIndex = 9;
            this.button5.Text = "d=30 side=9";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(785, 28);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "d=10 side=7";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(593, 28);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(90, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "d=30 side=0";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(497, 28);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 23);
            this.button3.TabIndex = 9;
            this.button3.Text = "d=10 side=0";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(689, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "d=10 side=3";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbSunRadius
            // 
            this.tbSunRadius.Location = new System.Drawing.Point(404, 26);
            this.tbSunRadius.Name = "tbSunRadius";
            this.tbSunRadius.Size = new System.Drawing.Size(63, 20);
            this.tbSunRadius.TabIndex = 7;
            this.tbSunRadius.Text = "0.25";
            this.tbSunRadius.TextChanged += new System.EventHandler(this.tbSunRadius_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(309, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Sun Radius (deg)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Scale";
            // 
            // tbScale
            // 
            this.tbScale.Location = new System.Drawing.Point(43, 26);
            this.tbScale.Maximum = 30;
            this.tbScale.Minimum = 1;
            this.tbScale.Name = "tbScale";
            this.tbScale.Size = new System.Drawing.Size(260, 45);
            this.tbScale.TabIndex = 5;
            this.tbScale.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbScale.Value = 1;
            this.tbScale.ValueChanged += new System.EventHandler(this.tbScale_ValueChanged);
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
            this.panel3.Size = new System.Drawing.Size(882, 22);
            this.panel3.TabIndex = 2;
            // 
            // btnUpdateFromAzEl
            // 
            this.btnUpdateFromAzEl.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnUpdateFromAzEl.Location = new System.Drawing.Point(807, 0);
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
            this.trackElevation.Size = new System.Drawing.Size(363, 22);
            this.trackElevation.TabIndex = 6;
            this.trackElevation.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackElevation.Scroll += new System.EventHandler(this.trackElevation_Scroll);
            this.trackElevation.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackElevation_MouseUp);
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
            this.trackAzimuth.Value = 90;
            this.trackAzimuth.Scroll += new System.EventHandler(this.trackAzimuth_Scroll);
            this.trackAzimuth.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackAzimuth_MouseUp);
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
            // panel4
            // 
            this.panel4.AutoScroll = true;
            this.panel4.Controls.Add(this.pictureBox1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(154, 109);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(728, 499);
            this.panel4.TabIndex = 4;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(497, 56);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(90, 23);
            this.button6.TabIndex = 9;
            this.button6.Text = "d=1 v=9 h=0";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(593, 56);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(90, 23);
            this.button7.TabIndex = 9;
            this.button7.Text = "d=1 v=20 h=1";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(689, 56);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(90, 23);
            this.button8.TabIndex = 9;
            this.button8.Text = "d=20 v=20 h=5";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(312, 56);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(90, 23);
            this.button9.TabIndex = 9;
            this.button9.Text = "d=5 v=20 h=5";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 608);
            this.Controls.Add(this.panel4);
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
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbScale)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackElevation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAzimuth)).EndInit();
            this.panel4.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem singleRayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem synthesize500X500ToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar tbScale;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ToolStripMenuItem open400X400ToolStripMenuItem;
        private System.Windows.Forms.TextBox tbSunRadius;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripMenuItem synthesize8000X8000ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem synthesize400X400ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem synthesize10X10ToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem takeTimingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printTimeEstimatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
    }
}

