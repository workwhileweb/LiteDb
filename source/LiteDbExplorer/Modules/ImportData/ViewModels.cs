using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Forge.Forms;
using Forge.Forms.Annotations;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.ImportData
{
    public interface IStepsScreen : INotifyPropertyChanged
    {
        bool HasNext { get; }
        object Next();
        bool Validate();
    }

    public abstract class StepsScreen : Screen, IStepsScreen
    {
        public virtual bool HasNext { get; }

        public abstract object Next();

        public virtual bool Validate()
        {
            return true;
        }
    }

    [Form(Grid = new[] { 1d, 1d })]
    public class ImportTargetDefaultOptions
    {
        public ImportTargetDefaultOptions()
        {
            Databases = Store.Current.Databases;
        }

        public IEnumerable<DatabaseReference> Databases { get; }

        [Field(Row = "0")]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding Databases}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(DatabaseReference.Name))]
        public DatabaseReference TargetDatabase { get; set; }

        [Field(Row = "0")]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding TargetDatabase.Collections}", SelectionType = SelectionType.ComboBoxEditable, DisplayPath = nameof(CollectionReference.Name))]
        public string TargetCollection { get; set; }
        
        [Break]

        [Field(Row = "1")]
        [SelectFrom(typeof(RecordIdHandlerPolice))]
        public RecordIdHandlerPolice InsertMode { get; set; }

        [Field(Row = "1")]
        [SelectFrom(typeof(RecordNullOrEmptyHandlerPolice))]
        public RecordNullOrEmptyHandlerPolice EmptyFieldsMode { get; set; }
    }

    public class ImportSourceFileOptions : INotifyPropertyChanged, IActionHandler
    {
        private readonly Lazy<IApplicationInteraction> _lazyApplicationInteraction;

        protected const string ACTION_OPEN_FILE = "open_file";

        public ImportSourceFileOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _lazyApplicationInteraction = lazyApplicationInteraction;
        }

        
        [Field(IsReadOnly = true)]
        [Value(Must.NotBeEmpty)]
        [Action(ACTION_OPEN_FILE, "Open", Placement = Placement.Inline, Icon = PackIconKind.FolderOpen)]
        public string SourceFile { get; set; }

        public async void HandleAction(IActionContext actionContext)
        {
            var action = actionContext.Action;
            switch (action)
            {
                case ACTION_OPEN_FILE:
                    await OpenFile();
                    break;
            }
        }

        protected virtual Task OnFileOpen(Maybe<string> maybeFilePath)
        {
            // Handler
            return Task.CompletedTask;
        }

        public virtual async Task OpenFile()
        {
            var (title, filter) = GetFileFilter();
            var maybeFilePath = await _lazyApplicationInteraction.Value.ShowOpenFileDialog(title, filter);
            SourceFile = maybeFilePath.HasValue ? maybeFilePath.Value : null;
            await OnFileOpen(maybeFilePath);
        }

        protected virtual (string title, string filter) GetFileFilter()
        {
            return ("All Files", "All Files|*.*");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}