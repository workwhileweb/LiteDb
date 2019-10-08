using System.Globalization;

namespace LiteDbExplorer.Wpf.Converters
{
    public class BooleanToIntegerConverter : ConverterBase<bool, int>
    {
        public BooleanToIntegerConverter()
        {
            TrueValue = 1;
            FalseValue = 0;
        }

        public int TrueValue { get; set; }
        public int FalseValue { get; set; }
        public bool Inverse { get; set; }

        public override int Convert(bool value, CultureInfo culture)
        {
            if (Inverse)
            {
                return value ? FalseValue : TrueValue;
            }

            return value ? TrueValue : FalseValue;
        }
    }
}