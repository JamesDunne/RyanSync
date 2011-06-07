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
            this.lstServer = new System.Windows.Forms.ListBox();
            this.lstFolder = new System.Windows.Forms.ListBox();
            this.btnSync = new System.Windows.Forms.Button();
            this.lblFrame = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.pgbUpdateProgress = new System.Windows.Forms.ProgressBar();
            this.lblNotification = new System.Windows.Forms.Label();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.cbxForceConnection = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lstServer
            // 
            this.lstServer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstServer.FormattingEnabled = true;
            this.lstServer.Location = new System.Drawing.Point(12, 31);
            this.lstServer.Name = "lstServer";
            this.lstServer.Size = new System.Drawing.Size(319, 290);
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
            this.lstFolder.Size = new System.Drawing.Size(319, 290);
            this.lstFolder.TabIndex = 1;
            // 
            // btnSync
            // 
            this.btnSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSync.Location = new System.Drawing.Point(570, 330);
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
            this.pgbUpdateProgress.Location = new System.Drawing.Point(276, 330);
            this.pgbUpdateProgress.Name = "pgbUpdateProgress";
            this.pgbUpdateProgress.Size = new System.Drawing.Size(288, 19);
            this.pgbUpdateProgress.TabIndex = 5;
            // 
            // lblNotification
            // 
            this.lblNotification.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblNotification.AutoSize = true;
            this.lblNotification.Location = new System.Drawing.Point(12, 328);
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
            this.cbxForceConnection.AutoSize = true;
            this.cbxForceConnection.Location = new System.Drawing.Point(135, 336);
            this.cbxForceConnection.Name = "cbxForceConnection";
            this.cbxForceConnection.Size = new System.Drawing.Size(96, 17);
            this.cbxForceConnection.TabIndex = 7;
            this.cbxForceConnection.Text = "Force Connect";
            this.cbxForceConnection.UseVisualStyleBackColor = true;
            this.cbxForceConnection.CheckedChanged += new System.EventHandler(this.cbxForceConnection_CheckedChanged);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 361);
            this.Controls.Add(this.cbxForceConnection);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.pgbUpdateProgress);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.lblFrame);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.lstFolder);
            this.Controls.Add(this.lstServer);
            this.MaximumSize = new System.Drawing.Size(670, 388);
            this.MinimumSize = new System.Drawing.Size(670, 388);
            this.Name = "frmMain";
            this.Text = "Sync Pictures";
            this.Load += new System.EventHandler(this.frmMain_Load);
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
    }
}

