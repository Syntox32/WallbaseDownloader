using System;
using System.IO;
using System.Windows.Forms;

namespace WallbaseDownloader
{
    public class Log : IDisposable
    {
        public string Url { get; set; }
        public string LogFolder { get; set; }

        public string LogName  
        { 
            get { return Path.Combine(LogFolder, "log.txt"); } 
        }

        private bool _disposed;

        private StreamWriter _writer = null;

        public Log(string path, string url)
        {
            this.LogFolder = path;
            this.Url = url;
        }

        public void Init() 
        {
            try
            {
                using (var writer = new StreamWriter(LogName))
                {
                    writer.WriteLine("Wallbase Downloader - By Syntox - {0}", DateTime.Now);
                    writer.WriteLine("{0}{1}", new String('-', 60), Environment.NewLine);
                    writer.WriteLine("Folder name: {0}", LogName);
                    writer.WriteLine("Url: {0}{1}", Url, Environment.NewLine);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(String.Format(
                    "An error oucurred trying to write to the log file: \n File: {0} \n Error: {1} \n Stacktrace: {2}" +
                    " \n\n The program will continue as normal. Though, if you keep getting this error, I suggest you restart the application.",
                    LogName, ex.Message, ex.StackTrace), 
                    "Error creating log file", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        public void WriteLine(string str) {
            using (_writer = new StreamWriter(LogName, true)) {
                _writer.WriteLine(str);
            }
        }

        public void Error(int id, string picId, string status) {
            using (_writer = new StreamWriter(LogName, true)) {
                _writer.WriteLine(String.Format("{0} ID: {1} [Downloading] ERROR: The requested picture does not exist or was not found: {2}", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' '), status));
            }
        }

        public void SuccessDownload(int id, string picId) {
            using (_writer = new StreamWriter(LogName, true)) {
                _writer.WriteLine(String.Format("{0} ID: {1} [Downloading] Successfully downloaded!", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' ')));
            }
        }

        public void ParsingAttempt(int id, string picId, Category cat, Format format) {
            using (_writer = new StreamWriter(LogName, true)) {
                _writer.WriteLine(String.Format("{0} ID: {1} [Parsing] Attempting with: [ Category: {2} , Format: {3} ]", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' '), cat, format));
            }
        }

        public void ParsingSuccess(int id, string picId) {
            using (_writer = new StreamWriter(LogName, true)) {
                _writer.WriteLine(String.Format("{0} ID: {1} [Parsing] Parse attempt was successfull!", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' ')));
            }
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Log() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) 
        {
            if (_disposed)
                return;

            if (disposing)
                if (_writer != null)
                    _writer.Dispose();

            _writer = null;

            _disposed = true;
        }
    }
}
