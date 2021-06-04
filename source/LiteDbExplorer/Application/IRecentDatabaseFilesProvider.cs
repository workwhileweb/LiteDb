using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace LiteDbExplorer
{
    public interface IRecentDatabaseFilesProvider
    {
        BindableCollection<RecentDatabaseFileInfo> RecentFiles { get; }
        void InsertRecentFile(int databaseVersion, string path, string password = null);
        bool RemoveRecentFile(string path);
        void SetRecentFileFixed(string path, bool add);
        bool TryGetPassword(string path, out string storedPassword);
        bool ResetPassword(string path, string password, bool onlyIfStored);
    }
}