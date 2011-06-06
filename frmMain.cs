using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Diagnostics;

namespace RyanSync
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            tmrUpdate.Enabled = true;
            tmrUpdate.Interval = 900000;
            lblNotification.Text = "";
            refreshServer();
            refreshFrame();
        }

        private Uri serverBaseUri = null;
        private string[] serverFiles = null;

        private DirectoryInfo frameDirectory = null;
        private string[] frameFiles = null;

        private void setupAuthorization(HttpWebRequest rq)
        {
            rq.Credentials = new NetworkCredential("ryan", "iscute");
        }

        private void refreshServer()
        {
            Uri serverUri = Properties.Settings.Default.RyanServer;

            lblServer.Text = "Server (" + serverUri.AbsoluteUri + "):";

            Uri listUri = new Uri(serverUri, "list.php");

            // Request the list.php JSON object:
            HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create(listUri);
            setupAuthorization(rq);

            // Asynchronously get the response from the URI:
            rq.BeginGetResponse(new AsyncCallback((ar) =>
            {
                HttpWebRequest myRq = (HttpWebRequest)ar.AsyncState;
                try
                {
                    HttpWebResponse rsp = (HttpWebResponse)myRq.EndGetResponse(ar);

                    handleServerResponse(myRq, rsp);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    
                    //Dispatcher.Invoke((Action)(() =>
                    BeginInvoke((Action)(() =>
                    {
                        //MessageBox.Show(this, ex.Message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblNotification.Text = "Server Error";
                    }));
                    
                }
            }), rq);
        }

        private void refreshServerSync()
        {
            Uri serverUri = Properties.Settings.Default.RyanServer;

            lblServer.Text = "Server (" + serverUri.AbsoluteUri + "):";

            Uri listUri = new Uri(serverUri, "list.php");

            // Request the list.php JSON object:
            HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create(listUri);
            setupAuthorization(rq);

            HttpWebResponse rsp = (HttpWebResponse)rq.GetResponse();

            handleServerResponse(rq, rsp);
        }

        private void handleServerResponse(HttpWebRequest req, HttpWebResponse rsp)
        {
            // Deserialize the JSON response into our data contract:
            var files = (FileList)new DataContractJsonSerializer(typeof(FileList)).ReadObject(rsp.GetResponseStream());
            serverBaseUri = new Uri(files.BaseUrl);
            serverFiles = (
                from f in files.FileNames
                orderby f ascending
                select f
            ).ToArray();

            // Add the filenames to the list:
            //Dispatcher.Invoke((Action)(() =>
            BeginInvoke((Action)(() =>
            {
                lstServer.Items.Clear();
                foreach (string fileName in serverFiles)
                {
                    lstServer.Items.Add(fileName);
                }
            }));
        }

        private void refreshFrameAndAsk()
        {
            if (!refreshFrame())
            {
                //MessageBox.Show(this, "Digital frame not connected!", "Not connected!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                lblNotification.Text = "Digital frame not connected!";
            }
        }

        private bool refreshFrame()
        {
            try
            {
                var frameDrive = (
                    from drv in System.IO.DriveInfo.GetDrives()
                    where drv.IsReady   // must be first check or else "Drive Not Ready" exception is thrown
                    where drv.DriveType == DriveType.Removable
                    where drv.VolumeLabel == Properties.Settings.Default.DigitalFrameLabel
                    select drv
                ).SingleOrDefault();
                if (frameDrive == null)
                    return false;

                // Now find the dedicated subdirectory (currently only one level deep allowed):
                frameDirectory = (
                    from dir in frameDrive.RootDirectory.GetDirectories()
                    where String.Compare(dir.Name, Properties.Settings.Default.DigitalFrameSubdirectory, true) == 0
                    select dir
                ).SingleOrDefault();

                if (frameDirectory == null) frameDirectory = frameDrive.RootDirectory;

                if (!frameDirectory.Exists) return false;

                lblFrame.Text = "Digital Frame (" + frameDirectory.FullName + "):";

                frameFiles = (
                    from fi in frameDirectory.GetFiles()
                    orderby fi.Name ascending
                    select fi.Name
                ).ToArray();

                lstFolder.Items.Clear();
                foreach (var name in frameFiles)
                {
                    lstFolder.Items.Add(name);
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //Dispatcher.Invoke((Action)(() =>
                BeginInvoke((Action)(() =>
                {
                    //MessageBox.Show(this, ex.Message, "Client Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblNotification.Text = "Client Error";
                }));
                return false;
            }
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            if (!syncServerToFrame())
            {
                //MessageBox.Show(this, "Failed", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
                lblNotification.Text = "Failed Sync";
            }
        }

        private int filesSynchronizing = 0;
        private int filesToSynchronize = 0;

        private bool syncServerToFrame()
        {
            if (filesSynchronizing < filesToSynchronize) return false;

            refreshFrameAndAsk();
            if (frameFiles == null) return false;
            refreshServerSync();
            if (serverFiles == null) return false;

            // Pick only new files from the server:
            var toSync = serverFiles.Except(frameFiles).ToList();
            if (toSync.Count == 0)
            {
                //MessageBox.Show(this, "Completed", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblNotification.Text = "Completed successfully.";
                return true;
            }

            pgbUpdateProgress.Maximum = toSync.Count;
            pgbUpdateProgress.Value = 0;
            pgbUpdateProgress.Visible = true;

            // Disable the sync button and proceed:
            btnSync.Enabled = false;
            filesToSynchronize = toSync.Count;
            filesSynchronizing = 0;

            foreach (string name in toSync)
            {
                string fileName = name;
                Uri fileUri = new Uri(serverBaseUri, fileName);

                // Request the file from the server:
                HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create(fileUri);
                setupAuthorization(rq);

                // Asynchronously get the response:
                rq.BeginGetResponse(new AsyncCallback((ar) =>
                {
                    HttpWebRequest myRq = (HttpWebRequest)ar.AsyncState;

                    try
                    {
                        HttpWebResponse rsp = (HttpWebResponse)myRq.EndGetResponse(ar);

                        // Download the content into a file:
                        using (FileStream fi = File.Open(System.IO.Path.Combine(frameDirectory.FullName, fileName), FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            // Allocate enough space to store the file:
                            fi.SetLength(rsp.ContentLength);
                            // Copy:
                            CopyStream(rsp.GetResponseStream(), fi);
                            fi.Close();
                        }

                        // Add the filename to the frame's list:
                        BeginInvoke((Action)(() =>
                        {
                            lstFolder.Items.Add(fileName);
                            pgbUpdateProgress.Value++;
                        }));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        BeginInvoke((Action)(() =>
                        {
                            lstFolder.Items.Add(fileName + " (ERROR: " + ex.Message + ")");
                        }));
                    }
                    finally
                    {
                        // Increment the number of files synchronized:
                        if (Interlocked.Increment(ref filesSynchronizing) == filesToSynchronize)
                        {
                            // If we're last in line, re-enable the sync button:
                            BeginInvoke((Action)(() =>
                            {
                                btnSync.Enabled = true;
                                //MessageBox.Show(this, "Completed", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                lblNotification.Text = "Completed Successfully";
                            }));
                        }
                    }
                }), rq);
            }
            pgbUpdateProgress.Visible = false;
            return true;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (!syncServerToFrame())
            {
                //MessageBox.Show(this, "Failed", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
                lblNotification.Text = "Failed Sync";
            }
        }

    }
}
