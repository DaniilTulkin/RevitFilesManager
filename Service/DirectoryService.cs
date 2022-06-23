using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevitFilesManager
{
    internal class DirectoryService
    {
        private string autodeskDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk";
        private RevitServerService revitServerService;

        internal List<DirectoryItem> GetRootItems(bool directoryIsWindows,
                                                         string selectedRSVersion,
                                                         string selectedRSAdress)
        {
            if (directoryIsWindows)
            {
                return Directory.GetLogicalDrives().Select(x => new DirectoryItem
                {
                    RootType = RootType.Windows,
                    FullPath = x,
                    Type = DirectoryItemType.RootType
                }).ToList();
            }
            else
            {
                revitServerService = new RevitServerService(selectedRSVersion, selectedRSAdress);
                ServerProperties sp = revitServerService.GetServerProperties();
                if (sp != null) return sp.Servers.Select(x => new DirectoryItem
                {
                    RootType = RootType.RS,
                    FullPath = "|",
                    Type = DirectoryItemType.RootType
                }).ToList();
                else return new List<DirectoryItem>();
            }
        }

        internal List<string> GetRSVersions()
        {
            return Directory.GetDirectories(autodeskDataPath)
                            .Where(x => x.Contains("Revit Server"))
                            .Select(x => x.Split(new[] { "Revit Server " }, StringSplitOptions.None)[1])
                            .ToList();
        }

        internal List<string> GetRSAdresses(string selectedRSVersion)
        {
            string file = Directory.GetDirectories(autodeskDataPath)
                                   .Where(x => x.Contains($"Revit Server {selectedRSVersion}"))
                                   .ToList()[0] + "\\Config\\RSN.ini";

            return File.ReadAllText(file)
                       .Split(new[] { "\r" }, StringSplitOptions.None)
                       .Select(x => x.Replace(@"\", ""))
                       .Select(x => x.Trim())
                       .ToList();
        }

        internal List<DirectoryItem> GetDirectoryItemContent(RootType rootType, string fullPath)
        {
            List<DirectoryItem> items = new List<DirectoryItem>();

            try
            {
                string[] dirs = null;
                if (rootType == RootType.Windows) dirs = Directory.GetDirectories(fullPath);
                else dirs = revitServerService.GetContents(fullPath).Folders.Select(x => x.Name).ToArray();

                if (dirs.Length > 0) items.AddRange(dirs.Select(x => new DirectoryItem
                {
                    RootType = rootType,
                    FullPath = rootType == RootType.Windows ? x : $"{fullPath}{x}|",
                    Type = DirectoryItemType.FolderType
                }));
            }
            catch { }

            try
            {
                string[] fs = null;
                if (rootType == RootType.Windows) fs = Directory.GetFiles(fullPath);
                else fs = revitServerService.GetContents(fullPath).Models.Select(x => x.Name).ToArray(); ;

                if (fs.Length > 0) items.AddRange(fs.Select(x => new DirectoryItem
                {
                    RootType = rootType,
                    FullPath = rootType == RootType.Windows ? x : $"{fullPath}{x}",
                    Type = DirectoryItemType.FileType
                }));
            }
            catch { }

            return items;
        }

        internal static string GetDirecroryItemName(RootType rootType, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;

            if (rootType == RootType.Windows) return fullPath.Split('\\').Last();
            else
            {
                string[] strings = fullPath.Split('|');
                string folderName = strings.ElementAt(fullPath.Split('|').Count() - 2);
                string fileName = strings.ElementAt(fullPath.Split('|').Count() - 1);
                if (fileName == "") return folderName;
                return fileName;
            }
        }
    }
}
