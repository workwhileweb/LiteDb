using System;
using System.Collections.Generic;

namespace LiteDbExplorer.Core
{
    public class CollectionReferenceChangedEventArgs<T> : EventArgs
    {
        public CollectionReferenceChangedEventArgs(ReferenceNodeChangeAction action, IEnumerable<T> items)
        {
            Action = action;
            Items = items;
        }

        public ReferenceNodeChangeAction Action { get; }
        public IEnumerable<T> Items { get; }
    }
}