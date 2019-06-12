using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework.Services;
using Serilog;

namespace LiteDbExplorer.Modules
{
    public class DefaultCommandsHandler : ApplicationCommandHandler
    {
        private static readonly ILogger Logger = Log.ForContext<DefaultCommandsHandler>();

        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IApplicationInteraction _applicationInteraction;

        [ImportingConstructor]
        public DefaultCommandsHandler(
            IDatabaseInteractions databaseInteractions,
            IApplicationInteraction applicationInteraction,
            IEventAggregator eventAggregator)
        {
            _databaseInteractions = databaseInteractions;
            _applicationInteraction = applicationInteraction;

            Add(Commands.Exit, (sender, args) =>
            {
                Store.Current.CloseDatabases();

                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
            });

            Add(ApplicationCommands.Open, async (sender, args) =>
            {
                await _databaseInteractions.OpenDatabase();
            });

            Add(ApplicationCommands.New, async (sender, args) =>
            {
                await _databaseInteractions.CreateAndOpenDatabase();
            });

            Add(Commands.FileDropped, async (sender, args) =>
            {
                if (!(args.Parameter is IDataObject dataObject))
                {
                    return;
                }

                try
                {
                    if (dataObject.GetDataPresent(DataFormats.FileDrop) && dataObject.GetData(DataFormats.FileDrop, false) is string[] paths)
                    {
                        await _databaseInteractions.OpenDatabases(paths);
                    }
                }
                catch (Exception exc)
                {
                    _applicationInteraction.ShowError("Failed to open database: " + exc.Message, "Database Error");
                }
            });
        }

    }
}