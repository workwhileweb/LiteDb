using CSharpFunctionalExtensions;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.Shared
{
    public class ImportDataOptions
    {
        public Maybe<DatabaseReference> DatabaseReference { get; set; }

        public Maybe<CollectionReference> CollectionReference { get; set; }
    }
}