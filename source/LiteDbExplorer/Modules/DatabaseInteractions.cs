using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Enterwell.Clients.Wpf.Notifications;
using Forge.Forms;
using ICSharpCode.AvalonEdit.Document;
using LiteDbExplorer.Core;
using LiteDB;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Presentation;
using OfficeOpenXml;
using Serilog;

namespace LiteDbExplorer.Modules
{
    public interface IDatabaseInteractions
    {
        Task CreateAndOpenDatabase();
        Task OpenDatabase();
        Task OpenDatabase(string path, string password = "");
        Task OpenDatabases(IEnumerable<string> paths);
        Task CloseDatabase(DatabaseReference database);
        Task<Maybe<string>> SaveDatabaseCopyAs(DatabaseReference database);
        Task<Result<CollectionDocumentChangeEventArgs>> AddFileToDatabase(DatabaseReference database);
        Task<Result<CollectionDocumentChangeEventArgs>> ImportDataFromText(CollectionReference collection, string textData);
        Task<Result<CollectionDocumentChangeEventArgs>> CreateItem(CollectionReference collection);
        Task<Result> CopyDocuments(IEnumerable<DocumentReference> documents);
        Task<Maybe<DocumentReference>> OpenEditDocument(DocumentReference document);
        Task<Result<CollectionReference>> AddCollection(DatabaseReference database);
        Task<Result> RenameCollection(CollectionReference collection);
        Task<Result<CollectionReference>> DropCollection(CollectionReference collection);
        Task<Result> RemoveDocuments(IEnumerable<DocumentReference> documents);

        Task<Maybe<string>> ExportToJson(IJsonSerializerProvider provider, string fileName = "");
        Task<Maybe<string>> ExportToExcel(ICollection<DocumentReference> documents, string name = "");
        Task<Maybe<string>> ExportToJson(ICollection<DocumentReference> documents, string name = "");
        Task<Maybe<string>> ExportStoredFiles(ICollection<DocumentReference> documents);
        Task<Maybe<string>> ExportToCsv(ICollection<DocumentReference> documents, string name = "");

        Task<Maybe<string>> ExportCollection(IScreen context, CollectionReference collectionReference, IList<DocumentReference> selectedDocuments = null);
        Task<Maybe<string>> ExportDocuments(IScreen context, ICollection<DocumentReference> documents, string name = "");
    }

