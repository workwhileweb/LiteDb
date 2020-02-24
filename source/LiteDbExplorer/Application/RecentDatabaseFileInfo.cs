using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PropertyChanged;

namespace LiteDbExplorer
{
    public class RecentDatabaseFileInfo : INotifyPropertyChanged
    {
        public RecentDatabaseFileInfo()
        {
        }

        public RecentDatabaseFileInfo(int databaseVersion, string fullPath)
        {
            FullPath = fullPath;
            DatabaseVersion = databaseVersion;
            InvalidateInfo();
        }

        public void InvalidateInfo()
        {
            FileName = Path.GetFileName(FullPath);
            DirectoryPath = Path.GetDirectoryName(FullPath);
            FileNotFound = !File.Exists(FullPath);
        }
        
        public string FullPath { get; set; }
        
        public DateTime? LastOpenedAt { get; set; }

        public DateTime? FixedAt { get; set; }

        public int DatabaseVersion { get; set; }

        [DoNotNotify]
        public string ProtectedPassword { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public string DirectoryPath { get; set; }

        [JsonIgnore]
        public bool? FileNotFound { get; set; }

        [JsonIgnore]
        public bool IsFixed => FixedAt.HasValue;

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}