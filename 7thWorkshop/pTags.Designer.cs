/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class pTags {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.clTags = new System.Windows.Forms.CheckedListBox();
            this.bClear = new System.Windows.Forms.Button();
            this.bAll = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bAll);
            this.groupBox1.Controls.Add(this.clTags);
            this.groupBox1.Controls.Add(this.bClear);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(150, 150);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tags";
            // 
            // clTags
            // 
            this.clTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clTags.FormattingEnabled = true;
            this.clTags.Location = new System.Drawing.Point(6, 19);
            this.clTags.Name = "clTags";
            this.clTags.Size = new System.Drawing.Size(138, 94);
            this.clTags.TabIndex = 2;
            this.clTags.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clTags_ItemCheck);
            // 
            // bClear
            // 
            this.bClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bClear.Location = new System.Drawing.Point(6, 121);
            this.bClear.Name = "bClear";
            this.bClear.Size = new System.Drawing.Size(63, 23);
            this.bClear.TabIndex = 1;
            this.bClear.Text = "Clear";
            this.bClear.UseVisualStyleBackColor = true;
            this.bClear.Click += new System.EventHandler(this.bClear_Click);
            // 
            // bAll
            // 
            this.bAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bAll.Location = new System.Drawing.Point(81, 121);
            this.bAll.Name = "bAll";
            this.bAll.Size = new System.Drawing.Size(63, 23);
            this.bAll.TabIndex = 3;
            this.bAll.Text = "All";
            this.bAll.UseVisualStyleBackColor = true;
            this.bAll.Click += new System.EventHandler(this.bAll_Click);
            // 
            // pTags
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "pTags";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox clTags;
        private System.Windows.Forms.Button bClear;
        private System.Windows.Forms.Button bAll;

    }
}
