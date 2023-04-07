using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FriwoControl.Utilities
{
    public class FileUtils
    {
        public static string[] WriteSafeReadAllLines(String path)
        {
            using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(csv))
                {
                    List<string> file = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        file.Add(sr.ReadLine());
                    }

                    return file.ToArray();
                }
            }
        }

        public static string GetAppDataPath(string dir = null)
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            root = Path.Combine(root, "FriwoControl");
            if (!String.IsNullOrEmpty(dir))
            {
                root = Path.Combine(root, dir);
            }

            return root;
        }
    }
}
