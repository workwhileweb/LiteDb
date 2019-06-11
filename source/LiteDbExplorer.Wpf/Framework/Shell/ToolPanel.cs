using System.Windows.Input;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Wpf.Framework.Shell
{
    public abstract class ToolPanel : LayoutItemBase, IToolPanel
    {
        private ICommand _closeCommand;

        public override ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => Close(true), p => CanClose())); }
        }

        public virtual double PreferredHeight => 200;

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public override bool ShouldReopenOnStart => false;

        protected virtual void Close(bool close)
        {
            IsVisible = false;
        }

        protected virtual bool CanClose()
        {
            return true;
        }

        protected ToolPanel()
        {
            IsVisible = true;
        }
    }
}