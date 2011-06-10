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
using UsbEject.Library;

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

        private string frameDriveLetter = "";
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

                    //This should not clear shouldSync.  It should only Set it.  
                    //shouldSync should only be cleared once the frame has been updated.
                    handleServerResponse(myRq, rsp);
                }
                catch (Exception ex)
                {
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns internal variable shouldSync indicating whether the frame needs to be be synced or not.</returns>
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

                if (doSync)
                {
                    // Cache the response if it's different than previous cache.
                    using (var fs = File.Create(cachedFileInfo.FullName, 8192, FileOptions.WriteThrough))
                    {
                        ms.Seek(0L, SeekOrigin.Begin);
                        CopyStream(ms, fs);
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

                if (doSync)
                {
                    shouldSync = true;
                }

                return shouldSync;
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

                //retrive drive letter:
                frameDriveLetter = frameDrive.Name.Substring(0, 1);

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
                //lblNotification.Text = "Failed Sync";
            }
        }

        private int filesSynchronizing = 0;
        private int filesToSynchronize = 0;

        private bool syncServerToFrame()
        {
            lblNotification.Text = "Syncing...";
            Application.DoEvents();         //Display Syncing..

            if (filesSynchronizing < filesToSynchronize) return false;

            if (!refreshServerSync() && !cbxForceSync.Checked)
            {
                lblNotification.Text = DateTime.Now.ToString("HH:mm tt") + " No new files to Sync with Server.";
                return false;
            }
            if (serverFiles == null)
            {
                lblNotification.Text = "No files found on server.";
                return false;
            }

            try
            {
                if (!mySerialPort.IsOpen)
                {
                    mySerialPort.Open();
                    System.Threading.Thread.Sleep(1000);        //give it a second to open the port before doing anything..
                }
                mySerialPort.DtrEnable = true;              //Connect
                mySerialPort.RtsEnable = false;             //Connect
                System.Threading.Thread.Sleep(15000);        //give it a LONG WHILE to find all the drives before doing anything..
            }
            catch (Exception ex)
            {
                lblNotification.Text = "Failed to open RS232 port. Error:" + ex.Message;
                return false;
            }

            refreshFrameAndAsk();
            if (frameFiles == null)
            {
                lblNotification.Text = "Couldn't connect to the frame.";
                DisconnectFromDrive(frameDriveLetter);
                return false;
            }

            // Pick only new files from the server:
            var toSync = serverFiles.Except(frameFiles).ToList();
            if (toSync.Count == 0)
            {
                //MessageBox.Show(this, "Completed", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblNotification.Text = "Frame is already up to date.";
                DisconnectFromDrive(frameDriveLetter);
                shouldSync = false;
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
                                //MessageBox.Show(this, "Completed", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                lblNotification.Text = DateTime.Now.ToString("HH:mm tt") + " Completed Successfully";
                                Application.DoEvents();

                                System.Threading.Thread.Sleep(5000);     //give time to finish writing

                                DisconnectFromDrive(frameDriveLetter);
                                btnSync.Enabled = true;
                                shouldSync = false;     //Clear this only after the entire sync operation has completed.
                            }));
                        }
                    }
                }), rq);
            }
            return true;
        }

        private void DisconnectFromDrive(string frameDriveLetter)
        {
            //Eject the USB drive:
            EjectUSBDrive(frameDriveLetter + ":");
            System.Threading.Thread.Sleep(10000);     //give time to eject

            if (mySerialPort.IsOpen)
            {
                mySerialPort.DtrEnable = false;
                System.Threading.Thread.Sleep(10000);     //give time to disconnect fully.
                mySerialPort.Close();
            }
        }

        public static void EjectUSBDrive(string DriveLetterToEject)
        {
            VolumeDeviceClass volumeDeviceClass = new VolumeDeviceClass();

            foreach (Volume device in volumeDeviceClass.Devices)
            {
                if (DriveLetterToEject == device.LogicalDrive)
                {
                    //Eject if found matching drive letter
                    device.Eject(false);
                    break;
                }
            }
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
                //lblNotification.Text = "Failed Sync";
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
                mySerialPort.RtsEnable = false;             //Connect
                System.Threading.Thread.Sleep(3000);        //give it a second to open the port before doing anything..
            }
            else
            {
                if (mySerialPort.IsOpen)
                {
                    mySerialPort.DtrEnable = false;
                    mySerialPort.RtsEnable = true;
                    System.Threading.Thread.Sleep(5000);     //give a WHILE to disconnect fully otherwise the frame goes NUTS!
                    mySerialPort.Close();
                }
            }
        }

        private void btnEject_Click(object sender, EventArgs e)
        {
            string Letter = Microsoft.VisualBasic.Interaction.InputBox("Enter drive letter to Eject: ","Letter", "", 0,0);
            EjectUSBDrive(Letter + ":");
        }


        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (mySerialPort.IsOpen)
            //{
            //    mySerialPort.DtrEnable = false;
            //    mySerialPort.RtsEnable = true;
            //    System.Threading.Thread.Sleep(5000);     //give a WHILE to disconnect fully otherwise the frame goes NUTS!
            //    mySerialPort.Close();
            //}
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                myNotifyIcon.Visible = true;
                myNotifyIcon.BalloonTipText = "RyanSync";
                myNotifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                myNotifyIcon.Visible = false;
            }
        }

        private void myNotifyIcon_Click(object sender, EventArgs e)
        {
            //Restore:
            Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
