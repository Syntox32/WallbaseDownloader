using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;

namespace WallbaseDownloader
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow(Wallbase wall)
        {
            InitializeComponent();

            using (var bw = new BackgroundWorker())
            {
                bw.DoWork += (sender, e) =>
                {
                    try
                    {
                        wall.OnDownloadCompleted += wall_OnDownloadCompleted;
                        wall.OnWallbaseDownload += wall_OnWallbaseDownload;
                        wall.DownloadWallpapers();
                    }
                    catch (Exception ex)
                    {
                       System.Windows.Forms.MessageBox.Show("There was an error trying to download: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                bw.RunWorkerAsync();
            }
        }

        void wall_OnDownloadCompleted(object sender, EventArgs e) { }

        void wall_OnWallbaseDownload(object sender, WallbaseDownloadEventArgs args)
        {
            progressBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        progressBar.Value = (int)((args.Downloaded / args.ToDownload) * 100);

                        statusLabel.Content = "Downloaded: {0} / {1} - {2}%".FormatString(args.Downloaded, args.ToDownload, (int)((args.Downloaded / args.ToDownload) * 100));

                        if (((int)((args.Downloaded / args.ToDownload) * 100) == 100))
                            statusLabel.Content += " - Download successfull";
                    }));
        }
    }
}
