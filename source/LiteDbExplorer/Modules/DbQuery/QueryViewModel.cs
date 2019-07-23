using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.Help;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework;
using LiteDbExplorer.Wpf.Framework.Shell;
using LiteDbExplorer.Wpf.Modules.Exception;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.DbQuery
{

    [Export(typeof(QueryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class QueryViewModel : DocumentConductorOneActive<RunQueryContext>, INavigationTarget<RunQueryContext>, IQueryHistoryHandler
    {
        private static int _queryRefCount;
        private readonly IApplicationInteraction _applicationInteraction;
        private readonly IQueryHistoryProvider _queryHistoryProvider;
        private IQueryEditorView _view;

        [ImportingConstructor]
        public QueryViewModel(IApplicationInteraction applicationInteraction, IQueryHistoryProvider queryHistoryProvider)
        {
            _applicationInteraction = applicationInteraction;
            _queryHistoryProvider = queryHistoryProvider;

            DisplayName = "Query";

            IconContent = new PackIcon {Kind = PackIconKind.CodeGreaterThan};

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CurrentDatabase))
                {
                    SetDisplay(CurrentDatabase);
                }
            };

            RunQueryCommand = new RelayCommand(_=> RunAllRawQuery(), _=> CanRunQuery);

            RunSelectedQueryCommand = new RelayCommand(_=> RunSelectedQuery(), _=> CanRunSelectedQuery);

            OpenHelpCommand = new RelayCommand(_=> OpenHelp(), _=> true);

            QueryHistoryView = IoC.Get<QueryHistoryViewModel>();

            QueryHistoryView.Parent = this;
        }

        public ICommand RunQueryCommand { get; }

        public ICommand RunSelectedQueryCommand { get; }

        public ICommand OpenHelpCommand { get; }

        public Screen QueryHistoryView { get; }

        [UsedImplicitly]
        public ObservableCollection<DatabaseReference> Databases => Store.Current.Databases;

        public RunQueryContext InitialQueryContext { get; set; }

        public DatabaseReference CurrentDatabase { get; set; }

        public QueryReference QueryReference { get; set; }

        public string RawQuery { get; set; } = string.Empty;

        public string RawQuerySelected { get; set; } = string.Empty;

        [UsedImplicitly]
        public bool CanRunQuery => CurrentDatabase != null && !string.IsNullOrWhiteSpace(RawQuery);

        [UsedImplicitly]
        public bool CanRunSelectedQuery => CurrentDatabase != null && 
                                           !string.IsNullOrWhiteSpace(RawQuerySelected) &&
                                           RawQuerySelected.Trim().StartsWith("db.", StringComparison.OrdinalIgnoreCase);

        [UsedImplicitly]
        public bool CanExportResult => false;

        [UsedImplicitly]
        public bool ShowHistory { get; set; }

        public override void Init(RunQueryContext item)
        {
            if (item == null)
            {
                TryClose(false);
                return;
            }

            _queryRefCount++;

            InitialQueryContext = item;

            CurrentDatabase = item.DatabaseReference;

            QueryReference = item.QueryReference;

            DisplayName = $"Query {_queryRefCount}";

            if (item.QueryReference != null)
            {
                RawQuery = item.QueryReference.GetRawQuery();
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            _view = view as IQueryEditorView;

            _view?.SelectEnd();

            if (InitialQueryContext?.RunOnStart == true)
            {
                await Task.Delay(250);
                RunAllRawQuery();
            }
        }

        private void SetDisplay(DatabaseReference databaseReference)
        {
            GroupId = databaseReference?.InstanceId;
            GroupDisplayName = databaseReference?.Name;
            GroupDisplayVisibility = GroupDisplayVisibility.AlwaysVisible;
        }

        [UsedImplicitly]
        public async Task NewQuery()
        {
            await _applicationInteraction.OpenQuery(new RunQueryContext(CurrentDatabase, QueryReference));
        }

        [UsedImplicitly]
        public void RunAllRawQuery()
        {
            RunQuery(RawQuery);
        }

        [UsedImplicitly]
        public void RunSelectedQuery()
        {
            if (string.IsNullOrWhiteSpace(RawQuerySelected))
            {
                return;
            }

            RunQuery(RawQuerySelected);
        }

        public void OpenHelp()
        {
            var documentSet = IoC.Get<IDocumentSet>();
            var context = new GithubWikiMarkdownDocContext("Using Shell - Command reference", "mbdavid", "LiteDB", "Shell");
            documentSet.OpenDocument<MarkdownDocViewModel, IMarkdownDocContext>(context);
        }

        private void RunQuery(string query)
        {
            ActiveItem = null;
            Items.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            var rawQueries = RemoveQueryComments(query)
                .Split(new[] { "db.", "DB." }, StringSplitOptions.RemoveEmptyEntries)
                .Select(q => $"db.{q.Trim()}")
                .ToList();

            var resultCount = 0;
            foreach (var rawQuery in rawQueries)
            {
                resultCount++;
                try
                {
                    var results = CurrentDatabase.LiteDatabase.Engine.Run(rawQuery);

                    var resultViewModel = IoC.Get<QueryResultViewModel>();
                    
                    resultViewModel.SetResult(
                        $"Result {resultCount}", 
                        rawQuery,
                        new QueryResult(results));

                    Items.Add(resultViewModel);
                }
                catch (Exception e)
                {
                    var title = $"Query {resultCount} Error";
                    var exceptionScreen = new ExceptionScreenViewModel(title, $"Error on Query {resultCount}:\n'{rawQuery}'", e);
                    Items.Add(exceptionScreen);
                }
            }

            var queryHistory = new RawQueryHistory
            {
                RawQuery = query.Trim(),
                DatabaseLocation = CurrentDatabase?.Location,
                CreatedAt = DateTime.UtcNow,
                LastRunAt = DateTime.UtcNow
            };

            _queryHistoryProvider.Add(queryHistory);

            ActiveItem = Items.FirstOrDefault();
        }

        private static string RemoveQueryComments(string sql)
        {
            const string pattern = @"(?<=^ ([^'""] |['][^']*['] |[""][^""]*[""])*) (--.*$|/\*(.|\n)*?\*/)";
            return Regex.Replace(sql, pattern, "", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
        }

        public Task<Result> InsertQuery(RawQueryHistory item)
        {
            if (item != null)
            {
                var database = Databases.FirstOrDefault(p => p.Location.Equals(item.DatabaseLocation, StringComparison.Ordinal));
                
                // TODO: Notify change current database
                if (database != null && database != CurrentDatabase)
                {
                    CurrentDatabase = database;
                }

                _view?.InsetDocumentText(item.RawQuery);
            }

            return Task.FromResult(Result.Ok());
        }
    }
}