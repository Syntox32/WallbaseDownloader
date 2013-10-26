using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WallbaseDownloader
{
    public static class Extensions
    {
        public static string ChangeExtentsion(this string str, Format format)
        {
            if (String.IsNullOrEmpty(str))
                throw new ArgumentNullException();

            if (format == Format.jpg)
                return Path.ChangeExtension(str, ".jpg");
            else if (format == Format.png)
                return Path.ChangeExtension(str, ".png");

            return null;
        }

        public static string CreateURL(this string str) 
        {
            if (String.IsNullOrEmpty(str))
                throw new ArgumentNullException();

            string downloadTemplate = "http://wallpapers.wallbase.cc/{0}/wallpaper-{1}.{2}";
            return String.Format(downloadTemplate, "rozne", str, Format.jpg);
        }

        public static string CreateFileName(this string id, string path)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentNullException();

            return Path.Combine(path, id + ".jpg");
        }

        public static List<Category> GetBoards(this string url) 
        {
            var temp = new List<Category>();

            var board = new Regex("(board=[0-9]{0,3})");
            var boardMatch = board.Match(url);
            var boards = Regex.Replace(boardMatch.Value, "[^0-9]+", string.Empty);
            
            if (boards.Contains("2"))
                temp.Add(Category.General);
            if (boards.Contains("1"))
                temp.Add(Category.Manga);
            if (boards.Contains("3"))
                temp.Add(Category.HighRes);

            return temp;
        }

        public static string ChangeCategory(this string str, Category category)
        {
            if (String.IsNullOrEmpty(str))
                throw new ArgumentNullException();

            var pattern = @"(high-resolution|rozne|manga)";

            switch ((int)category) 
            {
                case 2:
                    return (Regex.Replace(str, pattern, "rozne"));
                case 1:
                    return (Regex.Replace(str, pattern, "manga-anime"));
                case 3:
                    return (Regex.Replace(str, pattern, "high-resolution"));
            }
            return null;
        }
    }
}