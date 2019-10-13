using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Markup;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class EnumToValueDescriptionItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToValueDescriptionItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var result = EnumHelper.GetAllValuesAndDescriptions(_type);

            return result;
        }
    }

    public class EnumValueDescription
    {
        public EnumValueDescription(Enum value, string description)
        {
            Value = value;
            Description = description;
        }

        public Enum Value { get; set; }
        public string Description { get; set; }
    }

    public static class EnumHelper
    {
        public static string Description(this Enum value)
        {
            string result = null;
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
            {
                result = (attributes.First() as DescriptionAttribute)?.Description;
            }

            if (string.IsNullOrEmpty(result))
            {
                // If no description is found, the least we can do is replace underscores with spaces
                // You can add your own custom default formatting logic here
                var ti = CultureInfo.CurrentCulture.TextInfo;
                result = ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
            }

            return result;
        }

        public static IEnumerable<EnumValueDescription> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
            {
                throw new ArgumentException($"{nameof(t)} must be an enum type");
            }

            return Enum.GetValues(t).Cast<Enum>()
                .Select(e => new EnumValueDescription(e, e.Description()))
                .ToList();
        }
    }
}