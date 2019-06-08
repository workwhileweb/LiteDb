using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Humanizer;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.Shared;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.Diagnostics
{
    public class MemoryUsageStatusButton : ShellStatusBarButtonViewModel, IDisposable
    {
        private DispatcherTimer _updateTimer;

        public const string ContentIdTag = nameof(MemoryUsageStatusButton);

        public MemoryUsageStatusButton() : base(ContentIdTag)
        {
            ToolTip = "Memory Usage";

            Icon = new PackIcon {Kind = PackIconKind.Memory};

            MinWidth = 100;
        }

        public bool CanStart => _updateTimer == null || !_updateTimer.IsEnabled;

        public MemoryUsageStatusButton Start()
        {
            if (_updateTimer == null)
            {
                _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                _updateTimer.Tick += UpdateTimerOnTick;
            }

            _updateTimer.Start();

            UpdateStatus();

            NotifyOfPropertyChange(nameof(CanStart));

            return this;
        }

        public MemoryUsageStatusButton Stop()
        {
            _updateTimer.Stop();

            NotifyOfPropertyChange(nameof(CanStart));

            return this;
        }

        public void CollectGc()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void UpdateStatus()
        {
            var currentProc = Process.GetCurrentProcess();
            var memoryUsed = currentProc.PrivateMemorySize64;

            Text = memoryUsed.Bytes().Humanize("#.#");
        }

        public override void OnViewCreated(Button view)
        {
            view.Click += (sender, args) =>
            {
                if (sender is ButtonBase button && button.ContextMenu != null)
                {
                    button.ContextMenu.PlacementTarget = button;
                    button.ContextMenu.Placement = PlacementMode.Top;
                    button.ContextMenu.IsOpen = true;
                    button.ContextMenu.HorizontalOffset =  button.ActualWidth - button.ContextMenu.ActualWidth;
                    button.ContextMenu.VerticalOffset = -5;
                }
            };

            var contextMenu = new ContextMenu
            {
                MinWidth = 170,
                Items =
                {
                    new MenuItem
                    {
                        Header = "GC Collect",
                        Icon = new PackIcon { Kind = PackIconKind.TrashCircle },
                        Command = new RelayCommand(_ => CollectGc())
                    },
                    new Separator(),
                    new MenuItem
                    {
                        Header = "Start",
                        Icon = new PackIcon { Kind = PackIconKind.Play },
                        Command = new RelayCommand(_ => Start(), _ => CanStart)
                    },
                    new MenuItem
                    {
                        Header = "Stop",
                        Icon = new PackIcon { Kind = PackIconKind.Stop },
                        Command = new RelayCommand(_ => Stop(), _ => !CanStart)
                    },
                }
            };

            view.ContextMenu = contextMenu;
        }

        private void UpdateTimerOnTick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        public void Dispose()
        {
            _updateTimer.Tick -= UpdateTimerOnTick;
            _updateTimer.Stop();
        }
    }
}