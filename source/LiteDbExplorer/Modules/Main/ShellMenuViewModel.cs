using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework.Shell;
using LiteDbExplorer.Wpf.Modules.Settings;

namespace LiteDbExplorer.Modules.Main
{

    [Export(typeof(IShellMenu))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellMenuViewModel : PropertyChangedBase, IShellMenu
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IWindowManager _windowManager;
        private readonly IApplicationInteraction _applicationInteraction;
        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public ShellMenuViewModel(
            IDatabaseInteractions databaseInteractions,
            IWindowManager windowManager,
            IApplicationInteraction applicationInteraction,
            IEventAggregator eventAggregator,
            IRecentFilesProvider recentFilesProvider)
        {
            _databaseInteractions = databaseInteractions;
            _windowManager = windowManager;
            _applicationInteraction = applicationInteraction;
            _eventAggregator = eventAggregator;

            PathDefinitions = recentFilesProvider;
        }

        public IRecentFilesProvider PathDefinitions { get; }
        
        [UsedImplicitly]
        public async Task OpenRecentItem(RecentFileInfo info)
        {
            if (info == null)
            {
                return;
            }

            await _databaseInteractions.OpenDatabase(info.FullPath);
        }

        [UsedImplicitly]
        public void OpenIssuePage()
        {
            _applicationInteraction.ShowIssueHelper();
        }

        [UsedImplicitly]
        public void OpenHomepage()
        {
            Process.Start(Config.HomepageUrl);
        }

        [UsedImplicitly]
        public async Task OpenStartupDocument()
        {
            await IoC.Get<IDocumentSet>().OpenDocument<IStartupDocument>();
        }

        [UsedImplicitly]
        public void OpenSettings()
        {
            _windowManager.ShowDialog(IoC.Get<SettingsViewModel>());
        }

        [UsedImplicitly]
        public void OpenAbout()
        {
            _applicationInteraction.ShowAbout();
        }

        [UsedImplicitly]
        public void ShowReleaseNotes()
        {
            _applicationInteraction.ShowReleaseNotes();
        }

        [UsedImplicitly]
        public async Task ViewErrorLogs()
        {
            var lastErrorLogPath = Paths.GetLastErrorLogPath();
            if (lastErrorLogPath.HasNoValue)
            {
                _applicationInteraction.ShowAlert("No error log file available.");
                return;
            }

            await _applicationInteraction.OpenFileWithAssociatedApplication(lastErrorLogPath.Value);
        }

        [UsedImplicitly]
        public async Task ViewAppData()
        {
            var appDataPath = Paths.AppDataPath;
            
            await _applicationInteraction.RevealInExplorer(appDataPath);
        }

    }
}