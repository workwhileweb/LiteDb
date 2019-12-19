using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Forge.Forms;
using Forge.Forms.Annotations;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.ImportData
{
    [Form(Mode = DefaultFields.None)]
    public class ImportSourceFromFileScreen : Screen, IActionHandler
    {
        private readonly Lazy<IApplicationInteraction> _lazyApplicationInteraction;

        protected const string ACTION_OPEN_FILE = "open_file";

        public ImportSourceFromFileScreen(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            DisplayName = "Source Options";

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
    }
}