namespace RyanSync
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lstServer = new System.Windows.Forms.ListBox();
            this.lstFolder = new System.Windows.Forms.ListBox();
            this.btnSync = new System.Windows.Forms.Button();
            this.lblFrame = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.pgbUpdateProgress = new System.Windows.Forms.ProgressBar();
            this.lblNotification = new System.Windows.Forms.Label();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.cbxForceConnection = new System.Windows.Forms.CheckBox();
            this.cbxForceSync = new System.Windows.Forms.CheckBox();
            this.btnEject = new System.Windows.Forms.Button();
            this.myNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmuServer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmuServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstServer
            // 
            this.lstServer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstServer.ContextMenuStrip = this.cmuServer;
            this.lstServer.FormattingEnabled = true;
            this.lstServer.Location = new System.Drawing.Point(12, 31);
            this.lstServer.Name = "lstServer";
            this.lstServer.Size = new System.Drawing.Size(319, 303);
            this.lstServer.TabIndex = 0;
            // 
            // lstFolder
            // 
            this.lstFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFolder.FormattingEnabled = true;
            this.lstFolder.Location = new System.Drawing.Point(337, 31);
            this.lstFolder.Name = "lstFolder";
            this.lstFolder.Size = new System.Drawing.Size(319, 303);
            this.lstFolder.TabIndex = 1;
            // 
            // btnSync
            // 
            this.btnSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSync.Location = new System.Drawing.Point(570, 340);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(86, 27);
            this.btnSync.TabIndex = 2;
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // lblFrame
            // 
            this.lblFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrame.AutoSize = true;
            this.lblFrame.Location = new System.Drawing.Point(338, 6);
            this.lblFrame.Name = "lblFrame";
            this.lblFrame.Size = new System.Drawing.Size(68, 13);
            this.lblFrame.TabIndex = 3;
            this.lblFrame.Text = "Digital Frame";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(9, 11);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(38, 13);
            this.lblServer.TabIndex = 4;
            this.lblServer.Text = "Server";
            // 
            // pgbUpdateProgress
            // 
            this.pgbUpdateProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pgbUpdateProgress.Location = new System.Drawing.Point(337, 348);
            this.pgbUpdateProgress.Name = "pgbUpdateProgress";
            this.pgbUpdateProgress.Size = new System.Drawing.Size(227, 19);
            this.pgbUpdateProgress.TabIndex = 5;
            // 
            // lblNotification
            // 
            this.lblNotification.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblNotification.AutoSize = true;
            this.lblNotification.Location = new System.Drawing.Point(9, 360);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(70, 13);
            this.lblNotification.TabIndex = 6;
            this.lblNotification.Text = "lblNotification";
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 900000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // cbxForceConnection
            // 
            this.cbxForceConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxForceConnection.AutoSize = true;
            this.cbxForceConnection.Location = new System.Drawing.Point(158, 336);
            this.cbxForceConnection.Name = "cbxForceConnection";
            this.cbxForceConnection.Size = new System.Drawing.Size(152, 17);
            this.cbxForceConnection.TabIndex = 7;
            this.cbxForceConnection.Text = "Manually Connect to frame";
            this.cbxForceConnection.UseVisualStyleBackColor = true;
            this.cbxForceConnection.CheckedChanged += new System.EventHandler(this.cbxForceConnection_CheckedChanged);
            // 
            // cbxForceSync
            // 
            this.cbxForceSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxForceSync.AutoSize = true;
            this.cbxForceSync.Location = new System.Drawing.Point(12, 336);
            this.cbxForceSync.Name = "cbxForceSync";
            this.cbxForceSync.Size = new System.Drawing.Size(114, 17);
            this.cbxForceSync.TabIndex = 8;
            this.cbxForceSync.Text = "Force Server Sync";
            this.cbxForceSync.UseVisualStyleBackColor = true;
            // 
            // btnEject
            // 
            this.btnEject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEject.Location = new System.Drawing.Point(256, 355);
            this.btnEject.Name = "btnEject";
            this.btnEject.Size = new System.Drawing.Size(75, 23);
            this.btnEject.TabIndex = 9;
            this.btnEject.Text = "Eject";
            this.btnEject.UseVisualStyleBackColor = true;
            this.btnEject.Click += new System.EventHandler(this.btnEject_Click);
            // 
            // myNotifyIcon
            // 
            this.myNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("myNotifyIcon.Icon")));
            this.myNotifyIcon.Text = "Sync Pictures";
            this.myNotifyIcon.Click += new System.EventHandler(this.myNotifyIcon_Click);
            // 
            // cmuServer
            // 
            this.cmuServer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.cmuServer.Name = "cmuServer";
            this.cmuServer.Size = new System.Drawing.Size(117, 48);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.viewToolStripMenuItem.Text = "View";
            this.viewToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 379);
            this.Controls.Add(this.btnEject);
            this.Controls.Add(this.cbxForceSync);
            this.Controls.Add(this.cbxForceConnection);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.pgbUpdateProgress);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.lblFrame);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.lstFolder);
            this.Controls.Add(this.lstServer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(670, 388);
            this.Name = "frmMain";
            this.Text = "Sync Pictures";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.cmuServer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstServer;
        private System.Windows.Forms.ListBox lstFolder;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Label lblFrame;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.ProgressBar pgbUpdateProgress;
        private System.Windows.Forms.Label lblNotification;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.CheckBox cbxForceConnection;
        private System.Windows.Forms.CheckBox cbxForceSync;
        private System.Windows.Forms.Button btnEject;
        private System.Windows.Forms.NotifyIcon myNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip cmuServer;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
    }
}

