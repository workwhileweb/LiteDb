using System.Globalization;
using System.Windows.Media;
using LiteDB;
using LiteDbExplorer.Presentation.Behaviors;
using LiteDbExplorer.Wpf.Converters;

namespace LiteDbExplorer.Presentation.Converters
{
    public class BsonTypeToForegroundConverter : ConverterBase<BsonType?, SolidColorBrush>
    {
        public static readonly BsonTypeToForegroundConverter Instance = new BsonTypeToForegroundConverter();

        public override SolidColorBrush Convert(BsonType? value, CultureInfo culture)
        {
            return BsonValueForeground.GetBsonValueForeground(value);
        }
    }
}