    [Export(typeof(IDatabaseInteractions))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class DatabaseInteractions : IDatabaseInteractions
    {
        private static readonly ILogger Logger = Log.ForContext<DatabaseInteractions>();
        private readonly IApplicationInteraction _applicationInteraction;
        private readonly IRecentFilesProvider _recentFilesProvider;

        [ImportingConstructor]
        public DatabaseInteractions(IApplicationInteraction applicationInteraction, IRecentFilesProvider recentFilesProvider)
        {
            _applicationInteraction = applicationInteraction;
            _recentFilesProvider = recentFilesProvider;
        }

        public async Task CreateAndOpenDatabase()
        {
            var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("New Database");
            if (maybeFileName.HasNoValue)
            {
                return;
            }

            using (var stream = new FileStream(maybeFileName.Value, System.IO.FileMode.Create))
            {
                LiteEngine.CreateDatabase(stream);
            }

            await OpenDatabase(maybeFileName.Value).ConfigureAwait(false);
        }

        public async Task OpenDatabase()
        {
            var maybeFileName = await _applicationInteraction.ShowOpenFileDialog("Open Database");
            if (maybeFileName.HasNoValue)
            {
                return;
            }

            try
            {
                await OpenDatabase(maybeFileName.Value).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                Logger.Error(exc, "Failed to open database: ");
                _applicationInteraction.ShowError(exc,"Failed to open database: " + exc.Message);
            }
        }

        public async Task OpenDatabases(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                await OpenDatabase(path).ConfigureAwait(false);
            }
        }

        public async Task OpenDatabase(string path, string password = "")
        {
            if (Store.Current.IsDatabaseOpen(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                _applicationInteraction.ShowError("Cannot open database, file not found.", "File not found");
                return;
            }
            
            try
            {
                if (DatabaseReference.IsDbPasswordProtected(path))
                {
                    var maybePassword = await _applicationInteraction.ShowInputDialog("Database is password protected, enter password:", "Database password.", password);
                    if (maybePassword.HasNoValue)
                    {
                        return;    
                    }
                    
                    password = maybePassword.Value;
                }

                Store.Current.AddDatabase(new DatabaseReference(path, password));

                _recentFilesProvider.InsertRecentFile(path);
            }
            catch (LiteException liteException)
            {
                await OpenDatabaseExceptionHandler(liteException, path, password);
            }
            catch (NotSupportedException notSupportedException)
            {
                _applicationInteraction.ShowError(notSupportedException,"Failed to open database [NotSupportedException]:" + Environment.NewLine + notSupportedException.Message);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to open database: ");
                _applicationInteraction.ShowError(e,"Failed to open database [Exception]:" + Environment.NewLine + e.Message);
            }
        }

        protected virtual async Task OpenDatabaseExceptionHandler(LiteException liteException, string path, string password = "")
        {
            if (liteException.ErrorCode == LiteException.DATABASE_WRONG_PASSWORD)
            {
                if (!string.IsNullOrEmpty(password))
                {
                    _applicationInteraction.ShowError(liteException,"Failed to open database [LiteException]:" + Environment.NewLine + liteException.Message);
                }
                    
                await OpenDatabase(path, password).ConfigureAwait(false);
            }
        }
        
        public Task CloseDatabase(DatabaseReference database)
        {
            Store.Current.CloseDatabase(database);

            return Task.CompletedTask;
        }

        public async Task<Maybe<string>> SaveDatabaseCopyAs(DatabaseReference database)
        {
            var databaseLocation = database.Location;
            var fileInfo = new FileInfo(databaseLocation);
            if (fileInfo.DirectoryName == null)
            {
                throw new FileNotFoundException(databaseLocation);
            }

            var newFileName = fileInfo.EnsureUniqueFileName();

            var initialDirectory = fileInfo.DirectoryName ??
                               Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save database as...", fileName: newFileName, initialDirectory: initialDirectory);
            if (maybeFileName.HasNoValue)
            {
                return Maybe<string>.None;
            }

            fileInfo.CopyTo(maybeFileName.Value, false);

            return Maybe<string>.From(maybeFileName.Value);
        }

        public async Task<Result<CollectionDocumentChangeEventArgs>> AddFileToDatabase(DatabaseReference database)
        {
            var maybeFileName = await _applicationInteraction.ShowOpenFileDialog("Add file to database");
            if (maybeFileName.HasNoValue)
            {
                return Result.Fail<CollectionDocumentChangeEventArgs>("FILE_OPEN_CANCELED");
            }

            try
            {
                var maybeId = await _applicationInteraction.ShowInputDialog("New file id:", "Enter new file id", Path.GetFileName(maybeFileName.Value));
                if (maybeId.HasValue)
                {
                    var file = database.AddFile(maybeId.Value, maybeFileName.Value);
                    var documentsCreated = new CollectionDocumentChangeEventArgs(ReferenceNodeChangeAction.Add, new [] {file}, file.Collection);
                    return Result.Ok(documentsCreated);
                }
            }
            catch (Exception exc)
            {
                _applicationInteraction.ShowError(exc, "Failed to upload file:" + Environment.NewLine + exc.Message, "Database error");
            }


            return Result.Fail<CollectionDocumentChangeEventArgs>("FILE_OPEN_FAIL");
        }

        public Task<Result> RemoveDocuments(IEnumerable<DocumentReference> documents)
        {
            if (!_applicationInteraction.ShowConfirm("Are you sure you want to remove items?", "Are you sure?"))
            {
                return Task.FromResult(Result.Fail(Fails.Canceled));
            }

            foreach (var document in documents.ToList())
            {
                document.RemoveSelf();
            }
            
            return Task.FromResult(Result.Ok());
        }

        public async Task<Result<CollectionReference>> AddCollection(DatabaseReference database)
        {
            try
            {
                var maybeName = await _applicationInteraction.ShowInputDialog("New collection name:", "Enter new collection name");
                if (maybeName.HasValue)
                {
                    var collectionReference = database.AddCollection(maybeName.Value);
                    return Result.Ok(collectionReference);
                }

                return Result.Ok<CollectionReference>(null);
            }
            catch (Exception exc)
            {
                var message = "Failed to add new collection:" + Environment.NewLine + exc.Message;
                _applicationInteraction.ShowError(exc, message, "Database error");
                return Result.Fail<CollectionReference>(message);
            }
        }

        public async Task<Result> RenameCollection(CollectionReference collection)
        {
            try
            {
                var currentName = collection.Name;

                Result Validate(string value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return Result.Fail("Name cannot be empty.");
                    }

                    if (value.Any(char.IsWhiteSpace))
                    {
                        return Result.Fail("Name can not contain white spaces.");
                    }

                    if (collection.Database.ContainsCollection(value))
                    {
                        return Result.Fail($"Collection \"{value}\" already exists!");
                    }

                    return Result.Ok();
                }

                var maybeName = await _applicationInteraction
                    .ShowInputDialog("New collection name:", "Enter new collection name", currentName, Validate);

                if (maybeName.HasNoValue)
                {
                    return Result.Fail(Fails.Canceled);
                }

                collection.Database.RenameCollection(currentName, maybeName.Value);
                return Result.Ok();

            }
            catch (Exception exc)
            {
                var message = "Failed to rename collection:" + Environment.NewLine + exc.Message;
                _applicationInteraction.ShowError(exc, message, "Database error");
                return Result.Fail(message);
            }
        }

        public Task<Result<CollectionReference>> DropCollection(CollectionReference collection)
        {
            try
            {
                var collectionName = collection.Name;
                if (_applicationInteraction.ShowConfirm($"Are you sure you want to drop collection \"{collectionName}\"?", "Are you sure?"))
                {
                    collection.Database.DropCollection(collectionName);

                    return Task.FromResult(Result.Ok(collection));
                }

                return Task.FromResult(Result.Fail<CollectionReference>(Fails.Canceled));
            }
            catch (Exception exc)
            {
                var message = "Failed to drop collection:" + Environment.NewLine + exc.Message;
                _applicationInteraction.ShowError(exc, message, "Database error");
                return Task.FromResult(Result.Fail<CollectionReference>(message));
            }
        }

        public async Task<Maybe<string>> ExportCollection(
            IScreen context, 
            CollectionReference collectionReference, 
            IList<DocumentReference> selectedDocuments = null)
        {
            if (collectionReference == null)
            {
                return null;
            }

            var exportOptions =
                new CollectionExportOptions(collectionReference.IsFilesOrChunks, selectedDocuments?.Count);

            var dialogHostIdentifier = AppConstants.DialogHosts.Shell;
            if (context is LiteDbExplorer.Wpf.Framework.Shell.IDocument document)
            {
                dialogHostIdentifier = document.DialogHostIdentifier;
            }
            var result = await Show.Dialog(dialogHostIdentifier).For(exportOptions);
            if (result.Action is "cancel")
            {
                return null;
            }

            var itemsToExport = result.Model.GetSelectedRecordsFilter() == 0
                ? collectionReference.Items
                : selectedDocuments;
            var referenceName = collectionReference.Name;

            Maybe<string> maybePath = null;
            switch (result.Model.GetSelectedExportFormat())
            {
                case 0:
                    maybePath = await ExportToJson(itemsToExport, referenceName);
                    break;
                case 1:
                    maybePath = await ExportToExcel(itemsToExport, referenceName);
                    break;
                case 2:
                    maybePath = await ExportToCsv(itemsToExport, referenceName);
                    break;
                case 3:
                    maybePath = await ExportStoredFiles(itemsToExport);
                    break;
            }

            if (maybePath.HasValue)
            {
                var builder = NotificationInteraction.Default()
                    .HasMessage($"{result.Model.ExportFormat} saved in:\n{maybePath.Value.ShrinkPath(128)}");

                if (Path.HasExtension(maybePath.Value))
                {
                    builder.Dismiss().WithButton("Open",
                        async button =>
                        {
                            await _applicationInteraction.OpenFileWithAssociatedApplication(maybePath.Value);
                        });
                }

                builder.WithButton("Reveal in Explorer",
                        async button => { await _applicationInteraction.RevealInExplorer(maybePath.Value); })
                    .Dismiss().WithButton("Close", button => { });

                builder.Queue();
            }

            return maybePath;
        }

        public async Task<Maybe<string>> ExportDocuments(IScreen context, ICollection<DocumentReference> documents, string name = "")
        {
            if (documents == null || !documents.Any())
            {
                return null;
            }

            var documentReference = documents.First();
            if (documentReference.Collection is FileCollectionReference)
            {
                return await ExportStoredFiles(documents);
            }

            return await ExportToJson(documents, name);
        }

        public async Task<Maybe<string>> ExportToJson(ICollection<DocumentReference> documents, string name = "")
        {
            var fileName = ArchiveExtensions.EnsureFileName(name, "export", ".json", true);
            var maybeJsonFileName = await _applicationInteraction.ShowSaveFileDialog("Save Json export", "Json File|*.json", fileName);
            if (maybeJsonFileName.HasValue)
            {
                if (documents.Count == 1)
                {
                    using (var writer = new StreamWriter(maybeJsonFileName.Value))
                    {
                        JsonSerializer.Serialize(documents.First().LiteDocument, writer, true, false);
                    }
                }
                else
                {
                    var data = new BsonArray(documents.Select(a => a.LiteDocument));
                    using (var writer = new StreamWriter(maybeJsonFileName.Value))
                    {
                        JsonSerializer.Serialize(data, writer, true, false);
                    }
                }
            }

            return maybeJsonFileName;
        }

        public static readonly Dictionary<BsonType, Func<object, string>> BsonTypeToExcelNumberFormat =
            new Dictionary<BsonType, Func<object, string>>
            {
                {BsonType.Int32, _ => "0"},
                {BsonType.Int64, _ => "0"},
                {BsonType.DateTime, _ => $"{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern} {CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern}"}
            };

        public async Task<Maybe<string>> ExportToExcel(ICollection<DocumentReference> documents, string name = "")
        {
            var fileName = ArchiveExtensions.EnsureFileName(name, "export", ".xlsx", true);
            var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save Excel export", "Excel File|*.xlsx", fileName);
            if (maybeFileName.HasNoValue)
            {
                return null;
            }

            var keys = documents.SelectAllDistinctKeys().ToArray();

            var excelPackage = new ExcelPackage();
            var ws = excelPackage.Workbook.Worksheets.Add(name);

            // Add headers
            for (var i = 0; i < keys.Length; i++)
            {
                ws.Cells[1, i + 1].Value = keys[i];
            }

            // Add data
            var currentColl = 1;
            var currentRow = 2;
            foreach (var documentReference in documents)
            {
                foreach (var key in keys)
                {
                    if (documentReference.LiteDocument.ContainsKey(key))
                    {
                        var bsonValue = documentReference.LiteDocument[key];
                        object cellValue = null;
                        if (bsonValue.IsArray || bsonValue.IsDocument)
                        {
                            cellValue = bsonValue.ToDisplayValue();
                        }
                        else
                        {
                            cellValue = bsonValue.RawValue;     
                        }

                        var cell = ws.Cells[currentRow, currentColl];
                        cell.Value = cellValue;
                        if (BsonTypeToExcelNumberFormat.TryGetValue(bsonValue.Type, out var format))
                        {
                            cell.Style.Numberformat.Format = format(bsonValue);
                        }
                    }
                    
                    currentColl++;
                }

                currentColl = 1;
                currentRow++;
            }
            
            var tableRange = ws.Cells[1, 1, documents.Count + 1, keys.Length];
            var resultsTable = ws.Tables.Add(tableRange, $"{Regex.Replace(name, @"\s", "_")}_table");

            resultsTable.ShowFilter = true;
            resultsTable.ShowHeader = true;
            
            // AutoFit
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            excelPackage.SaveAs(new FileInfo(maybeFileName.Value));
            excelPackage.Dispose();

            return maybeFileName.Value;
        }

        public async Task<Maybe<string>> ExportToCsv(ICollection<DocumentReference> documents, string name = "")
        {
            var fileName = ArchiveExtensions.EnsureFileName(name, "export", ".csv", true);
            var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save CSV export", "Excel File|*.xlsx", fileName);
            if (maybeFileName.HasNoValue)
            {
                return null;
            }

            var separator = ",";
            var reservedTokens = new[] { '\"', ',', '\n', '\r' };
            var keys = documents.SelectAllDistinctKeys().ToArray();

            var contents = new List<string>
            {
                string.Join(separator, keys)
            };

            foreach (var documentReference in documents)
            {
                var rowCols = new string[keys.Length];
                var currentCol = 0;
                foreach (var key in keys)
                {
                    if (documentReference.LiteDocument.ContainsKey(key))
                    {
                        string cellValue = null;
                        var bsonValue = documentReference.LiteDocument[key];
                        if (!bsonValue.IsArray && !bsonValue.IsDocument && !bsonValue.IsNull)
                        {
                            cellValue = Convert.ToString(bsonValue.RawValue, CultureInfo.InvariantCulture);
                        }
                        // Escape reserved tokens
                        if (cellValue != null && cellValue.IndexOfAny(reservedTokens) >= 0)
                        {
                            cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";  
                        }
                        rowCols[currentCol] = cellValue;
                    }
                    currentCol++;
                }
                contents.Add(string.Join(separator, rowCols));
            }

            File.WriteAllLines(maybeFileName.Value, contents, Encoding.UTF8);

            return maybeFileName;

        }

        public async Task<Maybe<string>> ExportStoredFiles(ICollection<DocumentReference> documents)
        {
            var documentReference = documents.FirstOrDefault();
            if (!(documentReference?.Collection is FileCollectionReference))
            {
                return null;
            }

            Maybe<string> maybePath = null;
            if (documents.Count == 1)
            {
                var file = documentReference;
                maybePath = await _applicationInteraction.ShowSaveFileDialog(fileName: file.LiteDocument["filename"]);
                if (maybePath.HasValue)
                {
                    (file.Collection as FileCollectionReference)?.SaveFile(file, maybePath.Value);
                }
            }
            else
            {
                maybePath = await _applicationInteraction.ShowFolderPickerDialog("Select folder to export files to...");
                if (maybePath.HasValue)
                {
                    foreach (var file in documents)
                    {
                        var prefix = file.LiteDocument["_id"].AsString.Replace('/', ' ').Split('.').FirstOrDefault();

                        var path = Path.Combine(maybePath.Value, $"{prefix}-{file.LiteDocument["filename"].AsString}");
                            
                        ArchiveExtensions.EnsureFileDirectory(path);
                            
                        (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                    }
                }    
            }

            return maybePath;
        }

        public async Task<Maybe<string>> ExportToJson(IJsonSerializerProvider provider, string fileName = "")
        {
            var exportFileName = "export.json";
            if (!string.IsNullOrEmpty(fileName))
            {
                exportFileName = ArchiveExtensions.EnsureFileNameExtension(fileName, ".json");
            }

            var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save Json export", "Json File|*.json", exportFileName);
            if (maybeFileName.HasValue)
            {
                using (var writer = new StreamWriter(maybeFileName.Value))
                {
                    provider.Serialize(writer, true);
                }
            }

            return maybeFileName;
        }

        public Task<Result> CopyDocuments(IEnumerable<DocumentReference> documents)
        {
            var data = new BsonArray(documents.Select(a => a.LiteDocument));
            
            Clipboard.SetData(DataFormats.Text, JsonSerializer.Serialize(data, true, false));

            return Task.FromResult(Result.Ok());
        }
        
        public Task<Maybe<DocumentReference>> OpenEditDocument(DocumentReference document)
        {
            var result = _applicationInteraction.OpenEditDocument(document);
            return Task.FromResult(Maybe<DocumentReference>.From(result ? document: null));
        }
        
        public Task<Result<CollectionDocumentChangeEventArgs>> ImportDataFromText(CollectionReference collection, string textData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textData))
                {
                    return Task.FromResult(Result.Ok(CollectionDocumentChangeEventArgs.Nome));
                }

                var newValue = JsonSerializer.Deserialize(textData);
                var newDocs = new List<DocumentReference>();
                if (newValue.IsArray)
                {
                    foreach (var value in newValue.AsArray)
                    {
                        var doc = value.AsDocument;
                        var documentReference = collection.AddItem(doc);
                        newDocs.Add(documentReference);
                    }
                }
                else
                {
                    var doc = newValue.AsDocument;
                    var documentReference = collection.AddItem(doc);
                    newDocs.Add(documentReference);
                }

                var documentsUpdate = new CollectionDocumentChangeEventArgs(ReferenceNodeChangeAction.Add, newDocs, collection);

                return Task.FromResult(Result.Ok(documentsUpdate));
            }
            catch (Exception e)
            {
                var message = "Failed to import document from text content: " + e.Message;
                Logger.Warning(e, "Cannot process clipboard data.");
                _applicationInteraction.ShowError(e, message, "Import Error");

                return Task.FromResult(Result.Fail<CollectionDocumentChangeEventArgs>(message));
            }
        }
        
        public Task<Result<CollectionDocumentChangeEventArgs>> CreateItem(CollectionReference collection)
        {
            if (collection is FileCollectionReference)
            {
                return AddFileToDatabase(collection.Database);
            }

            var newDoc = new BsonDocument
            {
                ["_id"] = ObjectId.NewObjectId()
            };

            var documentReference = collection.AddItem(newDoc);
            
            var documentsCreated = new CollectionDocumentChangeEventArgs(ReferenceNodeChangeAction.Add, documentReference, collection);

            return Task.FromResult(Result.Ok(documentsCreated));
        }
    }
}