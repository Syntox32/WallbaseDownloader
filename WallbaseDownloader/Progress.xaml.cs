using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace WallbaseDownloader
{
    public partial class ProgressWindow : Window
    {
        BackgroundWorker bw = new BackgroundWorker();

        public ProgressWindow(Wallbase wall)
        {
            InitializeComponent();

            bw.RunWorkerCompleted += (sender, e) => 
            {
                bw.Dispose();
            };

            bw.DoWork += (sender, e) =>
            {
                try
                {
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

        void wall_OnWallbaseDownload(object sender, WallbaseDownloadEventArgs args)
        {
            progressBar.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.Normal, new Action( delegate()
                    {
                        progressBar.Value = (int)((args.Downloaded / args.ToDownload) * 100);

                        statusLabel.Content = String.Format("Downloaded: {0} / {1} - {2}%", args.Downloaded, args.ToDownload, (int)((args.Downloaded / args.ToDownload) * 100));

                        if (((int)((args.Downloaded / args.ToDownload) * 100) == 100))
                            statusLabel.Content += " - Download successfull";
                    }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (bw.IsBusy)
                bw.CancelAsync();
        }
    }
}
