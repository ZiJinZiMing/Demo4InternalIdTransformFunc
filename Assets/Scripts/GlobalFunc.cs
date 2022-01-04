using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GlobalFunc
{
    public const string Caching = "Caching";

    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            var info = new DirectoryInfo(path);
            info.Delete(true);
        }
    }
}