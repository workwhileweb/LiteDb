using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace LiteDbExplorer.Wpf.Converters
{
    public class RoutedCommandToInputGestureTextConverter : IValueConverter
    {
        public static RoutedCommandToInputGestureTextConverter Instance = new RoutedCommandToInputGestureTextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var (text, keyGestureText) = GetDisplay(value, culture);
            if (text != null && keyGestureText != null)
            {
                return $"{text} ({keyGestureText})";
            }
            
            if (keyGestureText != null)
            {
                return keyGestureText;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public static (string text, string keyGestureText) GetDisplay(object value, CultureInfo culture)
        {
            string text = null;
            string keyGestureText = null;
            switch (value)
            {
                case RoutedUICommand uiCommand:
                {
                    text = uiCommand.Text ?? uiCommand.Name;
                    var keyGesture = uiCommand.InputGestures.OfType<KeyGesture>().FirstOrDefault();
                    if (keyGesture != null)
                    {
                        keyGestureText = keyGesture.GetDisplayStringForCulture(culture ?? CultureInfo.CurrentCulture);
                    }
                    break;
                }

                case RoutedCommand command:
                {
                    var keyGesture = command.InputGestures.OfType<KeyGesture>().FirstOrDefault();
                    if (keyGesture != null)
                    {
                        keyGestureText = keyGesture.GetDisplayStringForCulture(culture ?? CultureInfo.CurrentCulture);
                    }
                    break;
                }
            }

            keyGestureText = keyGestureText?.Replace("+", " + ");

            return (text, keyGestureText);
        }
    }
}