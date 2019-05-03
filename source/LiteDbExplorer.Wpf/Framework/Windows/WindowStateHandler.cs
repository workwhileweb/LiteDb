using System;
using System.ComponentModel;
using System.Windows;
using LiteDbExplorer.Wpf.Framework.Windows;

namespace LiteDbExplorer
{
    public static class WindowPositionHandlerExtensions
    {
        public static void AttachPositionHandler(this Window window, IWindowStateStore store, string windowName)
        {
            var unused = new WindowStateHandler(store, window, windowName, true);
        }
    }

    public class WindowStateHandler
    {
        private readonly IWindowStateStore _store;
        private readonly Window _window;
        private readonly string _windowName;
        private bool _ignoreChanges;
        private bool _initialized;
        private WindowState? _lastWindowState;
        private Action _firstRestoreHandler;

        public WindowStateHandler(IWindowStateStore store, Window window, string windowName, bool autoAttach = false)
        {
            _store = store;
            _window = window;
            _windowName = windowName;

            if (autoAttach)
            {
                _window.Loaded += WindowOnLoaded;
                _window.Unloaded += WindowOnUnloaded;
                _window.Closing += WindowOnClosing;
                _window.StateChanged += WindowOnStateChanged;
                _window.LocationChanged += WindowOnLocationChanged;
                _window.SizeChanged += WindowOnSizeChanged;
            }

            // TODO: Track MetroWindow do not using restore bounds
            _firstRestoreHandler = () =>
            {
                if (_window.WindowState == WindowState.Normal)
                {
                    if (_store.WindowPositions.TryGetValue(_windowName, out var windowPosition))
                    {
                        windowPosition.SetPositionToWindow(_window);
                        windowPosition.SetSizeToWindow(_window);
                    }

                    _firstRestoreHandler = null;
                }
            };
        }

        public double MinSizeFactor { get; set; } = 0.45d;

        private bool Immediate { get; set; }

        public bool IsMainWindow { get; set; }

        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            RestoreState();

            _initialized = true;
        }

        private void WindowOnClosing(object sender, CancelEventArgs e)
        {
            if (_window != null)
            {
                SaveState();
            }
        }

        private void WindowOnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_window != null)
            {
                _window.Loaded -= WindowOnLoaded;
                _window.Unloaded -= WindowOnUnloaded;
                _window.Closing -= WindowOnClosing;
                _window.StateChanged -= WindowOnStateChanged;
                _window.LocationChanged -= WindowOnLocationChanged;
                _window.SizeChanged -= WindowOnSizeChanged;
            }
        }

        private void WindowOnStateChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            _firstRestoreHandler?.Invoke();

            if (_window.WindowState != WindowState.Minimized)
            {
                _lastWindowState = _window.WindowState;
            }

            if (Immediate)
            {
                SaveState();
            }
        }

        private void WindowOnLocationChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            if (Immediate)
            {
                SaveState();
            }
        }

        private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            if (Immediate)
            {
                SaveState();
            }
        }

        public void SaveState()
        {
            if (_store == null || _ignoreChanges)
            {
                return;
            }

            var storeWindowPosition = WindowPosition.FromWindow(_window);

            if (storeWindowPosition.WindowState == WindowState.Minimized && _lastWindowState.HasValue)
            {
                storeWindowPosition.WindowState = _lastWindowState.Value;
            }

            _store.WindowPositions[_windowName] = storeWindowPosition;
        }

        public void RestoreState()
        {
            _ignoreChanges = true;

            try
            {
                if (_store != null && _store.WindowPositions.TryGetValue(_windowName, out var data))
                {
                    WindowPosition.SizeToFit(ref data);
                    WindowPosition.MoveIntoView(ref data);
                
                    if (IsMainWindow)
                    {
                        WindowPosition.SizeToMinSize(ref data, MinSizeFactor);
                    }
                
                    WindowPosition.ToWindow(_window, data);
                }
            }
            finally
            {
                _ignoreChanges = false;
            }
        }

        
    }
}
