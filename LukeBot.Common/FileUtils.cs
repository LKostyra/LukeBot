﻿using System.IO;

namespace LukeBot.Common
{
    public class FileUtils
    {
        public static void SetUnifiedCWD()
        {
            string cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string newCwd = cwd + "/../../..";
            Logger.Info("Setting CWD to " + newCwd);
            Directory.SetCurrentDirectory(newCwd);
        }

        public static bool Exists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }
    }
}
