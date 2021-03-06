﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MFGwrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Diagnostics.Process proc;
        private System.ComponentModel.BackgroundWorker bgworker;
        private System.ComponentModel.BackgroundWorker restartworker;
        private Forwarder.Spliter spliter;
        private readonly string basepath = AppDomain.CurrentDomain.BaseDirectory;
        private System.IO.StreamWriter sw;
        private System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            bgworker = new System.ComponentModel.BackgroundWorker();
            bgworker.DoWork += Bgworker_DoWork;
            bgworker.RunWorkerCompleted += Bgworker_RunWorkerCompleted;
            restartworker = new System.ComponentModel.BackgroundWorker();
            restartworker.DoWork += Restartworker_DoWork;
            restartworker.RunWorkerCompleted += Restartworker_RunWorkerCompleted;
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = Properties.Resources.Kitakami;
            notifyIcon.Click += NotifyIcon_Click;
        }

        private void Bgworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int? inuse = Forwarder.Nettools.isPortInUse(new List<int> {
                Properties.Settings.Default.MFGPort,
                Properties.Settings.Default.ListenPort });
            if (inuse != null)
            {
                MessageBox.Show("Port " + inuse + " is already in use!");
                buttonSettings.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else
            {
                spliter = new Forwarder.Spliter(
                    Properties.Settings.Default.UpstreamPort,
                    Properties.Settings.Default.MFGPort,
                    Properties.Settings.Default.ListenPort);
                spliter.Start();
                System.IO.File.WriteAllText(System.IO.Path.Combine(basepath, "MyFleetGirls/application.conf"), @"
url {
    post: ""https://myfleet.moe""
    proxy {
        port: " + Properties.Settings.Default.UpstreamPort + @"
        host: ""localhost""
    }
}
proxy {
    port: " + Properties.Settings.Default.MFGPort + @"
    host: ""localhost""
}
upstream_proxy {
    port: " + Properties.Settings.Default.UpstreamPort + @"
    host: ""localhost""
}
auth {
    pass: " + Properties.Settings.Default.Password + @"
}");
                if (System.IO.File.Exists(System.IO.Path.Combine(basepath, "log-lastrun.txt")))
                    System.IO.File.Delete(System.IO.Path.Combine(basepath, "log-lastrun.txt"));
                if (System.IO.File.Exists(System.IO.Path.Combine(basepath, "log.txt")))
                    System.IO.File.Move(System.IO.Path.Combine(basepath, "log.txt"),
                        System.IO.Path.Combine(basepath, "log-lastrun.txt"));
                sw = new System.IO.StreamWriter(System.IO.Path.Combine(basepath, "log.txt"));
                proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "java";
                proc.StartInfo.Arguments = "-jar MyFleetGirls.jar";
                proc.StartInfo.WorkingDirectory = System.IO.Path.Combine(basepath, "MyFleetGirls");
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.OutputDataReceived += Proc_OutputDataReceived;
                proc.ErrorDataReceived += Proc_OutputDataReceived;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                sw.Close();
                spliter.Stop();
                System.Threading.Thread.Sleep(250);
            }
        }

        private void Proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            sw.WriteLine(e.Data);
        }

        private void Bgworker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show("MFG exited unexpectedly, something went wrong.");
            spliter?.Stop();
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
        }

        private void Restartworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
        }
        private void Restartworker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            buttonStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.AutoStartEnabled = (bool)checkBoxAutostart.IsChecked;
            Properties.Settings.Default.Save();
            buttonStop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            if (settings.ShowDialog() == true)
            {
                if (spliter != null)
                {
                    buttonStop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    buttonStart.IsEnabled = false;
                    restartworker.RunWorkerAsync();
                }
                else
                    buttonStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
            bgworker.RunWorkerAsync();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxAutostart.IsChecked = Properties.Settings.Default.AutoStartEnabled;
            if (Properties.Settings.Default.IsFirstBoot)
            {
                buttonSettings.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Properties.Settings.Default.IsFirstBoot = false;
                Properties.Settings.Default.Save();
            }
            if (Properties.Settings.Default.AutoStartEnabled)
                buttonStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            spliter?.Stop();
            if (proc != null)
                if (!proc.HasExited)
                    proc.Kill();
        }

        private void buttonLog_Click(object sender, RoutedEventArgs e)
        {
            if (buttonStop.IsEnabled)
                sw.Flush();
            var logpath = System.IO.Path.Combine(basepath, "log.txt");
            if (!System.IO.File.Exists(logpath))
                MessageBox.Show("No log available yet.");
            else
                System.Diagnostics.Process.Start(logpath);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (!this.IsVisible)
            {
                this.Show();
            }
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
            notifyIcon.Visible = false;
        }
    }
}