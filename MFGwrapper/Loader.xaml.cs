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
        static System.Threading.Mutex singleton = new System.Threading.Mutex(true, "MFGWrapper");

        public Uri MFGupdaterUri
        { get; set; } = new Uri("https://myfleetweb.herokuapp.com/redirect/assets/zip/MyFleetGirls.zip");

        public Uri MFGmirrorUri
        { get; set; } = new Uri("http://lunes.faith/MFG/MyFleetGirls.jar.zip");

        private bool RestartWithMirror = false;

        public Loader()
        {
            if (!singleton.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("You have already opened this app. Please check your system tray.");
                Application.Current.Shutdown();
            }
            InitializeComponent();
            buttonMirror.Visibility = Visibility.Hidden;
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
            MirrorUpdate,
            Done
        }

        private void Bgworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var bgworker = sender as System.ComponentModel.BackgroundWorker;
            var dirpath = System.IO.Path.Combine(basepath, "MyFleetGirls");
            if (Properties.Settings.Default.IsFirstBoot ||
                !File.Exists(System.IO.Path.Combine(dirpath, "update.jar")))
            {
                bgworker.ReportProgress((int)Status.CheckJava);
                if (!JavaAvailable())
                {
                    MessageBox.Show("Please download Java Runtime at https://www.java.com/.");
                    bgworker.CancelAsync();
                    this.Close();
                    return;
                }
                bgworker.ReportProgress((int)Status.CheckMFG);
                if (!File.Exists(
                    System.IO.Path.Combine(dirpath, "update.jar")))
                {
                    bgworker.ReportProgress((int)Status.DownloadMFG);
                    var filepath = System.IO.Path.Combine(basepath,
                            System.IO.Path.GetFileName(MFGupdaterUri.AbsolutePath));
                    var client = new System.Net.WebClient();
                    client.DownloadFile(MFGupdaterUri, filepath);
                    if (!Directory.Exists(dirpath))
                        Directory.CreateDirectory(dirpath);
                    System.IO.Compression.ZipFile.ExtractToDirectory(filepath, dirpath);
                    File.Delete(filepath);
                }
            }
            if (RestartWithMirror)
            {
                bgworker.ReportProgress((int)Status.MirrorUpdate);
                var filepath = System.IO.Path.Combine(basepath,
                            System.IO.Path.GetFileName(MFGmirrorUri.AbsolutePath));
                var client = new System.Net.WebClient();
                client.DownloadFile(MFGmirrorUri, filepath);
                if (File.Exists(System.IO.Path.Combine(dirpath, "MyFleetGirls.jar")))
                    File.Delete(System.IO.Path.Combine(dirpath, "MyFleetGirls.jar"));
                System.IO.Compression.ZipFile.ExtractToDirectory(filepath, dirpath);
                File.Delete(filepath);
                RestartWithMirror = false;
            }
            if (!File.Exists(System.IO.Path.Combine(dirpath, "MyFleetGirls.jar")) ||
                (DateTime.Now - Properties.Settings.Default.LastUpdateChecked).TotalDays > 1)
            {
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
                Properties.Settings.Default.LastUpdateChecked = DateTime.Now;
                Properties.Settings.Default.Save();
            }
            bgworker.ReportProgress((int)Status.Done);
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
                        + "This may take a while, please be Patient." + Environment.NewLine
                        + "(Approx. 3 mins on 10Mb cable if update is needed)";
                    buttonMirror.Visibility = Visibility.Visible;
                    break;
                case Status.MirrorUpdate:
                    labelStatus.Content = "Try alternative mirror instead..." + Environment.NewLine
                        + "This might be faster.";
                    buttonMirror.Visibility = Visibility.Hidden;
                    break;
                case Status.Done:
                    labelStatus.Content = "All systems green.";
                    buttonMirror.Visibility = Visibility.Hidden;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Bgworker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (RestartWithMirror)
                bgworker.RunWorkerAsync();
            else if (e.Error == null && !e.Cancelled)
            {
                MainWindow main = new MainWindow();
                this.Hide();
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

        private void buttonMirror_Click(object sender, RoutedEventArgs e)
        {
            RestartWithMirror = true;
            if (proc != null)
                if (!proc.HasExited)
                    proc.Kill();
        }
    }
}
