using System;
using System.Windows;

namespace LiteDbExplorer.Wpf.Framework.Windows
{
    public class WindowPosition
    {
        public class Point
        {
            /// <summary>
            /// Width or Left
            /// </summary>
            public double X { get; set; }

            /// <summary>
            /// Height or Top
            /// </summary>
            public double Y { get; set; }
        }

        /// <summary>
        /// Set Left and Top
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Set Width and Height
        /// </summary>
        public Point Size { get; set; }

        public WindowState? WindowState { get; set; }

        public static double GoldenHeight(double preferredSize)
        {
            var height = Math.Max(preferredSize, SystemParameters.VirtualScreenHeight / 1.66);
            if (height > SystemParameters.VirtualScreenHeight)
            {
                height = SystemParameters.VirtualScreenHeight;
            }

            return height;
        }

        public static double GoldenWidth(double preferredSize)
        {
            var width = Math.Min(preferredSize, SystemParameters.VirtualScreenWidth / 1.66);
            if (width > SystemParameters.VirtualScreenWidth)
            {
                width = SystemParameters.VirtualScreenWidth;
            }

            return width;
        }

        public static WindowPosition FromWindow(Window window)
        {
            var windowPosition = new WindowPosition
            {
                WindowState = window.WindowState,
                Position = new Point(),
                Size = new Point()
            };

            if (window.WindowState == System.Windows.WindowState.Maximized)
            {
                windowPosition.Position = new Point
                {
                    X = window.RestoreBounds.Left,
                    Y = window.RestoreBounds.Top
                };

                windowPosition.Size = new Point
                {
                    X = window.RestoreBounds.Width,
                    Y = window.RestoreBounds.Height
                };
            }
            else
            {
                windowPosition.Position = new Point
                {
                    X = window.Left,
                    Y = window.Top
                };

                windowPosition.Size = new Point
                {
                    X = window.Width,
                    Y = window.Height
                };
            }

            if (window.ResizeMode == ResizeMode.CanMinimize || window.ResizeMode == ResizeMode.NoResize)
            {
                windowPosition.WindowState = null;
            }

            return windowPosition;
        }

        public void SetPositionToWindow(Window window)
        {
            if (Position == null)
            {
                return;
            }

            window.Left = Position.X;
            window.Top = Position.Y;
        }

        public void SetSizeToWindow(Window window)
        {
            if (Size == null)
            {
                return;
            }

            window.Width = Size.X;
            window.Height = Size.Y;
        }

        public void SetWindowsStateToWindow(Window window)
        {
            if (WindowState.HasValue)
            {
                window.WindowState = WindowState.Value == System.Windows.WindowState.Minimized
                    ? System.Windows.WindowState.Normal
                    : WindowState.Value;
            }
        }

        public static void ToWindow(Window window, WindowPosition windowPosition)
        {
            try
            {
                windowPosition.SetPositionToWindow(window);
                windowPosition.SetSizeToWindow(window);
                windowPosition.SetWindowsStateToWindow(window);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        /// <summary>
        /// If the window is more than half off of the screen move it up and to the left 
        /// so half the height and half the width are visible.
        /// </summary>
        public static void MoveIntoView(ref WindowPosition windowPosition)
        {
            if (windowPosition.Position == null)
            {
                windowPosition.Position = new Point();
            }

            if (windowPosition.Position.Y + windowPosition.Size.Y / 2 > SystemParameters.VirtualScreenHeight)
            {
                windowPosition.Position.Y = SystemParameters.VirtualScreenHeight - windowPosition.Size.Y;
            }

            if (windowPosition.Position.X + windowPosition.Size.X / 2 > SystemParameters.VirtualScreenWidth)
            {
                windowPosition.Position.X = SystemParameters.VirtualScreenWidth - windowPosition.Size.X;
            }

            if (windowPosition.Position.Y < 0)
            {
                windowPosition.Position.Y = 0;
            }

            if (windowPosition.Position.X < 0)
            {
                windowPosition.Position.X = 0;
            }
        }

        /// <summary>
        /// If the saved window dimensions are larger than the current screen shrink the
        /// window to fit.
        /// </summary>
        public static void SizeToFit(ref WindowPosition windowPosition)
        {
            if (windowPosition.Size == null)
            {
                windowPosition.Size = new Point();
            }

            if (windowPosition.Size.Y > SystemParameters.VirtualScreenHeight)
            {
                windowPosition.Size.Y = SystemParameters.VirtualScreenHeight;
            }

            if (windowPosition.Size.X > SystemParameters.VirtualScreenWidth)
            {
                windowPosition.Size.X = SystemParameters.VirtualScreenWidth;
            }
        }

        public static void SizeToMinSize(ref WindowPosition windowPosition, double factor)
        {
            var height = SystemParameters.WorkArea.Height * factor;
            var width = SystemParameters.WorkArea.Width * factor;

            if (windowPosition.Size == null)
            {
                windowPosition.Size = new Point();
            }

            if (windowPosition.Size.Y < height)
            {
                windowPosition.Size.Y = height;
            }

            if (windowPosition.Size.X < width)
            {
                windowPosition.Size.X = width;
            }
        }
    }
}