/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class fNewVer {
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
            this.lNewVer = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.bNo = new System.Windows.Forms.Button();
            this.bLater = new System.Windows.Forms.Button();
            this.bYes = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lNewVer
            // 
            this.lNewVer.AutoSize = true;
            this.lNewVer.Location = new System.Drawing.Point(23, 27);
            this.lNewVer.Name = "lNewVer";
            this.lNewVer.Size = new System.Drawing.Size(70, 25);
            this.lNewVer.TabIndex = 0;
            this.lNewVer.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(481, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Do you want to download and install this update?";
            // 
            // bNo
            // 
            this.bNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.bNo.Location = new System.Drawing.Point(389, 174);
            this.bNo.Name = "bNo";
            this.bNo.Size = new System.Drawing.Size(123, 55);
            this.bNo.TabIndex = 2;
            this.bNo.Text = "No";
            this.bNo.UseVisualStyleBackColor = true;
            // 
            // bLater
            // 
            this.bLater.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bLater.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.bLater.Location = new System.Drawing.Point(260, 174);
            this.bLater.Name = "bLater";
            this.bLater.Size = new System.Drawing.Size(123, 55);
            this.bLater.TabIndex = 3;
            this.bLater.Text = "Later";
            this.bLater.UseVisualStyleBackColor = true;
            // 
            // bYes
            // 
            this.bYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.bYes.Location = new System.Drawing.Point(131, 174);
            this.bYes.Name = "bYes";
            this.bYes.Size = new System.Drawing.Size(123, 55);
            this.bYes.TabIndex = 4;
            this.bYes.Text = "Yes";
            this.bYes.UseVisualStyleBackColor = true;
            // 
            // fNewVer
            // 
            this.AcceptButton = this.bYes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bLater;
            this.ClientSize = new System.Drawing.Size(530, 241);
            this.Controls.Add(this.bYes);
            this.Controls.Add(this.bLater);
            this.Controls.Add(this.bNo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lNewVer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fNewVer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Update";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lNewVer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bNo;
        private System.Windows.Forms.Button bLater;
        private System.Windows.Forms.Button bYes;
    }
}
