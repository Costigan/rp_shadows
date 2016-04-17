namespace Shadows2
{
    partial class TriangleTests
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnClipTriangle = new System.Windows.Forms.Button();
            this.btnRotateTriangle = new System.Windows.Forms.Button();
            this.btnFlipTriangle = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTris = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblArea = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.panel1.Location = new System.Drawing.Point(156, 73);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(591, 559);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(116, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Run Basic Tests";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 73);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Run Triangle Tests";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnClipTriangle
            // 
            this.btnClipTriangle.Location = new System.Drawing.Point(12, 231);
            this.btnClipTriangle.Name = "btnClipTriangle";
            this.btnClipTriangle.Size = new System.Drawing.Size(116, 23);
            this.btnClipTriangle.TabIndex = 3;
            this.btnClipTriangle.Text = "Clip Triangle";
            this.btnClipTriangle.UseVisualStyleBackColor = true;
            this.btnClipTriangle.Click += new System.EventHandler(this.btnClipTriangle_Click);
            // 
            // btnRotateTriangle
            // 
            this.btnRotateTriangle.Location = new System.Drawing.Point(12, 202);
            this.btnRotateTriangle.Name = "btnRotateTriangle";
            this.btnRotateTriangle.Size = new System.Drawing.Size(47, 23);
            this.btnRotateTriangle.TabIndex = 4;
            this.btnRotateTriangle.Text = "Rotate";
            this.btnRotateTriangle.UseVisualStyleBackColor = true;
            this.btnRotateTriangle.Click += new System.EventHandler(this.btnRotateTriangle_Click);
            // 
            // btnFlipTriangle
            // 
            this.btnFlipTriangle.Location = new System.Drawing.Point(81, 202);
            this.btnFlipTriangle.Name = "btnFlipTriangle";
            this.btnFlipTriangle.Size = new System.Drawing.Size(47, 23);
            this.btnFlipTriangle.TabIndex = 4;
            this.btnFlipTriangle.Text = "Flip";
            this.btnFlipTriangle.UseVisualStyleBackColor = true;
            this.btnFlipTriangle.Click += new System.EventHandler(this.btnFlipTriangle_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 277);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Tries:";
            // 
            // lblTris
            // 
            this.lblTris.BackColor = System.Drawing.Color.White;
            this.lblTris.Location = new System.Drawing.Point(55, 277);
            this.lblTris.Name = "lblTris";
            this.lblTris.Size = new System.Drawing.Size(73, 13);
            this.lblTris.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 299);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Area:";
            // 
            // lblArea
            // 
            this.lblArea.BackColor = System.Drawing.Color.White;
            this.lblArea.Location = new System.Drawing.Point(54, 299);
            this.lblArea.Name = "lblArea";
            this.lblArea.Size = new System.Drawing.Size(73, 13);
            this.lblArea.TabIndex = 5;
            // 
            // TriangleTests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 697);
            this.Controls.Add(this.lblArea);
            this.Controls.Add(this.lblTris);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFlipTriangle);
            this.Controls.Add(this.btnRotateTriangle);
            this.Controls.Add(this.btnClipTriangle);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Name = "TriangleTests";
            this.Text = "TriangleTests";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnClipTriangle;
        private System.Windows.Forms.Button btnRotateTriangle;
        private System.Windows.Forms.Button btnFlipTriangle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTris;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblArea;
    }
}