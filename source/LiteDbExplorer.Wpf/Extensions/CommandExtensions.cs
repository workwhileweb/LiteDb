using System.Windows;
using System.Windows.Input;

namespace LiteDbExplorer.Presentation
{
    public static class CommandExtensions
    {
        public static void ExecuteOnMain(this RoutedUICommand command, object parameter = null)
        {
            command.Execute(parameter, Application.Current.MainWindow);
        }
    }
}