using System;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework.Services;

namespace LiteDbExplorer.Modules.DbCollection
{
    public interface ICollectionReferenceListView : IListViewInteractionProvider
    {
        void UpdateView(CollectionReference collectionReference);
        void UpdateView(DocumentReference documentReference);
        void Find(string text, bool matchCase);
        void FindPrevious(string text, bool matchCase);
        void FocusListView();
        Action CollectionLoadedAction { get; set; }
        void SelectItem(object item);
        void FindClear();
    }
}