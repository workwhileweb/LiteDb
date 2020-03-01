using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules.CultureSupport
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CultureSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        private string _cultureName = string.Empty;
        // private bool _useNumberGroupSeparator;

        public const string DefaultDateTimeFormat = @"G";

        public CultureSettingsViewModel()
        {
            CultureName = Properties.Settings.Default.CultureFormat_CultureName ?? string.Empty;

            var dateTimeFormat = Properties.Settings.Default.CultureFormat_DateTimeFormat;
            if (string.IsNullOrEmpty(dateTimeFormat))
            {
                dateTimeFormat = DefaultDateTimeFormat;
            }

            DateTimeFormat = dateTimeFormat;
        }

        public string SettingsPageName => Properties.Resources.SettingsPageCultureFormat;

        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;

        public int EditorDisplayOrder => 15;

        public string GroupDisplayName => "Culture and Format";

        public object AutoGenContext => this;

        [Category("Format Options")]
        [DisplayName("Format Culture")]
        [ItemsSourceProperty(nameof(Cultures))]
        [SelectedValuePath("Key")]
        [DisplayMemberPath("Value")]
        public string CultureName
        {
            get => _cultureName;
            set
            {
                var dateTimeFormat = DateTimeFormat;
                if (string.IsNullOrEmpty(dateTimeFormat))
                {
                    dateTimeFormat = DefaultDateTimeFormat;
                }

                // var numberFormat = NumberFormat;

                _cultureName = value;

                DateTimeFormat = dateTimeFormat;
                // NumberFormat = numberFormat;
            }
        }

        [Category("Format Options")]
        [DisplayName("DateTime Format")]
        [ItemsSourceProperty(nameof(DateTimeFormats))]
        [SelectedValuePath("Key")]
        [DisplayMemberPath("Value")]
        public string DateTimeFormat { get; set; }


        /*[Category("Number Options")]
        public bool UseNumberGroupSeparator
        {
            get => _useNumberGroupSeparator;
            set
            {
                var numberFormat = NumberFormat;

                _useNumberGroupSeparator = value;

                NumberFormat = numberFormat;
            }
        }*/

        /*[Category("Number Options")]
        [DisplayName("Number Format")]
        [ItemsSourceProperty(nameof(NumberFormats))]
        [SelectedValuePath("Key")]
        [DisplayMemberPath("Value")]
        public string NumberFormat { get; set; } = string.Empty;*/

        public IReadOnlyDictionary<string, string> Cultures => GetCultureFormats();

        public IReadOnlyDictionary<string, string> DateTimeFormats => GetDateTimeFormats();

        // public IReadOnlyDictionary<string, string> NumberFormats => GetNumberFormats();

        private IReadOnlyDictionary<string, string> GetCultureFormats()
        {
            var result = new Dictionary<string, string>();

            var invariantCulture = CultureInfo.InvariantCulture;
            result.Add(string.Empty, $"{invariantCulture.DisplayName}");

            var installedUICulture = CultureInfo.InstalledUICulture;
            result.Add(installedUICulture.Name, $"[{installedUICulture.Name}] {installedUICulture.DisplayName}");

            foreach (var cultureInfo in CultureInfo.GetCultures(
                CultureTypes.AllCultures & ~CultureTypes.SpecificCultures))
            {
                if (result.ContainsKey(cultureInfo.Name))
                {
                    continue;
                }

                result.Add(cultureInfo.Name, $"[{cultureInfo.Name}] {cultureInfo.DisplayName}");
            }

            return result;
        }

        private IReadOnlyDictionary<string, string> GetDateTimeFormats()
        {
            var now = DateTime.Now;
            var cultureInfo = new CultureInfo(CultureName);

            return new[] {"G", "F", "O", "R", "s", "u", "U"}
                .Select(key => new {key, value = now.ToString(key, cultureInfo)})
                .ToDictionary(p => p.key, p => $"{p.value}");
        }

        /*private IReadOnlyDictionary<string, string> GetNumberFormats()
        {
            var cultureInfo = new CultureInfo(CultureName);

            var sample = 1234567.12;

            var format = UseNumberGroupSeparator ? @"N" : @"G";

            var result = new Dictionary<string, string>
            {
                {string.Empty, $"Selected Culture ({sample.ToString(format, cultureInfo)})"},
                {
                    @"dot", sample.ToString(format,
                        new NumberFormatInfo {NumberDecimalSeparator = ".", NumberGroupSeparator = ","})
                },
                {
                    @"comma", sample.ToString(format,
                        new NumberFormatInfo {NumberDecimalSeparator = ",", NumberGroupSeparator = "."})
                }
            };


            return result;
        }*/

        public void ApplyChanges()
        {
            Properties.Settings.Default.CultureFormat_CultureName = CultureName;
            Properties.Settings.Default.CultureFormat_DateTimeFormat = DateTimeFormat;

            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
    }

}