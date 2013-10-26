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
    }
}
