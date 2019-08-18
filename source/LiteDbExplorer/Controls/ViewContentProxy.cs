using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LiteDbExplorer.Controls
{
    public class ViewContentProxy : INotifyPropertyChanged
    {
        public ViewContentProxy()
        {
        }

        public ViewContentProxy(object content)
        {
            Content = content;
        }

        public object Content { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}