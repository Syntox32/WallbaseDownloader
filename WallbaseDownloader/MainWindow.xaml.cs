using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace WallbaseDownloader
{
    public partial class MainWindow : Window
    {
        private string _folderPath;
        public string FolderPath
        {
            get
            {
                if (Properties.Settings.Default.nameDate)
                    return Path.Combine(_folderPath, String.Format("Wallbase - {0}", DateTime.Now.ToString("dd-MM-yy")));
                else if (!Properties.Settings.Default.nameDate)
                    return Path.Combine(_folderPath, "Wallbase");
                else
                    throw new ArgumentNullException("FolderPath cannot be null");
            }
            set { _folderPath = value; }
        }

        public string SaveUrl
        {
            get { return Properties.Settings.Default.saveUrl; }
            set 
            {
                Properties.Settings.Default.saveUrl = value;
                Properties.Settings.Default.Save(); 
            }
        }

        public bool ToSaveUrl
        {
            get { return Properties.Settings.Default.toSaveUrl; }
            set
            {
                Properties.Settings.Default.toSaveUrl = value;
                Properties.Settings.Default.Save();
            }
        }

        public string SavePath
        {
            get { return Properties.Settings.Default.savePath; }
            set
            {
                Properties.Settings.Default.savePath = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool ToSave
        {
            get { return Properties.Settings.Default.toSave; }
            set
            {
                Properties.Settings.Default.toSave = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool NameFolderWithDate
        {
            get { return Properties.Settings.Default.nameDate; }
            set
            {
                Properties.Settings.Default.nameDate = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool CreateList
        {
            get { return Properties.Settings.Default.createList; }
            set
            {
                Properties.Settings.Default.createList = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool CreateLog
        {
            get { return Properties.Settings.Default.createLog; }
            set
            {
                Properties.Settings.Default.createLog = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool Sort
        {
            get { return Properties.Settings.Default.sort; }
            set
            {
                Properties.Settings.Default.sort = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool UsePermissions
        {
            get { return Properties.Settings.Default.usePermissions; }
            set { Properties.Settings.Default.usePermissions = value; }
        }

        public SecureString Password
        {
            get { return Properties.Settings.Default.password; }
            set { Properties.Settings.Default.password = value; }
        }

        public string Username
        {
            get { return Properties.Settings.Default.username; }
            set
            {
                Properties.Settings.Default.username = value;
                Properties.Settings.Default.Save();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            checkRemember.IsChecked = ToSave;
            txtPath.Text = SavePath;

            checkSaveUrl.IsChecked = ToSaveUrl;
            txtUrl.Text = SaveUrl;

            UsePermissions = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((bool)checkRemember.IsChecked)
            {
                ToSave = true;
                SavePath = txtPath.Text;
            } else {
                ToSave = false;
                SavePath = String.Empty;
            }

            if ((bool)checkSaveUrl.IsChecked)
            {
                ToSaveUrl = true;
                SaveUrl = txtUrl.Text;
            } else {
                ToSaveUrl = false;
                SaveUrl = String.Empty;
            }
        }
       
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            PrepareDownload();
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
            if (!String.IsNullOrWhiteSpace(txtPath.Text))
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
                Properties.Settings.Default.createList = this.CreateList = (bool)wnd.checkCreateUrl.IsChecked;
                Properties.Settings.Default.createLog = this.CreateLog = (bool)wnd.checkCreateLog.IsChecked;
                Properties.Settings.Default.Save();
            };

            wnd.ShowDialog();
        }

        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            var hlp = new Help();
            hlp.ShowDialog();
        }

        private void btnPath_Click(object sender, RoutedEventArgs e)
        {
            var folderSelect = new FolderSelectDialog();
            folderSelect.Title = "Choose a download directory for your wallpapers";

            if (folderSelect.ShowDialog())
            {
                txtPath.Text = folderSelect.FileName;
                FolderPath = folderSelect.FileName;
            }
        }

        private void txtPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            FolderPath = txtPath.Text;
        }

        private void PrepareDownload()
        {
            if (!String.IsNullOrEmpty(txtPath.Text))
            {
                FolderPath = txtPath.Text;
                if (!String.IsNullOrWhiteSpace(txtUrl.Text))
                {
                    if (txtUrl.Text.StartsWith("http://wallbase.cc/") && txtUrl.Text.Contains("board="))
                    {
                        if ((int)txtUrl.Text.GetThpp() > 500)
                        {
                            System.Windows.Forms.MessageBox.Show("You went over the allowed downloading limit. Download aborting", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                        }

                        Download();
                    }
                    else if (txtUrl.Text.StartsWith("http://wallbase.cc/") && (txtUrl.Text.Contains("collection")))
                    {
                        if (!UsePermissions)
                        {
                            System.Windows.Forms.MessageBox.Show("To download collections you need to enable user permissions in the settings menu.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                        }

                        Download();
                    }
                    else System.Windows.Forms.MessageBox.Show("The given URL is not valid.\n" +
                        "\n\nIf you keep getting this error, please read the README.",
                        "URL Invalid Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else System.Windows.Forms.MessageBox.Show("Please specify a URL.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else System.Windows.Forms.MessageBox.Show("Please specify a folder location.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void Download()
        {
            if (Directory.Exists(FolderPath))
                if (!HandleExistingDir(FolderPath))
                    return;

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            LockGUI(false);

            var wall = new Wallbase(FolderPath, Username, Password, UsePermissions,
                 txtUrl.Text, CreateList, CreateLog, Sort);
            {
                var wnd = new ProgressWindow(wall);
                wnd.Closed += (sender, e) => { LockGUI(true); };
                wnd.ShowDialog();
            }
        }

        private bool HandleExistingDir(string path)
        {
            var result = System.Windows.Forms.MessageBox.Show("The directory already exist, do you want to overwrite it?\n\n" +
                "Press NO to merge with the existing wallpapers, both URL and LOG files will be overwritten.", 
                "The directory already exist", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                string[] files = Directory.GetFiles(FolderPath);
                foreach (var file in files)
                    File.Delete(file);

                Directory.Delete(FolderPath, true);
                System.Threading.Thread.Sleep(30);
                Directory.CreateDirectory(FolderPath);

                return true;
            }
            else if (result == System.Windows.Forms.DialogResult.No)
                return true;
            else
                return false;
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
    }
}
