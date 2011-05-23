using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Diagnostics;

namespace RyanSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

            lblServer.Content = "Server (" + serverUri.AbsoluteUri + "):";

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
                    Dispatcher.Invoke((Action)(() =>
                    {
                        MessageBox.Show(this, ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            }), rq);
        }

        private void refreshServerSync()
        {
            Uri serverUri = Properties.Settings.Default.RyanServer;

            lblServer.Content = "Server (" + serverUri.AbsoluteUri + "):";

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
            Dispatcher.Invoke((Action)(() =>
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
                MessageBoxResult result = MessageBox.Show(this, String.Format("Could not find folder '{0}'. Create it?", Properties.Settings.Default.DigitalFramePath), "Create folder?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    frameDirectory.Create();
                    refreshFrame();
                }
            }
        }

        private bool refreshFrame()
        {
            try
            {
                frameDirectory = new DirectoryInfo(Properties.Settings.Default.DigitalFramePath);
                if (!frameDirectory.Exists) return false;

                lblFrame.Content = "Digital Frame (" + frameDirectory.FullName + "):";

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
                Dispatcher.Invoke((Action)(() =>
                {
                    MessageBox.Show(this, ex.Message, "Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
                return false;
            }
        }

        private void btnSyncServerToFrame_Click(object sender, RoutedEventArgs e)
        {
            if (!syncServerToFrame())
            {
                MessageBox.Show(this, "Failed", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(this, "Completed", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }

            // Disable the sync button and proceed:
            btnSyncServerToFrame.IsEnabled = false;
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
                        Dispatcher.Invoke((Action)(() =>
                        {
                            lstFolder.Items.Add(fileName);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        Dispatcher.Invoke((Action)(() =>
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
                            Dispatcher.Invoke((Action)(() =>
                            {
                                btnSyncServerToFrame.IsEnabled = true;
                                MessageBox.Show(this, "Completed", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshServer();
            refreshFrame();
        }
    }
}
