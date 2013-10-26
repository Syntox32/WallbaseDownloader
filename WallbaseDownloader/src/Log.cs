using System;
using System.Collections.Generic;
using System.IO;

namespace WallbaseDownloader
{
    public class Log
    {
        public string Url { get; set; }
        public string LogFolder { get; set; }

        public string LogName  
        { 
            get { return Path.Combine(LogFolder, "log.txt"); } 
        }

        private List<string> _events;
        public List<string> Events {
            get { return _events; }
        }

        public Log(string path, string url)
        {
            this.LogFolder = path;
            this.Url = url;

            _events = new List<string>();

            _events.Add(String.Format("Wallbase Downloader - By Syntox - {0}", DateTime.Now));
            _events.Add(String.Format("{0}{1}", new String('-', 60), Environment.NewLine));
            _events.Add(String.Format("Folder name: {0}", Path.Combine(LogFolder, "log.txt")));
            _events.Add(String.Format("Url: {0}{1}", url, Environment.NewLine));
        }

        public void Add(string str) {
            _events.Add(str);
        }

        public void AddError(int id, string picId, string status) {
            _events.Add(String.Format("{0} ID: {1} [Downloading] ERROR: The requested picture does not exist or was not found: {2}", 
                id.ToString().PadRight(2, ' '), picId.PadRight(8, ' '), status));
        }

        public void AddSuccess(int id, string picId) {
            _events.Add(String.Format("{0} ID: {1} [Downloading] Successfully downloaded!", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' ')));
        }

        public void AddParsingAttempt(int id, string picId, Category cat, Format format) {
            _events.Add(String.Format("{0} ID: {1} [Parsing] Attempting with: [ Category: {2} , Format: {3} ]", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' '), cat, format));
        }

        public void AddParsingSuccess(int id, string picId) {
            _events.Add(String.Format("{0} ID: {1} [Parsing] Parse attempt was successfull!", id.ToString().PadRight(2, ' '), picId.PadRight(8, ' ')));
        }
    }
}
