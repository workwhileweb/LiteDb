using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Forge.Forms.Annotations;
using Forge.Forms.Validation;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using OfficeOpenXml.FormulaParsing;

namespace LiteDbExplorer.Modules.Shared
{
    [Title("Add collection options")]
    [Action("cancel", "CANCEL", IsCancel = true, ClosesDialog = true)]
    [Action("ok", "CREATE", IsDefault = true, ClosesDialog = true, Validates = true)]
    public class AddCollectionOptions : INotifyPropertyChanged
    {
        public AddCollectionOptions(DatabaseReference database)
        {
            ExistingCollectionNames = database.Collections.Select(p => p.Name).ToArray();
        }

        public IReadOnlyList<string> ExistingCollectionNames { get; }

        [Value(Must.NotBeEmpty)]
        [Value(Must.SatisfyMethod, nameof(ValidateNameNotContainsWhiteSpace), StrictValidation = true, 
            Message = "Collection name can not contain white spaces.")]
        [Value(Must.SatisfyMethod, nameof(ValidateCollectionNotExist), StrictValidation = true, 
            Message = "Cannot add collection {Value}, collection with that name already exists.")]
        [Field(Name = "Collection name", ToolTip = "Enter new collection name.")]
        public string NewCollectionName { get; set; }

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
            var model = validationContext.Model as AddCollectionOptions;
            var value = validationContext.PropertyValue as string;

            if (model == null || string.IsNullOrEmpty(value))
            {
                return true;
            }

            return !model.ExistingCollectionNames.Any(p => p.Equals(value, StringComparison.OrdinalIgnoreCase));
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