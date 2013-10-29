using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Security;
using System.Threading;

namespace WallbaseDownloader
{
    public class Wallbase
    {
        private bool CreateList { get; set; }
        private bool CreateLog { get; set; }
        private bool Sort { get; set; }
        private bool Login { get; set; }
        private bool UseCollections { get; set; }

        private string FolderPath { get; set; }

        private string _url;
        private string Url 
        {
            get { return _url; }
            set
            {
                if (_UriExist(value))
                    _url = value;
                else
                    throw new ArgumentException
                        ("The URI does not exist");

                if (value.Contains("thpp="))
                {
                    if (value.GetThpp() > 60)
                        Delay = 1000;
                    else
                        Delay = 0;
                }
                else
                    Delay = 0;
            }
        }

        private string Username { get; set; }
        private SecureString Password { get; set; }

        public double ToDownload { get; set; }
        public int Failed { get; set; }
        public int Delay { get; set; }

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

        private Log log;

        private int currentId;
        private string currenctWallId;

        private List<Category> _categories;
        private List<Wallpaper> wallpapers;

        public delegate void WallbaseDownloadHandler(object sender, WallbaseDownloadEventArgs args);
        public event WallbaseDownloadHandler OnWallbaseDownload;

        public Wallbase(string folderPath, string username, SecureString password, bool login, 
            string url, bool createList, bool createLog, bool sort) 
        {
            FolderPath = folderPath;
            Url = url;

            Sort = sort;
            CreateLog = createLog;
            CreateList = createList;
            Login = login;

            Username = username;
            Password = password;

            if (url.Contains("collection"))
                UseCollections = true;
            else
                UseCollections = false;

            wallpapers = new List<Wallpaper>();
        }

        public void DownloadWallpapers()
        {
            log = new Log(FolderPath, Url);

            var ids = GetIDList(Url);
            ToDownload = ids.Count;

            if (Sort)
            {
                if (!UseCollections)
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

            log.Add(new String('-', 60));

            foreach (var wall in wallpapers)
            {
                Task t = Task.Factory.StartNew(async () => 
                {
                    await _Download(wall);
                });

                t.Wait();

                Thread.Sleep(Delay);

                log.AddSuccess(wall.ID, wall.WallID);
            }

            if (CreateList)
                foreach (var wall in wallpapers)
                    using (var stream = new StreamWriter(Path.Combine(FolderPath, "links.txt"), true))
                        stream.WriteLine(wall.FileURL);

            if (CreateLog)
            {
                if(Failed > 0)
                    log.AddError(++currentId, "Attention", Failed + " wallpapers failed to download.");

                File.WriteAllLines(log.LogName, log.Events.ToArray());
            }
        }

        private async Task _Download(Wallpaper wall)
        {
            using (var client = new WebClient())
            {
                try 
                {
                    await client.DownloadFileTaskAsync(new Uri(wall.FileURL), wall.FilePath);                   

                    Downloaded++;
                }
                catch (WebException) 
                {
                    Failed++;
                }
            }
        }

        protected void OnWallbaseDownloadChanged(WallbaseDownloadEventArgs e)
        {
            WallbaseDownloadHandler handler = OnWallbaseDownload;
            if (handler != null)
                handler(this, e);
        }

        private bool _UriExist(string url)
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

                    var result = _UriExist(newUrl);

                    if (!parsing)
                    {
                        if (result)
                        {
                            log.AddParsingSuccess(currentId, currenctWallId);

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
                        log.AddParsingAttempt(currentId, currenctWallId, (Category)cat, (Format)format);

                        if (result)
                        {
                            log.AddParsingSuccess(currentId, currenctWallId);
                            
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

            log.AddError(currentId, currenctWallId, "Un-resolved error while parsing");

            return false;
        }

        private List<string> GetIDList(string requestURL)
        {
            _categories = requestURL.GetBoards();

            var regex = new Regex("data-id=\"[0-9]{0,9}\"");
            var csrfReg = new Regex("name=\"csrf\" value=\"(.+?)\"");
            var refReg = new Regex("name=\"ref\" value=\"(.+?)\"");

            var source = string.Empty;
            var ids = new List<string>();

            var uri = new Uri(requestURL);

            using(var client = new CookieClient())
            {
                if (Login)
                {
                    var html = client.DownloadString(@"http://wallbase.cc/user/login");

                    var csrfMatch = csrfReg.Match(html);
                    var refMatch = refReg.Match(html);

                    var credentials = new NameValueCollection 
                    { 
                        { "csrf", csrfMatch.Groups[1].ToString() },
                        { "ref", refMatch.Groups[1].ToString() },
                        { "username", Username },
                        { "password", Password.SecureToString() },
                    };

                    client.UploadValues(@"http://wallbase.cc/user/do_login", "post", credentials);
                }

                source = client.DownloadString(uri);
            }

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
    }

    // VS wants to open this in designer mode for whatever reason
    [System.ComponentModel.DesignerCategory("Code")]
    public class CookieClient : WebClient
    {
        private CookieContainer _cookie = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
            req.ProtocolVersion = HttpVersion.Version10;

            if (req is HttpWebRequest)
                (req as HttpWebRequest).CookieContainer = _cookie;

            return req;
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