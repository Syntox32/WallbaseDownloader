using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;

namespace WallbaseDownloader
{
    public static class Extensions
    {
        public static string ChangeExtentsion(this string str, Format format)
        {
            if (format == Format.jpg)
                return Path.ChangeExtension(str, ".jpg");
            else if (format == Format.png)
                return Path.ChangeExtension(str, ".png");

            return null;
        }

        public static string CreateURL(this string str) 
        {
            string downloadTemplate = "http://wallpapers.wallbase.cc/{0}/wallpaper-{1}.{2}";
            return String.Format(downloadTemplate, "rozne", str, Format.jpg);
        }

        public static string CreateFileName(this string id, string path)
        {
            return Path.Combine(path, "wallpaper-" + id + ".jpg");
        }

        public static List<Category> GetBoards(this string url) 
        {
            var temp = new List<Category>();

            if (url.Contains("collection"))
            {
                temp.Add(Category.General);
                temp.Add(Category.Manga);
                temp.Add(Category.HighRes);
                return temp;
            }

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

        public static string SecureToString(this SecureString securePassword)
        {
            if (securePassword == null)
                return "";

            var str = IntPtr.Zero;
            try 
            {
                str = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(str);
            } 
            finally 
            {
                Marshal.ZeroFreeGlobalAllocUnicode(str);
            }
        }

        public static int GetThpp(this string str)
        {
            var numReg = new Regex("thpp=([0-9]{0,9})");
            var numMatch = numReg.Match(str);

            return Convert.ToInt32(numMatch.Groups[1].ToString());
        }
    }
}