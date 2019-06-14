using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LiteDbExplorer
{
    public class RecentFileInfo : INotifyPropertyChanged
    {
        public RecentFileInfo()
        {
        }

        public RecentFileInfo(string fullPath)
        {
            FullPath = fullPath;
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

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public string DirectoryPath { get; set; }

        [JsonIgnore]
        public bool? FileNotFound { get; set; }

        [JsonIgnore]
        public bool IsFixed => FixedAt.HasValue;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}