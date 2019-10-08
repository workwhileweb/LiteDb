using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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

        public Thickness Margin { get; set; } = new Thickness();

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}