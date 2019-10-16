using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HL.Manager;
using Humanizer;
using Humanizer.Bytes;
using LiteDbExplorer.Wpf.Framework.Win32;
using ICSharpCode.AvalonEdit;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView : UserControl
    {
        public const double FileSizeMegabytesLimit = 20;

        public static List<FilePreviewHandler> FilePreviewHandlers { get; } = new List<FilePreviewHandler>();

        static FileView()
        {
            FilePreviewHandlers.Add(new ImageFilePreviewHandler());
            FilePreviewHandlers.Add(new TextFilePreviewHandler());            
        }

        public FileView()
        {
            InitializeComponent();

            Loaded += (sender, args) => ThemeManager.CurrentThemeChanged += ThemeManagerOnCurrentThemeChanged;
            Unloaded += (sender, args) => ThemeManager.CurrentThemeChanged -= ThemeManagerOnCurrentThemeChanged;
        }

        private void ThemeManagerOnCurrentThemeChanged(object sender, EventArgs e)
        {
            SetTheme();
        }

        public static readonly DependencyProperty FileSourceProperty = DependencyProperty.Register(
            nameof(FileSource), typeof(object), typeof(FileView), new PropertyMetadata(null, OnFileSourceChanged));

        private static void OnFileSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fileView = d as FileView;
            if (e.NewValue is LiteFileInfo liteFileInfo)
            {
                fileView?.LoadFile(liteFileInfo);
            }
            else
            {
                fileView?.Reset();
            }
        }

        public object FileSource
        {
            get => (object) GetValue(FileSourceProperty);
            set => SetValue(FileSourceProperty, value);
        }

        public object PreviewContent
        {
            get => ContentControl.Content;
            set
            {
                ContentControl.Content = value;
                ContentControl.Visibility = ContentControl.Content == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string FileExtension { get; private set; }

        public void LoadFile(LiteFileInfo file)
        {
            Reset();

            FileExtension = Path.GetExtension(file.Filename) ?? string.Empty;

            SetFileInfoContent(file);

            if (ByteSize.FromBytes(file.Length).Megabytes > FileSizeMegabytesLimit)
            {
                SetNoContentPreview(file, $"File larger than {FileSizeMegabytesLimit} MB.");
                return;
            }

            var handled = false;
            foreach (var filePreviewHandler in FilePreviewHandlers)
            {
                if (filePreviewHandler.CanHandle(file))
                {
                    SetCanContentScroll(filePreviewHandler.CanContentScroll);
                    PreviewContent = filePreviewHandler.GetPreview(file);
                    handled = true;
                    break;
                }
            }

            if (!handled)
            {
                SetNoContentPreview(file);
            }
        }

        protected void SetTheme()
        {
            if (!string.IsNullOrEmpty(FileExtension) && PreviewContent is TextEditor textEditor)
            {
                var highlightingDefinition = ThemedHighlightingManager.Instance.GetDefinitionByExtension(FileExtension);
                textEditor.SyntaxHighlighting = highlightingDefinition;
            }
        }

        protected void SetCanContentScroll(bool enableScroll)
        {
            if (enableScroll)
            {
                ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            else
            {
                ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        protected void ResetNoContentPreview()
        {
            NoFilePreviewPanel.Visibility = Visibility.Collapsed;
            NoFilePreviewImage.Source = null;
            NoFilePreviewText.Text = string.Empty;
        }

        protected void SetNoContentPreview(LiteFileInfo file, string prependMessage = null)
        {
            var message = $"No preview for \"{file.MimeType}\".";
            if (!string.IsNullOrEmpty(prependMessage))
            {
                message += $"\n\n{prependMessage}";
            }

            NoFilePreviewText.Text = message;
            TrySetFileIcon(NoFilePreviewImage, file.Filename);
            NoFilePreviewPanel.Visibility = Visibility.Visible;
        }

        public void Reset()
        {
            if (PreviewContent != null && PreviewContent is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ToolTip = null;
            PreviewContent = null;
            ResetNoContentPreview();
        }

        protected void SetFileInfoContent(LiteFileInfo file)
        {
            var fileInfo = new Dictionary<string, string>
            {
                { "File Name", file.Filename },
                { "File Size", file.Length.Bytes().Humanize("#.##") },
                { "Upload Date", file.UploadDate.ToString("G") },
            };

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                MaxWidth = 450
            }
            .SetDefinitionList(fileInfo);

            var fileIcon = new Image
            {
                Margin = new Thickness(0,0,15,0)
            };

            TrySetFileIcon(fileIcon, file.Filename);

            ToolTip = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    fileIcon,
                    textBlock
                }
            };
            ToolTipService.SetPlacement(this, PlacementMode.Mouse);
        }

        protected static void TrySetFileIcon(Image image, string fileName)
        {
            try
            {
                image.Source = IconManager.FindIconForFilename(fileName, true);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

    }


    public abstract class FilePreviewHandler
    {
        public abstract bool CanContentScroll { get; }
        public abstract bool CanHandle(LiteFileInfo file);
        public abstract FrameworkElement GetPreview(LiteFileInfo file);
    }

    public class ImageFilePreviewHandler : FilePreviewHandler
    {
        public override bool CanContentScroll => true;

        public override bool CanHandle(LiteFileInfo file)
        {
            return file.MimeType.StartsWith("image");
        }

        public override FrameworkElement GetPreview(LiteFileInfo file)
        {
            using (var fStream = file.OpenRead())
            {
                var stream = new MemoryStream();
                fStream.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return GetImagePreview(bitmap);
            }
        }

        protected FrameworkElement GetImagePreview(ImageSource imageSource)
        {
            return new Image{ Stretch = Stretch.None, Source = imageSource };
        }
    }

    public class TextFilePreviewHandler : FilePreviewHandler
    {
        private static readonly Regex TextRegex = new Regex("text|json|script");
        private static readonly string[] HandledTextExtension = {".log", ".md", ".sql", ".json", ".xml"};

        public override bool CanContentScroll => false;

        public override bool CanHandle(LiteFileInfo file)
        {
            var fileExtension = Path.GetExtension(file.Filename);
            return TextRegex.IsMatch(file.MimeType) || HandledTextExtension.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public override FrameworkElement GetPreview(LiteFileInfo file)
        {
            var fileExtension = Path.GetExtension(file.Filename);
            using (var fileStream = file.OpenRead())
            {
                using (var reader = new StreamReader(fileStream))
                {
                    var myStr = reader.ReadToEnd();
                    return GetTextPreview(fileExtension, myStr);
                }
            }
        }

        protected FrameworkElement GetTextPreview(string fileExtension, string text)
        {
            var textEditor = new TextEditor
            {
                IsReadOnly = true,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                ShowLineNumbers = true,
                Padding = new Thickness(15,15,15,0),
                FontSize = PointsToPixels(10),
                FontFamily = new FontFamily(@"Consolas"),
                Options = {EnableEmailHyperlinks = false, EnableHyperlinks = false},
                Document = {Text = text},
                SyntaxHighlighting = ThemedHighlightingManager.Instance.GetDefinitionByExtension(fileExtension)
            };

            return textEditor;
        }

        protected static double PointsToPixels(double points)
        {
            return points*(96.0/72.0);
        }
    }
}
