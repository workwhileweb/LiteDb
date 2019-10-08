using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using JetBrains.Annotations;

namespace LiteDbExplorer.Controls
{
    public class ViewModelContentProxy : INotifyPropertyChanged
    {
        public ViewModelContentProxy()
        {
        }

        public ViewModelContentProxy(IScreen content)
        {
            Content = content;
        }

        public IScreen Content { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}