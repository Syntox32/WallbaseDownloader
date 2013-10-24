using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace WallbaseDownloader
{
    public class Wallbase : IDisposable
    {
        private bool UseAsync { get; set; }
        private bool CreateList { get; set; }
        private bool CreateLog { get; set; }
        private bool Sort { get; set; }

        private string Url { get; set; }
        private string FolderPath { get; set; }

        public double ToDownload { get; set; }

        private double _downloaded;
        public double Downloaded 
        {
            get { return _downloaded; }
            set 
            {
                if (value != _downloaded)
                {
                    _downloaded = value;
                    OnWallbaseDownload(this, 
                        new WallbaseDownloadEventArgs(_downloaded, ToDownload));
                }
            }
        }

        private bool _disposed;

        private Log log;
        private WebClient client;

        private int currentId;
        private string currenctWallId;

        private List<Category> _categories;
        private List<Wallpaper> wallpapers;

        public delegate void WallbaseDownloadHandler(object sender, WallbaseDownloadEventArgs args);
        public event WallbaseDownloadHandler OnWallbaseDownload;

        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler OnDownloadCompleted;

        public Wallbase(string folderPath, string url)
        {
            FolderPath = folderPath;
            Url = url;

            Sort = true;
            CreateLog = true;
            CreateList = true;
            UseAsync = false;

            wallpapers = new List<Wallpaper>();
            log = new Log(FolderPath, url);
            client = new WebClient();
            client.Proxy = null;
        }

        public Wallbase(string folderPath, string url, 
            bool useAsync, bool createList, bool createLog, bool sort) 
        {
            FolderPath = folderPath;
            Url = url;

            Sort = sort;
            CreateLog = createLog;
            CreateList = createList;
            UseAsync = useAsync;

            wallpapers = new List<Wallpaper>();

            if (createLog)
                log = new Log(FolderPath, url);

            client = new WebClient();
            client.Proxy = null;
        }

        public void DownloadWallpapers()
        {
            if (CreateLog)
            {
                log.Init();
                System.Threading.Thread.Sleep(30); 
            }

            var ids = GetIDList(Url);
            ToDownload = ids.Count;

            if (Sort) 
            {
                foreach (var item in _categories)
                    Directory.CreateDirectory(Path.Combine(FolderPath, item.ToString()));
            }

            foreach (var ID in ids)
            {
                int i = ids.IndexOf(ID);

                currentId = i;
                currenctWallId = ids[i];

                var uri = ID.CreateURL();
                var path = ID.CreateFileName(FolderPath);

                ParseDownload(uri, path, currentId, currenctWallId);
            }

            if (log != null)
                log.WriteLine(new String('-', 60));

            foreach (var wall in wallpapers)
            {
                if (wall.FileURL.Download(wall.FilePath, UseAsync))
                {
                    if (log != null)
                        log.SuccessDownload(wall.ID, wall.WallID);

                    Downloaded++;
                }
                else
                    if (log != null)
                        log.Error(wall.ID, wall.WallID, "Possible 404");
            }

            if (CreateList)
                foreach (var wall in wallpapers)
                    using(var stream = new StreamWriter(Path.Combine(FolderPath, "url_list.txt"), true))
                        stream.WriteLine(wall.FileURL);

            OnDownloadCompleted(this, EventArgs.Empty);
        }

        protected void OnDownloadCompletedChanged(EventArgs e)
        {
            EventHandler handler = OnDownloadCompleted;
            if (handler != null)
                handler(this, e);
        }

        protected void OnWallbaseDownloadChanged(WallbaseDownloadEventArgs e)
        {
            WallbaseDownloadHandler handler = OnWallbaseDownload;
            if (handler != null)
                handler(this, e);
        }

        private bool download(string url, string path) 
        {
            try 
            { 
                client.DownloadFile(new Uri(url), path); 
                return true; 
            }
            catch (WebException) 
            { 
                return false; 
            }
        }

        private bool _FileExist(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "head";

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                var flag = response.StatusCode == HttpStatusCode.OK;
                response.Dispose();

                return flag;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ParseDownload(string url, string path, int id, string wallId) 
        {
            var wall = new Wallpaper();
            var parsing = false;

            foreach (var cat in _categories)
            {
                foreach (var format in Enum.GetValues(typeof(Format)))
                {
                    var newUrl = url.ChangeCategory((Category)cat).ChangeExtentsion((Format)format);
                    var newPath = string.Empty;

                    if (Sort)
                        newPath = Path.Combine(Path.GetDirectoryName(path), cat.ToString(), Path.GetFileName(path)).ChangeExtentsion((Format)format);
                    else if (!Sort)
                        newPath = path;

                    var result = _FileExist(newUrl);

                    if (!parsing)
                    {
                        if (result)
                        {
                            if (log != null)
                                log.ParsingSuccess(currentId, currenctWallId);

                            wall.FileURL = newUrl;
                            wall.FilePath = newPath;
                            wall.ID = id;
                            wall.WallID = wallId;
                            wallpapers.Add(wall);
                            return true;
                        }
                        else 
                            parsing = true;
                    } 
                    else 
                    {
                        if (log != null)
                            log.ParsingAttempt(currentId, currenctWallId, (Category)cat, (Format)format);

                        if (result)
                        {
                            if (log != null)
                                log.ParsingSuccess(currentId, currenctWallId);
                            
                            wall.FileURL = newUrl;
                            wall.FilePath = newPath;
                            wall.ID = id;
                            wall.WallID = wallId;
                            wallpapers.Add(wall);
                            return true;
                        }
                    }
                }
            }

            if (log != null)
                log.Error(currentId, currenctWallId, "404");

            return false;
        }

        private List<string> GetIDList(string requestURL)
        {
            _categories = requestURL.GetBoards();

            var regex = new Regex("data-id=\"[0-9]{0,7}\"");
            var source = string.Empty;
            var ids = new List<string>();

            var uri = new Uri(requestURL);
            source = client.DownloadString(uri);

            string[] temp = source.Split(
                new string[] { "class=\"wrapper\"" }, StringSplitOptions.None);

            foreach (string str in temp)
            {
                var match = regex.Match(str);
                while (match.Success)
                {
                    var id = Regex.Replace(match.Value, "[^0-9]+", string.Empty);

                    if (!ids.Contains(id))
                        ids.Add(id);
                    match = match.NextMatch();
                }
            }
            return ids;
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Wallbase() 
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                client.Dispose();
                if (log != null)
                    log.Dispose();
            }

            client = null;
            if (log != null)
                log = null;

            _disposed = true;
        }
    }

    public struct Wallpaper 
    {
        public string FilePath { get; set; }
        public string FileURL { get; set; }
        public string WallID { get; set; }
        public int ID { get; set; }
    }

    public class WallbaseDownloadEventArgs : EventArgs
    {
        private double toDownload;
        public double ToDownload { get { return toDownload; } }

        private double downloded;
        public double Downloaded { get { return downloded; } }

        internal WallbaseDownloadEventArgs(double downloaded, double toDownload)
        {
            this.toDownload = toDownload;
            this.downloded = downloaded;
        }
    }
}