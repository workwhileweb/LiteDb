using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Forge.Forms.Annotations;
using Forge.Forms.Validation;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.Shared
{
    [Title("Add file options")]
    [Action("cancel", "CANCEL", IsCancel = true, ClosesDialog = true)]
    [Action("ok", "CREATE", IsDefault = true, ClosesDialog = true, Validates = true)]
    public class AddFileOptions : INotifyPropertyChanged
    {
        private readonly DatabaseReference _database;

        public AddFileOptions(DatabaseReference database, string newFileId)
        {
            _database = database;

            NewFileId = newFileId;
        }

        [Value(Must.NotBeEmpty)]
        [Value(Must.SatisfyMethod, nameof(ValidateNameNotContainsWhiteSpace), StrictValidation = true, 
            Message = "File Id can not contain white spaces.")]
        [Value(Must.SatisfyMethod, nameof(ValidateCollectionNotExist), StrictValidation = true, 
            Message = "Cannot add file {Value}, file with that id already exists.")]
        [Field(Name = "File Id", ToolTip = "Enter new file id.")]
        public string NewFileId { get; set; }

        public static bool ValidateNameNotContainsWhiteSpace(ValidationContext validationContext)
        {
            var value = validationContext.PropertyValue as string;
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            return !value.Any(char.IsWhiteSpace);
        }

        public static bool ValidateCollectionNotExist(ValidationContext validationContext)
        {
            var model = validationContext.Model as AddFileOptions;
            var value = validationContext.PropertyValue as string;

            if (model?._database == null || string.IsNullOrEmpty(value))
            {
                return true;
            }

            return !model._database.FileExists(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}