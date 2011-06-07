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
using System.IO.Ports;

namespace RyanSync
{
    public partial class frmMain : Form
    {
        System.IO.Ports.SerialPort mySerialPort = new System.IO.Ports.SerialPort();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            string[] PortNames = System.IO.Ports.SerialPort.GetPortNames();
            //select the last port.  (Lame solution.. for now it'll work though)
            mySerialPort.PortName = PortNames[PortNames.Length-1];

            pgbUpdateProgress.Visible = false;
            tmrUpdate.Enabled = true;
            tmrUpdate.Interval = 900000;
            lblNotification.Text = "";
            refreshServer();
            //refreshFrame();
        }

        private Uri serverBaseUri = null;
        private string[] serverFiles = null;
        private bool shouldSync = false;

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

                    shouldSync = handleServerResponse(myRq, rsp);
                }
                catch (Exception ex)
                {
                    shouldSync = false;

                    Trace.WriteLine(ex.ToString());
                    
                    //Dispatcher.Invoke((Action)(() =>
                    UIBlockingInvoke(new MethodInvoker(delegate()
                    {
                        //MessageBox.Show(this, ex.Message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblNotification.Text = "Server Error";
                    }));
                    
                }
            }), rq);
        }

        private bool refreshServerSync()
        {
            Uri serverUri = Properties.Settings.Default.RyanServer;

            lblServer.Text = "Server (" + serverUri.AbsoluteUri + "):";

            Uri listUri = new Uri(serverUri, "list.php");

            // Request the list.php JSON object:
            HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create(listUri);
            setupAuthorization(rq);

            HttpWebResponse rsp = (HttpWebResponse)rq.GetResponse();

            return handleServerResponse(rq, rsp);
        }

        private bool handleServerResponse(HttpWebRequest req, HttpWebResponse rsp)
        {
            using (var ms = new MemoryStream())
            {
                // Copy the response stream to our MemoryStream:
                CopyStream(rsp.GetResponseStream(), ms);
                
                bool doSync = true;

                var cachedFileInfo = new FileInfo(@"cached.json");

                if (!cachedFileInfo.Exists)
                {
                    // Cache the response:
                    using (var fs = File.Create(cachedFileInfo.FullName, 8192, FileOptions.WriteThrough))
                    {
                        ms.Seek(0L, SeekOrigin.Begin);
                        CopyStream(ms, fs);
                    }
                }
                else
                {
                    // Check the diff between the last cached response and the current response:
                    if (ms.Length != cachedFileInfo.Length)
                    {
                        // Length changed, obvious indication that sync is needed:
                        doSync = true;
                    }
                    else
                    {
                        doSync = false;

                        // Read the cached response into memory and compare byte-by-byte with the latest response:
                        byte[] cached = File.ReadAllBytes(cachedFileInfo.FullName);
                        byte[] actual = ms.ToArray();

                        // If any of the bytes are not equal, do synchronization:
                        doSync = Enumerable.Range(0, cached.Length).Any(i => cached[i] != actual[i]);
                    }
                }

                // Deserialize the JSON response into our data contract:
                ms.Seek(0L, SeekOrigin.Begin);
                var files = (FileList)new DataContractJsonSerializer(typeof(FileList)).ReadObject(ms);
                serverBaseUri = new Uri(files.BaseUrl);
                serverFiles = (
                    from f in files.FileNames
                    orderby f ascending
                    select f
                ).ToArray();

                // Add the filenames to the list:
                //Dispatcher.Invoke((Action)(() =>
                UIBlockingInvoke(new MethodInvoker(delegate()
                {
                    lstServer.Items.Clear();
                    foreach (string fileName in serverFiles)
                    {
                        lstServer.Items.Add(fileName);
                    }
                }));

                return doSync;
            }
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
                UIBlockingInvoke(new MethodInvoker(delegate()
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

            try
            {
                if (!mySerialPort.IsOpen)
                {
                    mySerialPort.Open();
                    System.Threading.Thread.Sleep(1000);        //give it a second to open the port before doing anything..
                }
                mySerialPort.DtrEnable = true;              //Connect
                System.Threading.Thread.Sleep(15000);        //give it a LONG WHILE to find all the drives before doing anything..
            }
            catch (Exception ex)
            {
                lblNotification.Text = "Failed to open RS232 port. Error:" + ex.Message;
                return false;
            }

            if (!refreshServerSync()) return false;
            if (serverFiles == null) return false;
            refreshFrameAndAsk();
            if (frameFiles == null) return false;

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
            Application.DoEvents();         //display the progress bar change..

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
                        UIBlockingInvoke(new MethodInvoker(delegate()
                        {
                            lstFolder.Items.Add(fileName);
                            pgbUpdateProgress.Value++;
                        }));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        UIBlockingInvoke(new MethodInvoker(delegate()
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
                            UIBlockingInvoke(new MethodInvoker(delegate()
                            {
                                pgbUpdateProgress.Visible = false;
                                btnSync.Enabled = true;
                                //MessageBox.Show(this, "Completed", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                lblNotification.Text = "Completed Successfully";

                                if (mySerialPort.IsOpen)
                                {
                                    mySerialPort.DtrEnable = false;
                                    System.Threading.Thread.Sleep(5000);     //give an extra second to disconnect fully.
                                    mySerialPort.Close();
                                }
                            }));
                        }
                    }
                }), rq);
            }
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

        /// <summary>
        /// Runs a MethodInvoker delegate on the UI thread from whichever thread we are currently calling from and BLOCKS until it is complete
        /// </summary>
        /// <param name="ivk"></param>
        public void UIBlockingInvoke(MethodInvoker ivk)
        {
            System.Threading.ManualResetEvent UIAsyncComplete = new System.Threading.ManualResetEvent(false);
            UIAsyncComplete.Reset();
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    try
                    {
                        ivk();
                    }
                    finally
                    {
                        UIAsyncComplete.Set();
                    }
                }));

                UIAsyncComplete.WaitOne();
            }
            else
            {
                ivk();
            }
        }

        private void cbxForceConnection_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxForceConnection.Checked)
            {
                if (!mySerialPort.IsOpen)
                {
                    mySerialPort.Open();
                    System.Threading.Thread.Sleep(1000);        //give it a second to open the port before doing anything..
                }
                mySerialPort.DtrEnable = true;              //Connect
                System.Threading.Thread.Sleep(3000);        //give it a second to open the port before doing anything..
            }
            else
            {
                if (mySerialPort.IsOpen)
                {
                    mySerialPort.DtrEnable = false;
                    System.Threading.Thread.Sleep(5000);     //give a WHILE to disconnect fully otherwise the frame goes NUTS!
                    mySerialPort.Close();
                }
            }
        }

    }
}
