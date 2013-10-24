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

            checkAsync.IsChecked = UseAsync;
            checkCreateLog.IsChecked = CreateLog;
            checkCreateUrl.IsChecked = CreateList;
            checkUseDate.IsChecked = NameFolderWithDate;
            checkSort.IsChecked = Sort;
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
    }
}
