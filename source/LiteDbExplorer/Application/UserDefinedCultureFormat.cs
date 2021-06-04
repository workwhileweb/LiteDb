using System;
using System.Globalization;
using LiteDbExplorer.Core;

namespace LiteDbExplorer
{
    public class UserDefinedCultureFormat : ICultureFormat
    {
        public const string DefaultDateTimeFormat = @"G";

        public static readonly Lazy<UserDefinedCultureFormat> _default = new Lazy<UserDefinedCultureFormat>(() => new UserDefinedCultureFormat());

        private UserDefinedCultureFormat()
        {
            Refresh();

            Properties.Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals(nameof(Properties.Settings.CultureFormat_CultureName)) || 
                    args.PropertyName.Equals(nameof(Properties.Settings.CultureFormat_DateTimeFormat)))
                {
                    Refresh();
                }
            };
        }

        public static UserDefinedCultureFormat Default => _default.Value;

        public CultureInfo Culture { get; private set; }

        public string DateTimeFormat { get; private set; }

        protected void Refresh()
        {
            var cultureName = Properties.Settings.Default.CultureFormat_CultureName ?? string.Empty;

            Culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : new CultureInfo(cultureName);
            
            var dateTimeFormat = Properties.Settings.Default.CultureFormat_DateTimeFormat;
            if (string.IsNullOrEmpty(dateTimeFormat))
            {
                dateTimeFormat = DefaultDateTimeFormat;
            }

            DateTimeFormat = dateTimeFormat;
        }
    }
}