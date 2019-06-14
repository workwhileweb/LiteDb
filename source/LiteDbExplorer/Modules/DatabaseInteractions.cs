using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CSharpFunctionalExtensions;
using LiteDbExplorer.Core;
using LiteDbExplorer.Windows;
using LiteDB;
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
        Task ExportJson(IJsonSerializerProvider provider, string name = "");
        Task ExportCollection(CollectionReference collectionReference);
        Task ExportDocuments(ICollection<DocumentReference> documents);
        Task<Result<CollectionDocumentChangeEventArgs>> AddFileToDatabase(DatabaseReference database);
        Task<Result<CollectionDocumentChangeEventArgs>> ImportDataFromText(CollectionReference collection, string textData);
        Task<Result<CollectionDocumentChangeEventArgs>> CreateItem(CollectionReference collection);
        Task<Result> CopyDocuments(IEnumerable<DocumentReference> documents);
        Task<Maybe<DocumentReference>> OpenEditDocument(DocumentReference document);
        Task<Result<CollectionReference>> AddCollection(DatabaseReference database);
        Task<Result> RenameCollection(CollectionReference collection);
        Task<Result<CollectionReference>> DropCollection(CollectionReference collection);
        Task<Result> RemoveDocuments(IEnumerable<DocumentReference> documents);
        Task<Result<bool>> RevealInExplorer(DatabaseReference database);
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
            
            Logger.Information("Open database {path}", path);

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
                var maybeName = await _applicationInteraction.ShowInputDialog("New collection name:", "Enter new collection name", currentName);
                if (maybeName.HasValue)
                {
                    collection.Database.RenameCollection(currentName, maybeName.Value);
                    return Result.Ok();
                }

                return Result.Fail(Fails.Canceled);
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

        public async Task ExportCollection(CollectionReference collectionReference)
        {
            if (collectionReference == null)
            {
                return;
            }

            if (collectionReference.IsFilesOrChunks)
            {
                var maybeDirectoryPath = await _applicationInteraction.ShowFolderPickerDialog("Select folder to export files to...");
                if (maybeDirectoryPath.HasValue)
                {
                    foreach (var file in collectionReference.Items)
                    {
                        var path = Path.Combine(maybeDirectoryPath.Value, $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
                        
                        DirectoryExtensions.EnsureFileDirectory(path);
                        
                        (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                    }
                }
            }
            else
            {
                var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save Json export", "Json File|*.json", $"{collectionReference.Name}_export.json");
                if (maybeFileName.HasValue)
                {
                    var data = new BsonArray(collectionReference.LiteCollection.FindAll());
                    using (var writer = new StreamWriter(maybeFileName.Value))
                    {
                        JsonSerializer.Serialize(data, writer, true, false);
                    }
                }
            }
        }

        public async Task ExportDocuments(ICollection<DocumentReference> documents)
        {
            if (documents == null || !documents.Any())
            {
                return;
            }

            var documentReference = documents.First();

            if (documentReference.Collection is FileCollectionReference)
            {
                if (documents.Count == 1)
                {
                    var file = documentReference;
                    var maybeFileName = await _applicationInteraction.ShowSaveFileDialog(fileName: file.LiteDocument["filename"]);
                    if (maybeFileName.HasValue)
                    {
                        (file.Collection as FileCollectionReference)?.SaveFile(file, maybeFileName.Value);
                    }
                }
                else
                {
                    var maybeDirectoryPath = await _applicationInteraction.ShowFolderPickerDialog("Select folder to export files to...");
                    if (maybeDirectoryPath.HasValue)
                    {
                        foreach (var file in documents)
                        {
                            var path = Path.Combine(maybeDirectoryPath.Value, $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
                            
                            DirectoryExtensions.EnsureFileDirectory(path);
                            
                            (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                        }
                    }
                }
            }
            else
            {
                var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save Json export", "Json File|*.json", "export.json");
                if (maybeFileName.HasValue)
                {
                    if (documents.Count == 1)
                    {
                        using (var writer = new StreamWriter(maybeFileName.Value))
                        {
                            JsonSerializer.Serialize(documentReference.LiteDocument, writer, true, false);
                        }
                    }
                    else
                    {
                        var data = new BsonArray(documents.Select(a => a.LiteDocument));
                        using (var writer = new StreamWriter(maybeFileName.Value))
                        {
                            JsonSerializer.Serialize(data, writer, true, false);
                        }
                    }
                }
            }
        }

        public async Task ExportJson(IJsonSerializerProvider provider, string name = "")
        {
            var fileName = "export.json";
            if (!string.IsNullOrEmpty(name))
            {
                fileName = DirectoryExtensions.EnsureFileNameExtension(name, ".json");
            }

            var maybeFileName = await _applicationInteraction.ShowSaveFileDialog("Save Json export", "Json File|*.json", fileName);
            if (maybeFileName.HasValue)
            {
                using (var writer = new StreamWriter(maybeFileName.Value))
                {
                    provider.Serialize(writer, true);
                }
            }
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

        public Task<Result<bool>> RevealInExplorer(DatabaseReference database)
        {
            var isOpen = _applicationInteraction.RevealInExplorer(database.Location);

            return Task.FromResult(Result.Ok(isOpen));
        }

    }
}