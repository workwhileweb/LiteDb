using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using JetBrains.Annotations;

namespace LiteDbExplorer.Modules.Main
{
    public class ShellLayoutController : Freezable, INotifyPropertyChanged
    {
        private static readonly Lazy<ShellLayoutController> _instance = 
            new Lazy<ShellLayoutController>(()=> new ShellLayoutController());

        private ShellLayoutController()
        {
        }

        public static ShellLayoutController Current => _instance.Value;

        public bool LeftContentIsVisible { get; set; }

        public bool ToolsPanelIsVisible { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            return Current;
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