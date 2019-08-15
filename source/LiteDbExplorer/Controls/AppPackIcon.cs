using System.Collections.Generic;
using System.Windows;

namespace LiteDbExplorer.Controls
{
    public enum AppIconKind
    {
        FolderOutline,
        LayoutExpandLeft,
        LayoutExpandLeftInverse
    }

    public class AppPackIcon: LocalPackIconBase<AppIconKind>
    {        
        static AppPackIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppPackIcon), new FrameworkPropertyMetadata(typeof(AppPackIcon)));
        }

        public AppPackIcon() : base(AppIconDataFactory.Create) { }
    }

    internal static class AppIconDataFactory
    {
        internal static IDictionary<AppIconKind, string> Create() => new Dictionary<AppIconKind, string>
        {
            {
                AppIconKind.FolderOutline,
                "M5,4.000001L10,4.000001C10.553,4.000001 11,4.4480009 11,5.000001 11,5.552001 10.553,6.000001 10,6.000001L5,6.000001C4.447,6.000001 4,5.552001 4,5.000001 4,4.4480009 4.447,4.000001 5,4.000001z M2,2L2,22 30,22 30,6 16.544006,6C14.816956,6,13.334961,4.826004,12.940979,3.1439972L12.672974,2z M1.7699585,0L12.91095,0C13.699951,0,14.385986,0.54400635,14.565979,1.3119965L14.888,2.6880035C15.067993,3.4559937,15.753967,4,16.544006,4L30.22998,4C31.207031,4,32,4.7920074,32,5.7700043L32,22.229996C32,23.208008,31.207031,24,30.22998,24L1.7699585,24C0.79199219,24,0,23.208008,0,22.229996L0,1.7700043C0,0.79200745,0.79199219,0,1.7699585,0z"
            },
            {
                AppIconKind.LayoutExpandLeft,
                "F1 M 19,19L 19,57L 57,57L 57,19L 19,19 Z M 54,54L 22,54L 22,22.0001L 54,22L 54,54 Z M 52,24.0001L 40,24.0001L 40,52L 52,52L 52,24.0001 Z M 38,36L 29.3334,36L 33.0001,31.0001L 29.0001,31L 24,38L 29.0001,45L 33.0001,45L 29.3334,40L 38,40L 38,36 Z"
            },
            {
                AppIconKind.LayoutExpandLeftInverse,
                "F1 M 57,19L 57,57L 19,57L 19,19L 57,19 Z M 22,54L 54,54L 54,22.0001L 22,22L 22,54 Z M 24,24.0001L 36,24.0001L 36,52L 24,52L 24,24.0001 Z M 52,36L 43.3334,36L 47,31.0001L 43,31L 38,38L 43,45L 47,45L 43.3334,40L 52,40L 52,36 Z"
            }
        };
    }
}