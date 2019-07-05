using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DynamicData;
using Forge.Forms.Annotations;
using JetBrains.Annotations;

namespace LiteDbExplorer.Modules.Shared
{
    [Title("Export options")]
    [Action("cancel", "CANCEL", IsCancel = true, ClosesDialog = true)]
    [Action("ok", "EXPORT", Parameter = "{Binding ExportFormat}", IsDefault = true, ClosesDialog = true,
        Validates = true)]
    public class CollectionExportOptions : INotifyPropertyChanged
    {
        public CollectionExportOptions(bool isFilesCollection, int? selectedItemsCount)
        {
            var exportFormatTypes = new HashSet<string>
            {
                "JSON file",
                "Excel spreadsheet",
                "CSV file"
            };

            if (isFilesCollection)
            {
                exportFormatTypes.Add("Stored files");
            }

            ExportFormatOptions = exportFormatTypes;

            var recordsFilterOptions = new HashSet<string>
            {
                "All records"
            };
            if (selectedItemsCount.HasValue)
            {
                recordsFilterOptions.Add($"Selected records ({selectedItemsCount})");
            }

            RecordsFilterOptions = recordsFilterOptions;

            RecordsFilter = RecordsFilterOptions.FirstOrDefault();
        }

        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding RecordsFilterOptions}", SelectionType = SelectionType.RadioButtons)]
        public string RecordsFilter { get; set; }

        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding ExportFormatOptions}", SelectionType = SelectionType.RadioButtons)]
        public string ExportFormat { get; set; }

        public IEnumerable<string> RecordsFilterOptions { get; }

        public IEnumerable<string> ExportFormatOptions { get; }

        public int GetSelectedRecordsFilter()
        {
            return RecordsFilterOptions.IndexOf(RecordsFilter);
        }

        public int GetSelectedExportFormat()
        {
            return ExportFormatOptions.IndexOf(ExportFormat);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}