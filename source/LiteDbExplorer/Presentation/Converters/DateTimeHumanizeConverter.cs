using System;
using System.Globalization;
using Humanizer;
using LiteDbExplorer.Wpf.Converters;

namespace LiteDbExplorer.Presentation.Converters
{
    public class DateTimeHumanizeConverter : ConverterBase<DateTime?, string>
    {
        public override string Convert(DateTime? value, CultureInfo culture)
        {
            if (!value.HasValue)
            {
                return string.Empty;

            }

            return value.Value.Humanize(utcDate:true, culture: CultureInfo.InvariantCulture);
        }
    }
}