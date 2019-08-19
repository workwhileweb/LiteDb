using Caliburn.Micro;

namespace LiteDbExplorer
{
    public interface IRecentFilesProvider
    {
        IObservableCollection<RecentFileInfo> RecentFiles { get; }
        void InsertRecentFile(string path, string password = null);
        bool RemoveRecentFile(string path);
        void SetRecentFileFixed(string path, bool add);
        bool TryGetPassword(string path, out string storedPassword);
        bool ResetPassword(string path, string password, bool onlyIfStored);
    }
}