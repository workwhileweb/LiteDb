using System.ComponentModel.Composition;
using System.Windows.Controls;
using Caliburn.Micro;
using LiteDbExplorer.Controls;

namespace LiteDbExplorer.Wpf.Modules.Exception
{
    [Export(typeof(ExceptionScreenViewModel))]
    public class ExceptionScreenViewModel : Screen
    {
        private readonly string _message;
        private readonly System.Exception _exception;
        private ExceptionScreenView _exceptionScreenView;

        public ExceptionScreenViewModel(string displayName, string message, System.Exception exception)
        {
            _message = message;
            _exception = exception;

            DisplayName = displayName;
        }

        protected override void OnViewLoaded(object view)
        {
            _exceptionScreenView = view as ExceptionScreenView;
            _exceptionScreenView?.SetException(_message, _exception);
        }
    }

    public class ExceptionScreenView : ContentControl
    {
        public void SetException(string message, System.Exception exception)
        {
            Content = new ExceptionViewer(message, exception);
        }

        public void Clear()
        {
            Content = null;
        }
    }
}