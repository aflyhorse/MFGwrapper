using System;
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
using System.Windows.Shapes;

namespace MFGwrapper
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            textBoxSS.Text = Properties.Settings.Default.UpstreamPort.ToString();
            textBoxMFG.Text = Properties.Settings.Default.MFGPort.ToString();
            textBoxWrapper.Text = Properties.Settings.Default.ListenPort.ToString();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!(String.IsNullOrEmpty(textBoxSS.Text)
                || String.IsNullOrEmpty(textBoxMFG.Text))
                || String.IsNullOrEmpty(textBoxWrapper.Text)
                || String.IsNullOrEmpty(textBoxPassword.Text))
            {
                Properties.Settings.Default.UpstreamPort = Int32.Parse(textBoxSS.Text);
                Properties.Settings.Default.MFGPort = Int32.Parse(textBoxMFG.Text);
                Properties.Settings.Default.ListenPort = Int32.Parse(textBoxWrapper.Text);
                Properties.Settings.Default.Password = textBoxPassword.Text;
                Properties.Settings.Default.Save();
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
