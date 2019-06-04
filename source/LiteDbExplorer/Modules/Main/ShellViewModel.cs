using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShell))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ShellViewModel : Screen, IShell
    {
        public ShellViewModel()
        {
            DisplayName = "LiteDB Explorer";

            WindowMenu = IoC.Get<IShellMenu>();

            WindowRightMenu = IoC.Get<IShellRightMenu>();

            StatusBarContent = IoC.Get<IShellStatusBar>();

            LeftContent = IoC.Get<IDocumentExplorer>();

            MainContent = IoC.Get<IDocumentSet>();

            MainContent.ActiveDocumentChanged += async (sender, args) =>
            {
                if (!MainContent.Documents.Any() && Properties.Settings.Default.ShowStartOnCloseAll)
                {
                    await MainContent.OpenDocument<IStartupDocument>();
                }
            };
        }

        protected override void OnViewReady(object view)
        {
            try
            {
                if (Application.Current.Properties["ArbitraryArgName"] != null)
                {
                    var arg = Application.Current.Properties["ArbitraryArgName"].ToString();
                    switch (arg)
                    {
                        case CmdlineCommands.New:
                        {
                            IoC.Get<IDatabaseInteractions>().CreateAndOpenDatabase().Wait();
                            break;
                        }
                        case CmdlineCommands.Open:
                        {
                            IoC.Get<IDatabaseInteractions>().OpenDatabase().Wait();
                            break;
                        }
                    }

                    Application.Current.Properties["ArbitraryArgName"] = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            if (Properties.Settings.Default.ShowStartPageOnOpen)
            {
                await MainContent.OpenDocument<IStartupDocument>();
            }
        }

        public object WindowMenu { get; }

        public object WindowRightMenu { get; }

        public object StatusBarContent { get; set; }

        public object LeftContent { get; }

        public IDocumentSet MainContent { get; }
    }
}