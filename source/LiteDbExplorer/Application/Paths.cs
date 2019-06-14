using System;
using System.Collections.Generic;
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
    public class Paths : INotifyPropertyChanged, IRecentFilesProvider
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreParentPropertiesResolver(true),
            Formatting = Formatting.Indented
        };

        private readonly Lazy<BindableCollection<RecentFileInfo>> _lazyRecentFiles =
            new Lazy<BindableCollection<RecentFileInfo>>(() =>
            {
                var list = new List<RecentFileInfo>();

                var recentFilesExists = File.Exists(RecentFilesPath);
                if (recentFilesExists)
                {
                    try
                    {
                        var value = File.ReadAllText(RecentFilesPath);
                        if (!string.IsNullOrEmpty(value))
                        {
                            var recentFileInfos =
                                JsonConvert.DeserializeObject<RecentFileInfo[]>(value, _jsonSerializerSettings);
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

                            list.Add(new RecentFileInfo(filesPath));
                        }

                        File.Delete(LegacyRecentFilesPath);
                    }
                    catch (Exception e)
                    {
                        SettingsFileReadErrorHandler(e, LegacyRecentFilesPath);
                    }
                }

                var collection = new BindableCollection<RecentFileInfo>(list);

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

        public static string ErrorLogsFilePath => Path.Combine(AppDataPath, "errors.log");

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

        public IObservableCollection<RecentFileInfo> RecentFiles => _lazyRecentFiles.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        public static Maybe<string> GetLastErrorLogPath()
        {
            var directoryInfo = new DirectoryInfo(ErrorLogsFilePath);

            if (!directoryInfo.Exists)
            {
                return null;
            }

            var lastLogFile = directoryInfo.GetFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(p => p.LastWriteTime)
                .FirstOrDefault(p => p.Name.StartsWith("errors"));

            return lastLogFile?.FullName;
        }

        public void InsertRecentFile(string path)
        {
            var recentFileInfo =
                RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                RecentFiles.Remove(recentFileInfo);
            }
            else
            {
                recentFileInfo = new RecentFileInfo(path);
            }

            recentFileInfo.LastOpenedAt = DateTime.Now;

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

        private static void ReorderRecentFiles(IObservableCollection<RecentFileInfo> target)
        {
            if (target == null)
            {
                return;
            }

            var orderedItem = new List<RecentFileInfo>();
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
            if (sender is IObservableCollection<RecentFileInfo> collection)
            {
                var json = JsonConvert.SerializeObject(collection, _jsonSerializerSettings);

                File.WriteAllText(RecentFilesPath, json);

                if (File.Exists(LegacyRecentFilesPath))
                {
                    // File.WriteAllLines(LegacyRecentFilesPath, collection.Select(p => p.FullPath));
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