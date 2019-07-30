using System.Collections.Generic;
using System.Windows;

namespace LiteDbExplorer.Framework.Windows
{
    public class DialogOptions
    {
        private readonly Dictionary<string, object> _value;
        private double _height;
        private double _width;
        private bool _isMinButtonEnabled;
        private WindowStartupLocation _windowStartupLocation;
        private ResizeMode _resizeMode;
        private SizeToContent _sizeToContent;
        private WindowStyle _windowStyle;
        private bool _showMinButton = true;
        private bool _showMaxRestoreButton = true;
        private bool _showDialogsOverTitleBar;
        private bool _showIconOnTitleBar = true;
        private bool _showInTaskbar = true;
        private double _minWidth;
        private double _minHeight;

        public DialogOptions()
        {
            _value = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Value => _value;

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                _value[nameof(Height)] = value;
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                _value[nameof(Width)] = value;
            }
        }

        public double MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                _value[nameof(MinHeight)] = value;
            }
        }

        public double MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value;
                _value[nameof(MinWidth)] = value;
            }
        }

        public bool IsMinButtonEnabled
        {
            get => _isMinButtonEnabled;
            set
            {
                _isMinButtonEnabled = value;
                _value[nameof(IsMinButtonEnabled)] = value;
            }
        }

        public WindowStartupLocation WindowStartupLocation
        {
            get => _windowStartupLocation;
            set
            {
                _windowStartupLocation = value;
                _value[nameof(WindowStartupLocation)] = value;
            }
        }

        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set
            {
                _resizeMode = value;
                _value[nameof(ResizeMode)] = value;
            }
        }

        public SizeToContent SizeToContent
        {
            get => _sizeToContent;
            set
            {
                _sizeToContent = value;
                _value[nameof(SizeToContent)] = value;
            }
        }

        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set
            {
                _windowStyle = value;
                _value[nameof(WindowStyle)] = value;
            }
        }

        public bool ShowMinButton
        {
            get => _showMinButton;
            set
            {
                _showMinButton = value;
                _value[nameof(ShowMinButton)] = value;
            }
        }

        public bool ShowMaxRestoreButton
        {
            get => _showMaxRestoreButton;
            set
            {
                _showMaxRestoreButton = value;
                _value[nameof(ShowMaxRestoreButton)] = value;
            }
        }

        public bool ShowDialogsOverTitleBar
        {
            get => _showDialogsOverTitleBar;
            set
            {
                _showDialogsOverTitleBar = value;
                _value[nameof(ShowDialogsOverTitleBar)] = value;
            }
        }
        
        public bool ShowIconOnTitleBar
        {
            get => _showIconOnTitleBar;
            set
            {
                _showIconOnTitleBar = value;
                _value[nameof(ShowIconOnTitleBar)] = value;
            }
        }

        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set
            {
                _showInTaskbar = value;
                _value[nameof(ShowInTaskbar)] = value;
            }
        }
    }

    public static class DialogOptionsExtensions
    {
        public static DialogOptions SizeToFit(this DialogOptions dialogOptions)
        {
            if (dialogOptions.Height > SystemParameters.VirtualScreenHeight)
            {
                dialogOptions.Height = SystemParameters.VirtualScreenHeight;
            }

            if (dialogOptions.Width > SystemParameters.VirtualScreenWidth)
            {
                dialogOptions.Width = SystemParameters.VirtualScreenWidth;
            }

            return dialogOptions;
        }

        public static DialogOptions SizeToMinSize(this DialogOptions dialogOptions, double factor)
        {
            var height = SystemParameters.WorkArea.Height * factor;
            var width = SystemParameters.WorkArea.Width * factor;

            if (dialogOptions.Height < height)
            {
                dialogOptions.Height = height;
            }

            if (dialogOptions.Width < width)
            {
                dialogOptions.Width = width;
            }

            return dialogOptions;
        }
    }
}