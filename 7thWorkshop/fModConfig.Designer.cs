/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class fModConfig {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbOptions = new System.Windows.Forms.ListBox();
            this.pOption = new System.Windows.Forms.Panel();
            this.ddOption = new System.Windows.Forms.ComboBox();
            this.cbOption = new System.Windows.Forms.CheckBox();
            this.lDescription = new System.Windows.Forms.Label();
            this.lCompat = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.bPreview = new System.Windows.Forms.Button();
            this.PB = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.pOption.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbOptions);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(16, 15, 16, 15);
            this.groupBox1.Size = new System.Drawing.Size(296, 795);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // lbOptions
            // 
            this.lbOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbOptions.FormattingEnabled = true;
            this.lbOptions.ItemHeight = 25;
            this.lbOptions.Location = new System.Drawing.Point(16, 39);
            this.lbOptions.Margin = new System.Windows.Forms.Padding(6);
            this.lbOptions.Name = "lbOptions";
            this.lbOptions.Size = new System.Drawing.Size(264, 741);
            this.lbOptions.TabIndex = 0;
            this.lbOptions.SelectedIndexChanged += new System.EventHandler(this.lbOptions_SelectedIndexChanged);
            // 
            // pOption
            // 
            this.pOption.Controls.Add(this.bPreview);
            this.pOption.Controls.Add(this.ddOption);
            this.pOption.Controls.Add(this.cbOption);
            this.pOption.Controls.Add(this.lDescription);
            this.pOption.Controls.Add(this.PB);
            this.pOption.Controls.Add(this.lCompat);
            this.pOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pOption.Location = new System.Drawing.Point(296, 0);
            this.pOption.Margin = new System.Windows.Forms.Padding(6);
            this.pOption.Name = "pOption";
            this.pOption.Padding = new System.Windows.Forms.Padding(10);
            this.pOption.Size = new System.Drawing.Size(850, 795);
            this.pOption.TabIndex = 1;
            // 
            // ddOption
            // 
            this.ddOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOption.FormattingEnabled = true;
            this.ddOption.Location = new System.Drawing.Point(21, 161);
            this.ddOption.Margin = new System.Windows.Forms.Padding(6);
            this.ddOption.Name = "ddOption";
            this.ddOption.Size = new System.Drawing.Size(414, 33);
            this.ddOption.TabIndex = 3;
            this.ddOption.SelectedIndexChanged += new System.EventHandler(this.ddOption_SelectedIndexChanged);
            // 
            // cbOption
            // 
            this.cbOption.AutoSize = true;
            this.cbOption.Location = new System.Drawing.Point(21, 163);
            this.cbOption.Margin = new System.Windows.Forms.Padding(6);
            this.cbOption.Name = "cbOption";
            this.cbOption.Size = new System.Drawing.Size(150, 29);
            this.cbOption.TabIndex = 2;
            this.cbOption.Text = "checkBox1";
            this.cbOption.UseVisualStyleBackColor = true;
            this.cbOption.CheckedChanged += new System.EventHandler(this.cbOption_CheckedChanged);
            // 
            // lDescription
            // 
            this.lDescription.Location = new System.Drawing.Point(16, 17);
            this.lDescription.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lDescription.Name = "lDescription";
            this.lDescription.Size = new System.Drawing.Size(810, 138);
            this.lDescription.TabIndex = 1;
            this.lDescription.Text = "label1";
            // 
            // lCompat
            // 
            this.lCompat.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lCompat.Location = new System.Drawing.Point(10, 674);
            this.lCompat.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lCompat.Name = "lCompat";
            this.lCompat.Size = new System.Drawing.Size(830, 111);
            this.lCompat.TabIndex = 4;
            this.lCompat.Text = "lCompat";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bOK);
            this.panel1.Controls.Add(this.bCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 795);
            this.panel1.Margin = new System.Windows.Forms.Padding(6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1146, 96);
            this.panel1.TabIndex = 2;
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Location = new System.Drawing.Point(810, 25);
            this.bOK.Margin = new System.Windows.Forms.Padding(6);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(150, 44);
            this.bOK.TabIndex = 1;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(972, 25);
            this.bCancel.Margin = new System.Windows.Forms.Padding(6);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(150, 44);
            this.bCancel.TabIndex = 0;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // bPreview
            // 
            this.bPreview.Image = global::Iros._7th.Workshop.Properties.Resources.startwithoutdebugging_6556;
            this.bPreview.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bPreview.Location = new System.Drawing.Point(340, 579);
            this.bPreview.Name = "bPreview";
            this.bPreview.Size = new System.Drawing.Size(198, 48);
            this.bPreview.TabIndex = 5;
            this.bPreview.Text = "Preview";
            this.bPreview.UseVisualStyleBackColor = true;
            this.bPreview.Visible = false;
            this.bPreview.Click += new System.EventHandler(this.bPreview_Click);
            // 
            // PB
            // 
            this.PB.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PB.Location = new System.Drawing.Point(10, 241);
            this.PB.Margin = new System.Windows.Forms.Padding(6);
            this.PB.Name = "PB";
            this.PB.Size = new System.Drawing.Size(830, 433);
            this.PB.TabIndex = 0;
            this.PB.TabStop = false;
            // 
            // fModConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 891);
            this.Controls.Add(this.pOption);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fModConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Mod";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fModConfig_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.pOption.ResumeLayout(false);
            this.pOption.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lbOptions;
        private System.Windows.Forms.Panel pOption;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.PictureBox PB;
        private System.Windows.Forms.Label lDescription;
        private System.Windows.Forms.CheckBox cbOption;
        private System.Windows.Forms.ComboBox ddOption;
        private System.Windows.Forms.Label lCompat;
        private System.Windows.Forms.Button bPreview;
    }
}
