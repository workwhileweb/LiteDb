using Caliburn.Micro;

namespace LiteDbExplorer
{
    public interface IRecentDatabaseFilesProvider
    {
        IObservableCollection<RecentDatabaseFileInfo> RecentFiles { get; }
        void InsertRecentFile(int databaseVersion, string path, string password = null);
        bool RemoveRecentFile(string path);
        void SetRecentFileFixed(string path, bool add);
        bool TryGetPassword(string path, out string storedPassword);
        bool ResetPassword(string path, string password, bool onlyIfStored);
    }
}