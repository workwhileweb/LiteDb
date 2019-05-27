using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using NLog;

namespace LiteDbExplorer.Modules
{
    [Export(typeof(PipeServiceBootstrapper))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PipeServiceBootstrapper
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string[] _commands = {CmdlineCommands.Open, CmdlineCommands.New, CmdlineCommands.Focus};

        private PipeService _pipeService;
        private PipeServer _pipeServer;

        [ImportingConstructor]
        public PipeServiceBootstrapper(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;
        }

        public void Init()
        {
            if ((Application.Current as App)?.OriginalInstance == true)
            {
                _pipeService = new PipeService();
                _pipeService.CommandExecuted += PipeService_CommandExecuted;
                _pipeServer = new PipeServer(Config.PipeEndpoint);
                _pipeServer.StartServer(_pipeService);

                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1 && !_commands.Contains(args[1], StringComparer.OrdinalIgnoreCase))
                {
                    PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommands.Open, args[1]));
                }
            }
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            Logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                {
                    RestoreWindow();
                    break;
                }

                case CmdlineCommands.New:
                {
                    _databaseInteractions.CreateAndOpenDatabase().Wait();
                    RestoreWindow();
                    break;
                }

                case CmdlineCommands.Open:
                {
                    if (!string.IsNullOrEmpty(args.Args))
                    {
                        if (args.Args.Equals("new", StringComparison.OrdinalIgnoreCase))
                        {
                            _databaseInteractions.CreateAndOpenDatabase().Wait();
                        }
                        else if (args.Args.Equals("open", StringComparison.OrdinalIgnoreCase))
                        {
                            _databaseInteractions.OpenDatabase().Wait();
                        }
                        else
                        {
                            _databaseInteractions.OpenDatabase(args.Args).Wait();
                        }
                    }
                    else
                    {
                        _databaseInteractions.OpenDatabase().Wait();    
                    }

                    RestoreWindow();
                    break;
                }

                default:
                {
                    Logger.Warn("Unknown command received");
                    break;
                }
            }
        }

        private void RestoreWindow()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                // Show();
                // WindowState = WindowState.Normal;
                mainWindow.Activate();
                mainWindow.Focus();
            }
        }
    }
}