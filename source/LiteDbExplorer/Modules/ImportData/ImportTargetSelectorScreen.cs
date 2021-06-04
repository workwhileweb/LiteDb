using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Forge.Forms.Annotations;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.ImportData
{
    [Form(Grid = new[] { 1d, 1d }, Mode = DefaultFields.None)]
    public class ImportTargetSelectorScreen : Screen
    {
        public ImportTargetSelectorScreen()
        {
            DisplayName = "Target Options";

            Databases = Store.Current.Databases;
        }

        public IEnumerable<DatabaseReference> Databases { get; }

        [UsedImplicitly]
        public IEnumerable<string> TargetDatabaseCollections
        {
            get
            {
                if (TargetDatabase == null)
                {
                    return Enumerable.Empty<string>();
                }

                return TargetDatabase.Collections
                    .Where(p => !p.IsFilesOrChunks)
                    .Select(p => p.Name);
            }
        }

        [Field(Row = "0")]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding Databases}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(DatabaseReference.Name))]
        public DatabaseReference TargetDatabase { get; set; }

        [Field(Row = "0")]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding TargetDatabaseCollections}", SelectionType = SelectionType.ComboBoxEditable)]
        public string TargetCollection { get; set; }
        
        [Break]

        [Field(Row = "1")]
        [SelectFrom(typeof(RecordIdHandlerPolice))]
        public RecordIdHandlerPolice InsertMode { get; set; }

        [Field(Row = "1")]
        [SelectFrom(typeof(RecordNullOrEmptyHandlerPolice))]
        public RecordNullOrEmptyHandlerPolice EmptyFieldsMode { get; set; }

        public CollectionReference GetTargetCollection()
        {
            return TargetDatabase?.Collections.FirstOrDefault(p => p.Name.Equals(TargetCollection));
        }
    }
}