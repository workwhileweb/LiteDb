using Caliburn.Micro;

namespace LiteDbExplorer
{
    public interface IRecentFilesProvider
    {
        IObservableCollection<RecentFileInfo> RecentFiles { get; }
        void InsertRecentFile(string path);
        bool RemoveRecentFile(string path);
        void SetRecentFileFixed(string path, bool add);
    }
}