using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using LiteDB;
using LiteDbExplorer.Core;
using LiteDbExplorer.Modules.DbQuery;

namespace LiteDbExplorer.Modules
{
    public interface IApplicationInteraction
    {
        bool OpenDatabaseProperties(DatabaseReference database);
        bool OpenEditDocument(DocumentReference document);
        Task<Result> OpenQuery(RunQueryContext queryContext);
        bool RevealInExplorer(string filePath);
        Task<Result> ActivateDocument(DocumentReference document);
        Task<Result> ActivateCollection(CollectionReference collection, IEnumerable<DocumentReference> selectedDocuments = null);
        bool ShowConfirm(string message, string title = "Are you sure?");
        void ShowError(string message, string title = "");
        void ShowError(Exception exception, string message, string title = "");
        void PutClipboardText(string text);
        void ShowAbout();
        void ShowReleaseNotes(Version version = null);
        void ShowIssueHelper();
    }
}