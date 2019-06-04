using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using LiteDbExplorer.Core;
using LiteDbExplorer.Windows;
using LiteDB;
using LiteDbExplorer.Core.Events;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer.Modules
{
    public interface IDatabaseInteractions
    {
        Paths PathDefinitions { get; }
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
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationInteraction _applicationInteraction;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public DatabaseInteractions(
            IEventAggregator eventAggregator, 
            IApplicationInteraction applicationInteraction)
        {
            _eventAggregator = eventAggregator;
            _applicationInteraction = applicationInteraction;
            PathDefinitions = new Paths();
        }

        public Paths PathDefinitions { get; }

        public async Task CreateAndOpenDatabase()
        {
            var dialog = new SaveFileDialog
            {
                Title = "New Database",
                Filter = "All files|*.*",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            using (var stream = new FileStream(dialog.FileName, System.IO.FileMode.Create))
            {
                LiteEngine.CreateDatabase(stream);
            }

            await OpenDatabase(dialog.FileName).ConfigureAwait(false);
        }

        public async Task OpenDatabase()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Database",
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                await OpenDatabase(dialog.FileName).ConfigureAwait(false);
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
                if (DatabaseReference.IsDbPasswordProtected(path) && 
                    InputBoxWindow.ShowDialog("Database is password protected, enter password:", "Database password.", password, out password) != true)
                {
                    return;
                }

                Store.Current.AddDatabase(new DatabaseReference(path, password));

                PathDefinitions.InsertRecentFile(path);
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
            _eventAggregator.PublishOnUIThread(new DatabaseChangeEventArgs(ReferenceNodeChangeAction.Remove, database));

            Store.Current.CloseDatabase(database);

            return Task.CompletedTask;
        }

        public Task<Maybe<string>> SaveDatabaseCopyAs(DatabaseReference database)
        {
            var databaseLocation = database.Location;
            var fileInfo = new FileInfo(databaseLocation);
            if (fileInfo?.DirectoryName == null)
            {
                throw new FileNotFoundException(databaseLocation);
            }

            var fileCount = 0;
            var newFileName = fileInfo.Name;
            do
            {
                fileCount++;
                newFileName = $"{Path.GetFileNameWithoutExtension(fileInfo.Name)} {(fileCount > 0 ? "(" + fileCount + ")" : "")}{Path.GetExtension(fileInfo.Name)}";
            }
            while (File.Exists(Path.Combine(fileInfo.DirectoryName, newFileName)));

            var dialog = new SaveFileDialog
            {
                Title = "Save database as...",
                Filter = "All files|*.*",
                OverwritePrompt = true,
                InitialDirectory = fileInfo.DirectoryName ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FileName = newFileName
            };

            if (dialog.ShowDialog() != true)
            {
                return Task.FromResult(Maybe<string>.None);
            }

            fileInfo.CopyTo(dialog.FileName, false);

            return Task.FromResult(Maybe<string>.From(dialog.FileName));
        }

        public Task<Result<CollectionDocumentChangeEventArgs>> AddFileToDatabase(DatabaseReference database)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Add file to database",
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return Task.FromResult(Result.Fail<CollectionDocumentChangeEventArgs>("FILE_OPEN_CANCELED"));
            }

            try
            {
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName),
                        out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);

                    var documentsCreated = new CollectionDocumentChangeEventArgs(ReferenceNodeChangeAction.Add, new [] {file}, file.Collection);

                    _eventAggregator.PublishOnUIThread(documentsCreated);

                    return Task.FromResult(Result.Ok(documentsCreated));
                }
            }
            catch (Exception exc)
            {
                _applicationInteraction.ShowError(exc, "Failed to upload file:" + Environment.NewLine + exc.Message, "Database error");
            }


            return Task.FromResult(Result.Fail<CollectionDocumentChangeEventArgs>("FILE_OPEN_FAIL"));
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

        public Task<Result<CollectionReference>> AddCollection(DatabaseReference database)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New collection name:", "Enter new collection name", "", out string name) == true)
                {
                    var collectionReference = database.AddCollection(name);

                    return Task.FromResult(Result.Ok(collectionReference));
                }

                return Task.FromResult(Result.Ok<CollectionReference>(null));
            }
            catch (Exception exc)
            {
                var message = "Failed to add new collection:" + Environment.NewLine + exc.Message;
                _applicationInteraction.ShowError(exc, message, "Database error");
                return Task.FromResult(Result.Fail<CollectionReference>(message));
            }
        }

        public Task<Result> RenameCollection(CollectionReference collection)
        {
            try
            {
                var currentName = collection.Name;
                if (InputBoxWindow.ShowDialog("New name:", "Enter new collection name", currentName, out string name) == true)
                {
                    collection.Database.RenameCollection(currentName, name);

                    return Task.FromResult(Result.Ok());
                }

                return Task.FromResult(Result.Fail(Fails.Canceled));
            }
            catch (Exception exc)
            {
                var message = "Failed to rename collection:" + Environment.NewLine + exc.Message;
                _applicationInteraction.ShowError(exc, message, "Database error");
                return Task.FromResult(Result.Fail(message));
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

                    _eventAggregator.PublishOnUIThread(new CollectionChangeEventArgs(ReferenceNodeChangeAction.Remove, collection));
                    
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

        public Task ExportCollection(CollectionReference collectionReference)
        {
            if (collectionReference == null)
            {
                return Task.CompletedTask;
            }

            if (collectionReference.IsFilesOrChunks)
            {
                var folderDialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Select folder to export files to..."
                };

                if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    foreach (var file in collectionReference.Items)
                    {
                        var path = Path.Combine(folderDialog.FileName, $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                    }
                }
            }
            else
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save Json export",
                    Filter = "Json File|*.json",
                    FileName = $"{collectionReference.Name}_export.json",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() == true)
                {
                    var data = new BsonArray(collectionReference.LiteCollection.FindAll());

                    using (var writer = new StreamWriter(dialog.FileName))
                    {
                        JsonSerializer.Serialize(data, writer, true, false);
                    }
                }   
            }

            return Task.CompletedTask;
        }

        public Task ExportDocuments(ICollection<DocumentReference> documents)
        {
            if (documents == null || !documents.Any())
            {
                return Task.CompletedTask;
            }

            var documentReference = documents.First();

            if (documentReference.Collection is FileCollectionReference)
            {
                if (documents.Count == 1)
                {
                    var file = documentReference;

                    var dialog = new SaveFileDialog
                    {
                        Filter = "All files|*.*",
                        FileName = file.LiteDocument["filename"],
                        OverwritePrompt = true
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        (file.Collection as FileCollectionReference)?.SaveFile(file, dialog.FileName);
                    }
                }
                else
                {
                    var dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = true,
                        Title = "Select folder to export files to..."
                    };

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        foreach (var file in documents)
                        {
                            var path = Path.Combine(dialog.FileName,
                                $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
                            var dir = Path.GetDirectoryName(path);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                        }
                    }
                }
            }
            else
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save Json export",
                    Filter = "Json File|*.json",
                    FileName = "export.json",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() == true)
                {
                    if (documents.Count == 1)
                    {

                        using (var writer = new StreamWriter(dialog.FileName))
                        {
                            JsonSerializer.Serialize(documentReference.LiteDocument, writer, true, false);
                        }
                    }
                    else
                    {
                        var data = new BsonArray(documents.Select(a => a.LiteDocument));
                        using (var writer = new StreamWriter(dialog.FileName))
                        {
                            JsonSerializer.Serialize(data, writer, true, false);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task ExportJson(IJsonSerializerProvider provider, string name = "")
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Json export",
                Filter = "Json File|*.json",
                FileName = "export.json",
                OverwritePrompt = true
            };

            if (!string.IsNullOrEmpty(name))
            {
                if (!name.EndsWith(".json"))
                {
                    name += ".json";
                }

                dialog.FileName = $"{name}";
            }

            if (dialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(dialog.FileName))
                {
                    provider.Serialize(writer, true);
                }
            }

            return Task.CompletedTask;
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
            if (result)
            {
                _eventAggregator.PublishOnUIThread(new DocumentChangeEventArgs(ReferenceNodeChangeAction.Update, document));

                return Task.FromResult(Maybe<DocumentReference>.From(document));
            }
            return Task.FromResult(Maybe<DocumentReference>.From(null));
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
                Logger.Warn(e, "Cannot process clipboard data.");
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