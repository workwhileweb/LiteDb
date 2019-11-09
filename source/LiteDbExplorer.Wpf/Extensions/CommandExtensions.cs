using System.Windows;
using System.Windows.Input;

namespace LiteDbExplorer.Presentation
{
    public static class CommandExtensions
    {
        public static void ExecuteOnMainWindow(this RoutedUICommand command, object parameter = null)
        {
            command.Execute(parameter, Application.Current.MainWindow);
        }
    }
}