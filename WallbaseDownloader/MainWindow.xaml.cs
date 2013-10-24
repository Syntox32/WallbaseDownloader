using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Forms;

namespace WallbaseDownloader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            checkRemember.Checked += (sender, e) => 
            {
                Properties.Settings.Default.toSave = true;
                Properties.Settings.Default.savePath = txtPath.Text;
                Properties.Settings.Default.Save();
            };

            checkRemember.Unchecked += (sender, e) => 
            {
                Properties.Settings.Default.toSave = false;
                Properties.Settings.Default.savePath = "";
                Properties.Settings.Default.Save();
            };

            txtPath.TextChanged += (sender, e) => 
            {
                _folderPath = txtPath.Text;
            };

            checkRemember.IsChecked = Properties.Settings.Default.toSave;
            txtPath.Text = Properties.Settings.Default.savePath;
        }

        private string _folderPath;
        public string FolderPath
        {
            get
            {
                if (NameFolderWithDate)
                    return Path.Combine(_folderPath, String.Format("Wallbase - {0}", DateTime.Now.ToString("dd-MM-yy")));
                else
                    return Path.Combine(_folderPath, "Wallbase");
            }
            set { _folderPath = value; }
        }

        public bool NameFolderWithDate 
        {
            get { return Properties.Settings.Default.nameDate; }
            set { Properties.Settings.Default.nameDate = value; Properties.Settings.Default.Save(); }
        }
 
        public bool CreateList 
        {
            get { return Properties.Settings.Default.createList; }
            set { Properties.Settings.Default.nameDate = value; Properties.Settings.Default.Save(); }
        }

        public bool CreateLog 
        {
            get { return Properties.Settings.Default.createLog; }
            set { Properties.Settings.Default.createLog = value; Properties.Settings.Default.Save(); }
        }

        public bool UseAsync 
        {
            get { return Properties.Settings.Default.useAsync; }
            set { Properties.Settings.Default.useAsync = value; Properties.Settings.Default.Save(); }
        }

        public bool Sort 
        {
            get { return Properties.Settings.Default.sort; }
            set { Properties.Settings.Default.sort = value; Properties.Settings.Default.Save(); }
        }
       
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(_folderPath))
            if (Directory.Exists(FolderPath))
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show("The directory already exist, do you want to overwrite it?\n\n" +
                    "By pressing 'No' the new wallpapers with be merged with the existing ones, but the 'url_list' and 'log' files will be overwritten.",
                    "The directory already exist", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    string[] files = Directory.GetFiles(FolderPath);
                    foreach (var file in files)
                        File.Delete(file);

                    Directory.Delete(FolderPath, true);
                    System.Threading.Thread.Sleep(30);
                    Directory.CreateDirectory(FolderPath);

                    LockGUI(false);
                    Download();
                }
                else if (result == System.Windows.Forms.DialogResult.No)
                {
                    LockGUI(false);
                    Download();
                }
                else if (result == System.Windows.Forms.DialogResult.Abort)
                {
                    System.Windows.Forms.MessageBox.Show("Downloading aborted.", "Aborting",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Work on this
                LockGUI(false);
                Download();
            }
        }

        public void LockGUI(bool b)
        {
            btnDownload.IsEnabled = b;
            btnPath.IsEnabled = b;
            btnShowLog.IsEnabled = b;
            btnShowFolder.IsEnabled = b;

            txtPath.IsEnabled = b;
            txtUrl.IsEnabled = b;

            checkRemember.IsEnabled = b;

            menuSettings.IsEnabled = b;
        }

        private void Download() 
        {
            if (!txtUrl.Text.StartsWith("http://wallbase.cc/") || !txtUrl.Text.Contains("board="))
            {
                System.Windows.Forms.MessageBox.Show("The given URL is invalid\nEither toplists or search results." +
                    "\n\nIf you keep getting this error, please read the README.",
                    "URL Invalid Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!String.IsNullOrEmpty(txtPath.Text))
            {
                FolderPath = txtPath.Text;
                if (!String.IsNullOrWhiteSpace(txtUrl.Text))
                {
                    using (var wall = new Wallbase(FolderPath, txtUrl.Text, 
                        UseAsync, CreateList, CreateLog, Sort))
                    {
                        var wnd = new ProgressWindow(wall);
                        wnd.Closed += (sender, e) => { LockGUI(true); };
                        wnd.ShowDialog();
                    }
                }
                else
                    System.Windows.Forms.MessageBox.Show("Please specify a URL.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else System.Windows.Forms.MessageBox.Show("Please specify a folder location.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void btnShowFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(_folderPath))
                using (var pro = new Process())
                {
                    try
                    {
                        pro.StartInfo.FileName = FolderPath;
                        pro.Start();
                    }
                    catch { } 
                }
        }

        private void btnShowLog_Click(object sender, RoutedEventArgs e) 
        {
            if (!String.IsNullOrEmpty(txtPath.Text))
                using (var proc = new Process())
                {
                    try
                    {
                        proc.StartInfo.FileName = FolderPath;
                        proc.Start();
                    }
                    catch { } 
                }
        }

        private void menuSettings_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new Settings();
            wnd.Closing += (s, args) => 
            {
                Properties.Settings.Default.sort = this.Sort = (bool)wnd.checkSort.IsChecked;
                Properties.Settings.Default.nameDate = this.NameFolderWithDate = (bool)wnd.checkUseDate.IsChecked;
                Properties.Settings.Default.useAsync = this.UseAsync = (bool)wnd.checkAsync.IsChecked;
                Properties.Settings.Default.createList = this.CreateList = (bool)wnd.checkCreateUrl.IsChecked;
                Properties.Settings.Default.createLog = this.CreateLog = (bool)wnd.checkCreateLog.IsChecked;
            };

            wnd.ShowDialog();
        }

        private void btnPath_Click(object sender, RoutedEventArgs e)
        {
            var folderSelect = new FolderSelectDialog();
            folderSelect.Title = "Choose a download directory for your wallpapers";

            if (folderSelect.ShowDialog(IntPtr.Zero))
            {
                txtPath.Text = folderSelect.FileName;
                FolderPath = folderSelect.FileName;
            }
        }
    }
}
