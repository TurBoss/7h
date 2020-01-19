using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SeventhHeaven.Classes
{
    public class FileUtils
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal static List<string> CopyDirectoryRecursively(string sourceDirName, string destDirName, List<string> filesToExclude, List<string> foldersToExclude, bool doContainsSearch)
        {
            List<string> filesCopied = new List<string>();

            if (filesToExclude == null)
            {
                filesToExclude = new List<string>();
            }

            if (foldersToExclude == null)
            {
                foldersToExclude = new List<string>();
            }

            CopySettings settings = new CopySettings()
            {
                IsMovingFiles = false,
                CopySubFolders = true,
                ExcludeFiles = filesToExclude,
                ExcludeFolders = foldersToExclude,
                ContainsSearchForFiles = doContainsSearch
            };

            try
            {
                CopyOrMoveDirectoryRecursively(sourceDirName, destDirName, settings, filesCopied);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return filesCopied;
        }

        internal static void CopyDirectoryRecursively(string sourceDirName, string destDirName)
        {
            CopySettings settings = new CopySettings()
            {
                IsMovingFiles = false,
                CopySubFolders = true,
                ContainsSearchForFiles = false
            };

            try
            {
                CopyOrMoveDirectoryRecursively(sourceDirName, destDirName, settings);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal static void MoveDirectoryRecursively(string sourceDirName, string destDirName, List<string> filesToExclude, List<string> foldersToExclude, bool doContainsSearch)
        {
            if (filesToExclude == null)
            {
                filesToExclude = new List<string>();
            }

            if (foldersToExclude == null)
            {
                foldersToExclude = new List<string>();
            }

            CopySettings settings = new CopySettings()
            {
                IsMovingFiles = true,
                CopySubFolders = true,
                ExcludeFiles = filesToExclude,
                ExcludeFolders = foldersToExclude,
                ContainsSearchForFiles = doContainsSearch
            };

            try
            {
                CopyOrMoveDirectoryRecursively(sourceDirName, destDirName, settings);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal static string ListAllFiles(string rootPath, int indentLevel = 0)
        {
            StringBuilder allFiles = new StringBuilder();
            string tabs = new String(' ', indentLevel * 4);
            string dashes = new String('-', indentLevel * 4); 

            if (indentLevel == 0)
            {
                // have the string start on a new line
                allFiles.AppendLine();
            }

            foreach (string file in Directory.GetFiles(rootPath))
            {
                allFiles.AppendLine($"{tabs} {file}");
            }


            foreach (string dir in Directory.GetDirectories(rootPath).OrderByDescending(s => s)) // order by descending due to recursive nature; output will be ascending order
            {
                allFiles.AppendLine($"{dashes} {dir}");
                allFiles.Append(ListAllFiles(dir, indentLevel + 1));
            }

            return allFiles.ToString();
        }

        internal static void MoveDirectoryRecursively(string sourceDirName, string destDirName)
        {
            CopySettings settings = new CopySettings()
            {
                IsMovingFiles = true,
                CopySubFolders = true,
                ContainsSearchForFiles = false
            };

            try
            {
                CopyOrMoveDirectoryRecursively(sourceDirName, destDirName, settings);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static void CopyOrMoveDirectoryRecursively(string sourceDirName, string destDirName, CopySettings settings, List<string> copiedFiles = null)
        {
            if (copiedFiles == null)
            {
                copiedFiles = new List<string>();
            }

            if (settings.ExcludeFiles == null)
            {
                settings.ExcludeFiles = new List<string>();
            }

            if (settings.ExcludeFolders == null)
            {
                settings.ExcludeFolders = new List<string>();
            }

            // reference: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (settings.ExcludeFile(file))
                {
                    continue; // skip file as it is excluded
                }

                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                string temppath = Path.Combine(destDirName, file.Name);

                if (settings.IsMovingFiles)
                {
                    if (File.Exists(temppath))
                    {
                        File.Delete(temppath); // delete existing file before moving new file
                    }
                    file.MoveTo(temppath);
                }
                else
                {
                    file.CopyTo(temppath, true);
                }

                copiedFiles.Add(temppath);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (settings.CopySubFolders)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();

                foreach (DirectoryInfo subdir in dirs)
                {
                    if (settings.ExcludeFolders.Contains(subdir.Name))
                    {
                        continue; // skip folder as it is excluded
                    }

                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    CopyOrMoveDirectoryRecursively(subdir.FullName, tempPath, settings, copiedFiles);
                }
            }
        }

        internal static void DeleteEmptyFolders(string pathToCache)
        {
            if (!Directory.Exists(pathToCache))
            {
                return;
            }

            string[] childDirs = Directory.GetDirectories(pathToCache);

            if (childDirs.Length == 0)
            {
                return;
            }

            // recurse to each child directory to delete
            foreach (string child in childDirs)
            {
                DeleteEmptyFolders(child);

                // delete the child if it is empty
                if (Directory.GetFileSystemEntries(child).Length == 0)
                {
                    Directory.Delete(child);
                }
            }
        }

        public static List<string> GetAllFilesInDirectory(string directoryPath)
        {
            List<string> allFiles = new List<string>();

            if (Directory.Exists(directoryPath) == false)
            {
                return allFiles;
            }

            foreach (string file in Directory.GetFiles(directoryPath))
            {
                allFiles.Add(file);
            }

            foreach (string dir in Directory.GetDirectories(directoryPath))
            {
                List<string> subDirFiles = GetAllFilesInDirectory(dir);

                if (subDirFiles.Count > 0)
                {
                    allFiles.AddRange(subDirFiles);
                }
            }

            return allFiles;
        }

        public static BoolWithMessage DeleteFiles(List<string> filesToDelete)
        {
            try
            {
                foreach (string file in filesToDelete)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }

                return BoolWithMessage.True();
            }
            catch (Exception e)
            {
                return BoolWithMessage.False($"Failed to delete files: {e.Message}");
            }
        }

        public static bool AreFilesEqual(string source, string dest)
        {
            return GetMD5Checksum(source).SequenceEqual(GetMD5Checksum(dest)) ? true : false;
        }

        public static string GetMD5Checksum(string s)
        {
            if (!File.Exists(s))
                return "";

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(s))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        class CopySettings
        {
            public bool IsMovingFiles { get; set; }
            public bool CopySubFolders { get; set; }
            public List<string> ExcludeFolders { get; set; }
            public List<string> ExcludeFiles { get; set; }
            public bool ContainsSearchForFiles { get; set; }

            internal bool ExcludeFile(FileInfo file)
            {
                if (ContainsSearchForFiles)
                {
                    return ExcludeFiles.Any(f => file.Name.Contains(f));
                }
                else
                {
                    return ExcludeFiles.Contains(file.Name);
                }
            }
        }
    }


}
