using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;

namespace LiteDbExplorer.Wpf.Controls
{
    public enum SplitOrientation
    {
        [Display(Order = 0)]
        Auto = 0,
        /// <summary> Control or layout should be horizontally oriented. </summary>
        [Display(Order = 1)]
        Horizontal = 1,
        /// <summary> Control or layout should be vertically oriented. </summary>
        [Display(Order = 2)]
        Vertical = 2,
    }

    public static class SplitOrientationExtensions
    {
        public static SplitOrientation ToSplitOrientation(this Orientation? orientation)
        {
            if (!orientation.HasValue)
            {
                return SplitOrientation.Auto;
            }

            return orientation == Orientation.Horizontal ? SplitOrientation.Horizontal : SplitOrientation.Vertical;
        }

        public static Orientation? ToOrientation(this SplitOrientation orientation)
        {
            if (orientation == SplitOrientation.Auto)
            {
                return null;
            }

            return orientation == SplitOrientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical;
        }
    }

    public class SplitContainerOrientationEventArgs : EventArgs
    {
        public SplitContainerOrientationEventArgs(SplitOrientation orientation)
        {
            Orientation = orientation;
        }

        public SplitOrientation Orientation { get; }
    }

    public class SplitContainer : Control
    {
        public event EventHandler<SplitContainerOrientationEventArgs> OrientationChanged;

        static SplitContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainer),
                new FrameworkPropertyMetadata(typeof(SplitContainer)));
        }

        public SplitContainer()
        {
            SizeChanged += OnSizeChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        private static readonly DependencyPropertyKey CurrentOrientationPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(CurrentOrientation), typeof(Orientation), typeof(SplitContainer),
                new FrameworkPropertyMetadata(
                    System.Windows.Controls.Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty CurrentOrientationProperty
            = CurrentOrientationPropertyKey.DependencyProperty;

        public Orientation CurrentOrientation
        {
            get => (Orientation)GetValue(CurrentOrientationProperty);
            protected set => SetValue(CurrentOrientationPropertyKey, value);
        }

        public Orientation? Orientation
        {
            get => (Orientation?) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation?), typeof(SplitContainer),
                new PropertyMetadata(null, OnOrientationPropertyChanged));

        public UIElement FirstChild
        {
            get => (UIElement) GetValue(FirstChildProperty);
            set => SetValue(FirstChildProperty, value);
        }
        
        public static readonly DependencyProperty FirstChildProperty =
            DependencyProperty.Register(nameof(FirstChild), typeof(UIElement), typeof(SplitContainer),
                new PropertyMetadata(null));

        public UIElement SecondChild
        {
            get => (UIElement) GetValue(SecondChildProperty);
            set => SetValue(SecondChildProperty, value);
        }
        
        public static readonly DependencyProperty SecondChildProperty =
            DependencyProperty.Register(nameof(SecondChild), typeof(UIElement), typeof(SplitContainer),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SecondChildIsCollapsedProperty = DependencyProperty.Register(
            nameof(SecondChildIsCollapsed), typeof(bool), typeof(SplitContainer), new PropertyMetadata(false));

        public bool SecondChildIsCollapsed
        {
            get => (bool) GetValue(SecondChildIsCollapsedProperty);
            set => SetValue(SecondChildIsCollapsedProperty, value);
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SplitContainer splitContainer)
            {
                splitContainer.UpdateCurrentOrientation();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCurrentOrientation();
        }

        protected void UpdateCurrentOrientation()
        {
            if (Orientation.HasValue)
            {
                if (CurrentOrientation != Orientation.Value)
                {
                    CurrentOrientation = Orientation.Value;
                    OnOrientationChanged(new SplitContainerOrientationEventArgs(Orientation.ToSplitOrientation()));
                }
            }
            else if (Parent is FrameworkElement frameworkElement)
            {
                // golden ratio based Width
                var elementActualWidth = frameworkElement.ActualWidth / 1.61;
                var elementActualHeight = frameworkElement.ActualHeight;

                var currentOrientation = elementActualWidth < elementActualHeight ? System.Windows.Controls.Orientation.Vertical : System.Windows.Controls.Orientation.Horizontal;
                if (CurrentOrientation != currentOrientation)
                {
                    CurrentOrientation = currentOrientation;
                    OnOrientationChanged(new SplitContainerOrientationEventArgs(Orientation.ToSplitOrientation()));
                }
            }
        }

        protected virtual void OnOrientationChanged(SplitContainerOrientationEventArgs e)
        {
            OrientationChanged?.Invoke(this, e);
        }
    }
}