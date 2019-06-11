using System;
using System.Windows;
using System.Windows.Controls;
using Dragablz;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl, IOwnerViewModelMessageHandler
    {
        public ShellView()
        {
            InitializeComponent();
        }

        public void Handle(string message, object payload = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                switch (message)
                {
                    case "CloseToolSetPanel":
                        toolPanelRowDefinition.Height = new GridLength(0);
                        break;
                    case "OpenToolSetPanel":
                        var size = Math.Max(200, ActualHeight / 3.32);
                        toolPanelRowDefinition.Height = new GridLength(size);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
