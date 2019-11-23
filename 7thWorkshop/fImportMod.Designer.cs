/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class fImportMod {
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
            this.tcSource = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.bFolder = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.bIro = new System.Windows.Forms.Button();
            this.txtIro = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.bBatch = new System.Windows.Forms.Button();
            this.txtBatch = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.ofIro = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.tcSource.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcSource
            // 
            this.tcSource.Controls.Add(this.tabPage2);
            this.tcSource.Controls.Add(this.tabPage1);
            this.tcSource.Controls.Add(this.tabPage3);
            this.tcSource.Location = new System.Drawing.Point(13, 13);
            this.tcSource.Name = "tcSource";
            this.tcSource.SelectedIndex = 0;
            this.tcSource.Size = new System.Drawing.Size(375, 78);
            this.tcSource.TabIndex = 0;
            this.tcSource.SelectedIndexChanged += new System.EventHandler(this.tcSource_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.bFolder);
            this.tabPage1.Controls.Add(this.txtFolder);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(367, 52);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "From folder";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // bFolder
            // 
            this.bFolder.Location = new System.Drawing.Point(330, 18);
            this.bFolder.Name = "bFolder";
            this.bFolder.Size = new System.Drawing.Size(25, 20);
            this.bFolder.TabIndex = 1;
            this.bFolder.Text = "...";
            this.bFolder.UseVisualStyleBackColor = true;
            this.bFolder.Click += new System.EventHandler(this.bFolder_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtFolder.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtFolder.Location = new System.Drawing.Point(6, 18);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(318, 20);
            this.txtFolder.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.bIro);
            this.tabPage2.Controls.Add(this.txtIro);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(367, 52);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "From IRO archive";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // bIro
            // 
            this.bIro.Location = new System.Drawing.Point(333, 17);
            this.bIro.Name = "bIro";
            this.bIro.Size = new System.Drawing.Size(25, 20);
            this.bIro.TabIndex = 3;
            this.bIro.Text = "...";
            this.bIro.UseVisualStyleBackColor = true;
            this.bIro.Click += new System.EventHandler(this.bIro_Click);
            // 
            // txtIro
            // 
            this.txtIro.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtIro.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtIro.Location = new System.Drawing.Point(9, 17);
            this.txtIro.Name = "txtIro";
            this.txtIro.Size = new System.Drawing.Size(318, 20);
            this.txtIro.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.bBatch);
            this.tabPage3.Controls.Add(this.txtBatch);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(367, 52);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Batch import";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // bBatch
            // 
            this.bBatch.Location = new System.Drawing.Point(333, 16);
            this.bBatch.Name = "bBatch";
            this.bBatch.Size = new System.Drawing.Size(25, 20);
            this.bBatch.TabIndex = 3;
            this.bBatch.Text = "...";
            this.bBatch.UseVisualStyleBackColor = true;
            this.bBatch.Click += new System.EventHandler(this.bBatch_Click);
            // 
            // txtBatch
            // 
            this.txtBatch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtBatch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtBatch.Location = new System.Drawing.Point(9, 16);
            this.txtBatch.Name = "txtBatch";
            this.txtBatch.Size = new System.Drawing.Size(318, 20);
            this.txtBatch.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bOK);
            this.panel1.Controls.Add(this.bCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 182);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(401, 50);
            this.panel1.TabIndex = 1;
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Location = new System.Drawing.Point(233, 13);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(75, 23);
            this.bOK.TabIndex = 1;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(314, 13);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 0;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(82, 133);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(302, 20);
            this.txtName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 136);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Mod Name:";
            // 
            // flFolder
            // 
            this.flFolder.ShowNewFolderButton = false;
            // 
            // ofIro
            // 
            this.ofIro.Filter = "IRO archives|*.iro";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(286, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "The selected mod file(s) will be copied into the library folder.";
            // 
            // fImportMod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 232);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tcSource);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fImportMod";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Mod";
            this.tcSource.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tcSource;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button bFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button bIro;
        private System.Windows.Forms.TextBox txtIro;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog flFolder;
        private System.Windows.Forms.OpenFileDialog ofIro;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button bBatch;
        private System.Windows.Forms.TextBox txtBatch;
    }
}
