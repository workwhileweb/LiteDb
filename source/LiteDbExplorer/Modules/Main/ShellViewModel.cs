using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Shell;
using LiteDbExplorer.Modules.Diagnostics;
using LiteDbExplorer.Modules.Shared;

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

            MainContent.ActiveDocumentChanged += MainContentOnActiveDocumentChanged;

            Properties.Settings.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        public object WindowMenu { get; }

        public object WindowRightMenu { get; }

        public object LeftContent { get; }

        public IShellStatusBar StatusBarContent { get; set; }

        public IDocumentSet MainContent { get; }

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

            if (Properties.Settings.Default.Diagnostics_ShowManagedMemory)
            {
                StatusBarContent.ActivateContent(new MemoryUsageStatusButton().Start(), StatusBarContentLocation.Right);
            }
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.Diagnostics_ShowManagedMemory))
            {
                if (Properties.Settings.Default.Diagnostics_ShowManagedMemory)
                {
                    var usageStatusButton = StatusBarContent.ActivateContent(new MemoryUsageStatusButton(), StatusBarContentLocation.Right) as MemoryUsageStatusButton;
                    usageStatusButton?.Start();
                }
                else
                {
                    StatusBarContent.DeactivateContent(MemoryUsageStatusButton.ContentIdTag);
                }
            }
        }

        private async void MainContentOnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (!MainContent.Documents.Any() && Properties.Settings.Default.ShowStartOnCloseAll)
            {
                await MainContent.OpenDocument<IStartupDocument>();
            }            
        }
    }
}