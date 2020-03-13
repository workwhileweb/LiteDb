using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LiteDbExplorer
{
    public class Paths : INotifyPropertyChanged, IRecentDatabaseFilesProvider
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreParentPropertiesResolver(true),
            Formatting = Formatting.Indented
        };

        private readonly Lazy<BindableCollection<RecentDatabaseFileInfo>> _lazyRecentFiles =
            new Lazy<BindableCollection<RecentDatabaseFileInfo>>(() =>
            {
                var list = new List<RecentDatabaseFileInfo>();

                var recentFilesExists = File.Exists(RecentFilesPath);
                if (recentFilesExists)
                {
                    try
                    {
                        var value = File.ReadAllText(RecentFilesPath);
                        if (!string.IsNullOrEmpty(value))
                        {
                            var recentFileInfos =
                                JsonConvert.DeserializeObject<RecentDatabaseFileInfo[]>(value, _jsonSerializerSettings);
                            foreach (var recentFileInfo in recentFileInfos)
                            {
                                recentFileInfo.InvalidateInfo();
                                list.Add(recentFileInfo);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SettingsFileReadErrorHandler(e, RecentFilesPath);
                    }
                }

                if (File.Exists(LegacyRecentFilesPath) && !recentFilesExists)
                {
                    try
                    {
                        var filesPaths = File.ReadLines(LegacyRecentFilesPath);
                        foreach (var filesPath in filesPaths)
                        {
                            if (list.Any(p => p.FullPath.Equals(filesPath, StringComparison.OrdinalIgnoreCase)))
                            {
                                continue;
                            }

                            list.Add(new RecentDatabaseFileInfo(4, filesPath));
                        }

                        File.Delete(LegacyRecentFilesPath);
                    }
                    catch (Exception e)
                    {
                        SettingsFileReadErrorHandler(e, LegacyRecentFilesPath);
                    }
                }

                var collection = new BindableCollection<RecentDatabaseFileInfo>(list);

                ReorderRecentFiles(collection);

                collection.CollectionChanged += RecentFiles_CollectionChanged;

                return collection;
            });

        public static string AppDataPath
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LiteDbExplorer");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static string ProgramFolder => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

        public static string UninstallerPath => Path.Combine(ProgramFolder, "uninstall.exe");

        public static string LegacyRecentFilesPath => Path.Combine(AppDataPath, "recentfiles.txt");

        public static string RecentFilesPath => Path.Combine(AppDataPath, "recentfiles.json");

        public static string SettingsFilePath => Path.Combine(AppDataPath, "settings.json");

        public static string ErrorLogsFileFullPath => Path.Combine(AppDataPath, "errors.log");

        public static string ErrorLogsDirectoryPath => AppDataPath;

        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "LiteDbExplorer");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public BindableCollection<RecentDatabaseFileInfo> RecentFiles => _lazyRecentFiles.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        public static IEnumerable<string> GetAllErrorLogPaths()
        {
            var directoryInfo = new DirectoryInfo(ErrorLogsDirectoryPath);
            if (!directoryInfo.Exists)
            {
                return Enumerable.Empty<string>();
            }

            return directoryInfo.GetFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(p => p.LastWriteTime)
                .Where(p => p.Name.StartsWith("errors"))
                .Select(p => p.FullName);
        }

        public static Maybe<string> GetLastErrorLogPath()
        {
            var directoryInfo = new DirectoryInfo(ErrorLogsDirectoryPath);
            if (!directoryInfo.Exists)
            {
                return null;
            }

            var lastLogFile = directoryInfo.GetFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(p => p.LastWriteTime)
                .FirstOrDefault(p => p.Name.StartsWith("errors"));

            return lastLogFile?.FullName;
        }

        public void InsertRecentFile(int databaseVersion, string path, string password = null)
        {
            var recentFileInfo =
                RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            
            if (recentFileInfo != null)
            {
                RecentFiles.Remove(recentFileInfo);
            }
            else
            {
                recentFileInfo = new RecentDatabaseFileInfo(databaseVersion, path);
            }

            if (!string.IsNullOrEmpty(password))
            {
                recentFileInfo.ProtectedPassword = DataProtectionProvider.TryProtectPassword(password, out var securePassword) ? securePassword : null;
            }
            else
            {
                recentFileInfo.ProtectedPassword = null;
            }

            recentFileInfo.LastOpenedAt = DateTime.Now;
            recentFileInfo.DatabaseVersion = databaseVersion;

            RecentFiles.IsNotifying = false;
            RecentFiles.Insert(0, recentFileInfo);
            RecentFiles.IsNotifying = true;

            ReorderRecentFiles(RecentFiles);
        }

        public bool RemoveRecentFile(string path)
        {
            var recentFileInfo =
                RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                return RecentFiles.Remove(recentFileInfo);
            }

            return false;
        }

        public void SetRecentFileFixed(string path, bool add)
        {
            var recentFileInfo =
                RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                recentFileInfo.FixedAt = add ? DateTime.Now : (DateTime?) null;

                ReorderRecentFiles(RecentFiles);
            }
        }

        public bool TryGetPassword(string path, out string storedPassword)
        {
            storedPassword = null;
            var recentFileInfo = RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            
            if (recentFileInfo != null && 
                !string.IsNullOrEmpty(recentFileInfo.ProtectedPassword) && 
                DataProtectionProvider.TryUnprotectPassword(recentFileInfo.ProtectedPassword, out var insecurePassword))
            {
                storedPassword = insecurePassword;
                return true;
            }

            return false;
        }

        public bool ResetPassword(string path, string password, bool onlyIfStored)
        {
            var recentFileInfo =
                RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                if (onlyIfStored && string.IsNullOrEmpty(recentFileInfo.ProtectedPassword))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(password))
                {
                    recentFileInfo.ProtectedPassword = DataProtectionProvider.TryProtectPassword(password, out var securePassword) ? securePassword : null;
                }
                else
                {
                    recentFileInfo.ProtectedPassword = null;
                }

                ReorderRecentFiles(RecentFiles);

                return true;
            }

            return false;
        }

        private static void ReorderRecentFiles(IObservableCollection<RecentDatabaseFileInfo> target)
        {
            if (target == null)
            {
                return;
            }

            var orderedItem = new List<RecentDatabaseFileInfo>();
            orderedItem.AddRange(target.Where(p => p.FixedAt.HasValue).OrderByDescending(p => p.FixedAt));
            orderedItem.AddRange(target.Where(p => !p.FixedAt.HasValue).OrderByDescending(p => p.LastOpenedAt));

            target.IsNotifying = false;

            target.Clear();

            target.IsNotifying = true;

            target.AddRange(orderedItem);
        }

        private static void SettingsFileReadErrorHandler(Exception e, string path)
        {
            App.ShowError(e,
                $"An error occurred while reading the configuration file: '{path}'.\n\nTo avoid this error again a new configuration will be created!");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is IObservableCollection<RecentDatabaseFileInfo> collection)
            {
                var json = JsonConvert.SerializeObject(collection, _jsonSerializerSettings);

                File.WriteAllText(RecentFilesPath, json);

                if (File.Exists(LegacyRecentFilesPath))
                {
                    File.Delete(LegacyRecentFilesPath);
                }
            }
        }

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}