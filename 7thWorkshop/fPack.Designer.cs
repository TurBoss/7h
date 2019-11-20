/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class fPack {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fPack));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cbCompress = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.bPack = new System.Windows.Forms.Button();
            this.txtIroDest = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bIroDest = new System.Windows.Forms.Button();
            this.txtSourceF = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bSourceF = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.bUnpack = new System.Windows.Forms.Button();
            this.txtDestF = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.bDestF = new System.Windows.Forms.Button();
            this.txtSourceIro = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.bSourceIro = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.txtPatchSave = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.bPatchSave = new System.Windows.Forms.Button();
            this.cbPatchCompress = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.bGoPatch = new System.Windows.Forms.Button();
            this.txtPatchNew = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.bPatchNew = new System.Windows.Forms.Button();
            this.txtPatchOrig = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.bPatchOrig = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtPatchDelete = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtPatchASrc = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.bPatchASrc = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPatchASave = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.bPatchASave = new System.Windows.Forms.Button();
            this.cbPatchACompress = new System.Windows.Forms.ComboBox();
            this.bGoPatchA = new System.Windows.Forms.Button();
            this.PB = new System.Windows.Forms.ProgressBar();
            this.flFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.ofIro = new System.Windows.Forms.OpenFileDialog();
            this.sfIro = new System.Windows.Forms.SaveFileDialog();
            this.lStatus = new System.Windows.Forms.Label();
            this.sfIrop = new System.Windows.Forms.SaveFileDialog();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(556, 201);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cbCompress);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.bPack);
            this.tabPage1.Controls.Add(this.txtIroDest);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.bIroDest);
            this.tabPage1.Controls.Add(this.txtSourceF);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.bSourceF);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(548, 175);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Pack IRO";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cbCompress
            // 
            this.cbCompress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCompress.FormattingEnabled = true;
            this.cbCompress.Location = new System.Drawing.Point(107, 74);
            this.cbCompress.Margin = new System.Windows.Forms.Padding(2);
            this.cbCompress.Name = "cbCompress";
            this.cbCompress.Size = new System.Drawing.Size(142, 21);
            this.cbCompress.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Compress:";
            // 
            // bPack
            // 
            this.bPack.Location = new System.Drawing.Point(107, 108);
            this.bPack.Name = "bPack";
            this.bPack.Size = new System.Drawing.Size(75, 23);
            this.bPack.TabIndex = 12;
            this.bPack.Text = "Go";
            this.bPack.UseVisualStyleBackColor = true;
            this.bPack.Click += new System.EventHandler(this.bPack_Click);
            // 
            // txtIroDest
            // 
            this.txtIroDest.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtIroDest.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtIroDest.Location = new System.Drawing.Point(107, 43);
            this.txtIroDest.Name = "txtIroDest";
            this.txtIroDest.Size = new System.Drawing.Size(246, 20);
            this.txtIroDest.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Save as IRO:";
            // 
            // bIroDest
            // 
            this.bIroDest.Location = new System.Drawing.Point(364, 43);
            this.bIroDest.Name = "bIroDest";
            this.bIroDest.Size = new System.Drawing.Size(26, 20);
            this.bIroDest.TabIndex = 9;
            this.bIroDest.Text = "...";
            this.bIroDest.UseVisualStyleBackColor = true;
            this.bIroDest.Click += new System.EventHandler(this.bIroDest_Click);
            // 
            // txtSourceF
            // 
            this.txtSourceF.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtSourceF.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtSourceF.Location = new System.Drawing.Point(107, 17);
            this.txtSourceF.Name = "txtSourceF";
            this.txtSourceF.Size = new System.Drawing.Size(246, 20);
            this.txtSourceF.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Source Folder:";
            // 
            // bSourceF
            // 
            this.bSourceF.Location = new System.Drawing.Point(364, 17);
            this.bSourceF.Name = "bSourceF";
            this.bSourceF.Size = new System.Drawing.Size(26, 20);
            this.bSourceF.TabIndex = 6;
            this.bSourceF.Text = "...";
            this.bSourceF.UseVisualStyleBackColor = true;
            this.bSourceF.Click += new System.EventHandler(this.bSourceF_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.bUnpack);
            this.tabPage2.Controls.Add(this.txtDestF);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.bDestF);
            this.tabPage2.Controls.Add(this.txtSourceIro);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.bSourceIro);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(548, 175);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Unpack IRO";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // bUnpack
            // 
            this.bUnpack.Location = new System.Drawing.Point(114, 77);
            this.bUnpack.Name = "bUnpack";
            this.bUnpack.Size = new System.Drawing.Size(75, 23);
            this.bUnpack.TabIndex = 19;
            this.bUnpack.Text = "Go";
            this.bUnpack.UseVisualStyleBackColor = true;
            this.bUnpack.Click += new System.EventHandler(this.bUnpack_Click);
            // 
            // txtDestF
            // 
            this.txtDestF.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtDestF.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtDestF.Location = new System.Drawing.Point(114, 40);
            this.txtDestF.Name = "txtDestF";
            this.txtDestF.Size = new System.Drawing.Size(246, 20);
            this.txtDestF.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Output  Folder:";
            // 
            // bDestF
            // 
            this.bDestF.Location = new System.Drawing.Point(371, 40);
            this.bDestF.Name = "bDestF";
            this.bDestF.Size = new System.Drawing.Size(26, 20);
            this.bDestF.TabIndex = 16;
            this.bDestF.Text = "...";
            this.bDestF.UseVisualStyleBackColor = true;
            this.bDestF.Click += new System.EventHandler(this.bDestF_Click);
            // 
            // txtSourceIro
            // 
            this.txtSourceIro.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtSourceIro.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtSourceIro.Location = new System.Drawing.Point(114, 14);
            this.txtSourceIro.Name = "txtSourceIro";
            this.txtSourceIro.Size = new System.Drawing.Size(246, 20);
            this.txtSourceIro.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Source IRO:";
            // 
            // bSourceIro
            // 
            this.bSourceIro.Location = new System.Drawing.Point(371, 14);
            this.bSourceIro.Name = "bSourceIro";
            this.bSourceIro.Size = new System.Drawing.Size(26, 20);
            this.bSourceIro.TabIndex = 13;
            this.bSourceIro.Text = "...";
            this.bSourceIro.UseVisualStyleBackColor = true;
            this.bSourceIro.Click += new System.EventHandler(this.bSourceIro_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.txtPatchSave);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.bPatchSave);
            this.tabPage3.Controls.Add(this.cbPatchCompress);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.bGoPatch);
            this.tabPage3.Controls.Add(this.txtPatchNew);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.bPatchNew);
            this.tabPage3.Controls.Add(this.txtPatchOrig);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.bPatchOrig);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(548, 175);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Patch IRO";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // txtPatchSave
            // 
            this.txtPatchSave.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtPatchSave.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtPatchSave.Location = new System.Drawing.Point(116, 73);
            this.txtPatchSave.Name = "txtPatchSave";
            this.txtPatchSave.Size = new System.Drawing.Size(246, 20);
            this.txtPatchSave.TabIndex = 26;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(26, 76);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Save as IROP:";
            // 
            // bPatchSave
            // 
            this.bPatchSave.Location = new System.Drawing.Point(374, 73);
            this.bPatchSave.Name = "bPatchSave";
            this.bPatchSave.Size = new System.Drawing.Size(26, 20);
            this.bPatchSave.TabIndex = 24;
            this.bPatchSave.Text = "...";
            this.bPatchSave.UseVisualStyleBackColor = true;
            this.bPatchSave.Click += new System.EventHandler(this.bPatchSave_Click);
            // 
            // cbPatchCompress
            // 
            this.cbPatchCompress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPatchCompress.FormattingEnabled = true;
            this.cbPatchCompress.Location = new System.Drawing.Point(116, 106);
            this.cbPatchCompress.Margin = new System.Windows.Forms.Padding(2);
            this.cbPatchCompress.Name = "cbPatchCompress";
            this.cbPatchCompress.Size = new System.Drawing.Size(142, 21);
            this.cbPatchCompress.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 108);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Compress:";
            // 
            // bGoPatch
            // 
            this.bGoPatch.Location = new System.Drawing.Point(116, 140);
            this.bGoPatch.Name = "bGoPatch";
            this.bGoPatch.Size = new System.Drawing.Size(75, 23);
            this.bGoPatch.TabIndex = 21;
            this.bGoPatch.Text = "Go";
            this.bGoPatch.UseVisualStyleBackColor = true;
            this.bGoPatch.Click += new System.EventHandler(this.bGoPatch_Click);
            // 
            // txtPatchNew
            // 
            this.txtPatchNew.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtPatchNew.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtPatchNew.Location = new System.Drawing.Point(116, 46);
            this.txtPatchNew.Name = "txtPatchNew";
            this.txtPatchNew.Size = new System.Drawing.Size(246, 20);
            this.txtPatchNew.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 49);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "New IRO:";
            // 
            // bPatchNew
            // 
            this.bPatchNew.Location = new System.Drawing.Point(374, 46);
            this.bPatchNew.Name = "bPatchNew";
            this.bPatchNew.Size = new System.Drawing.Size(26, 20);
            this.bPatchNew.TabIndex = 18;
            this.bPatchNew.Text = "...";
            this.bPatchNew.UseVisualStyleBackColor = true;
            this.bPatchNew.Click += new System.EventHandler(this.bPatchNew_Click);
            // 
            // txtPatchOrig
            // 
            this.txtPatchOrig.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtPatchOrig.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtPatchOrig.Location = new System.Drawing.Point(116, 20);
            this.txtPatchOrig.Name = "txtPatchOrig";
            this.txtPatchOrig.Size = new System.Drawing.Size(246, 20);
            this.txtPatchOrig.TabIndex = 17;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Original IRO:";
            // 
            // bPatchOrig
            // 
            this.bPatchOrig.Location = new System.Drawing.Point(374, 20);
            this.bPatchOrig.Name = "bPatchOrig";
            this.bPatchOrig.Size = new System.Drawing.Size(26, 20);
            this.bPatchOrig.TabIndex = 15;
            this.bPatchOrig.Text = "...";
            this.bPatchOrig.UseVisualStyleBackColor = true;
            this.bPatchOrig.Click += new System.EventHandler(this.bPatchOrig_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.txtPatchDelete);
            this.tabPage4.Controls.Add(this.label13);
            this.tabPage4.Controls.Add(this.txtPatchASrc);
            this.tabPage4.Controls.Add(this.label12);
            this.tabPage4.Controls.Add(this.bPatchASrc);
            this.tabPage4.Controls.Add(this.label11);
            this.tabPage4.Controls.Add(this.txtPatchASave);
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.bPatchASave);
            this.tabPage4.Controls.Add(this.cbPatchACompress);
            this.tabPage4.Controls.Add(this.bGoPatchA);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage4.Size = new System.Drawing.Size(548, 175);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Patch IRO (Advanced)";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtPatchDelete
            // 
            this.txtPatchDelete.Location = new System.Drawing.Point(282, 30);
            this.txtPatchDelete.Margin = new System.Windows.Forms.Padding(2);
            this.txtPatchDelete.Multiline = true;
            this.txtPatchDelete.Name = "txtPatchDelete";
            this.txtPatchDelete.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPatchDelete.Size = new System.Drawing.Size(261, 143);
            this.txtPatchDelete.TabIndex = 37;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(368, 10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(72, 13);
            this.label13.TabIndex = 36;
            this.label13.Text = "Files to delete";
            // 
            // txtPatchASrc
            // 
            this.txtPatchASrc.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtPatchASrc.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtPatchASrc.Location = new System.Drawing.Point(84, 46);
            this.txtPatchASrc.Name = "txtPatchASrc";
            this.txtPatchASrc.Size = new System.Drawing.Size(102, 20);
            this.txtPatchASrc.TabIndex = 35;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 49);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(76, 13);
            this.label12.TabIndex = 34;
            this.label12.Text = "Source Folder:";
            // 
            // bPatchASrc
            // 
            this.bPatchASrc.Location = new System.Drawing.Point(190, 45);
            this.bPatchASrc.Name = "bPatchASrc";
            this.bPatchASrc.Size = new System.Drawing.Size(26, 20);
            this.bPatchASrc.TabIndex = 33;
            this.bPatchASrc.Text = "...";
            this.bPatchASrc.UseVisualStyleBackColor = true;
            this.bPatchASrc.Click += new System.EventHandler(this.bPatchASrc_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 116);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Compress:";
            // 
            // txtPatchASave
            // 
            this.txtPatchASave.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtPatchASave.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtPatchASave.Location = new System.Drawing.Point(84, 81);
            this.txtPatchASave.Name = "txtPatchASave";
            this.txtPatchASave.Size = new System.Drawing.Size(102, 20);
            this.txtPatchASave.TabIndex = 31;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 84);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Save as IROP:";
            // 
            // bPatchASave
            // 
            this.bPatchASave.Location = new System.Drawing.Point(190, 80);
            this.bPatchASave.Name = "bPatchASave";
            this.bPatchASave.Size = new System.Drawing.Size(26, 20);
            this.bPatchASave.TabIndex = 29;
            this.bPatchASave.Text = "...";
            this.bPatchASave.UseVisualStyleBackColor = true;
            this.bPatchASave.Click += new System.EventHandler(this.bPatchASave_Click);
            // 
            // cbPatchACompress
            // 
            this.cbPatchACompress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPatchACompress.FormattingEnabled = true;
            this.cbPatchACompress.Location = new System.Drawing.Point(84, 113);
            this.cbPatchACompress.Margin = new System.Windows.Forms.Padding(2);
            this.cbPatchACompress.Name = "cbPatchACompress";
            this.cbPatchACompress.Size = new System.Drawing.Size(134, 21);
            this.cbPatchACompress.TabIndex = 28;
            // 
            // bGoPatchA
            // 
            this.bGoPatchA.Location = new System.Drawing.Point(106, 148);
            this.bGoPatchA.Name = "bGoPatchA";
            this.bGoPatchA.Size = new System.Drawing.Size(75, 23);
            this.bGoPatchA.TabIndex = 27;
            this.bGoPatchA.Text = "Go";
            this.bGoPatchA.UseVisualStyleBackColor = true;
            this.bGoPatchA.Click += new System.EventHandler(this.bGoPatchA_Click);
            // 
            // PB
            // 
            this.PB.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PB.Location = new System.Drawing.Point(0, 213);
            this.PB.Name = "PB";
            this.PB.Size = new System.Drawing.Size(556, 23);
            this.PB.TabIndex = 2;
            // 
            // ofIro
            // 
            this.ofIro.Filter = "IRO files|*.iro";
            // 
            // sfIro
            // 
            this.sfIro.Filter = "IRO files|*.iro";
            // 
            // lStatus
            // 
            this.lStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lStatus.Location = new System.Drawing.Point(0, 201);
            this.lStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lStatus.Name = "lStatus";
            this.lStatus.Size = new System.Drawing.Size(556, 12);
            this.lStatus.TabIndex = 3;
            // 
            // sfIrop
            // 
            this.sfIrop.Filter = "IRO patches|*.irop";
            // 
            // fPack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 236);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.lStatus);
            this.Controls.Add(this.PB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fPack";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IRO Tools";
            this.Load += new System.EventHandler(this.fPack_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ProgressBar PB;
        private System.Windows.Forms.FolderBrowserDialog flFolder;
        private System.Windows.Forms.OpenFileDialog ofIro;
        private System.Windows.Forms.SaveFileDialog sfIro;
        private System.Windows.Forms.TextBox txtIroDest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bIroDest;
        private System.Windows.Forms.TextBox txtSourceF;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bSourceF;
        private System.Windows.Forms.Button bPack;
        private System.Windows.Forms.Button bUnpack;
        private System.Windows.Forms.TextBox txtDestF;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bDestF;
        private System.Windows.Forms.TextBox txtSourceIro;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button bSourceIro;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ComboBox cbCompress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lStatus;
        private System.Windows.Forms.TextBox txtPatchSave;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button bPatchSave;
        private System.Windows.Forms.ComboBox cbPatchCompress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button bGoPatch;
        private System.Windows.Forms.TextBox txtPatchNew;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bPatchNew;
        private System.Windows.Forms.TextBox txtPatchOrig;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button bPatchOrig;
        private System.Windows.Forms.SaveFileDialog sfIrop;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox txtPatchDelete;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtPatchASrc;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button bPatchASrc;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtPatchASave;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button bPatchASave;
        private System.Windows.Forms.ComboBox cbPatchACompress;
        private System.Windows.Forms.Button bGoPatchA;
    }
}
