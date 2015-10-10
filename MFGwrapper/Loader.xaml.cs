using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MFGwrapper
{
    /// <summary>
    /// Interaction logic for Loader.xaml
    /// </summary>
    public partial class Loader : Window
    {
        private readonly System.ComponentModel.BackgroundWorker bgworker;
        private readonly string basepath;
        private System.Diagnostics.Process proc;

        public Uri MFGbinaryUri
        { get; set; } = new Uri("https://myfleetweb.herokuapp.com/redirect/assets/zip/MyFleetGirls.zip");

        public Loader()
        {
            InitializeComponent();
            bgworker = new System.ComponentModel.BackgroundWorker();
            bgworker.WorkerReportsProgress = true;
            bgworker.WorkerSupportsCancellation = true;
            bgworker.ProgressChanged += Bgworker_ProgressChanged;
            bgworker.RunWorkerCompleted += Bgworker_RunWorkerCompleted;
            bgworker.DoWork += Bgworker_DoWork;
            basepath = AppDomain.CurrentDomain.BaseDirectory;
        }

        private enum Status : int
        {
            CheckJava,
            CheckMFG,
            DownloadMFG,
            CheckUpdate,
            Done
        }

        private void Bgworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var bgworker = sender as System.ComponentModel.BackgroundWorker;
            bgworker.ReportProgress((int)Status.CheckJava);
            if (!JavaAvailable())
            {
                MessageBox.Show("Please download Java Runtime at https://www.java.com/.");
                bgworker.CancelAsync();
                this.Close();
            }
            else
            {
                bgworker.ReportProgress((int)Status.CheckMFG);
                var dirpath = System.IO.Path.Combine(basepath, "MyFleetGirls");
                if (!File.Exists(
                    System.IO.Path.Combine(dirpath, "update.jar")))
                {
                    bgworker.ReportProgress((int)Status.DownloadMFG);
                    var filepath = System.IO.Path.Combine(basepath,
                            System.IO.Path.GetFileName(MFGbinaryUri.AbsolutePath));
                    var client = new System.Net.WebClient();
                    client.DownloadFile(MFGbinaryUri, filepath);
                    if (!Directory.Exists(dirpath))
                        Directory.CreateDirectory(dirpath);
                    System.IO.Compression.ZipFile.ExtractToDirectory(filepath, dirpath);
                    File.Delete(filepath);
                }
                bgworker.ReportProgress((int)Status.CheckUpdate);
                proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "java";
                proc.StartInfo.WorkingDirectory = dirpath;
                proc.StartInfo.Arguments = "-jar update.jar";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                proc.Close();
                bgworker.ReportProgress((int)Status.Done);
            }
        }

        private bool JavaAvailable()
        {
            int ecode = 127;
            try
            {
                proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "java";
                proc.StartInfo.Arguments = "-version";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                ecode = proc.ExitCode;
                proc.Close();
            }
            catch { }
            return (ecode == 0);
        }

        private void Bgworker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            switch ((Status)e.ProgressPercentage)
            {
                case Status.CheckJava:
                    labelStatus.Content = "Checking Java...";
                    break;
                case Status.CheckMFG:
                    labelStatus.Content = "Checking MyFleetGirls Binary...";
                    break;
                case Status.DownloadMFG:
                    labelStatus.Content = "Downloading MyFleetGirls Downloader...";
                    break;
                case Status.CheckUpdate:
                    labelStatus.Content = "Checking MyFleetGirls Updates." + Environment.NewLine
                        + "This may take a while, please be Patient.";
                    break;
                case Status.Done:
                    labelStatus.Content = "All systems green.";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Bgworker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bgworker.RunWorkerAsync();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (proc != null)
                if (!proc.HasExited)
                    proc.Kill();
            this.Close();
        }
    }
}
