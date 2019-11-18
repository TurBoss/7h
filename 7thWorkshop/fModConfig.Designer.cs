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
            this.PB = new System.Windows.Forms.PictureBox();
            this.lCompat = new System.Windows.Forms.Label();
            this.bPreview = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.pOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbOptions);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(12);
            this.groupBox1.Size = new System.Drawing.Size(263, 786);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // lbOptions
            // 
            this.lbOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbOptions.FormattingEnabled = true;
            this.lbOptions.HorizontalScrollbar = true;
            this.lbOptions.ItemHeight = 20;
            this.lbOptions.Location = new System.Drawing.Point(12, 31);
            this.lbOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbOptions.Name = "lbOptions";
            this.lbOptions.Size = new System.Drawing.Size(239, 743);
            this.lbOptions.TabIndex = 0;
            this.lbOptions.SelectedIndexChanged += new System.EventHandler(this.lbOptions_SelectedIndexChanged);
            // 
            // pOption
            // 
            this.pOption.Controls.Add(this.ddOption);
            this.pOption.Controls.Add(this.cbOption);
            this.pOption.Controls.Add(this.lDescription);
            this.pOption.Controls.Add(this.PB);
            this.pOption.Controls.Add(this.lCompat);
            this.pOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pOption.Location = new System.Drawing.Point(263, 0);
            this.pOption.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pOption.Name = "pOption";
            this.pOption.Padding = new System.Windows.Forms.Padding(8);
            this.pOption.Size = new System.Drawing.Size(913, 786);
            this.pOption.TabIndex = 1;
            // 
            // ddOption
            // 
            this.ddOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOption.FormattingEnabled = true;
            this.ddOption.Location = new System.Drawing.Point(16, 111);
            this.ddOption.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ddOption.Name = "ddOption";
            this.ddOption.Size = new System.Drawing.Size(312, 28);
            this.ddOption.TabIndex = 3;
            this.ddOption.SelectedIndexChanged += new System.EventHandler(this.ddOption_SelectedIndexChanged);
            // 
            // cbOption
            // 
            this.cbOption.AutoSize = true;
            this.cbOption.Location = new System.Drawing.Point(12, 68);
            this.cbOption.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbOption.Name = "cbOption";
            this.cbOption.Size = new System.Drawing.Size(113, 24);
            this.cbOption.TabIndex = 2;
            this.cbOption.Text = "checkBox1";
            this.cbOption.UseVisualStyleBackColor = true;
            this.cbOption.CheckedChanged += new System.EventHandler(this.cbOption_CheckedChanged);
            // 
            // lDescription
            // 
            this.lDescription.AutoEllipsis = true;
            this.lDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.lDescription.Location = new System.Drawing.Point(8, 8);
            this.lDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lDescription.Name = "lDescription";
            this.lDescription.Size = new System.Drawing.Size(897, 49);
            this.lDescription.TabIndex = 1;
            this.lDescription.Text = "label1";
            // 
            // PB
            // 
            this.PB.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PB.Location = new System.Drawing.Point(8, 347);
            this.PB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PB.Name = "PB";
            this.PB.Size = new System.Drawing.Size(897, 346);
            this.PB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PB.TabIndex = 0;
            this.PB.TabStop = false;
            // 
            // lCompat
            // 
            this.lCompat.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lCompat.Location = new System.Drawing.Point(8, 693);
            this.lCompat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lCompat.Name = "lCompat";
            this.lCompat.Size = new System.Drawing.Size(897, 85);
            this.lCompat.TabIndex = 4;
            this.lCompat.Text = "lCompat";
            // 
            // bPreview
            // 
            this.bPreview.Image = global::Iros._7th.Workshop.Properties.Resources.startwithoutdebugging_6556;
            this.bPreview.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bPreview.Location = new System.Drawing.Point(263, 20);
            this.bPreview.Name = "bPreview";
            this.bPreview.Size = new System.Drawing.Size(148, 38);
            this.bPreview.TabIndex = 5;
            this.bPreview.Text = "Preview";
            this.bPreview.UseVisualStyleBackColor = true;
            this.bPreview.Visible = false;
            this.bPreview.Click += new System.EventHandler(this.bPreview_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bPreview);
            this.panel1.Controls.Add(this.bOK);
            this.panel1.Controls.Add(this.bCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 786);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1176, 77);
            this.panel1.TabIndex = 2;
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Location = new System.Drawing.Point(924, 20);
            this.bOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(112, 35);
            this.bOK.TabIndex = 1;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(1046, 20);
            this.bCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(112, 35);
            this.bCancel.TabIndex = 0;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // fModConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 863);
            this.Controls.Add(this.pOption);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "fModConfig";
            this.Text = "Configure Mod";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fModConfig_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.pOption.ResumeLayout(false);
            this.pOption.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB)).EndInit();
            this.panel1.ResumeLayout(false);
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
