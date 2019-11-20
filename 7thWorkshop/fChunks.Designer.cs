/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class fChunks {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.txtFLevel = new System.Windows.Forms.TextBox();
            this.bFLevel = new System.Windows.Forms.Button();
            this.bGo = new System.Windows.Forms.Button();
            this.bOutput = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pbChunk = new System.Windows.Forms.ProgressBar();
            this.ofFLevel = new System.Windows.Forms.OpenFileDialog();
            this.fbOutput = new System.Windows.Forms.FolderBrowserDialog();
            this.section1 = new System.Windows.Forms.CheckBox();
            this.section2 = new System.Windows.Forms.CheckBox();
            this.section3 = new System.Windows.Forms.CheckBox();
            this.section4 = new System.Windows.Forms.CheckBox();
            this.section5 = new System.Windows.Forms.CheckBox();
            this.section6 = new System.Windows.Forms.CheckBox();
            this.section7 = new System.Windows.Forms.CheckBox();
            this.section8 = new System.Windows.Forms.CheckBox();
            this.section9 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select FLEVEL:";
            // 
            // txtFLevel
            // 
            this.txtFLevel.Location = new System.Drawing.Point(98, 10);
            this.txtFLevel.Margin = new System.Windows.Forms.Padding(2);
            this.txtFLevel.Name = "txtFLevel";
            this.txtFLevel.Size = new System.Drawing.Size(385, 20);
            this.txtFLevel.TabIndex = 1;
            // 
            // bFLevel
            // 
            this.bFLevel.Location = new System.Drawing.Point(498, 11);
            this.bFLevel.Margin = new System.Windows.Forms.Padding(2);
            this.bFLevel.Name = "bFLevel";
            this.bFLevel.Size = new System.Drawing.Size(26, 16);
            this.bFLevel.TabIndex = 2;
            this.bFLevel.Text = "...";
            this.bFLevel.UseVisualStyleBackColor = true;
            this.bFLevel.Click += new System.EventHandler(this.bFLevel_Click);
            // 
            // bGo
            // 
            this.bGo.Location = new System.Drawing.Point(210, 203);
            this.bGo.Margin = new System.Windows.Forms.Padding(2);
            this.bGo.Name = "bGo";
            this.bGo.Size = new System.Drawing.Size(138, 22);
            this.bGo.TabIndex = 3;
            this.bGo.Text = "Extract";
            this.bGo.UseVisualStyleBackColor = true;
            this.bGo.Click += new System.EventHandler(this.bGo_Click);
            // 
            // bOutput
            // 
            this.bOutput.Location = new System.Drawing.Point(498, 36);
            this.bOutput.Margin = new System.Windows.Forms.Padding(2);
            this.bOutput.Name = "bOutput";
            this.bOutput.Size = new System.Drawing.Size(26, 16);
            this.bOutput.TabIndex = 6;
            this.bOutput.Text = "...";
            this.bOutput.UseVisualStyleBackColor = true;
            this.bOutput.Click += new System.EventHandler(this.bOutput_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(98, 35);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(2);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(385, 20);
            this.txtOutput.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output Folder:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 63);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Section #:";
            // 
            // pbChunk
            // 
            this.pbChunk.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbChunk.Location = new System.Drawing.Point(0, 245);
            this.pbChunk.Margin = new System.Windows.Forms.Padding(2);
            this.pbChunk.Name = "pbChunk";
            this.pbChunk.Size = new System.Drawing.Size(530, 12);
            this.pbChunk.TabIndex = 9;
            // 
            // ofFLevel
            // 
            this.ofFLevel.Filter = "FLevel.lgp|FLevel.lgp";
            // 
            // section1
            // 
            this.section1.AutoSize = true;
            this.section1.Location = new System.Drawing.Point(98, 63);
            this.section1.Name = "section1";
            this.section1.Size = new System.Drawing.Size(162, 17);
            this.section1.TabIndex = 10;
            this.section1.Text = "Section 1 Field Script & Dialog";
            this.section1.UseVisualStyleBackColor = true;
            this.section1.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section2
            // 
            this.section2.AutoSize = true;
            this.section2.Location = new System.Drawing.Point(98, 86);
            this.section2.Name = "section2";
            this.section2.Size = new System.Drawing.Size(141, 17);
            this.section2.TabIndex = 11;
            this.section2.Text = "Section 2 Camera Matrix";
            this.section2.UseVisualStyleBackColor = true;
            this.section2.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section3
            // 
            this.section3.AutoSize = true;
            this.section3.Location = new System.Drawing.Point(98, 109);
            this.section3.Name = "section3";
            this.section3.Size = new System.Drawing.Size(139, 17);
            this.section3.TabIndex = 12;
            this.section3.Text = "Section 3 Model Loader";
            this.section3.UseVisualStyleBackColor = true;
            this.section3.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section4
            // 
            this.section4.AutoSize = true;
            this.section4.Location = new System.Drawing.Point(98, 132);
            this.section4.Name = "section4";
            this.section4.Size = new System.Drawing.Size(107, 17);
            this.section4.TabIndex = 13;
            this.section4.Text = "Section 4 Palette";
            this.section4.UseVisualStyleBackColor = true;
            this.section4.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section5
            // 
            this.section5.AutoSize = true;
            this.section5.Location = new System.Drawing.Point(98, 155);
            this.section5.Name = "section5";
            this.section5.Size = new System.Drawing.Size(124, 17);
            this.section5.TabIndex = 14;
            this.section5.Text = "Section 5 Walkmesh";
            this.section5.UseVisualStyleBackColor = true;
            this.section5.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section6
            // 
            this.section6.AutoSize = true;
            this.section6.Location = new System.Drawing.Point(325, 63);
            this.section6.Name = "section6";
            this.section6.Size = new System.Drawing.Size(158, 17);
            this.section6.TabIndex = 15;
            this.section6.Text = "Section 6 TileMap (Unused)";
            this.section6.UseVisualStyleBackColor = true;
            this.section6.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section7
            // 
            this.section7.AutoSize = true;
            this.section7.Location = new System.Drawing.Point(325, 86);
            this.section7.Name = "section7";
            this.section7.Size = new System.Drawing.Size(123, 17);
            this.section7.TabIndex = 16;
            this.section7.Text = "Section 7 Encounter";
            this.section7.UseVisualStyleBackColor = true;
            this.section7.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section8
            // 
            this.section8.AutoSize = true;
            this.section8.Location = new System.Drawing.Point(325, 109);
            this.section8.Name = "section8";
            this.section8.Size = new System.Drawing.Size(112, 17);
            this.section8.TabIndex = 17;
            this.section8.Text = "Section 8 Triggers";
            this.section8.UseVisualStyleBackColor = true;
            this.section8.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // section9
            // 
            this.section9.AutoSize = true;
            this.section9.Location = new System.Drawing.Point(325, 132);
            this.section9.Name = "section9";
            this.section9.Size = new System.Drawing.Size(132, 17);
            this.section9.TabIndex = 18;
            this.section9.Text = "Section 9 Background";
            this.section9.UseVisualStyleBackColor = true;
            this.section9.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // fChunks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 257);
            this.Controls.Add(this.section9);
            this.Controls.Add(this.section8);
            this.Controls.Add(this.section7);
            this.Controls.Add(this.section6);
            this.Controls.Add(this.section5);
            this.Controls.Add(this.section4);
            this.Controls.Add(this.section3);
            this.Controls.Add(this.section2);
            this.Controls.Add(this.section1);
            this.Controls.Add(this.pbChunk);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bOutput);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bGo);
            this.Controls.Add(this.bFLevel);
            this.Controls.Add(this.txtFLevel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fChunks";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chunk Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFLevel;
        private System.Windows.Forms.Button bFLevel;
        private System.Windows.Forms.Button bGo;
        private System.Windows.Forms.Button bOutput;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar pbChunk;
        private System.Windows.Forms.OpenFileDialog ofFLevel;
        private System.Windows.Forms.FolderBrowserDialog fbOutput;
        private System.Windows.Forms.CheckBox section1;
        private System.Windows.Forms.CheckBox section2;
        private System.Windows.Forms.CheckBox section3;
        private System.Windows.Forms.CheckBox section4;
        private System.Windows.Forms.CheckBox section5;
        private System.Windows.Forms.CheckBox section6;
        private System.Windows.Forms.CheckBox section7;
        private System.Windows.Forms.CheckBox section8;
        private System.Windows.Forms.CheckBox section9;
    }
}
