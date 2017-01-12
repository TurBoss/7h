/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class pMod {
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
            this.gMain = new System.Windows.Forms.GroupBox();
            this.PB = new System.Windows.Forms.PictureBox();
            this.rtInfo = new System.Windows.Forms.RichTextBox();
            this.bAction = new System.Windows.Forms.Button();
            this.pProfile = new System.Windows.Forms.Panel();
            this.bReadme = new System.Windows.Forms.Button();
            this.bDown = new System.Windows.Forms.Button();
            this.bUp = new System.Windows.Forms.Button();
            this.bConfigure = new System.Windows.Forms.Button();
            this.bActivate = new System.Windows.Forms.Button();
            this.gMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB)).BeginInit();
            this.pProfile.SuspendLayout();
            this.SuspendLayout();
            // 
            // gMain
            // 
            this.gMain.Controls.Add(this.PB);
            this.gMain.Controls.Add(this.rtInfo);
            this.gMain.Controls.Add(this.bAction);
            this.gMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gMain.Location = new System.Drawing.Point(0, 0);
            this.gMain.Name = "gMain";
            this.gMain.Padding = new System.Windows.Forms.Padding(5);
            this.gMain.Size = new System.Drawing.Size(383, 156);
            this.gMain.TabIndex = 0;
            this.gMain.TabStop = false;
            this.gMain.Text = "Mod Title";
            // 
            // PB
            // 
            this.PB.Dock = System.Windows.Forms.DockStyle.Left;
            this.PB.Location = new System.Drawing.Point(5, 18);
            this.PB.Name = "PB";
            this.PB.Size = new System.Drawing.Size(171, 133);
            this.PB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PB.TabIndex = 0;
            this.PB.TabStop = false;
            // 
            // rtInfo
            // 
            this.rtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtInfo.BackColor = System.Drawing.SystemColors.Control;
            this.rtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtInfo.Location = new System.Drawing.Point(182, 50);
            this.rtInfo.Name = "rtInfo";
            this.rtInfo.ReadOnly = true;
            this.rtInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtInfo.Size = new System.Drawing.Size(193, 98);
            this.rtInfo.TabIndex = 2;
            this.rtInfo.Text = "";
            this.rtInfo.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtInfo_LinkClicked);
            // 
            // bAction
            // 
            this.bAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bAction.Location = new System.Drawing.Point(182, 21);
            this.bAction.Name = "bAction";
            this.bAction.Size = new System.Drawing.Size(100, 23);
            this.bAction.TabIndex = 1;
            this.bAction.Text = "Download";
            this.bAction.UseVisualStyleBackColor = false;
            this.bAction.Click += new System.EventHandler(this.bAction_Click);
            // 
            // pProfile
            // 
            this.pProfile.BackColor = System.Drawing.Color.DimGray;
            this.pProfile.Controls.Add(this.bReadme);
            this.pProfile.Controls.Add(this.bDown);
            this.pProfile.Controls.Add(this.bUp);
            this.pProfile.Controls.Add(this.bConfigure);
            this.pProfile.Controls.Add(this.bActivate);
            this.pProfile.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pProfile.Location = new System.Drawing.Point(0, 156);
            this.pProfile.Name = "pProfile";
            this.pProfile.Size = new System.Drawing.Size(383, 34);
            this.pProfile.TabIndex = 1;
            // 
            // bReadme
            // 
            this.bReadme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bReadme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bReadme.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bReadme.Location = new System.Drawing.Point(111, 6);
            this.bReadme.Name = "bReadme";
            this.bReadme.Size = new System.Drawing.Size(100, 23);
            this.bReadme.TabIndex = 6;
            this.bReadme.Text = "Readme";
            this.bReadme.UseVisualStyleBackColor = false;
            this.bReadme.Click += new System.EventHandler(this.bReadme_Click);
            // 
            // bDown
            // 
            this.bDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bDown.Image = global::Iros._7th.Workshop.Properties.Resources._010_LowPriority_16x16_72;
            this.bDown.Location = new System.Drawing.Point(258, 6);
            this.bDown.Name = "bDown";
            this.bDown.Size = new System.Drawing.Size(35, 23);
            this.bDown.TabIndex = 5;
            this.bDown.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bDown.UseVisualStyleBackColor = false;
            this.bDown.Click += new System.EventHandler(this.bDown_Click);
            // 
            // bUp
            // 
            this.bUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bUp.Image = global::Iros._7th.Workshop.Properties.Resources._010_LowPriority_16x16_72_Flip;
            this.bUp.Location = new System.Drawing.Point(217, 6);
            this.bUp.Name = "bUp";
            this.bUp.Size = new System.Drawing.Size(35, 23);
            this.bUp.TabIndex = 4;
            this.bUp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bUp.UseVisualStyleBackColor = false;
            this.bUp.Click += new System.EventHandler(this.bUp_Click);
            // 
            // bConfigure
            // 
            this.bConfigure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bConfigure.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bConfigure.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bConfigure.Location = new System.Drawing.Point(111, 6);
            this.bConfigure.Name = "bConfigure";
            this.bConfigure.Size = new System.Drawing.Size(100, 23);
            this.bConfigure.TabIndex = 3;
            this.bConfigure.Text = "Configure";
            this.bConfigure.UseVisualStyleBackColor = false;
            this.bConfigure.Click += new System.EventHandler(this.bConfigure_Click);
            // 
            // bActivate
            // 
            this.bActivate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bActivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bActivate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bActivate.Location = new System.Drawing.Point(5, 6);
            this.bActivate.Name = "bActivate";
            this.bActivate.Size = new System.Drawing.Size(100, 23);
            this.bActivate.TabIndex = 2;
            this.bActivate.Text = "Activate";
            this.bActivate.UseVisualStyleBackColor = false;
            this.bActivate.Click += new System.EventHandler(this.bActivate_Click);
            // 
            // pMod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gMain);
            this.Controls.Add(this.pProfile);
            this.Name = "pMod";
            this.Size = new System.Drawing.Size(383, 190);
            this.gMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PB)).EndInit();
            this.pProfile.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gMain;
        private System.Windows.Forms.PictureBox PB;
        private System.Windows.Forms.RichTextBox rtInfo;
        private System.Windows.Forms.Button bAction;
        private System.Windows.Forms.Panel pProfile;
        private System.Windows.Forms.Button bUp;
        private System.Windows.Forms.Button bConfigure;
        private System.Windows.Forms.Button bActivate;
        private System.Windows.Forms.Button bDown;
        private System.Windows.Forms.Button bReadme;
    }
}
