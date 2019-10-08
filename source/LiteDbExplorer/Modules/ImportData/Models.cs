using System.ComponentModel;
using Forge.Forms.Annotations;

namespace LiteDbExplorer.Modules.ImportData
{
    public interface IDataImportTaskHandler
    {
        string HandlerDisplayName { get; }
        int HandlerDisplayOrder { get; }
        object SourceOptionsContext { get; }
        object TargetOptionsContext { get; }
    }

    public enum RecordIdHandlerPolice
    {
        [EnumDisplay("Insert with new _id if exists")]
        InsertNewIfExists,

        [EnumDisplay("Overwrite documents with some _id")]
        OverwriteDocuments,

        [EnumDisplay("Skip documents with some _id")]
        SkipDocuments,

        [EnumDisplay("Always insert with new _id")]
        AlwaysInsertNew,

        [EnumDisplay("Drop collection first if already exists")]
        DropCollectionIfExists,

        [EnumDisplay("Abort if id already exists")]
        AbortIfExists,
    }

    public enum RecordNullOrEmptyHandlerPolice
    {
        [EnumDisplay("Import as Null")]
        ImportAsNull,

        [EnumDisplay("Import as Default Type Value")]
        ImportAsDefaultTypeValue,

        [EnumDisplay("Exclude")]
        Exclude,
    }

}