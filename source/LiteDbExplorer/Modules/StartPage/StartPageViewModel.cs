using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using LiteDbExplorer.Presentation;
using LiteDbExplorer.Wpf.Framework.Shell;
using MaterialDesignExtensions.Model;

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

        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<RecentFileInfo> _recentFilesFiltered;

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
            
            var recentFilesTermFilter = this.WhenValueChanged(vm => vm.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(150))
                .Select(CreatePredicate);

            _cleanUp = PathDefinitions.RecentFiles
                .AsObservableChangeSet()
                .Filter(recentFilesTermFilter)
                .Sort(
                    SortExpressionComparer<RecentFileInfo>
                        .Descending(p => p.FixedAt.HasValue)
                        .ThenByDescending(p => p.FixedAt ?? p.LastOpenedAt)
                )
                .ObserveOnDispatcher()
                .Bind(out _recentFilesFiltered)
                .Do(p =>
                {
                    NotifyOfPropertyChange(nameof(RecentFilesListIsEmpty));
                    NotifyOfPropertyChange(nameof(RecentFilesListEmptyMessage));
                })
                .Subscribe();
        }
        
        public override string DisplayName => "Start";

        public override string InstanceId => "StartPage";

        public override object IconContent => IconProvider.GetResourceDrawingImageIcon(@"AppIconImage", new ImageIconOptions{Height = 16});

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
        public bool RecentFilesListIsEmpty => !_recentFilesFiltered.Any();

        [UsedImplicitly]
        public string RecentFilesListEmptyMessage
        {
            get
            {
                if (!RecentFilesListIsEmpty)
                {
                    return null;
                }

                return !string.IsNullOrWhiteSpace(SearchTerm)
                    ? $"No results found for '{SearchTerm}'"
                    : "No recent items in the list";
            }
        }

        public string SearchTerm { get; set; }

        [UsedImplicitly]
        public ReadOnlyObservableCollection<RecentFileInfo> RecentFilesFiltered => _recentFilesFiltered;

        private Func<RecentFileInfo, bool> CreatePredicate(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return info => true;
            }

            var lowerTerm = term.ToLower();
            return info => info.FileName.ToLower().Contains(lowerTerm) || info.DirectoryPath.ToLower().Contains(lowerTerm);
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.ShowStartPageOnOpen = ShowStartPageOnOpen;
            Properties.Settings.Default.Save();
        }

        [UsedImplicitly]
        public void ClearSearch()
        {
            SearchTerm = null;
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