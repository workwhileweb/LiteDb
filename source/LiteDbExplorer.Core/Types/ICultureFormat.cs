using System.Globalization;

namespace LiteDbExplorer.Core
{
    public interface ICultureFormat
    {
        CultureInfo Culture { get; }
        string DateTimeFormat { get; }
    }

    public class DefaultCultureFormat : ICultureFormat
    {
        public const string DefaultDateTimeFormat = @"G";

        public DefaultCultureFormat(CultureInfo culture, string dateTimeFormat = DefaultDateTimeFormat)
        {
            Culture = culture;
            DateTimeFormat = dateTimeFormat;
        }

        public static void EnsureValue(ref ICultureFormat cultureFormat)
        {
            if (cultureFormat == null)
            {
                cultureFormat = Invariant;
            }
        }

        public static ICultureFormat Invariant => new DefaultCultureFormat(CultureInfo.InvariantCulture, DefaultDateTimeFormat);

        public CultureInfo Culture { get; }
        public string DateTimeFormat { get; }
    }
}