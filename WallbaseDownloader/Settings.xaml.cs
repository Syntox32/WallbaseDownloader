using System.Security;
using System.Windows;

namespace WallbaseDownloader
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            checkCreateLog.IsChecked = CreateLog;
            checkCreateUrl.IsChecked = CreateList;
            checkUseDate.IsChecked = NameFolderWithDate;
            checkSort.IsChecked = Sort;
            checkUseCredentials.IsChecked = UsePermissions;

            txtUser.Text = Username;
            txtPass.Password = Password.SecureToString();
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

        private void checkUseDate_Checked(object sender, RoutedEventArgs e)
        {
            NameFolderWithDate = (bool)checkUseDate.IsChecked;
        }

        private void checkSort_Checked(object sender, RoutedEventArgs e)
        {
            Sort = (bool)checkSort.IsChecked;
        }

        private void checkCreateUrl_Checked(object sender, RoutedEventArgs e)
        {
            CreateList = (bool)checkCreateUrl.IsChecked;
        }

        private void checkCreateLog_Checked(object sender, RoutedEventArgs e)
        {
            CreateLog = (bool)checkCreateLog.IsChecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void checkUseCredentials_Checked(object sender, RoutedEventArgs e)
        {
            UsePermissions = true;

            txtPass.IsEnabled = true;
            txtUser.IsEnabled = true;
        }

        private void checkUseCredentials_Unchecked(object sender, RoutedEventArgs e)
        {
            UsePermissions = false;

            txtPass.IsEnabled = false;
            txtUser.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UsePermissions)
            {
                Username = txtUser.Text;
                Password = txtPass.SecurePassword;
            }
        }
    }
}
