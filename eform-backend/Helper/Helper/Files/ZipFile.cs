using System;
using System.IO;
using System.IO.Compression;

namespace Helper.Files
{
    public class ZipFiles
    {
        public static void CreateFromDirectory(string startPath, string zipPath)
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            ZipFile.CreateFromDirectory(startPath, zipPath);
        }
        public static void ExtractToDirectory(string zipPath, string extractPath)
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
    }
}
