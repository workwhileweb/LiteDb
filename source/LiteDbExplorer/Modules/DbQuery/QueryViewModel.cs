using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
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
    public class QueryViewModel : DocumentConductorOneActive<RunQueryContext>
    {
        private readonly IApplicationInteraction _applicationInteraction;
        private static int _queryRefCount = 0;
        private IOwnerViewModelMessageHandler _view;

        [ImportingConstructor]
        public QueryViewModel(IApplicationInteraction applicationInteraction)
        {
            _applicationInteraction = applicationInteraction;

            DisplayName = "Query";

            IconContent = new PackIcon {Kind = PackIconKind.CodeGreaterThan};

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CurrentDatabase))
                {
                    SetDisplay(CurrentDatabase);
                }
            };

            RunQueryCommand = new RelayCommand(_=> RunQuery(), _=> CanRunQuery);

            RunSelectedQueryCommand = new RelayCommand(_=> RunSelectedQuery(), _=> CanRunSelectedQuery);
        }

        public ICommand RunQueryCommand { get; set; }

        public ICommand RunSelectedQueryCommand { get; set; }

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
            _view = view as IOwnerViewModelMessageHandler;

            _view?.Handle(@"QueryEditorFocus", @"end");

            if (InitialQueryContext?.RunOnStart == true)
            {
                await Task.Delay(250);
                RunQuery();
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
        public void RunQuery()
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

            var rawQueries = query
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

            ActiveItem = Items.FirstOrDefault();
        }
    }

    public class RunQueryContext : IReferenceId
    {
        private RunQueryContext()
        {
            InstanceId = Guid.NewGuid().ToString("D");
        }

        public RunQueryContext(DatabaseReference databaseReference = null, QueryReference queryReference = null) : this()
        {
            DatabaseReference = databaseReference;
            QueryReference = queryReference;
        }

        public string InstanceId { get; }

        public DatabaseReference DatabaseReference { get; }

        public QueryReference QueryReference { get; }

        public bool RunOnStart { get; set; }
    }
}