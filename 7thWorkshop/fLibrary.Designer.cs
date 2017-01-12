/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

namespace Iros._7th.Workshop {
    partial class fLibrary {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fLibrary));
            this.TC = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.bGLConfig = new System.Windows.Forms.Button();
            this.cbCompact = new System.Windows.Forms.CheckBox();
            this.bActivateAll = new System.Windows.Forms.Button();
            this.pProfileOuter = new System.Windows.Forms.Panel();
            this.pProfile = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bProfileDetails = new System.Windows.Forms.Button();
            this.bOpenProfile = new System.Windows.Forms.Button();
            this.bLaunch = new System.Windows.Forms.Button();
            this.mLaunch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.launchWithVariableDumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchWithDebugLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bNewProfile = new System.Windows.Forms.Button();
            this.lProfile = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pTagsL = new Iros._7th.Workshop.pTags();
            this.pSearchResultsL = new System.Windows.Forms.Panel();
            this.lSearchL = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bImport = new System.Windows.Forms.Button();
            this.bTagsL = new System.Windows.Forms.Button();
            this.bSearchL = new System.Windows.Forms.Button();
            this.txtSearchL = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pTagsC = new Iros._7th.Workshop.pTags();
            this.pSearchResultsC = new System.Windows.Forms.Panel();
            this.lSearchC = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bTagsC = new System.Windows.Forms.Button();
            this.bSearchC = new System.Windows.Forms.Button();
            this.txtSearchC = new System.Windows.Forms.TextBox();
            this.pMessages = new System.Windows.Forms.Panel();
            this.bClearAllMsg = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.mMod = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.uninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mUpdateType = new System.Windows.Forms.ToolStripMenuItem();
            this.mUpdateNotify = new System.Windows.Forms.ToolStripMenuItem();
            this.mUpdateAuto = new System.Windows.Forms.ToolStripMenuItem();
            this.mUpdateIgnore = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.workshopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDownloadsWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkSubscriptionsNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.openGLDriverConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modGeneratorAssistantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packUnpackiroArchivesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chunkToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TC.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.pProfileOuter.SuspendLayout();
            this.panel2.SuspendLayout();
            this.mLaunch.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.pMessages.SuspendLayout();
            this.mMod.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TC
            // 
            this.TC.Controls.Add(this.tabPage1);
            this.TC.Controls.Add(this.tabPage2);
            this.TC.Controls.Add(this.tabPage3);
            this.TC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TC.Location = new System.Drawing.Point(0, 46);
            this.TC.Margin = new System.Windows.Forms.Padding(6);
            this.TC.Name = "TC";
            this.TC.SelectedIndex = 0;
            this.TC.Size = new System.Drawing.Size(1556, 745);
            this.TC.TabIndex = 0;
            this.TC.SelectedIndexChanged += new System.EventHandler(this.TC_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.bGLConfig);
            this.tabPage1.Controls.Add(this.cbCompact);
            this.tabPage1.Controls.Add(this.bActivateAll);
            this.tabPage1.Controls.Add(this.pProfileOuter);
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Controls.Add(this.lProfile);
            this.tabPage1.Location = new System.Drawing.Point(8, 44);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage1.Size = new System.Drawing.Size(1540, 693);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Active Mods";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // bGLConfig
            // 
            this.bGLConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bGLConfig.Location = new System.Drawing.Point(1267, 10);
            this.bGLConfig.Margin = new System.Windows.Forms.Padding(6);
            this.bGLConfig.Name = "bGLConfig";
            this.bGLConfig.Size = new System.Drawing.Size(252, 50);
            this.bGLConfig.TabIndex = 12;
            this.bGLConfig.Text = "Custom GL Config";
            this.bGLConfig.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bGLConfig.UseVisualStyleBackColor = true;
            this.bGLConfig.Click += new System.EventHandler(this.bGLConfig_Click);
            // 
            // cbCompact
            // 
            this.cbCompact.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCompact.AutoSize = true;
            this.cbCompact.BackColor = System.Drawing.Color.DimGray;
            this.cbCompact.ForeColor = System.Drawing.Color.White;
            this.cbCompact.Location = new System.Drawing.Point(765, 15);
            this.cbCompact.Margin = new System.Windows.Forms.Padding(6);
            this.cbCompact.Name = "cbCompact";
            this.cbCompact.Size = new System.Drawing.Size(187, 34);
            this.cbCompact.TabIndex = 11;
            this.cbCompact.Text = "Compact list";
            this.cbCompact.UseVisualStyleBackColor = false;
            this.cbCompact.CheckedChanged += new System.EventHandler(this.cbCompact_CheckedChanged);
            // 
            // bActivateAll
            // 
            this.bActivateAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bActivateAll.Location = new System.Drawing.Point(990, 10);
            this.bActivateAll.Margin = new System.Windows.Forms.Padding(6);
            this.bActivateAll.Name = "bActivateAll";
            this.bActivateAll.Size = new System.Drawing.Size(252, 50);
            this.bActivateAll.TabIndex = 10;
            this.bActivateAll.Text = "Activate all mods";
            this.bActivateAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bActivateAll.UseVisualStyleBackColor = true;
            this.bActivateAll.Click += new System.EventHandler(this.bActivateAll_Click);
            // 
            // pProfileOuter
            // 
            this.pProfileOuter.AutoScroll = true;
            this.pProfileOuter.BackColor = System.Drawing.SystemColors.Control;
            this.pProfileOuter.Controls.Add(this.pProfile);
            this.pProfileOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pProfileOuter.Location = new System.Drawing.Point(6, 64);
            this.pProfileOuter.Margin = new System.Windows.Forms.Padding(6);
            this.pProfileOuter.Name = "pProfileOuter";
            this.pProfileOuter.Size = new System.Drawing.Size(1528, 544);
            this.pProfileOuter.TabIndex = 7;
            // 
            // pProfile
            // 
            this.pProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pProfile.Location = new System.Drawing.Point(0, 0);
            this.pProfile.Margin = new System.Windows.Forms.Padding(6);
            this.pProfile.Name = "pProfile";
            this.pProfile.Size = new System.Drawing.Size(1528, 171);
            this.pProfile.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DimGray;
            this.panel2.Controls.Add(this.bProfileDetails);
            this.panel2.Controls.Add(this.bOpenProfile);
            this.panel2.Controls.Add(this.bLaunch);
            this.panel2.Controls.Add(this.bNewProfile);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(6, 608);
            this.panel2.Margin = new System.Windows.Forms.Padding(6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1528, 79);
            this.panel2.TabIndex = 9;
            // 
            // bProfileDetails
            // 
            this.bProfileDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bProfileDetails.BackColor = System.Drawing.Color.LightGray;
            this.bProfileDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bProfileDetails.Location = new System.Drawing.Point(809, 13);
            this.bProfileDetails.Margin = new System.Windows.Forms.Padding(6);
            this.bProfileDetails.Name = "bProfileDetails";
            this.bProfileDetails.Size = new System.Drawing.Size(202, 52);
            this.bProfileDetails.TabIndex = 5;
            this.bProfileDetails.Text = "Profile Details";
            this.bProfileDetails.UseVisualStyleBackColor = false;
            this.bProfileDetails.Click += new System.EventHandler(this.bProfileDetails_Click);
            // 
            // bOpenProfile
            // 
            this.bOpenProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOpenProfile.BackColor = System.Drawing.Color.LightGray;
            this.bOpenProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bOpenProfile.Location = new System.Drawing.Point(1312, 13);
            this.bOpenProfile.Margin = new System.Windows.Forms.Padding(6);
            this.bOpenProfile.Name = "bOpenProfile";
            this.bOpenProfile.Size = new System.Drawing.Size(202, 52);
            this.bOpenProfile.TabIndex = 4;
            this.bOpenProfile.Text = "Open Profile";
            this.bOpenProfile.UseVisualStyleBackColor = false;
            this.bOpenProfile.Click += new System.EventHandler(this.bOpenProfile_Click);
            // 
            // bLaunch
            // 
            this.bLaunch.BackColor = System.Drawing.Color.LightGreen;
            this.bLaunch.ContextMenuStrip = this.mLaunch;
            this.bLaunch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bLaunch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bLaunch.Location = new System.Drawing.Point(10, 13);
            this.bLaunch.Margin = new System.Windows.Forms.Padding(6);
            this.bLaunch.Name = "bLaunch";
            this.bLaunch.Size = new System.Drawing.Size(486, 52);
            this.bLaunch.TabIndex = 3;
            this.bLaunch.Text = "Launch Game";
            this.bLaunch.UseVisualStyleBackColor = false;
            this.bLaunch.Click += new System.EventHandler(this.bLaunch_Click);
            // 
            // mLaunch
            // 
            this.mLaunch.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.mLaunch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchWithVariableDumpToolStripMenuItem,
            this.launchWithDebugLogToolStripMenuItem});
            this.mLaunch.Name = "mLaunch";
            this.mLaunch.Size = new System.Drawing.Size(404, 80);
            // 
            // launchWithVariableDumpToolStripMenuItem
            // 
            this.launchWithVariableDumpToolStripMenuItem.Name = "launchWithVariableDumpToolStripMenuItem";
            this.launchWithVariableDumpToolStripMenuItem.Size = new System.Drawing.Size(403, 38);
            this.launchWithVariableDumpToolStripMenuItem.Text = "Launch with variable dump";
            this.launchWithVariableDumpToolStripMenuItem.Click += new System.EventHandler(this.launchWithVariableDumpToolStripMenuItem_Click);
            // 
            // launchWithDebugLogToolStripMenuItem
            // 
            this.launchWithDebugLogToolStripMenuItem.Name = "launchWithDebugLogToolStripMenuItem";
            this.launchWithDebugLogToolStripMenuItem.Size = new System.Drawing.Size(403, 38);
            this.launchWithDebugLogToolStripMenuItem.Text = "Launch with debug log";
            this.launchWithDebugLogToolStripMenuItem.Click += new System.EventHandler(this.launchWithDebugLogToolStripMenuItem_Click);
            // 
            // bNewProfile
            // 
            this.bNewProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bNewProfile.BackColor = System.Drawing.Color.LightGray;
            this.bNewProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bNewProfile.Location = new System.Drawing.Point(1098, 13);
            this.bNewProfile.Margin = new System.Windows.Forms.Padding(6);
            this.bNewProfile.Name = "bNewProfile";
            this.bNewProfile.Size = new System.Drawing.Size(202, 52);
            this.bNewProfile.TabIndex = 0;
            this.bNewProfile.Text = "New Profile";
            this.bNewProfile.UseVisualStyleBackColor = false;
            this.bNewProfile.Click += new System.EventHandler(this.bNewProfile_Click);
            // 
            // lProfile
            // 
            this.lProfile.BackColor = System.Drawing.Color.DimGray;
            this.lProfile.Dock = System.Windows.Forms.DockStyle.Top;
            this.lProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lProfile.ForeColor = System.Drawing.Color.Gainsboro;
            this.lProfile.Location = new System.Drawing.Point(6, 6);
            this.lProfile.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lProfile.Name = "lProfile";
            this.lProfile.Padding = new System.Windows.Forms.Padding(10);
            this.lProfile.Size = new System.Drawing.Size(1528, 58);
            this.lProfile.TabIndex = 6;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.pTagsL);
            this.tabPage2.Controls.Add(this.pSearchResultsL);
            this.tabPage2.Controls.Add(this.lSearchL);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(8, 44);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage2.Size = new System.Drawing.Size(1540, 695);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Library";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // pTagsL
            // 
            this.pTagsL.Location = new System.Drawing.Point(978, 122);
            this.pTagsL.Margin = new System.Windows.Forms.Padding(18, 14, 18, 14);
            this.pTagsL.Name = "pTagsL";
            this.pTagsL.Size = new System.Drawing.Size(485, 468);
            this.pTagsL.TabIndex = 7;
            this.pTagsL.Visible = false;
            this.pTagsL.SelectionChanged += new System.EventHandler(this.pTagsL_SelectionChanged);
            // 
            // pSearchResultsL
            // 
            this.pSearchResultsL.AutoScroll = true;
            this.pSearchResultsL.BackColor = System.Drawing.SystemColors.Control;
            this.pSearchResultsL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pSearchResultsL.Location = new System.Drawing.Point(6, 177);
            this.pSearchResultsL.Margin = new System.Windows.Forms.Padding(6);
            this.pSearchResultsL.Name = "pSearchResultsL";
            this.pSearchResultsL.Size = new System.Drawing.Size(1528, 512);
            this.pSearchResultsL.TabIndex = 1;
            // 
            // lSearchL
            // 
            this.lSearchL.BackColor = System.Drawing.Color.DimGray;
            this.lSearchL.Dock = System.Windows.Forms.DockStyle.Top;
            this.lSearchL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lSearchL.ForeColor = System.Drawing.Color.Gainsboro;
            this.lSearchL.Location = new System.Drawing.Point(6, 125);
            this.lSearchL.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lSearchL.Name = "lSearchL";
            this.lSearchL.Padding = new System.Windows.Forms.Padding(10);
            this.lSearchL.Size = new System.Drawing.Size(1528, 52);
            this.lSearchL.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bImport);
            this.groupBox1.Controls.Add(this.bTagsL);
            this.groupBox1.Controls.Add(this.bSearchL);
            this.groupBox1.Controls.Add(this.txtSearchL);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox1.Size = new System.Drawing.Size(1528, 119);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search";
            // 
            // bImport
            // 
            this.bImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bImport.Location = new System.Drawing.Point(1328, 44);
            this.bImport.Margin = new System.Windows.Forms.Padding(6);
            this.bImport.Name = "bImport";
            this.bImport.Size = new System.Drawing.Size(170, 50);
            this.bImport.TabIndex = 3;
            this.bImport.Text = "Import...";
            this.bImport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bImport.UseVisualStyleBackColor = true;
            this.bImport.Click += new System.EventHandler(this.bImport_Click);
            // 
            // bTagsL
            // 
            this.bTagsL.Location = new System.Drawing.Point(882, 44);
            this.bTagsL.Margin = new System.Windows.Forms.Padding(6);
            this.bTagsL.Name = "bTagsL";
            this.bTagsL.Size = new System.Drawing.Size(170, 50);
            this.bTagsL.TabIndex = 2;
            this.bTagsL.Text = "Tags...";
            this.bTagsL.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bTagsL.UseVisualStyleBackColor = true;
            this.bTagsL.Click += new System.EventHandler(this.bTagsL_Click);
            // 
            // bSearchL
            // 
            this.bSearchL.Image = global::Iros._7th.Workshop.Properties.Resources.SearchFolderHS;
            this.bSearchL.Location = new System.Drawing.Point(700, 44);
            this.bSearchL.Margin = new System.Windows.Forms.Padding(6);
            this.bSearchL.Name = "bSearchL";
            this.bSearchL.Size = new System.Drawing.Size(170, 50);
            this.bSearchL.TabIndex = 1;
            this.bSearchL.Text = "Search";
            this.bSearchL.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bSearchL.UseVisualStyleBackColor = true;
            this.bSearchL.Click += new System.EventHandler(this.bSearchL_Click);
            // 
            // txtSearchL
            // 
            this.txtSearchL.AcceptsReturn = true;
            this.txtSearchL.Location = new System.Drawing.Point(12, 48);
            this.txtSearchL.Margin = new System.Windows.Forms.Padding(6);
            this.txtSearchL.Name = "txtSearchL";
            this.txtSearchL.Size = new System.Drawing.Size(672, 37);
            this.txtSearchL.TabIndex = 0;
            this.txtSearchL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearchL_KeyPress);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.pTagsC);
            this.tabPage3.Controls.Add(this.pSearchResultsC);
            this.tabPage3.Controls.Add(this.lSearchC);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Location = new System.Drawing.Point(8, 44);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage3.Size = new System.Drawing.Size(1540, 695);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Catalog";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pTagsC
            // 
            this.pTagsC.Location = new System.Drawing.Point(968, 120);
            this.pTagsC.Margin = new System.Windows.Forms.Padding(22, 16, 22, 16);
            this.pTagsC.Name = "pTagsC";
            this.pTagsC.Size = new System.Drawing.Size(502, 464);
            this.pTagsC.TabIndex = 8;
            this.pTagsC.Visible = false;
            this.pTagsC.SelectionChanged += new System.EventHandler(this.pTagsC_SelectionChanged);
            // 
            // pSearchResultsC
            // 
            this.pSearchResultsC.AutoScroll = true;
            this.pSearchResultsC.BackColor = System.Drawing.SystemColors.Control;
            this.pSearchResultsC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pSearchResultsC.Location = new System.Drawing.Point(6, 177);
            this.pSearchResultsC.Margin = new System.Windows.Forms.Padding(6);
            this.pSearchResultsC.Name = "pSearchResultsC";
            this.pSearchResultsC.Size = new System.Drawing.Size(1528, 512);
            this.pSearchResultsC.TabIndex = 3;
            // 
            // lSearchC
            // 
            this.lSearchC.BackColor = System.Drawing.Color.DimGray;
            this.lSearchC.Dock = System.Windows.Forms.DockStyle.Top;
            this.lSearchC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lSearchC.ForeColor = System.Drawing.Color.Gainsboro;
            this.lSearchC.Location = new System.Drawing.Point(6, 125);
            this.lSearchC.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lSearchC.Name = "lSearchC";
            this.lSearchC.Padding = new System.Windows.Forms.Padding(10);
            this.lSearchC.Size = new System.Drawing.Size(1528, 52);
            this.lSearchC.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bTagsC);
            this.groupBox2.Controls.Add(this.bSearchC);
            this.groupBox2.Controls.Add(this.txtSearchC);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox2.Size = new System.Drawing.Size(1528, 119);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Search";
            // 
            // bTagsC
            // 
            this.bTagsC.Location = new System.Drawing.Point(882, 44);
            this.bTagsC.Margin = new System.Windows.Forms.Padding(6);
            this.bTagsC.Name = "bTagsC";
            this.bTagsC.Size = new System.Drawing.Size(170, 50);
            this.bTagsC.TabIndex = 2;
            this.bTagsC.Text = "Tags...";
            this.bTagsC.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bTagsC.UseVisualStyleBackColor = true;
            this.bTagsC.Click += new System.EventHandler(this.bTagsC_Click);
            // 
            // bSearchC
            // 
            this.bSearchC.Image = global::Iros._7th.Workshop.Properties.Resources.SearchFolderHS;
            this.bSearchC.Location = new System.Drawing.Point(700, 44);
            this.bSearchC.Margin = new System.Windows.Forms.Padding(6);
            this.bSearchC.Name = "bSearchC";
            this.bSearchC.Size = new System.Drawing.Size(170, 50);
            this.bSearchC.TabIndex = 1;
            this.bSearchC.Text = "Search";
            this.bSearchC.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bSearchC.UseVisualStyleBackColor = true;
            this.bSearchC.Click += new System.EventHandler(this.bSearchC_Click);
            // 
            // txtSearchC
            // 
            this.txtSearchC.AcceptsReturn = true;
            this.txtSearchC.Location = new System.Drawing.Point(12, 48);
            this.txtSearchC.Margin = new System.Windows.Forms.Padding(6);
            this.txtSearchC.Name = "txtSearchC";
            this.txtSearchC.Size = new System.Drawing.Size(672, 37);
            this.txtSearchC.TabIndex = 0;
            this.txtSearchC.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearchC_KeyPress);
            // 
            // pMessages
            // 
            this.pMessages.AutoScroll = true;
            this.pMessages.Controls.Add(this.bClearAllMsg);
            this.pMessages.Dock = System.Windows.Forms.DockStyle.Right;
            this.pMessages.Location = new System.Drawing.Point(1556, 46);
            this.pMessages.Margin = new System.Windows.Forms.Padding(6);
            this.pMessages.Name = "pMessages";
            this.pMessages.Size = new System.Drawing.Size(396, 745);
            this.pMessages.TabIndex = 1;
            // 
            // bClearAllMsg
            // 
            this.bClearAllMsg.Dock = System.Windows.Forms.DockStyle.Top;
            this.bClearAllMsg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bClearAllMsg.Location = new System.Drawing.Point(0, 0);
            this.bClearAllMsg.Margin = new System.Windows.Forms.Padding(6);
            this.bClearAllMsg.Name = "bClearAllMsg";
            this.bClearAllMsg.Size = new System.Drawing.Size(396, 44);
            this.bClearAllMsg.TabIndex = 0;
            this.bClearAllMsg.Text = "Clear All";
            this.bClearAllMsg.UseVisualStyleBackColor = true;
            this.bClearAllMsg.Click += new System.EventHandler(this.bClearAllMsg_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip.Location = new System.Drawing.Point(0, 791);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            this.statusStrip.Size = new System.Drawing.Size(1952, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";
            // 
            // mMod
            // 
            this.mMod.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.mMod.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uninstallToolStripMenuItem,
            this.mUpdateType});
            this.mMod.Name = "mMod";
            this.mMod.Size = new System.Drawing.Size(251, 80);
            this.mMod.Opening += new System.ComponentModel.CancelEventHandler(this.mMod_Opening);
            // 
            // uninstallToolStripMenuItem
            // 
            this.uninstallToolStripMenuItem.Name = "uninstallToolStripMenuItem";
            this.uninstallToolStripMenuItem.Size = new System.Drawing.Size(250, 38);
            this.uninstallToolStripMenuItem.Text = "Uninstall";
            this.uninstallToolStripMenuItem.Click += new System.EventHandler(this.uninstallToolStripMenuItem_Click);
            // 
            // mUpdateType
            // 
            this.mUpdateType.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mUpdateNotify,
            this.mUpdateAuto,
            this.mUpdateIgnore});
            this.mUpdateType.Name = "mUpdateType";
            this.mUpdateType.Size = new System.Drawing.Size(250, 38);
            this.mUpdateType.Text = "Update Type";
            // 
            // mUpdateNotify
            // 
            this.mUpdateNotify.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mUpdateNotify.Name = "mUpdateNotify";
            this.mUpdateNotify.Size = new System.Drawing.Size(341, 38);
            this.mUpdateNotify.Tag = "0";
            this.mUpdateNotify.Text = "Notify me";
            this.mUpdateNotify.Click += new System.EventHandler(this.mUpdateNotify_Click);
            // 
            // mUpdateAuto
            // 
            this.mUpdateAuto.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mUpdateAuto.Name = "mUpdateAuto";
            this.mUpdateAuto.Size = new System.Drawing.Size(341, 38);
            this.mUpdateAuto.Tag = "1";
            this.mUpdateAuto.Text = "Update automatically";
            this.mUpdateAuto.Click += new System.EventHandler(this.mUpdateNotify_Click);
            // 
            // mUpdateIgnore
            // 
            this.mUpdateIgnore.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mUpdateIgnore.Name = "mUpdateIgnore";
            this.mUpdateIgnore.Size = new System.Drawing.Size(341, 38);
            this.mUpdateIgnore.Tag = "2";
            this.mUpdateIgnore.Text = "Ignore updates";
            this.mUpdateIgnore.Click += new System.EventHandler(this.mUpdateNotify_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.workshopToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.menuStrip1.Size = new System.Drawing.Size(1952, 46);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // workshopToolStripMenuItem
            // 
            this.workshopToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDownloadsWindowToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.checkSubscriptionsNowToolStripMenuItem,
            this.toolStripMenuItem2,
            this.openGLDriverConfigurationToolStripMenuItem,
            this.modGeneratorAssistantToolStripMenuItem,
            this.packUnpackiroArchivesToolStripMenuItem,
            this.chunkToolToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.workshopToolStripMenuItem.Name = "workshopToolStripMenuItem";
            this.workshopToolStripMenuItem.Size = new System.Drawing.Size(134, 38);
            this.workshopToolStripMenuItem.Text = "&Workshop";
            // 
            // showDownloadsWindowToolStripMenuItem
            // 
            this.showDownloadsWindowToolStripMenuItem.Name = "showDownloadsWindowToolStripMenuItem";
            this.showDownloadsWindowToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.showDownloadsWindowToolStripMenuItem.Text = "Show Downloads Window";
            this.showDownloadsWindowToolStripMenuItem.Click += new System.EventHandler(this.showDownloadsWindowToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // checkSubscriptionsNowToolStripMenuItem
            // 
            this.checkSubscriptionsNowToolStripMenuItem.Name = "checkSubscriptionsNowToolStripMenuItem";
            this.checkSubscriptionsNowToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.checkSubscriptionsNowToolStripMenuItem.Text = "Check subscriptions now";
            this.checkSubscriptionsNowToolStripMenuItem.Click += new System.EventHandler(this.checkSubscriptionsNowToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(416, 6);
            // 
            // openGLDriverConfigurationToolStripMenuItem
            // 
            this.openGLDriverConfigurationToolStripMenuItem.Name = "openGLDriverConfigurationToolStripMenuItem";
            this.openGLDriverConfigurationToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.openGLDriverConfigurationToolStripMenuItem.Text = "OpenGL driver configuration";
            this.openGLDriverConfigurationToolStripMenuItem.Click += new System.EventHandler(this.openGLDriverConfigurationToolStripMenuItem_Click);
            // 
            // modGeneratorAssistantToolStripMenuItem
            // 
            this.modGeneratorAssistantToolStripMenuItem.Name = "modGeneratorAssistantToolStripMenuItem";
            this.modGeneratorAssistantToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.modGeneratorAssistantToolStripMenuItem.Text = "Mod Generator Assistant";
            this.modGeneratorAssistantToolStripMenuItem.Click += new System.EventHandler(this.modGeneratorAssistantToolStripMenuItem_Click);
            // 
            // packUnpackiroArchivesToolStripMenuItem
            // 
            this.packUnpackiroArchivesToolStripMenuItem.Name = "packUnpackiroArchivesToolStripMenuItem";
            this.packUnpackiroArchivesToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.packUnpackiroArchivesToolStripMenuItem.Text = "Pack/Unpack .iro archives";
            this.packUnpackiroArchivesToolStripMenuItem.Click += new System.EventHandler(this.packUnpackiroArchivesToolStripMenuItem_Click);
            // 
            // chunkToolToolStripMenuItem
            // 
            this.chunkToolToolStripMenuItem.Name = "chunkToolToolStripMenuItem";
            this.chunkToolToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.chunkToolToolStripMenuItem.Text = "Chunk tool";
            this.chunkToolToolStripMenuItem.Click += new System.EventHandler(this.chunkToolToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(416, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // fLibrary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1952, 813);
            this.Controls.Add(this.TC);
            this.Controls.Add(this.pMessages);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "fLibrary";
            this.Text = "7thHeaven";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fLibrary_FormClosed);
            this.Load += new System.EventHandler(this.fLibrary_Load);
            this.TC.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.pProfileOuter.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.mLaunch.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.pMessages.ResumeLayout(false);
            this.mMod.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl TC;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel pMessages;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bSearchL;
        private System.Windows.Forms.TextBox txtSearchL;
        private System.Windows.Forms.Button bTagsL;
        private System.Windows.Forms.Panel pSearchResultsL;
        private System.Windows.Forms.Panel pSearchResultsC;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button bTagsC;
        private System.Windows.Forms.Button bSearchC;
        private System.Windows.Forms.TextBox txtSearchC;
        private System.Windows.Forms.ContextMenuStrip mMod;
        private System.Windows.Forms.ToolStripMenuItem uninstallToolStripMenuItem;
        private System.Windows.Forms.Label lSearchC;
        private System.Windows.Forms.Label lSearchL;
        private pTags pTagsL;
        private pTags pTagsC;
        private System.Windows.Forms.ToolStripMenuItem mUpdateType;
        private System.Windows.Forms.ToolStripMenuItem mUpdateNotify;
        private System.Windows.Forms.ToolStripMenuItem mUpdateAuto;
        private System.Windows.Forms.ToolStripMenuItem mUpdateIgnore;
        private System.Windows.Forms.Label lProfile;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button bNewProfile;
        private System.Windows.Forms.Panel pProfileOuter;
        private System.Windows.Forms.Button bLaunch;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem workshopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDownloadsWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button bOpenProfile;
        private System.Windows.Forms.Button bImport;
        private System.Windows.Forms.ToolStripMenuItem modGeneratorAssistantToolStripMenuItem;
        private System.Windows.Forms.Button bClearAllMsg;
        private System.Windows.Forms.Panel pProfile;
        private System.Windows.Forms.Button bActivateAll;
        private System.Windows.Forms.ToolStripMenuItem checkSubscriptionsNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem packUnpackiroArchivesToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbCompact;
        private System.Windows.Forms.ContextMenuStrip mLaunch;
        private System.Windows.Forms.ToolStripMenuItem launchWithVariableDumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chunkToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem launchWithDebugLogToolStripMenuItem;
        private System.Windows.Forms.Button bGLConfig;
        private System.Windows.Forms.ToolStripMenuItem openGLDriverConfigurationToolStripMenuItem;
        private System.Windows.Forms.Button bProfileDetails;
    }
}

