using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Controls.Editor
{
    public class ImageOrIconPresenter : ContentControl
    {
        public static readonly DependencyProperty ContentSourceProperty = DependencyProperty.Register(
            nameof(ContentSource), typeof(object), typeof(ImageOrIconPresenter), new PropertyMetadata(null, OnContentSourceChanged));

        public object ContentSource
        {
            get => GetValue(ContentSourceProperty);
            set => SetValue(ContentSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource), typeof(ImageSource), typeof(ImageOrIconPresenter), new PropertyMetadata(default(ImageSource), OnImageOrIconChanged));

        public ImageSource ImageSource
        {
            get => (ImageSource) GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty PackIconKindProperty = DependencyProperty.Register(
            nameof(PackIconKind), typeof(PackIconKind?), typeof(ImageOrIconPresenter), new PropertyMetadata(null, OnImageOrIconChanged));

        public PackIconKind? PackIconKind
        {
            get => (PackIconKind) GetValue(PackIconKindProperty);
            set => SetValue(PackIconKindProperty, value);
        }

        private static void OnImageOrIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageOrIconPresenter imageOrIconPresenter)
            {
                imageOrIconPresenter.InvalidateSource();
            }
        }

        private static void OnContentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageOrIconPresenter imageOrIconPresenter)
            {
                imageOrIconPresenter.InvalidateContent();
            }
        }

        private void InvalidateSource()
        {
            if (ImageSource != null)
            {
                ContentSource = ImageSource;
            }
            else if (PackIconKind != null)
            {
                ContentSource = PackIconKind;
            }
            else
            {
                ContentSource = null;
            }
        }

        private void InvalidateContent()
        {
            object content = null;
            if (ContentSource is ImageSource imageSource)
            {
                var image = new Image
                {
                    Source = imageSource,
                    Stretch = Stretch.UniformToFill,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                content = image;
            }
            else if (ContentSource is PackIconKind packIconKind)
            {
                var packIcon = new PackIcon
                {
                    Kind = packIconKind
                };
                content = packIcon;
            }

            Content = content;
        }
    }
}