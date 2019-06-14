using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LiteDbExplorer.Presentation;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.StartPage
{
    [Export(typeof(StartPageViewModel))]
    [Export(typeof(IStartupDocument))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class StartPageViewModel : Document, IStartupDocument
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IApplicationInteraction _applicationInteraction;
        private bool _showStartPageOnOpen;

        [ImportingConstructor]
        public StartPageViewModel(
            IDatabaseInteractions databaseInteractions, 
            IApplicationInteraction applicationInteraction,
            IRecentFilesProvider recentFilesProvider)
        {
            _databaseInteractions = databaseInteractions;
            _applicationInteraction = applicationInteraction;

            PathDefinitions = recentFilesProvider;

            ShowStartPageOnOpen = Properties.Settings.Default.ShowStartPageOnOpen;
            
            PathDefinitions.RecentFiles.CollectionChanged += (sender, args) =>
            {
                NotifyOfPropertyChange(nameof(RecentFilesIsEmpty));
            };
        }
        
        public override string DisplayName => "Start";

        public override object IconContent => IconProvider.GetImageIcon("/Images/icon.png", new ImageIconOptions{Height = 16});

        public IRecentFilesProvider PathDefinitions { get; }
        
        public bool ShowStartPageOnOpen
        {
            get => _showStartPageOnOpen;
            set
            {
                if (!Equals(_showStartPageOnOpen, value))
                {
                    _showStartPageOnOpen = value;
                    SaveSettings();
                }
            }
        }

        [UsedImplicitly]
        public bool RecentFilesIsEmpty => !PathDefinitions.RecentFiles.Any();

        public void SaveSettings()
        {
            Properties.Settings.Default.ShowStartPageOnOpen = ShowStartPageOnOpen;
            Properties.Settings.Default.Save();
        }
        
        [UsedImplicitly]
        public async Task OpenDatabase()
        {
            await _databaseInteractions.OpenDatabase();
        }

        [UsedImplicitly]
        public async Task OpenRecentItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo == null)
            {
                return;
            }

            if (recentFileInfo.FileNotFound.HasValue && recentFileInfo.FileNotFound == true)
            {
                var message = $"File {recentFileInfo.FullPath} not found.\n\nRemove from list?";
                if (_applicationInteraction.ShowConfirm(message, "File not found!"))
                {
                    RemoveFromList(recentFileInfo);
                }
                return;
            }

            await _databaseInteractions.OpenDatabase(recentFileInfo.FullPath);
        }
        
        [UsedImplicitly]
        public void OpenIssuePage()
        {
            Process.Start(Config.IssuesUrl);
        }

        [UsedImplicitly]
        public void OpenHomepage()
        {
            Process.Start(Config.HomepageUrl);
        }

        [UsedImplicitly]
        public void OpenDocs()
        {
            Process.Start("https://github.com/mbdavid/LiteDB/wiki");
        }

        [UsedImplicitly]
        public void RevealInExplorer(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                _applicationInteraction.RevealInExplorer(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void CopyPath(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                _applicationInteraction.PutClipboardText(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void RemoveFromList(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                PathDefinitions.RemoveRecentFile(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void PinItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                PathDefinitions.SetRecentFileFixed(recentFileInfo.FullPath, true);
            }
        }

        [UsedImplicitly]
        public bool CanPinItem(RecentFileInfo recentFileInfo)
        {
            return recentFileInfo != null && !recentFileInfo.IsFixed;
        }

        [UsedImplicitly]
        public void UnPinItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                PathDefinitions.SetRecentFileFixed(recentFileInfo.FullPath, false);
            }
        }

        [UsedImplicitly]
        public bool CanUnPinItem(RecentFileInfo recentFileInfo)
        {
            return recentFileInfo != null && recentFileInfo.IsFixed;
        }
    }
}