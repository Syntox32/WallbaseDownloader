﻿using System;

namespace WallbaseDownloader
{
    public enum Format
    {
        jpg,
        png
    }

    [Flags]
    public enum Category : int
    {
        General = 2,
        Manga = 1,
        HighRes = 3
    }
}